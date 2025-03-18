document.addEventListener("DOMContentLoaded", function () {
    const abrirModalPagoBtn = document.getElementById("abrirModalPago");
    const confirmarPagoBtn = document.getElementById("confirmarPago");
    const errorMessage = document.getElementById("error-message");

    // 🔹 Captura el ID del vuelo desde la URL
    const urlParams = new URLSearchParams(window.location.search);
    const idVuelo = parseInt(urlParams.get("idVuelo")) || 0;

    console.log("📌 ID de Vuelo:", idVuelo);

    // 🔹 Función para obtener los asientos seleccionados desde la tabla
    function obtenerAsientosSeleccionados() {
        let asientosSeleccionados = Array.from(document.querySelectorAll(".seat.selected")) // Cambié el selector para que solo obtenga los asientos seleccionados
            .map(seatCell => seatCell.getAttribute("data-id")) // Obtenemos el ID del asiento desde el atributo data-id
            .filter(id => id !== undefined && id !== ""); // Filtramos cualquier valor vacío

        console.log("📌 Asientos Seleccionados (IDs):", asientosSeleccionados);
        return asientosSeleccionados;
    }

    // 🔹 Evento para abrir el modal de confirmación
    if (abrirModalPagoBtn) {
        abrirModalPagoBtn.addEventListener("click", function () {
            let modal = new bootstrap.Modal(document.getElementById("modalConfirmacion"));
            modal.show();
        });
    }

    // 🔹 Evento para confirmar el pago
    if (confirmarPagoBtn) {
        confirmarPagoBtn.addEventListener("click", function () {
            confirmarPagoBtn.disabled = true;
            confirmarPagoBtn.innerText = "Procesando...";

            let asientos = obtenerAsientosSeleccionados();

            // ⚠️ Validación: No permitir continuar si no hay asientos
            if (asientos.length === 0) {
                console.error("❌ No se han seleccionado asientos.");
                errorMessage.innerText = "Debe seleccionar al menos un asiento para continuar.";
                errorMessage.style.display = "block";
                confirmarPagoBtn.disabled = false;
                confirmarPagoBtn.innerText = "Confirmar Pago";
                return;
            }

            console.log("📌 Enviando ID Vuelo:", idVuelo);
            console.log("📌 Enviando Asientos (IDs):", asientos);

            // 🔹 Enviar datos al backend
            fetch("/Pagos/ConfirmarPago", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    idVuelo: idVuelo,
                    asientos: asientos.join(",") // Convertimos el array de IDs en un string separado por comas
                })
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        alert(`✅ Reserva confirmada. Número: ${data.numeroReserva}`);
                        window.location.href = `/Vuelos/Detalle/${idVuelo}`;
                    } else {
                        console.error("❌ Error en ConfirmarPago:", data.message);
                        errorMessage.innerText = data.message || "Error al confirmar la reserva.";
                        errorMessage.style.display = "block";
                        confirmarPagoBtn.disabled = false;
                        confirmarPagoBtn.innerText = "Confirmar Pago";
                    }
                })
                .catch(error => {
                    console.error("❌ Error en la conexión con el servidor:", error);
                    errorMessage.innerText = "Error en la conexión con el servidor.";
                    errorMessage.style.display = "block";
                    confirmarPagoBtn.disabled = false;
                    confirmarPagoBtn.innerText = "Confirmar Pago";
                });
        });
    }
});
