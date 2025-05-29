
document.addEventListener("DOMContentLoaded", function () {

    const abrirModalPagoBtn = document.getElementById("abrirModalPago");
    const validarTarjetaBtn = document.getElementById("validarTarjeta");
    const confirmarPagoBtn = document.getElementById("confirmarPago");
    const errorMessage = document.getElementById("error-message");
    const modalTitle = document.getElementById("modalTitle");
    const tarjetaSection = document.getElementById("tarjetaSection");
    const confirmacionSection = document.getElementById("confirmacionSection");
    const modalPagoElement = document.getElementById("modalPago");
    let modalPago = new bootstrap.Modal(modalPagoElement);


    abrirModalPagoBtn.addEventListener("click", function () {
        modalTitle.innerText = "Ingresar Datos de Tarjeta";
        tarjetaSection.style.display = "block";
        confirmacionSection.style.display = "none";
        validarTarjetaBtn.style.display = "block";
        confirmarPagoBtn.style.display = "none";
        modalPago.show();
    });

    // 🔹 Captura el ID del vuelo desde la URL
    const urlParams = new URLSearchParams(window.location.search);
    const idVuelo = parseInt(urlParams.get("idVuelo")) || 0;

    console.log("📌 ID de Vuelo:", idVuelo);

    // 🔹 Función para obtener los asientos seleccionados desde la tabla en Pago.cshtml
    function obtenerAsientosDesdeTabla() {
        let asientosSeleccionados = Array.from(document.querySelectorAll("#asientos-seleccionados tbody tr"))
            .map(row => parseInt(row.getAttribute("data-id")))
            .filter(id => !isNaN(id));

        console.log("📌 Asientos desde la Tabla:", asientosSeleccionados);
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

            let asientos = obtenerAsientosDesdeTabla(); // ✅ Obtener los asientos desde la tabla

            // ⚠️ Validación: No permitir continuar si no hay asientos
            if (asientos.length === 0) {
                console.error("❌ No se han seleccionado asientos.");
                errorMessage.innerText = "Debe seleccionar al menos un asiento para continuar.";
                errorMessage.style.display = "block";
                confirmarPagoBtn.disabled = false;
                confirmarPagoBtn.innerText = "Confirmar Pago";
                return;
            }

            // 🔹 Validaciones de la tarjeta
            let numeroTarjeta = document.getElementById("numeroTarjeta").value;
            let fechaExp = document.getElementById("fechaExp").value;
            let cvv = document.getElementById("cvv").value;
            let nombreTitular = document.getElementById("nombreTitular").value;

            if (!/^[0-9]{16}$/.test(numeroTarjeta)) {
                alert("Número de tarjeta inválido");
                confirmarPagoBtn.disabled = false;
                confirmarPagoBtn.innerText = "Confirmar Pago";
                return;
            }
            if (!/^(0[1-9]|1[0-2])\/(\d{2})$/.test(fechaExp)) {
                alert("Fecha de expiración inválida");
                confirmarPagoBtn.disabled = false;
                confirmarPagoBtn.innerText = "Confirmar Pago";
                return;
            }
            if (!/^[0-9]{3}$/.test(cvv)) {
                alert("CVV inválido");
                confirmarPagoBtn.disabled = false;
                confirmarPagoBtn.innerText = "Confirmar Pago";
                return;
            }
            if (nombreTitular.trim() === "") {
                alert("El nombre del titular no puede estar vacío");
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
                    asientos: asientos // ✅ Se envía como un array real
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
                    errorMessage.innerText = "Error en la conexión con el servidor. Por favor, intenta de nuevo.";
                    errorMessage.style.display = "block";
                    confirmarPagoBtn.disabled = false;
                    confirmarPagoBtn.innerText = "Confirmar Pago";
                });
        });
    }
























    //// 🔹 Validar datos de tarjeta
    //validarTarjetaBtn.addEventListener("click", function () {
    //    let numeroTarjeta = document.getElementById("numeroTarjeta").value;
    //    let fechaExp = document.getElementById("fechaExp").value;
    //    let cvv = document.getElementById("cvv").value;
    //    let nombreTitular = document.getElementById("nombreTitular").value;

    //    if (!/^[0-9]{16}$/.test(numeroTarjeta)) {
    //        alert("Número de tarjeta inválido");
    //        return;
    //    }
    //    if (!/^(0[1-9]|1[0-2])\/(\d{2})$/.test(fechaExp)) {
    //        alert("Fecha de expiración inválida");
    //        return;
    //    }
    //    if (!/^[0-9]{3}$/.test(cvv)) {
    //        alert("CVV inválido");
    //        return;
    //    }
    //    if (nombreTitular.trim() === "") {
    //        alert("El nombre del titular no puede estar vacío");
    //        return;
    //    }
    //});


    //// 🔹 Evento para confirmar el pago
    //if (confirmarPagoBtn) {
    //    confirmarPagoBtn.addEventListener("click", function () {
    //        confirmarPagoBtn.disabled = true;
    //        confirmarPagoBtn.innerText = "Procesando...";

    //        let asientos = obtenerAsientosDesdeTabla(); // ✅ Obtener los asientos desde la tabla

    //        // ⚠️ Validación: No permitir continuar si no hay asientos
    //        if (asientos.length === 0) {
    //            console.error("❌ No se han seleccionado asientos.");
    //            errorMessage.innerText = "Debe seleccionar al menos un asiento para continuar.";
    //            errorMessage.style.display = "block";
    //            confirmarPagoBtn.disabled = false;
    //            confirmarPagoBtn.innerText = "Confirmar Pago";
    //            return;
    //        }

    //        console.log("📌 Enviando ID Vuelo:", idVuelo);
    //        console.log("📌 Enviando Asientos (IDs):", asientos);

    //        // 🔹 Enviar datos al backend
    //        fetch("/Pagos/ConfirmarPago", {
    //            method: "POST",
    //            headers: { "Content-Type": "application/json" },
    //            body: JSON.stringify({
    //                idVuelo: idVuelo,
    //                asientos: asientos // ✅ Se envía como un array real
    //            })
    //        })
    //            .then(response => {
    //                if (!response.ok) {
    //                    throw new Error(`HTTP error! status: ${response.status}`);
    //                }
    //                return response.json();
    //            })
    //            .then(data => {
    //                if (data.success) {
    //                    alert(`✅ Reserva confirmada. Número: ${data.numeroReserva}`);
    //                    window.location.href = `/Vuelos/Detalle/${idVuelo}`;
    //                } else {
    //                    console.error("❌ Error en ConfirmarPago:", data.message);
    //                    errorMessage.innerText = data.message || "Error al confirmar la reserva.";
    //                    errorMessage.style.display = "block";
    //                    confirmarPagoBtn.disabled = false;
    //                    confirmarPagoBtn.innerText = "Confirmar Pago";
    //                }
    //            })
    //            .catch(error => {
    //                console.error("❌ Error en la conexión con el servidor:", error);
    //                errorMessage.innerText = "Error en la conexión con el servidor. Por favor, intenta de nuevo.";
    //                errorMessage.style.display = "block";
    //                confirmarPagoBtn.disabled = false;
    //                confirmarPagoBtn.innerText = "Confirmar Pago";
    //            });
    //    });
    //}



});
