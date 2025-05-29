document.addEventListener("DOMContentLoaded", () => {
    const asientosBody = document.getElementById("asientos-body");
    const pagarBtn = document.getElementById("pagarBtn");
    const listaAsientosSeleccionados = document.getElementById("lista-asientos-seleccionados");
    const idClienteSelect = document.getElementById("idCliente");
    const esAgente = sessionStorage.getItem("esAgente") === "true";
    let seleccionados = [];
    let idVuelo = new URLSearchParams(window.location.search).get("idVuelo");

    // ✅ Obtener el precio del vuelo desde el HTML
    const precioVuelo = parseFloat(document.getElementById("precio-vuelo").textContent) || 0;

    function actualizarTotal() {
        const precioVuelo = parseFloat(document.getElementById("precio-vuelo").textContent) || 0;
        let totalAdicional = 0;

        seleccionados.forEach(id => {
            let asientoElement = document.querySelector(`[data-id="${id}"]`);
            if (asientoElement) {
                totalAdicional += parseFloat(asientoElement.dataset.precio) || 0;
            }
        });

        const totalVuelo = precioVuelo * seleccionados.length;
        const totalFinal = totalVuelo + totalAdicional;

        document.getElementById("total").textContent = totalFinal.toFixed(2);
        document.getElementById("multiplicador").textContent = seleccionados.length > 0 ? ` x${seleccionados.length}` : "";
    }

    function actualizarListaAsientos() {
        listaAsientosSeleccionados.innerHTML = "";
        seleccionados.forEach(id => {
            let asientoElement = document.querySelector(`[data-id="${id}"]`);
            if (asientoElement) {
                let numero = asientoElement.textContent;
                let precio = asientoElement.dataset.precio;
                let idCategoria = asientoElement.dataset.categoria;
                let nombreCategoria = asientoElement.dataset.nombrecategoria;

                let li = document.createElement("li");
                li.innerHTML = `Asiento ${numero}: $${parseFloat(precio).toFixed(2)} - <span class="category-label-${idCategoria}">${nombreCategoria}</span>`;
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

                // ✅ Agregar clase visual según categoría
                let categoriaClase = asiento.idCategoria ? `category-${asiento.idCategoria}` : "category-default";
                seatCell.classList.add(categoriaClase);

                // ✅ Añadir todos los datos necesarios como atributos personalizados
                seatCell.dataset.id = asiento.idVueloAsiento;
                seatCell.dataset.precio = asiento.precio;
                seatCell.dataset.categoria = asiento.idCategoria;
                seatCell.dataset.nombrecategoria = asiento.nombreCategoria;
                seatCell.textContent = asiento.numero;

                // ✅ Imagen de fondo del asiento
                seatCell.style.backgroundImage = "url('/images/asiento.png')";

                // 🔴 Marcar ocupados
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
                    verificarBotonPagar(); // 🔥 Agrega esta línea
                    console.log("Asientos seleccionados:", seleccionados);

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

        const esAgente = sessionStorage.getItem("esAgente") === "true";
        const esRegreso = sessionStorage.getItem("esRegreso") === "true";
        let idCliente = null;

        // Validación para agentes
        if (esAgente) {
            idCliente = esRegreso ?
                sessionStorage.getItem("idClienteRegreso") :
                (idClienteSelect ? idClienteSelect.value : null);

            if (!idCliente) {
                alert("Debes seleccionar un cliente para continuar.");
                return;
            }
        }
        const asientosParam = seleccionados.join(",");
        let url = `/Pagos/Pago?idVuelo=${idVuelo}&asientos=${encodeURIComponent(asientosParam)}&esRegreso=${esRegreso}`;

        // Agregar idCliente si es necesario
        if (idCliente) {
            url += `&idCliente=${idCliente}`;
        }

        console.log("URL de redirección:", url);

        fetch('/Auth/CheckSession')
            .then(response => response.json())
            .then(data => {
                if (!data.authenticated) {
                    window.location.href = `/Auth/Authentication?redirectUrl=${encodeURIComponent(url)}`;
                    return;
                }

                // Lógica para AGENTES
                if (esAgente) {
                    const idCliente = document.getElementById("idCliente")?.value ||
                        sessionStorage.getItem("idClienteRegreso");

                    if (!idCliente) {
                        alert("Debes seleccionar un cliente para continuar.");
                        return;
                    }

                    url += `&idCliente=${idCliente}`;
                    window.location.href = url;
                }
                // Lógica para CLIENTES NORMALES
                else {
                    fetch('/Auth/GetLoggedInUserId')
                        .then(response => response.json())
                        .then(userData => {
                            if (userData.id) {
                                url += `&idCliente=${userData.id}`;
                                window.location.href = url;
                            } else {
                                throw new Error("No se pudo obtener ID de usuario");
                            }
                        })
                        .catch(error => {
                            console.error("Error:", error);
                            alert("Error al obtener tus datos. Intenta nuevamente.");
                        });
                }
            })
            .catch(error => {
                console.error("Error al verificar sesión:", error);
                alert("Error al verificar sesión");
            });
});

});