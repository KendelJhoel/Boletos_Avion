using System.ComponentModel.DataAnnotations;

namespace Boletos_Avion.Models
{
    public class ChangeEmailViewModel
    {
        [Required(ErrorMessage = "El nuevo correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        public string NewEmail { get; set; }
    }
}
