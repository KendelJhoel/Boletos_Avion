// change-email-monitor.js

// ✅ Script idéntico al que usás para usuarios, pero funciona para monitores
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
        newEmailInput.classList.remove('is-valid', 'is-invalid');
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

function toggleSubmitButton() {
    const emailValid = document.getElementById('newEmail').classList.contains('is-valid');
    const codeValid = document.getElementById('verificationCode').classList.contains('is-valid');

    document.getElementById('saveChangesBtn').disabled = !(emailValid && codeValid);
}

function showPasswordModal(event) {
    event.preventDefault();
    $('#passwordConfirmModal').modal('show');
}

function validatePassword() {
    const password = document.getElementById('modalPasswordInput').value.trim();
    const errorElement = document.getElementById('modalPasswordError');

    if (password.length === 0) {
        errorElement.textContent = '❌ Ingresa tu contraseña.';
        return;
    }

    fetch(`/Monitor/CheckPassword?password=${encodeURIComponent(password)}`)
        .then(response => response.json())
        .then(data => {
            if (data.valid) {
                $('#passwordConfirmModal').modal('hide');
                setTimeout(() => { submitEmailChange(password); }, 300);
            } else {
                errorElement.textContent = '❌ Contraseña incorrecta.';
            }
        })
        .catch(error => {
            console.error("⚠️ Error en la validación:", error);
            alert("❌ Hubo un problema al verificar la contraseña.");
        });
}

function submitEmailChange(password) {
    const newEmail = document.getElementById('newEmail').value.trim();

    fetch('/Monitor/ChangeEmail', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            Nombre: newEmail,
            Contrasena: password
        })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert("✅ ¡Correo cambiado exitosamente!");
                window.location.href = "/Monitor/ProfileMo";
            } else {
                alert("❌ " + data.message);
            }
        })
        .catch(error => {
            console.error("⚠️ Error al cambiar el correo:", error);
            alert("❌ Ocurrió un error. Intenta de nuevo.");
        });
}  

document.addEventListener('DOMContentLoaded', () => {
    const editEmailBtn = document.getElementById('editEmailBtn');

    if (editEmailBtn) {
        editEmailBtn.addEventListener('click', () => {
            // 🔥 Enviar el correo con código automáticamente al hacer clic
            fetch('/Monitor/SendEmailVerification', {
                method: 'POST'
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        console.log("📩 Código de verificación enviado al correo.");
                    } else {
                        alert("❌ No se pudo enviar el correo de verificación: " + data.message);
                    }
                })
                .catch(error => {
                    console.error("⚠️ Error al enviar el correo:", error);
                    alert("❌ Error al enviar el correo de verificación.");
                });
        });
    }
});

