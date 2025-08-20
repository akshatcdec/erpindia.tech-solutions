document.addEventListener('DOMContentLoaded', function () {
    // Initialize Flatpickr

    // Initialize toggles for transport and hostel sections
    function toggleTransport() {
        var transportSwitch = document.getElementById('transportSwitch');
        document.getElementById('transportFields').style.display = transportSwitch.checked ? 'block' : 'none';
    }

    function toggleHostel() {
        var hostelSwitch = document.getElementById('hostelSwitch');
        document.getElementById('hostelFields').style.display = hostelSwitch.checked ? 'block' : 'none';
    }

    $('#transportSwitch').change(function () {
        toggleTransport();
    });

    $('#hostelSwitch').change(function () {
        toggleHostel();
    });

    // Initialize toggle states on page load
    toggleTransport();
    toggleHostel();

    // Handle guardian type selection
    $('#parents, #guardian, #other').change(function () {
        if ($('#parents').is(':checked')) {
            // Get selected parent radio value
            var selectedParent = $('input[name="parentType"]:checked').val() || 'father';

            if (selectedParent === 'father') {
                $('#Family_GName').val($('#Family_FName').val());
                $('#Family_GPhone').val($('#Family_FPhone').val());
                $('#Family_GEmail').val($('#Family_FEmail').val());
                $('#Family_GRelation').val('Father');
                $('#Family_GOccupation').val($('#Family_FOccupation').val());
            } else {
                $('#Family_GName').val($('#Family_MName').val());
                $('#Family_GPhone').val($('#Family_MPhone').val());
                $('#Family_GEmail').val($('#Family_MEmail').val());
                $('#Family_GRelation').val('Mother');
                $('#Family_GOccupation').val($('#Family_MOccupation').val());
            }
            $('#Family_GAddress').val($('#Family_StCurrentAddress').val());
        }
    });

    // Image preview handling
    $('input[type="file"].image-sign').change(function () {
        var input = this;
        if (input.files && input.files[0]) {
            var reader = new FileReader();
            reader.onload = function (e) {
                var frame = $(input).closest('.d-flex').find('.frames');
                frame.empty(); // Clear the icon

                var imgPreview = $('<img>').attr({
                    'src': e.target.result,
                    'class': 'img-thumbnail',
                    'style': 'max-height: 100%; max-width: 100%; object-fit: cover;'
                });

                frame.append(imgPreview);
            }
            reader.readAsDataURL(input.files[0]);
        }
    });

   // Live validation for key fields
    $('#Basic_AdmsnNo, #Basic_Class, #Basic_FirstName').on('input change', function () {
        var field = $(this);
        var fieldName = field.prev('label').text().replace('*', '').trim();

        if (!field.val()) {
            field.addClass('is-invalid');
            field.next('.text-danger').text(fieldName + ' is required');
        } else if (field.attr('id') === 'Basic_FirstName' && (field.val().length < 2 || field.val().length > 50)) {
            field.addClass('is-invalid');
            field.next('.text-danger').text('Student Name must be between 1 and 50 characters');
        } else {
            field.removeClass('is-invalid');
            field.next('.text-danger').text('');
        }
    });

});