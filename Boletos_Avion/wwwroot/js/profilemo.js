$(document).ready(function () {
    var originalData = {};
    var isEditing = false;
    var hasChanges = false;
    var originalPassword = $('#passwordField').val();

    function captureOriginalData() {
        $('#editProfileForm').find('input[type="text"], input[type="password"]').each(function () {
            originalData[$(this).attr('name')] = $(this).val();
        });
    }
    captureOriginalData();

    function checkForChanges() {
        hasChanges = false;

        let duiValido = $('input[name="DocumentoIdentidad"]').val().length === 10;
        let passwordChanged = $('#passwordField').val().trim() !== originalPassword;
        let passwordValido = !passwordChanged || $('#passwordStrength').hasClass('bg-success');

        $('#editProfileForm').find('input[type="text"], input[type="password"]').each(function () {
            if ($(this).val() !== originalData[$(this).attr('name')]) {
                hasChanges = true;
            }
        });

        let validForm = hasChanges && duiValido && passwordValido;
        $('#saveChangesBtn').prop('disabled', !validForm);
    }

    $('#editProfileForm').find('input').on('input', function () {
        checkForChanges();
    });

    $('#toggleEditBtn').click(function (e) {
        e.preventDefault();
        if (!isEditing) {
            isEditing = true;
            $('#editProfileForm').find('input').removeAttr('readonly');
            $('#editIcon').removeClass('fa-pencil').addClass('fa-lock');
            $('#saveChangesBtn').show();
            $('#togglePasswordVisibility').prop('disabled', false);
        } else {
            if (hasChanges) {
                $('#discardChangesModal').modal('show');
            } else {
                toggleToReadOnly();
            }
        }
    });

    $('#modalConfirmBtn').click(function () {
        for (var key in originalData) {
            $('[name="' + key + '"]').val(originalData[key]);
        }
        checkForChanges();
        $('#discardChangesModal').modal('hide');
        toggleToReadOnly();
    });

    $('#modalCancelBtn').click(function () {
        $('#discardChangesModal').modal('hide');
    });

    function toggleToReadOnly() {
        isEditing = false;
        $('#editProfileForm').find('input').prop('readonly', true);
        $('#editIcon').removeClass('fa-lock').addClass('fa-pencil');
        $('#saveChangesBtn').prop('disabled', true).hide();
        $('#togglePasswordVisibility').prop('disabled', true);
        $('#passwordField').attr('type', 'password');
        $('#eyeIcon').removeClass('fa-eye-slash').addClass('fa-eye');
    }

    $('#togglePasswordVisibility').click(function () {
        let passwordField = $('#passwordField');
        let icon = $('#eyeIcon');

        if (passwordField.attr('type') === 'password') {
            passwordField.attr('type', 'text');
            icon.removeClass('fa-eye').addClass('fa-eye-slash');
        } else {
            passwordField.attr('type', 'password');
            icon.removeClass('fa-eye-slash').addClass('fa-eye');
        }
    });

    $('#saveChangesBtn').click(function (e) {
        e.preventDefault();
        if (hasChanges) {
            $('#confirmPasswordModal').modal('show');
        }
    });

    $('#confirmSaveChanges').click(function (event) {
        event.preventDefault();
        var password = $('#confirmPasswordInput').val().trim();
        var errorElement = $('#confirmPasswordError');

        if (password.length === 0) {
            errorElement.text('❌ Ingresa tu contraseña.');
            return;
        }

        fetch(`/Monitor/CheckPassword?password=${encodeURIComponent(password)}`)

            .then(response => response.json())
            .then(data => {
                if (data.valid) {
                    $('#confirmPasswordModal').modal('hide');
                    setTimeout(() => { $('#editProfileForm').submit(); }, 300);
                } else {
                    errorElement.text('❌ Contraseña incorrecta.');
                }
            })
            .catch(error => {
                console.error("⚠️ Error en la validación:", error);
                alert("❌ Hubo un problema al verificar la contraseña.");
            });
    });

    $('input[name="DocumentoIdentidad"]').on('input', function () {
        let value = $(this).val().replace(/[^0-9]/g, '');
        if (value.length > 9) {
            value = value.slice(0, 9);
        }
        if (value.length > 8) {
            $(this).val(value.slice(0, 8) + '-' + value.slice(8, 9));
        } else {
            $(this).val(value);
        }
        $('#duiError').text('').hide();
        checkForChanges();
    }).on('blur', function () {
        if ($(this).val().length !== 10) {
            $('#duiError').text('El DUI debe tener el formato #########-#').show();
        }
        checkForChanges();
    });

    $('#passwordField').on('input', function () {
        let password = $(this).val().trim();
        let strength = 0;
        const passwordStrength = $('#passwordStrength');
        const requirementsList = $('#passwordRequirements');

        if (password !== originalPassword) {
            passwordStrength.show();
            requirementsList.show();
        } else {
            passwordStrength.hide();
            requirementsList.hide();
        }

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
                $('#' + condition.elementId).removeClass('text-danger').addClass('text-success');
                fulfilledConditions++;
            } else {
                $('#' + condition.elementId).removeClass('text-success').addClass('text-danger');
            }
        });

        strength = (fulfilledConditions / conditions.length) * 100;
        passwordStrength.css('width', `${strength}%`);

        if (fulfilledConditions === conditions.length) {
            passwordStrength.removeClass('bg-danger bg-warning').addClass('bg-success');
        } else {
            passwordStrength.removeClass('bg-success').addClass('bg-warning');
        }

        checkForChanges();
    });

    $('#modalConfirmBtn').click(function () {
        $('#passwordField').val(originalPassword);
        $('#passwordStrength, #passwordRequirements').hide();
    });

    $('#saveChangesBtn').hide();
    $('#togglePasswordVisibility').prop(true);

    $("#editProfileForm").on("submit", function () {
        isEditing = false;
    });

    window.addEventListener('beforeunload', function (e) {
        if (isEditing && hasChanges) {
            e.preventDefault();
            e.returnValue = 'Tienes cambios sin guardar. ¿Estás seguro de que quieres salir?';
        }
    });
});

// Accionar cambio de correo
$('#confirmEmailChangeBtn').click(function () {
    $.post('/Account/SendEmailVerification', function (response) {
        if (response.success) {
            window.location.href = '/Account/ChangeEmail';
        } else {
            alert("Hubo un error al generar el código. Inténtalo de nuevo.");
        }
    });
});
