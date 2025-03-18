document.addEventListener("DOMContentLoaded", () => {
    const asientosBody = document.getElementById("asientos-body");
    const pagarBtn = document.getElementById("pagarBtn");
    const listaAsientosSeleccionados = document.getElementById("lista-asientos-seleccionados");
    let seleccionados = [];
    let idVuelo = new URLSearchParams(window.location.search).get("idVuelo");

    // ✅ Obtener el precio del vuelo desde el HTML
    const precioVuelo = parseFloat(document.getElementById("precio-vuelo").textContent) || 0;

    function actualizarTotal() {
        let totalAsientos = seleccionados.reduce((acc, id) => {
            let asientoElement = document.querySelector(`[data-id="${id}"]`);
            return acc + (asientoElement ? parseFloat(asientoElement.dataset.precio) : 0);
        }, 0);

        let totalFinal = precioVuelo + totalAsientos; // ✅ Sumar el precio del vuelo al total
        document.getElementById("total").textContent = totalFinal.toFixed(2);
        verificarBotonPagar();
    }

    function actualizarListaAsientos() {
        listaAsientosSeleccionados.innerHTML = "";
        seleccionados.forEach(id => {
            let asientoElement = document.querySelector(`[data-id="${id}"]`);
            if (asientoElement) {
                let li = document.createElement("li");
                li.textContent = `Asiento ${asientoElement.textContent}: $${asientoElement.dataset.precio}`;
                listaAsientosSeleccionados.appendChild(li);
            }
        });
    }

    function verificarBotonPagar() {
        if (seleccionados.length > 0) {
            pagarBtn.removeAttribute("disabled");
        } else {
            pagarBtn.setAttribute("disabled", "true");
        }
    }

    fetch(`/Asientos/Obtener?idVuelo=${idVuelo}`)
        .then(response => response.json())
        .then(asientos => {
            console.log("📌 Datos recibidos:", asientos);

            if (!asientos || !Array.isArray(asientos)) {
                console.error("❌ Error: No se recibieron asientos válidos.");
                return;
            }

            asientosBody.innerHTML = "";

            let row;
            let asientosPorFila = 6;
            const categoriasMap = new Map();

            asientos.forEach((asiento, index) => {
                let columna = index % asientosPorFila;

                if (!categoriasMap.has(asiento.idCategoria)) {
                    categoriasMap.set(asiento.idCategoria, {
                        nombre: asiento.nombreCategoria,
                        precio: asiento.precio
                    });
                }

                if (columna === 0) {
                    row = document.createElement("tr");
                    asientosBody.appendChild(row);
                }

                if (columna === 3) {
                    let separatorCell = document.createElement("td");
                    separatorCell.classList.add("separator");
                    row.appendChild(separatorCell);
                }

                let seatCell = document.createElement("td");
                seatCell.classList.add("seat");

                // ✅ Asegurar que siempre tenga una categoría
                let categoriaClase = asiento.idCategoria ? `category-${asiento.idCategoria}` : "category-default";
                seatCell.classList.add(categoriaClase);

                seatCell.dataset.id = asiento.idVueloAsiento;
                seatCell.dataset.precio = asiento.precio;
                seatCell.textContent = asiento.numero;
                seatCell.style.backgroundImage = "url('/images/asiento.png')";

                if (asiento.estado === "Reservado") {
                    seatCell.classList.add("occupied");
                }

                seatCell.addEventListener("click", () => {
                    if (seatCell.classList.contains("occupied")) return;

                    seatCell.classList.toggle("selected");

                    let precio = parseFloat(seatCell.dataset.precio);
                    if (seatCell.classList.contains("selected")) {
                        seleccionados.push(asiento.idVueloAsiento);
                    } else {
                        seleccionados = seleccionados.filter(id => id !== asiento.idVueloAsiento);
                    }

                    actualizarTotal();
                    actualizarListaAsientos();
                });

                row.appendChild(seatCell);
            });

            categoriasMap.forEach((categoria, idCategoria) => {
                let listItem = document.createElement("li");
                listItem.classList.add(`category-label-${idCategoria}`);
                listItem.textContent = `Categoría: ${categoria.nombre} - Precio: $${categoria.precio}`;
                document.getElementById("categorias-list").appendChild(listItem);
            });

            if (categoriasMap.size > 0) {
                document.getElementById("categorias-container").style.display = "block";
            }

            actualizarTotal();
        })
        .catch(error => console.error("❌ Error al cargar los asientos:", error));

    // ✅ Evento del botón "Pagar"
    pagarBtn.addEventListener("click", () => {
        if (seleccionados.length === 0) {
            alert("No has seleccionado asientos.");
            return;
        }

        fetch('/Auth/CheckSession')
            .then(response => response.json())
            .then(data => {
                if (!data.authenticated) {
                    window.location.href = `/Auth/Authentication?redirectUrl=${encodeURIComponent(window.location.href)}`;
                } else {
                    let asientosIds = seleccionados.join(",");
                    window.location.href = `/Pagos/Pago?idVuelo=${idVuelo}&asientos=${asientosIds}`;
                }
            })
            .catch(error => {
                console.error("❌ Error al verificar sesión:", error);
                alert("Error al verificar sesión. Intenta de nuevo.");
            });
    });
});
