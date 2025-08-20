document.addEventListener('DOMContentLoaded', function () {
    // Map of all toggle sections with their controls
    const toggleSections = [
        // Transport section
        {
            switchId: 'transportSwitch',
            contentId: 'transportFields',
            defaultState: false
        },
        // Hostel section
        {
            switchId: 'hostelSwitch',
            contentId: 'hostelFields',
            defaultState: false
        },
        // Siblings section
        {
            switchId: 'hasSiblings',
            contentId: 'siblingsContainer',
            defaultState: false
        },
        // Subject selection section
        {
            switchId: 'subjectSelectionSwitch',
            contentId: 'subjectSelectionFields',
            defaultState: false
        },
        // Documents section
        {
            switchId: 'documentsSwitch',
            contentId: 'documentsFields',
            defaultState: false
        },
        // Medical section
        {
            switchId: 'medicalSwitch',
            contentId: 'medicalFields',
            defaultState: false
        },
        // Previous school section
        {
            switchId: 'previousSchoolSwitch',
            contentId: 'previousSchoolFields',
            defaultState: false
        },
        // Educational details section
        {
            switchId: 'educationalDetailsSwitch',
            contentId: 'educationalDetailsFields',
            defaultState: false
        },
        // Banking details section
        {
            switchId: 'bankingDetailsSwitch',
            contentId: 'bankingDetailsFields',
            defaultState: false
        },
        // Miscellaneous details section
        {
            switchId: 'miscDetailsSwitch',
            contentId: 'miscDetailsFields',
            defaultState: false
        }
    ];

    // Find all toggle switches by class for any we might have missed
    document.querySelectorAll('.form-check-input[role="switch"]').forEach(switchElement => {
        const switchId = switchElement.id;
        // Only process if not already in our list and has an ID
        if (switchId && !toggleSections.some(section => section.switchId === switchId)) {
            // Try to find a related content section
            // This assumes a naming convention where content IDs end with "Fields"
            const possibleContentId = switchId.replace('Switch', 'Fields');
            const contentElement = document.getElementById(possibleContentId);

            if (contentElement) {
                toggleSections.push({
                    switchId: switchId,
                    contentId: possibleContentId,
                    defaultState: false
                });
            }
        }
    });

    // Process all identified toggle sections
    toggleSections.forEach(section => {
        const switchElement = document.getElementById(section.switchId);
        const contentElement = document.getElementById(section.contentId);

        if (switchElement && contentElement) {
            // Set initial state
            switchElement.checked = section.defaultState;
            contentElement.style.display = section.defaultState ? 'block' : 'none';

            // Add event listener
            switchElement.addEventListener('change', function () {
                contentElement.style.display = this.checked ? 'block' : 'none';

                // If this is the first time showing this section, trigger any initialization needed
                if (this.checked && !this.dataset.initialized) {
                    this.dataset.initialized = 'true';

                    // Special initialization for specific sections
                    if (section.switchId === 'subjectSelectionSwitch') {
                        // Initialize any special subject selection functionality
                        console.log('Subject selection section initialized');
                    }
                }

                // Trigger validation recheck if this is a form field
                if (typeof ($.validator) !== 'undefined' && $('#studentForm').length) {
                    $('#studentForm').validate().element(this);
                }
            });

            // Create global toggle functions for sections that need them
            if (section.switchId === 'transportSwitch') {
                window.toggleTransport = function () {
                    contentElement.style.display = switchElement.checked ? 'block' : 'none';
                };
            } else if (section.switchId === 'hostelSwitch') {
                window.toggleHostel = function () {
                    contentElement.style.display = switchElement.checked ? 'block' : 'none';
                };
            }
        }
    });

    // Handle radio button toggles (like guardian options)
    const radioToggleSets = [
        {
            name: 'guardianType',
            options: [
                { value: 'Parents', showElementIds: [], hideElementIds: ['guardianDetailsFields'] },
                { value: 'Guardian', showElementIds: ['guardianDetailsFields'], hideElementIds: [] },
                { value: 'Others', showElementIds: ['guardianDetailsFields'], hideElementIds: [] }
            ]
        },
        {
            name: 'Other.MedicalCondition',
            options: [
                { value: 'Good', showElementIds: [], hideElementIds: ['medicalDetailsFields'] },
                { value: 'Bad', showElementIds: ['medicalDetailsFields'], hideElementIds: [] },
                { value: 'Others', showElementIds: ['medicalDetailsFields'], hideElementIds: [] }
            ]
        }
        // Add other radio toggle sets here
    ];

    // Process radio button toggles
    radioToggleSets.forEach(set => {
        const radioButtons = document.querySelectorAll(`input[type="radio"][name="${set.name}"]`);

        radioButtons.forEach(radio => {
            // Set initial state based on checked status
            if (radio.checked) {
                const option = set.options.find(opt => opt.value === radio.value);
                if (option) {
                    option.showElementIds.forEach(id => {
                        const element = document.getElementById(id);
                        if (element) element.style.display = 'block';
                    });

                    option.hideElementIds.forEach(id => {
                        const element = document.getElementById(id);
                        if (element) element.style.display = 'none';
                    });
                }
            }

            // Add change event listeners
            radio.addEventListener('change', function () {
                if (this.checked) {
                    const option = set.options.find(opt => opt.value === this.value);
                    if (option) {
                        option.showElementIds.forEach(id => {
                            const element = document.getElementById(id);
                            if (element) element.style.display = 'block';
                        });

                        option.hideElementIds.forEach(id => {
                            const element = document.getElementById(id);
                            if (element) element.style.display = 'none';
                        });
                    }
                }
            });
        });
    });

    // Additional initializations for special cases

    // "Add Sibling" button functionality - ensure it's properly initialized
    const addSiblingBtn = document.getElementById('addSiblingBtn');
    if (addSiblingBtn) {
        // Make sure this button is visible even if the siblings section is hidden by default
        const siblingParent = addSiblingBtn.closest('.row');
        if (siblingParent) {
            siblingParent.style.display = 'flex';
        }
    }

    // Initialize datepickers if present (ensure they work in hidden sections)
    if (typeof (flatpickr) !== 'undefined') {
        const datepickers = document.querySelectorAll('.datepicker');
        if (datepickers.length > 0) {
            datepickers.forEach(picker => {
                flatpickr(picker, {
                    allowInput: true,
                    dateFormat: "Y-m-d"
                });
            });
        }
    }

    // Initialize tag inputs if present
    if (typeof ($.fn.tagsinput) !== 'undefined') {
        $('.tag-input').tagsinput({
            trimValue: true,
            confirmKeys: [13, 44, 32], // Enter, comma, space
            tagClass: 'badge bg-primary'
        });
    }

    // Handle form submission - ensure all fields are included even in hidden sections
    $('#studentForm').submit(function (e) {
        var isValid = true;

        // Show all hidden sections temporarily to validate all fields
        var hiddenSections = $('.card-body[style*="display: none"]');
        hiddenSections.each(function () {
            $(this).data('originalDisplay', $(this).css('display'));
            $(this).css('display', 'block');
        });

        // Validate Admission Number
        var admissionNo = $('#Basic_AdmsnNo').val();
        if (!admissionNo) {
            $('#Basic_AdmsnNo').addClass('is-invalid');
            $('#Basic_AdmsnNo').next('.text-danger').text('Admission Number is required');
            isValid = false;
        } else {
            $('#Basic_AdmsnNo').removeClass('is-invalid');
            $('#Basic_AdmsnNo').next('.text-danger').text('');
        }

        // Validate SR Number
        var srNo = $('#Basic_SrNo').val();
        if (!srNo) {
            $('#Basic_SrNo').addClass('is-invalid');
            $('#Basic_SrNo').next('.text-danger').text('SR Number is required');
            isValid = false;
        } else {
            $('#Basic_SrNo').removeClass('is-invalid');
            $('#Basic_SrNo').next('.text-danger').text('');
        }

        // Validate Class
        var studentClass = $('#Basic_Class').val();
        if (!studentClass) {
            $('#Basic_Class').addClass('is-invalid');
            $('#Basic_Class').next('.text-danger').text('Class is required');
            isValid = false;
        } else {
            $('#Basic_Class').removeClass('is-invalid');
            $('#Basic_Class').next('.text-danger').text('');
        }

        // Validate First Name
        var firstName = $('#Basic_FirstName').val();
        if (!firstName) {
            $('#Basic_FirstName').addClass('is-invalid');
            $('#Basic_FirstName').next('.text-danger').text('First Name is required');
            isValid = false;
        } else if (firstName.length < 2 || firstName.length > 50) {
            $('#Basic_FirstName').addClass('is-invalid');
            $('#Basic_FirstName').next('.text-danger').text('First Name must be between 2 and 50 characters');
            isValid = false;
        } else {
            $('#Basic_FirstName').removeClass('is-invalid');
            $('#Basic_FirstName').next('.text-danger').text('');
        }

        // Validate other required fields marked with '*'
        $('.required').each(function () {
            var inputField = $(this).parent().find('input, select').first();
            if (inputField.length > 0 && !inputField.val()) {
                inputField.addClass('is-invalid');
                var fieldName = $(this).text().replace('*', '').trim();
                inputField.next('.text-danger').text(fieldName + ' is required');
                isValid = false;
            }
        });

        // If validation fails, prevent form submission and scroll to first error
        if (!isValid) {
            e.preventDefault();

            // Re-hide sections that were initially hidden
            hiddenSections.each(function () {
                $(this).css('display', $(this).data('originalDisplay'));
            });

            // Scroll to the first error
            $('html, body').animate({
                scrollTop: $('.is-invalid:first').offset().top - 100
            }, 500);

            return false;
        }

        // If validation passes, ensure all form fields are visible for submission
        console.log('Form validation passed, submitting form');

        // Leave all sections visible for the actual form submission
        return true;
    });
});