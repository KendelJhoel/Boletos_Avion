document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('newEmail').addEventListener('input', updateNewEmailDisplay);
    document.getElementById('newEmail').addEventListener('blur', validateNewEmail);
    document.getElementById('verificationCode').addEventListener('input', validateVerificationCode);
    
    document.getElementById('saveChangesBtn').addEventListener('click', showPasswordModal);
    document.getElementById('confirmPasswordBtn').addEventListener('click', validatePassword);
});

function updateNewEmailDisplay() {
    const newEmail = document.getElementById('newEmail').value.trim();
    document.getElementById('newEmailDisplay').textContent = newEmail || "______";
}

function validateNewEmail() {
    const newEmailInput = document.getElementById('newEmail');
    const newEmail = newEmailInput.value.trim();
    const currentEmail = document.getElementById('currentEmail').innerText.trim();
    const errorElement = document.getElementById('emailError');

    if (newEmail.length === 0) {
        errorElement.textContent = '';
        newEmailInput.classList.remove('is-valid', 'is-invalid'); // No se evalúa si está vacío
        toggleSubmitButton();
        return;
    }

    if (newEmail === currentEmail) {
        errorElement.textContent = '❌ No puedes ingresar el mismo correo.';
        setInputInvalid('newEmail');
        toggleSubmitButton();
        return;
    }

    if (!validateEmailFormat(newEmail)) {
        errorElement.textContent = '❌ Formato de correo no válido.';
        setInputInvalid('newEmail');
        toggleSubmitButton();
        return;
    }

    // Verifica si el correo ya existe en la BD
    fetch(`/Auth/CheckCorreo?correo=${encodeURIComponent(newEmail)}`)
        .then(response => response.json())
        .then(data => {
            if (data.exists) {
                errorElement.textContent = '❌ El correo ya está registrado.';
                setInputInvalid('newEmail');
            } else {
                errorElement.textContent = '';
                setInputValid('newEmail');
            }
            toggleSubmitButton();
        });
}

function toggleSubmitButton() {
    const emailValid = document.getElementById('newEmail').classList.contains('is-valid');
    const codeValid = document.getElementById('verificationCode').classList.contains('is-valid');

    document.getElementById('saveChangesBtn').disabled = !(emailValid && codeValid);
}

function validateEmailFormat(email) {
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailPattern.test(email);
}

function setInputValid(inputId) {
    const input = document.getElementById(inputId);
    input.classList.remove('is-invalid');
    input.classList.add('is-valid');
}

function setInputInvalid(inputId) {
    const input = document.getElementById(inputId);
    input.classList.remove('is-valid');
    input.classList.add('is-invalid');
}

$(document).ready(function () {
    let shouldWarnOnExit = true; // Variable para controlar si se muestra la alerta

    // Capturar intento de cierre de pestaña o recarga con alerta de navegador
    $(window).on('beforeunload', function (event) {
        if (shouldWarnOnExit) {
            $.post('/Account/ClearVerificationCode'); // 🔥 Eliminar el código antes de salir
            return "Si sales, el código de verificación se eliminará y deberás solicitar uno nuevo.";
        }
    });

    // Manejar el clic en el botón "Cancelar" para también mostrar la alerta del navegador
    $('#cancelChangeEmailBtn').click(function (event) {
        event.preventDefault();
        let confirmExit = confirm("Si sales, el código de verificación se eliminará y deberás solicitar uno nuevo. ¿Deseas continuar?");
        if (confirmExit) {
            shouldWarnOnExit = false; // Evita que el mensaje aparezca dos veces
            $.post('/Account/ClearVerificationCode', function () {
                window.location.href = '/Account/Profile'; // 🔥 Redirigir al perfil
            });
        }
    });
});

function validateVerificationCode() {
    const codeInput = document.getElementById('verificationCode');
    const code = codeInput.value.trim();

    if (code.length === 0) {
        codeInput.classList.remove('is-valid', 'is-invalid'); // No evaluar si está vacío
        toggleSubmitButton();
        return;
    }

    if (code.length !== 6 || isNaN(code)) {
        setInputInvalid('verificationCode');
        toggleSubmitButton();
        return;
    }

    // 🔍 Enviar el código al servidor para validarlo
    fetch(`/Account/ValidateVerificationCode?code=${encodeURIComponent(code)}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                setInputValid('verificationCode');
            } else {
                setInputInvalid('verificationCode');
            }
            toggleSubmitButton();
        });

}


// 🔹 Abre el modal al presionar "Confirmar Cambio"
function showPasswordModal(event) {
    event.preventDefault();
    $('#passwordConfirmModal').modal('show');
}

// 🔹 Validar la contraseña antes de enviar el cambio de correo
function validatePassword() {
    const password = document.getElementById('modalPasswordInput').value.trim();
    const errorElement = document.getElementById('modalPasswordError');

    if (password.length === 0) {
        errorElement.textContent = '❌ Ingresa tu contraseña.';
        return;
    }

    // 🔥 SOLO verifica la contraseña, nada más
    fetch(`/Auth/CheckPassword?password=${encodeURIComponent(password)}`)
        .then(response => response.json())
        .then(data => {
            console.log("📢 Respuesta del servidor (Validación de contraseña):", data);
            if (data.valid) {
                $('#passwordConfirmModal').modal('hide'); // Cerrar modal
                setTimeout(() => { submitEmailChange(password); }, 300); // Esperar cierre del modal
            } else {
                errorElement.textContent = '❌ Contraseña incorrecta.';
            }
        })
        .catch(error => {
            console.error("⚠️ Error en la validación:", error);
            alert("❌ Hubo un problema al verificar la contraseña.");
        });
}

// 🔹 Enviar el nuevo correo al servidor
function submitEmailChange(password) {
    const newEmail = document.getElementById('newEmail').value.trim();
    const currentEmail = document.getElementById('currentEmail').innerText.trim(); // 🔥 Obtener el correo actual

    if (!newEmail || !password) {
        return;
    }

    shouldWarnOnExit = false; // 🔥 Desactiva alerta antes de enviar solicitud
    $(window).off('beforeunload');

    fetch('/Account/ChangeEmail', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            Nombre: newEmail,
            Contrasena: password, // 🔥 SOLO la contraseña actual
            EmailActual: currentEmail // 🔥 Se envía el correo actual para que el backend lo use en la validación
        })
    })
        .then(response => response.json())
        .then(data => {
            console.log("📢 Respuesta del servidor (Cambio de correo):", data);
            if (data.success) {
                alert("✅ ¡Correo cambiado exitosamente!");
                window.location.href = "/Account/Profile";
            } else {
                alert("❌ " + data.message);
            }
        })
        .catch(error => {
            console.error("⚠️ Error al cambiar el correo:", error);
            alert("❌ Ocurrió un error. Intenta de nuevo.");
        });
}

document.getElementById('confirmChangeEmailBtn').addEventListener('click', function () {
    const newEmail = document.getElementById('newEmail').value.trim();
    const verificationCode = document.getElementById('verificationCode').value.trim();

    fetch('/Account/ChangeEmail', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ Nombre: newEmail }) // Aquí se envía el nuevo correo
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                window.location.href = data.redirectUrl;
            } else {
                alert("❌ " + data.message);
            }
        })
        .catch(() => alert("❌ Ocurrió un error. Intenta de nuevo."));
});
