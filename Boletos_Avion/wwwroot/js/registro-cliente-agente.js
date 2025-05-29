document.addEventListener('DOMContentLoaded', function () {
    // Configurar eventos de validación
    setupValidationEvents();

    // Configurar formato automático de campos
    setupAutoFormatting();

    // Configurar envío del formulario
    setupFormSubmission();
});

function setupValidationEvents() {
    // Validación en tiempo real para correo, teléfono y DUI
    document.getElementById('correoInput')?.addEventListener('blur', validateEmail);
    document.querySelector('input[name="Telefono"]')?.addEventListener('blur', validatePhone);
    document.querySelector('input[name="DocumentoIdentidad"]')?.addEventListener('blur', validateDUI);
}

function setupAutoFormatting() {
    // Formato automático de teléfono
    const telefonoInput = document.querySelector('input[name="Telefono"]');
    if (telefonoInput) {
        telefonoInput.addEventListener('input', function () {
            this.value = formatTelefonoNumber(this.value);
        });
    }

    // Formato automático de DUI
    const duiInput = document.querySelector('input[name="DocumentoIdentidad"]');
    if (duiInput) {
        duiInput.addEventListener('input', function () {
            this.value = formatDuiNumber(this.value);
        });
    }
}

function setupFormSubmission() {
    const form = document.getElementById('registerForm');
    if (form) {
        form.addEventListener('submit', function (e) {
            if (!validateForm()) {
                e.preventDefault();
                showError('Por favor, corrige los errores en el formulario');
            }
        });
    }
}

// Funciones de validación
function validateEmail() {
    const emailInput = document.getElementById('correoInput');
    const errorElement = document.getElementById('correoError');

    if (!emailInput || !errorElement) return;

    const email = emailInput.value.trim();

    // Validación básica de formato
    if (!isValidEmail(email)) {
        errorElement.textContent = 'Formato de correo inválido';
        setInputInvalid(emailInput);
        return false;
    }

    // Validación de duplicados (asincrónica)
    checkEmailExists(email).then(exists => {
        if (exists) {
            errorElement.textContent = 'El correo ya está registrado';
            setInputInvalid(emailInput);
        } else {
            errorElement.textContent = '';
            setInputValid(emailInput);
        }
    }).catch(error => {
        console.error('Error validando email:', error);
        errorElement.textContent = 'Error al validar el correo';
        setInputInvalid(emailInput);
    });

    return true;
}

function validatePhone() {
    const phoneInput = document.querySelector('input[name="Telefono"]');
    const errorElement = document.getElementById('telefonoError');

    if (!phoneInput || !errorElement) return;

    const phone = phoneInput.value.trim();

    // Validación básica de formato
    if (!isValidPhone(phone)) {
        errorElement.textContent = 'Formato: 1234-5678';
        setInputInvalid(phoneInput);
        return false;
    }

    // Validación de duplicados (asincrónica)
    checkPhoneExists(phone).then(exists => {
        if (exists) {
            errorElement.textContent = 'El teléfono ya está registrado';
            setInputInvalid(phoneInput);
        } else {
            errorElement.textContent = '';
            setInputValid(phoneInput);
        }
    }).catch(error => {
        console.error('Error validando teléfono:', error);
        errorElement.textContent = 'Error al validar el teléfono';
        setInputInvalid(phoneInput);
    });

    return true;
}

function validateDUI() {
    const duiInput = document.querySelector('input[name="DocumentoIdentidad"]');
    const errorElement = document.getElementById('documentoError');

    if (!duiInput || !errorElement) return;

    const dui = duiInput.value.trim();

    // Validación básica de formato
    if (!isValidDui(dui)) {
        errorElement.textContent = 'Formato: 12345678-9';
        setInputInvalid(duiInput);
        return false;
    }

    // Validación de duplicados (asincrónica)
    checkDuiExists(dui).then(exists => {
        if (exists) {
            errorElement.textContent = 'El documento ya está registrado';
            setInputInvalid(duiInput);
        } else {
            errorElement.textContent = '';
            setInputValid(duiInput);
        }
    }).catch(error => {
        console.error('Error validando DUI:', error);
        errorElement.textContent = 'Error al validar el documento';
        setInputInvalid(duiInput);
    });

    return true;
}

// Validación general del formulario
function validateForm() {
    let isValid = true;

    // Validar campos requeridos
    const requiredFields = [
        { name: 'Nombre', input: document.querySelector('input[name="Nombre"]') },
        { name: 'Apellido', input: document.querySelector('input[name="Apellido"]') },
        { name: 'Correo', input: document.getElementById('correoInput') },
        { name: 'Telefono', input: document.querySelector('input[name="Telefono"]') },
        { name: 'Direccion', input: document.querySelector('input[name="Direccion"]') },
        { name: 'DocumentoIdentidad', input: document.querySelector('input[name="DocumentoIdentidad"]') }
    ];

    requiredFields.forEach(field => {
        if (!field.input || !field.input.value.trim()) {
            setInputInvalid(field.input);
            isValid = false;
        }
    });

    // Ejecutar validaciones específicas
    isValid = validateEmail() && isValid;
    isValid = validatePhone() && isValid;
    isValid = validateDUI() && isValid;

    return isValid;
}

// Funciones de ayuda
function isValidEmail(email) {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}

function isValidPhone(phone) {
    return /^[0-9]{4}-[0-9]{4}$/.test(phone);
}

function isValidDui(dui) {
    return /^[0-9]{8}-[0-9]{1}$/.test(dui);
}

function formatTelefonoNumber(value) {
    const numbers = value.replace(/[^0-9]/g, '');
    if (numbers.length <= 4) return numbers;
    return `${numbers.slice(0, 4)}-${numbers.slice(4, 8)}`;
}

function formatDuiNumber(value) {
    const numbers = value.replace(/[^0-9]/g, '');
    if (numbers.length <= 8) return numbers;
    return `${numbers.slice(0, 8)}-${numbers.slice(8, 9)}`;
}

function setInputValid(input) {
    if (!input) return;
    input.classList.remove('is-invalid');
    input.classList.add('is-valid');
}

function setInputInvalid(input) {
    if (!input) return;
    input.classList.remove('is-valid');
    input.classList.add('is-invalid');
}

function showError(message) {
    const errorElement = document.getElementById('termsError');
    if (errorElement) {
        errorElement.textContent = message;
    }
}

// Funciones de API
async function checkEmailExists(email) {
    const response = await fetch(`/Agent/CheckEmail?email=${encodeURIComponent(email)}`);
    const data = await response.json();
    return data.exists;
}

async function checkPhoneExists(phone) {
    const response = await fetch(`/Agent/CheckPhone?phone=${encodeURIComponent(phone)}`);
    const data = await response.json();
    return data.exists;
}

async function checkDuiExists(dui) {
    const response = await fetch(`/Agent/CheckDocument?document=${encodeURIComponent(dui)}`);
    const data = await response.json();
    return data.exists;
}