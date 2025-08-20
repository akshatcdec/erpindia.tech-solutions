// Scripts/forgotPassword.js

$(document).ready(function () {
    // Hide preloader when page loads
    setTimeout(function () {
        $('#preloader').fadeOut('slow');
    }, 1000);

    // Initialize form validation
    initializeValidation();
});

let currentFoundEmail = '';

// Option selection
function selectOption(option) {
    // Remove active class from all cards
    $('.option-card').removeClass('active');

    // Hide option selection
    $('#optionSelection').hide();

    // Show selected form
    if (option === 'email') {
        $('#emailSection').addClass('active');
        $(event.target).closest('.option-card').addClass('active');
    } else if (option === 'schoolCode') {
        $('#schoolCodeSection').addClass('active');
        $(event.target).closest('.option-card').addClass('active');
    }
}

// Go back to options
function goBackToOptions() {
    $('.form-section').removeClass('active');
    $('#optionSelection').show();

    // Reset forms
    $('#emailForm')[0].reset();
    $('#schoolCodeForm')[0].reset();
    $('#emailResult').hide();
    currentFoundEmail = '';

    // Add these two lines:
    $('.loading-spinner').hide();           // Hide all spinners
    $('button[type="submit"]').prop('disabled', false);  // Enable all submit buttons
}

// Email form submission
$('#emailForm').on('submit', function (e) {
    e.preventDefault();

    const email = $('#email').val().trim();
    const $submitBtn = $(this).find('button[type="submit"]');
    const $spinner = $('#emailSpinner');

    if (!email) {
        swal('Error!', 'Please enter your email address.', 'error');
        return;
    }

    // Validate email format
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
        swal('Error!', 'Please enter a valid email address.', 'error');
        return;
    }

    // Show loading
    $spinner.show();
    $submitBtn.prop('disabled', true);

    // AJAX call to server
    $.ajax({
        url: '/Account/ForgotPassword',
        type: 'POST',
        data: { Email: email },
        dataType: 'json',
        success: function (response) {
            $spinner.hide();
            $submitBtn.prop('disabled', false);

            if (response.Success) {
                swal({
                    title: 'Success!',
                    text: response.Message,
                    type: 'success',
                    confirmButtonText: 'OK'
                }, function () {
                    // Optionally redirect to login page
                    // window.location.href = '/Account/Login';
                });
            } else {
                swal('Error!', response.Message, 'error');
            }
        },
        error: function (xhr, status, error) {
            $spinner.hide();
            $submitBtn.prop('disabled', false);

            swal('Error!', 'An error occurred. Please try again later.', 'error');
            console.error('AJAX Error:', status, error);
        }
    });
});

// client code form submission
$('#schoolCodeForm').on('submit', function (e) {
    e.preventDefault();

    const schoolCode = $('#schoolCodeInput').val().trim();
    const $submitBtn = $(this).find('button[type="submit"]');
    const $spinner = $('#schoolSpinner');

    if (!schoolCode) {
        swal('Error!', 'Please enter your client code.', 'error');
        return;
    }

    // Show loading
    $spinner.show();
    $submitBtn.prop('disabled', true);

    // AJAX call to server
    $.ajax({
        url: '/Account/FindEmailBySchoolCode',
        type: 'POST',
        data: { SchoolCode: schoolCode },
        dataType: 'json',
        success: function (response) {
            $spinner.hide();
            $submitBtn.prop('disabled', false);

            if (response.Success) {
                currentFoundEmail = response.Email;

                $('#foundEmail').text(response.Email);
                $('#emailResult').show();
                $('#schoolCodeForm').hide();

                swal({
                    title: 'Email Found!',
                    text: response.Message,
                    type: 'success',
                    confirmButtonText: 'Great!'
                });
            } else {
                swal('Error!', response.Message, 'error');
            }
        },
        error: function (xhr, status, error) {
            $spinner.hide();
            $submitBtn.prop('disabled', false);

            swal('Error!', 'An error occurred. Please try again later.', 'error');
            console.error('AJAX Error:', status, error);
        }
    });
});

// Use found email to reset password
function useFoundEmail() {
    $('#email').val(currentFoundEmail);
    $('#schoolCodeSection').removeClass('active');
    $('#emailSection').addClass('active');

    // Focus on the email input
    setTimeout(function () {
        $('#email').focus();
    }, 300);
}

// Navigation functions
function goBack() {
    swal({
        title: 'Are you sure?',
        text: 'Do you want to go back to the login page?',
        type: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, go back',
        cancelButtonText: 'Stay here'
    }, function (isConfirm) {
        if (isConfirm) {
            window.location.href = '/Account/Login';
        }
    });
}

// Initialize form validation
function initializeValidation() {
    // Email validation
    $('#email').on('blur', function () {
        const email = $(this).val().trim();
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        if (email && !emailRegex.test(email)) {
            $(this).addClass('is-invalid');
            if (!$(this).next('.invalid-feedback').length) {
                $(this).after('<div class="invalid-feedback">Please enter a valid email address.</div>');
            }
        } else {
            $(this).removeClass('is-invalid');
            $(this).next('.invalid-feedback').remove();
        }
    });

    // client code validation
    $('#schoolCodeInput').on('input', function () {
        // Convert to uppercase as user types
        $(this).val($(this).val().toUpperCase());
    });

    // Add focus effects to form inputs
    $('.form-control').on('focus', function () {
        $(this).parent().css({
            'transform': 'scale(1.02)',
            'transition': 'transform 0.2s ease'
        });
    }).on('blur', function () {
        $(this).parent().css('transform', 'scale(1)');
    });
}

// CSRF Token handler for MVC
$.ajaxSetup({
    beforeSend: function (xhr) {
        var token = $('input[name="__RequestVerificationToken"]').val();
        if (token) {
            xhr.setRequestHeader('RequestVerificationToken', token);
        }
    }
});