document.addEventListener("DOMContentLoaded", function () {
    const abrirModalPagoBtn = document.getElementById("abrirModalPago");
    const confirmarPagoBtn = document.getElementById("confirmarPago");
    const errorMessage = document.getElementById("error-message");

    const urlParams = new URLSearchParams(window.location.search);
    const idVuelo = parseInt(urlParams.get("idVuelo")) || 0;
    const idClienteInput = document.getElementById("idCliente");
    const userRole = parseInt(document.getElementById("userRole").value);
    const idCliente = idClienteInput ? parseInt(idClienteInput.value) : null;
    const esAgente = userRole === 2;

    function obtenerAsientosDesdeTabla() {
        return Array.from(document.querySelectorAll("#asientos-seleccionados tbody tr"))
            .map(row => parseInt(row.getAttribute("data-id")))
            .filter(id => !isNaN(id));
    }

    if (abrirModalPagoBtn) {
        abrirModalPagoBtn.addEventListener("click", function () {
            let modal = new bootstrap.Modal(document.getElementById("modalConfirmacion"));
            modal.show();
        });
    }

    if (confirmarPagoBtn) {
        confirmarPagoBtn.addEventListener("click", function () {
            confirmarPagoBtn.disabled = true;
            confirmarPagoBtn.innerText = "Procesando...";

            let asientos = obtenerAsientosDesdeTabla();

            if (asientos.length === 0) {
                errorMessage.innerText = "Debe seleccionar al menos un asiento para continuar.";
                errorMessage.style.display = "block";
                confirmarPagoBtn.disabled = false;
                confirmarPagoBtn.innerText = "💳 Confirmar Pago";
                return;
            }

            fetch("/Pagos/ConfirmarPago", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    idVuelo: idVuelo,
                    asientos: asientos,
                    idCliente: userRole === 3 ? userId : (esAgente ? idCliente : null)
                })
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`HTTP error! status: ${response.status}`);
                    }
                    return response.json();
                })
                .then(data => {
                    if (data.success) {
                        let redirigirUrl = data.redirigir;
                        const esRegresoFlag = sessionStorage.getItem("esRegreso");
                        if (esRegresoFlag === "true") {
                            redirigirUrl += "&esRegreso=true";
                        }

                        window.location.href = redirigirUrl;
                    } else {
                        errorMessage.innerText = data.message || "Error al confirmar la reserva.";
                        errorMessage.style.display = "block";
                        confirmarPagoBtn.disabled = false;
                        confirmarPagoBtn.innerText = "💳 Confirmar Pago";
                    }
                })
                .catch(error => {
                    console.error("❌ Error en la conexión con el servidor:", error);
                    errorMessage.innerText = "Error en la conexión con el servidor. Por favor, intenta de nuevo.";
                    errorMessage.style.display = "block";
                    confirmarPagoBtn.disabled = false;
                    confirmarPagoBtn.innerText = "💳 Confirmar Pago";
                });
        });
    }

    function recalcularTotales() {
        const precioVuelo = parseFloat(document.getElementById("precio-vuelo").textContent) || 0;
        const filas = document.querySelectorAll("#asientos-seleccionados tbody tr");
        let totalAsientos = 0;
        let count = 0;

        filas.forEach(row => {
            const precioTexto = row.querySelector("td:nth-child(2)").textContent.replace("$", "").replace(",", "");
            const precio = parseFloat(precioTexto);
            if (!isNaN(precio)) {
                totalAsientos += precio;
                count++;
            }
        });

        const subtotal = (precioVuelo * count) + totalAsientos;
        const iva = subtotal * 0.13;
        const total = subtotal + iva;

        document.getElementById("subtotal").textContent = subtotal.toFixed(2);
        document.getElementById("iva").textContent = iva.toFixed(2);
        document.getElementById("total").textContent = total.toFixed(2);
    }

    recalcularTotales();
});
