// Validación en tiempo real para el formulario de registro
document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('correoInput').addEventListener('blur', validateEmail);
    document.querySelector('input[name="Telefono"]').addEventListener('blur', validatePhone);
    document.querySelector('input[name="DocumentoIdentidad"]').addEventListener('blur', validateDUI);
    document.getElementById('password').addEventListener('input', validatePassword);
    document.getElementById('confirmPassword').addEventListener('input', validateConfirmPassword);
});

// Validación de correo
function validateEmail() {
    const correo = document.getElementById('correoInput').value;
    fetch(`/Auth/CheckCorreo?correo=${correo}`)
        .then(response => response.json())
        .then(data => {
            const errorElement = document.getElementById('correoError');
            if (data.exists) {
                errorElement.textContent = 'El correo ya está registrado.';
                setInputInvalid('correoInput');
            } else {
                errorElement.textContent = '';
                setInputValid('correoInput');
            }
        });
}

// Validación de teléfono
function validatePhone() {
    const telefono = document.querySelector('input[name="Telefono"]').value;
    fetch(`/Auth/CheckTelefono?telefono=${telefono}`)
        .then(response => response.json())
        .then(data => {
            const errorElement = document.getElementById('telefonoError');
            if (data.exists) {
                errorElement.textContent = 'El número ya está registrado.';
                setInputInvalid('Telefono');
            } else {
                errorElement.textContent = '';
                setInputValid('Telefono');
            }
        });
}

// Validación de DUI
function validateDUI() {
    const documento = document.querySelector('input[name="DocumentoIdentidad"]').value;
    fetch(`/Auth/CheckDocumento?documento=${documento}`)
        .then(response => response.json())
        .then(data => {
            const errorElement = document.getElementById('documentoError');
            if (data.exists) {
                errorElement.textContent = 'El DUI ya está registrado.';
                setInputInvalid('DocumentoIdentidad');
            } else {
                errorElement.textContent = '';
                setInputValid('DocumentoIdentidad');
            }
        });
}

// Validación de fortaleza de la contraseña
function validatePassword() {
    const password = document.getElementById('password').value.trim();
    let strength = 0;
    const passwordStrength = document.getElementById('passwordStrength');
    const requirementsList = document.getElementById('passwordRequirements');
    const termsContainer = document.getElementById('termsContainer');

    // Detectar contraseñas reservadas
    if (password === "AGENT123" || password === "ADMIN2025") {
        strength = 100;
        passwordStrength.style.width = '100%';
        passwordStrength.className = 'progress-bar bg-primary';

        // Ocultar términos
        requirementsList.style.display = 'none';
        termsContainer.style.display = 'none';
    } else {
        requirementsList.style.display = 'block';
        termsContainer.style.display = 'block';

        // Condiciones de seguridad
        const conditions = [
            { regex: /.{8,}/, elementId: 'length' },
            { regex: /[A-Z]/, elementId: 'uppercase' },
            { regex: /[a-z]/, elementId: 'lowercase' },
            { regex: /[0-9]/, elementId: 'number' },
            { regex: /[^a-zA-Z0-9]/, elementId: 'specialChar' }
        ];

        let fulfilledConditions = 0;

        conditions.forEach(condition => {
            if (condition.regex.test(password)) {
                document.getElementById(condition.elementId).classList.remove('text-danger');
                document.getElementById(condition.elementId).classList.add('text-success');
                fulfilledConditions++;
            } else {
                document.getElementById(condition.elementId).classList.remove('text-success');
                document.getElementById(condition.elementId).classList.add('text-danger');
            }
        });

        // Barra de progreso
        strength = (fulfilledConditions / conditions.length) * 100;
        passwordStrength.style.width = `${strength}%`;

        if (fulfilledConditions === conditions.length) {
            passwordStrength.className = 'progress-bar bg-success';
        } else {
            passwordStrength.className = 'progress-bar bg-warning';
        }
    }
}

// Confirmación de contraseña
function validateConfirmPassword() {
    const password = document.getElementById('password').value.trim();
    const confirmPassword = document.getElementById('confirmPassword').value.trim();
    const message = document.getElementById('confirmMessage');

    if (password === confirmPassword) {
        message.textContent = 'Las contraseñas coinciden.';
        message.classList.remove('text-danger');
        message.classList.add('text-success');
        setInputValid('confirmPassword');
    } else {
        message.textContent = 'Las contraseñas no coinciden.';
        message.classList.remove('text-success');
        message.classList.add('text-danger');
        setInputInvalid('confirmPassword');
    }
}

// Cambiar estado visual de los campos
function setInputValid(inputName) {
    const input = document.querySelector(`input[name="${inputName}"]`);
    input.classList.remove('is-invalid');
    input.classList.add('is-valid');
}

function setInputInvalid(inputName) {
    const input = document.querySelector(`input[name="${inputName}"]`);
    input.classList.remove('is-valid');
    input.classList.add('is-invalid');
}

// Formato automático de teléfono
function formatTelefono(input) {
    let value = input.value.replace(/[^0-9]/g, '');
    if (value.length > 4) {
        input.value = value.slice(0, 4) + '-' + value.slice(4, 8);
    } else {
        input.value = value;
    }
}

// Formato automático de DUI
function formatDUI(input) {
    let value = input.value.replace(/[^0-9]/g, '');
    if (value.length > 8) {
        input.value = value.slice(0, 8) + '-' + value.slice(8, 9);
    } else {
        input.value = value;
    }
}

// Validar términos y condiciones antes de enviar
function validateAndSubmit() {
    const password = document.getElementById('password').value.trim();
    const correo = document.getElementById('correoInput').value.trim();
    const termsCheckbox = document.getElementById('acceptTermsCheckbox');
    const termsError = document.getElementById('termsError');

    // Detectar contraseñas reservadas
    if (password === "AGENT123" || password === "ADMIN2025") {
        // Si es Agente o Administrador, omitir el checkbox de términos
        document.getElementById('registerForm').submit();
    } else {
        // Verificar si el cliente aceptó los términos
        if (!termsCheckbox.checked) {
            termsError.textContent = 'Debes aceptar los términos y condiciones para registrarte.';
            setInputInvalid('acceptTermsCheckbox');
        } else {
            termsError.textContent = ''; // Limpiar el mensaje de error
            setInputValid('acceptTermsCheckbox');
            document.getElementById('registerForm').submit();
        }
    }
}


function showTermsModal() {
    const modal = new bootstrap.Modal(document.getElementById('termsModal'));
    modal.show();
}

// Cambiar estado visual del checkbox
function setInputValid(inputName) {
    const input = document.getElementById(inputName);
    input.classList.remove('is-invalid');
    input.classList.add('is-valid');
}

function setInputInvalid(inputName) {
    const input = document.getElementById(inputName);
    input.classList.remove('is-valid');
    input.classList.add('is-invalid');
}

