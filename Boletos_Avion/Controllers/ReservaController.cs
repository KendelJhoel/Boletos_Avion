using Boletos_Avion.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Boletos_Avion.Services;
using System.Net.Mail;
using System.Net;

public class ReservaController : Controller
{
    private readonly ReservaService _reservaService;
    private readonly IConfiguration _configuration;

    public ReservaController(IConfiguration configuration)
    {
        _reservaService = new ReservaService();
        _configuration = configuration;

    }

    public IActionResult MisReservas()
    {
        var userEmail = HttpContext.Session.GetString("UserEmail");
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userEmail == null || userId == null)
        {
            return RedirectToAction("Authentication", "Auth");
        }

        List<Reservas> reservasUsuario = _reservaService.GetReservasActivas((int)userId);
        return View("~/Views/Account/MisReservas.cshtml", reservasUsuario);
    }

    [HttpPost]
    public IActionResult NotificarCancelacionesVuelo(int idVuelo)
    {
        var reservaService = new ReservaService();
        var reservas = reservaService.ObtenerReservasPorVuelo(idVuelo);

        var accountService = new AccountService();

        Console.WriteLine($"📩 Notificando cancelaciones a {reservas.Count} clientes...");

        foreach (var reserva in reservas)
        {
            var cliente = accountService.GetUserById(reserva.IdUsuario);

            if (cliente == null)
            {
                Console.WriteLine($"⚠️ Cliente con ID {reserva.IdUsuario} no encontrado.");
                continue;
            }

            Console.WriteLine($"📤 Enviando correo a: {cliente.Nombre} ({cliente.Correo}) - Total: ${reserva.Total}");

            bool enviado = EnviarCorreoCancelacion(
                cliente.Correo,
                cliente.Nombre,
                reserva.NumeroReserva,
                reserva.Total
            );

            if (enviado)
                Console.WriteLine($"✅ Correo enviado correctamente a {cliente.Correo}");
            else
                Console.WriteLine($"❌ Error al enviar correo a {cliente.Correo}");
        }

        return Ok("Correos enviados correctamente.");
    }

    private bool EnviarCorreoCancelacion(string emailDestino, string nombreUsuario, string numeroReserva, decimal total)
    {
        try
        {
            string senderEmail = _configuration["EmailSettings:SenderEmail"];
            string senderPassword = _configuration["EmailSettings:SenderPassword"];

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail),
                Subject = "Cancelación de vuelo - Reserva anulada",
                Body = $@"
<p>Hola <strong>{nombreUsuario}</strong>,</p>
<p>Te informamos que el vuelo que habías reservado ha sido cancelado.</p>
<p><strong>Número de Reserva:</strong> {numeroReserva}</p>
<p><strong>Total Reembolsado:</strong> ${total.ToString("F2")}</p>
<p>Lamentamos los inconvenientes. Se ha realizado un reembolso del 100% del total pagado.</p>
<br>
<p>Atentamente,</p>
<p><strong>Soporte de Boleto Avión</strong></p>",
                IsBodyHtml = true,
            };

            mailMessage.To.Add(emailDestino);
            smtpClient.Send(mailMessage);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error al enviar correo de cancelación: {ex.Message}");
            return false;
        }
    }

    public IActionResult Detalle(int id)
    {
        var reserva = _reservaService.ObtenerReservaPorId(id);
        if (reserva == null)
            return NotFound("Reserva no encontrada.");

        var vueloService = new VuelosService();
        var asientoService = new AsientoService();

        var vuelo = vueloService.GetVueloDetallesById(reserva.IdVuelo);
        if (vuelo == null)
            return NotFound("No se pudo obtener la información del vuelo.");

        var asientos = asientoService.ObtenerAsientosPorReserva(id);

        var viewModel = new DetalleReservaViewModel
        {
            Reserva = reserva,
            Vuelo = vuelo,
            Asientos = asientos
        };

        return View("~/Views/Reserva/Detalle.cshtml", viewModel);
    }

    [HttpPost]
    public IActionResult CancelarReserva(int id)
    {
        var reserva = _reservaService.ObtenerReservaPorId(id);
        if (reserva == null)
            return NotFound("Reserva no encontrada.");

        var vueloService = new VuelosService();
        var vuelo = vueloService.GetVueloDetallesById(reserva.IdVuelo);

        if (vuelo == null)
            return NotFound("Vuelo no encontrado.");

        var diasRestantes = (vuelo.FechaSalida - DateTime.Now).TotalDays;
        decimal montoReembolso = 0;
        string mensajeReembolso = "";

        if (diasRestantes >= 15)
        {
            montoReembolso = reserva.Total * 0.50m;
            mensajeReembolso = $"Tu reserva fue cancelada con más de 15 días de antelación, por lo tanto se te devolverá el 50% del total: ${montoReembolso:F2}.";

        }
        else if (diasRestantes >= 7)
        {
            montoReembolso = 0;  // No hay reembolso
            mensajeReembolso = "Tu reserva fue cancelada, pero no se te devolverá nada debido a que la cancelación fue con menos de 15 días de antelación.";
        }
        else
        {
            return BadRequest("No se puede cancelar la reserva con menos de 7 días de anticipación.");
        }


        bool actualizado = _reservaService.CambiarEstadoReserva(id, "Inactivo");

        if (!actualizado)
            return StatusCode(500, "Error al cancelar la reserva.");

        var asientoService = new AsientoService();
        var asientos = asientoService.ObtenerAsientosPorReserva(id);
        asientoService.LiberarAsientosPorReserva(id);

        var usuario = new AccountService().GetUserById(reserva.IdUsuario);
        List<string> numerosAsientos = asientos.Select(a => a.Numero).ToList();

        bool enviado = EnviarCorreoCancelacionReserva(
            usuario.Correo,
            usuario.Nombre,
            reserva.NumeroReserva,
            numerosAsientos,
            montoReembolso
        );

        if (enviado)
            Console.WriteLine("✅ Correo de cancelación enviado al cliente.");
        else
            Console.WriteLine("⚠ No se pudo enviar el correo de cancelación.");

        return Json(new { success = true, mensajeReembolso });
    }

    private bool EnviarCorreoCancelacionReserva(string emailDestino, string nombreUsuario, string numeroReserva, List<string> asientos, decimal reembolso)
    {
        try
        {
            string senderEmail = _configuration["EmailSettings:SenderEmail"];
            string senderPassword = _configuration["EmailSettings:SenderPassword"];

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true,
            };

            string asientosList = string.Join(", ", asientos);
            string mensajeReembolso = reembolso > 0
                ? $"Se ha procesado un reembolso del <strong>50%</strong> del total pagado: <strong>${reembolso:F2}</strong>."
                : "Lamentablemente, cancelaste con menos de 15 días de anticipación, por lo tanto <strong>no aplica reembolso</strong>.";

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail),
                Subject = "Cancelación de tu reserva",
                Body = $@"
            <p>Hola <strong>{nombreUsuario}</strong>,</p>
            <p>Tu reserva con número <strong>{numeroReserva}</strong> ha sido <span style='color:red;'>cancelada</span>.</p>
            <p><strong>Asientos reservados:</strong> {asientosList}</p>
            <p>{mensajeReembolso}</p>
            <br>
            <p><strong>Soporte de Boleto Avión</strong></p>",
                IsBodyHtml = true,
            };

            mailMessage.To.Add(emailDestino);
            smtpClient.Send(mailMessage);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error al enviar correo de cancelación: {ex.Message}");
            return false;
        }
    }
}
