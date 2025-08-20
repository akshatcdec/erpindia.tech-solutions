// siblingValidation.js - Add this as a new JavaScript file

// Function to validate that father's name and Aadhar are filled for all visible siblings
function setupSiblingRemoval() {
    // Remove existing event handlers for sibling removal buttons
    $(document).off('click', '.remove-sibling-btn');

    // Add new event handler for sibling removal
    $(document).on('click', '.remove-sibling-btn', function () {
        const siblingSection = $(this).closest('.sibling-section');
        const index = siblingSection.attr('id').replace('sibling-section-', '');

        // If this is not the only/first sibling section, remove it completely
        if (index > 0) {
            siblingSection.remove();

            // Renumber the remaining siblings
            $('.sibling-section').each(function (newIndex) {
                // Update heading
                $(this).find('h5').text('Sibling ' + (newIndex + 1));

                // Update the section ID
                $(this).attr('id', 'sibling-section-' + newIndex);

                // Update all input IDs and names
                $(this).find('input, select').each(function () {
                    const oldId = $(this).attr('id');
                    if (oldId) {
                        const newId = oldId.replace(/\d+$/, newIndex);
                        $(this).attr('id', newId);
                    }

                    const oldName = $(this).attr('name');
                    if (oldName && oldName.includes('Siblings')) {
                        const newName = oldName.replace(/\[\d+\]/, '[' + newIndex + ']');
                        $(this).attr('name', newName);
                    }
                });

                // Update search container
                const searchContainer = $(this).find('.student-search-container');
                if (searchContainer.length) {
                    searchContainer.attr('id', 'search-container-' + newIndex);

                    // Update search input
                    const searchInput = searchContainer.find('.sibling-search');
                    searchInput.attr('id', 'siblingSearchInput-' + newIndex);
                    searchInput.attr('data-index', newIndex);

                    // Update toggle button
                    const toggleBtn = searchContainer.find('.toggle-btn');
                    toggleBtn.attr('id', 'dropdownToggle-' + newIndex);
                    toggleBtn.attr('data-index', newIndex);

                    // Update results container and related elements
                    const resultsContainer = searchContainer.find('.results-container');
                    resultsContainer.attr('id', 'resultsContainer-' + newIndex);
                    resultsContainer.find('tbody').attr('id', 'resultsTableBody-' + newIndex);
                    resultsContainer.find('.message').each(function () {
                        const oldId = $(this).attr('id');
                        if (oldId) {
                            const newId = oldId.replace(/\d+$/, newIndex);
                            $(this).attr('id', newId);
                        }
                    });
                }

                // Update sibling details container
                const detailsContainer = $(this).find('.sibling-details');
                if (detailsContainer.length) {
                    detailsContainer.attr('id', 'siblingDetails-' + newIndex);
                }
            });
        } else {
            // If it's the first/only sibling, just clear the fields and hide details
            siblingSection.find('.sibling-error-message').remove();
            siblingSection.find('.is-invalid').removeClass('is-invalid');
            siblingSection.find('#siblingSearchInput-0').val('');
            siblingSection.find('#siblingDetails-0').hide();
            siblingSection.find('#siblingDetails-0 input').val('');
        }

        // Remove any validation errors
        $('.sibling-validation-error').remove();
    });
}

// Add a function to enhance the sibling section with a remove button
function enhanceSiblingSection(index) {
    const siblingSection = $(`#sibling-section-${index}`);
    const siblingHeading = siblingSection.find('h5');

    // Check if remove button already exists to avoid duplicates
    if (siblingSection.find('.remove-sibling-btn').length === 0) {
        // Create a wrap for the heading and button
        const headingWrap = $('<div class="d-flex justify-content-between align-items-center mb-3"></div>');
        siblingHeading.wrap(headingWrap);

        // Add the remove button
        siblingHeading.after(
            `<button type="button" class="btn btn-sm btn-outline-danger remove-sibling-btn">
                <i class="ti ti-trash"></i> Remove
            </button>`
        );
    }
}

// Modify the addSibling function (this would be in your sibling.js file)
// This is a patch since we can't directly modify your existing addSibling function
function patchAddSiblingFunction() {
    // Store original function if it exists
    if (typeof window.originalAddSibling === 'undefined' && typeof window.addSibling === 'function') {
        window.originalAddSibling = window.addSibling;

        // Override with our enhanced version
        window.addSibling = function () {
            // Call the original function first
            const result = window.originalAddSibling.apply(this, arguments);

            // Get the index of the newly added sibling (it should be the last one)
            const newIndex = $('.sibling-section').length - 1;

            // Enhance the newly added sibling section
            enhanceSiblingSection(newIndex);

            return result;
        };
    }
}
function validateSiblingsOnSubmit() {
    // If siblings aren't enabled, no validation needed
    if (!$('#hasSiblings').is(':checked')) {
        return true;
    }

    // Check if there are any visible sibling details
    const visibleSiblingDetails = $('.sibling-details:visible');

    // If no siblings are added but siblings are enabled, show error
    if (visibleSiblingDetails.length === 0) {
        $('#siblingsContainer').prepend(
            '<div class="alert alert-danger sibling-validation-error mb-3">' +
            'You have enabled siblings but haven\'t added any. Please add a sibling or uncheck the siblings option.' +
            '</div>'
        );

        // Scroll to error message
        $('html, body').animate({
            scrollTop: $('.sibling-validation-error').offset().top - 100
        }, 500);

        return false;
    }

    // Check if all visible siblings have valid data
    let allValid = true;

    visibleSiblingDetails.each(function () {
        const sectionIndex = $(this).attr('id').replace('siblingDetails-', '');
        const siblingId = $(`#siblingId-${sectionIndex}`).val();
        const fatherName = $(`#siblingFatherName-${sectionIndex}`).val();
        const fatherAadhar = $(`#siblingFatherAadharNo-${sectionIndex}`).val();

        // Validate that required fields are filled
        if (!siblingId || !fatherName || !fatherAadhar) {
            $(`#sibling-section-${sectionIndex}`).prepend(
                '<div class="alert alert-danger sibling-error-message mb-3">' +
                'This sibling is missing required information. Please make sure all siblings have valid data.' +
                '</div>'
            );

            // Highlight missing fields
            if (!fatherName) {
                $(`#siblingFatherName-${sectionIndex}`).addClass('is-invalid');
            }

            if (!fatherAadhar) {
                $(`#siblingFatherAadharNo-${sectionIndex}`).addClass('is-invalid');
            }

            allValid = false;
        }
    });

    if (!allValid) {
        // Scroll to the first error
        $('html, body').animate({
            scrollTop: $('.sibling-error-message:first').offset().top - 100
        }, 500);
    }

    return allValid;
}
function validateSiblingFatherInfo() {
    let isValid = true;
    let errorMessage = '';

    // Check each visible sibling section
    $('.sibling-details:visible').each(function (index) {
        const sectionIndex = $(this).attr('id').replace('siblingDetails-', '');
        const fatherName = $(`#siblingFatherName-${sectionIndex}`).val();
        const fatherAadhar = $(`#siblingFatherAadharNo-${sectionIndex}`).val();

        // Check if either field is empty
        if (!fatherName || fatherName.trim() === '') {
            $(`#siblingFatherName-${sectionIndex}`).addClass('is-invalid');
            isValid = false;
            errorMessage = 'Father\'s name is required for all siblings.';
        } else {
            $(`#siblingFatherName-${sectionIndex}`).removeClass('is-invalid');
        }

        if (!fatherAadhar || fatherAadhar.trim() === '') {
            $(`#siblingFatherAadharNo-${sectionIndex}`).addClass('is-invalid');
            isValid = false;
            errorMessage = errorMessage || 'Father\'s Aadhar number is required for all siblings.';
        } else {
            $(`#siblingFatherAadharNo-${sectionIndex}`).removeClass('is-invalid');
        }
    });

    // If validation failed, show the error message
    if (!isValid) {
        // Remove any existing error messages
        $('.sibling-validation-error').remove();

        // Add the error message to the siblings container
        $('#siblingsContainer').prepend(
            `<div class="alert alert-danger sibling-validation-error mb-3">${errorMessage}Please Edit The sibling Student </div>`
        );

        // Scroll to the error message
        $('html, body').animate({
            scrollTop: $('.sibling-validation-error').offset().top - 100
        }, 500);
    } else {
        // If validation passed, remove any existing error messages
        $('.sibling-validation-error').remove();
    }

    return isValid;
}

// Initialize sibling validation
function initSiblingValidation() {

    setupSiblingRemoval();

    // Enhance existing sibling sections with remove buttons
    $('.sibling-section').each(function (index) {
        enhanceSiblingSection(index);
    });

    // Patch the addSibling function
    patchAddSiblingFunction();

    // Modify the add sibling button click handler
    $('#addSiblingBtn').off('click').on('click', function () {
        // First validate that all current siblings have father's name and Aadhar
        if (validateSiblingFatherInfo()) {
            // Only add a new sibling if validation passes
            addSibling();
        }
    });
    // Modify the add sibling button click handler

    // Also add validation when a sibling is selected from search results
    $(document).on('click', '.results-table tr', function () {
        const siblingId = $(this).data('id');
        const index = $(this).closest('.results-container').attr('id').replace('resultsContainer-', '');

        // When a sibling is selected, fetch and validate the details
        $.ajax({
            url: '/Student/GetSiblingDetails',
            type: 'GET',
            data: { id: siblingId },
            dataType: 'json',
            success: function (response) {
                if (response && response.success) {
                    const data = response.data;

                    // First remove any existing error messages
                    $(`#sibling-section-${index} .sibling-error-message`).remove();

                    // Check if father name and Aadhar are available
                    if (!data.fatherName || data.fatherName.trim() === '' ||
                        !data.fatherAadhar || data.fatherAadhar.trim() === '') {

                        // Show specific error message
                        const errorMsg = 'This student cannot be added as a sibling because father\'s name or Aadhar number is missing.';

                        // Display error in the sibling section
                        $(`#sibling-section-${index}`).prepend(
                            `<div class="alert alert-danger sibling-error-message mb-3">${errorMsg}</div>`
                        );

                        // Hide the results container
                        $(`#resultsContainer-${index}`).removeClass('active');

                        // Don't fill in the sibling details
                        return;
                    }

                    // If father info is valid, proceed with filling in the details
                    $(`#siblingId-${index}`).val(data.id);
                    $(`#siblingName-${index}`).val(data.name);
                    $(`#siblingRollNo-${index}`).val(data.rollNo);
                    $(`#siblingAdmNo-${index}`).val(data.admissionNo);
                    $(`#siblingClass-${index}`).val(data.className);
                    $(`#siblingFatherName-${index}`).val(data.fatherName);
                    $(`#siblingFatherAadharNo-${index}`).val(data.fatherAadhar);

                    // Show the sibling details
                    $(`#siblingDetails-${index}`).show();

                    // Hide the results container
                    $(`#resultsContainer-${index}`).removeClass('active');
                }
            },
            error: function () {
                // Handle error
               // alert('Error retrieving sibling details');
            }
        });
    });

    // Add event listeners to remove validation errors when user corrects the issues
    $(document).on('input', '.sibling-fathername, .sibling-fatheraadhar', function () {
        // Remove the invalid class if there's content
        if ($(this).val().trim() !== '') {
            $(this).removeClass('is-invalid');

            // Check if all invalid fields are now valid
            if ($('.sibling-details:visible .is-invalid').length === 0) {
                // Remove the error message
                $('.sibling-validation-error').remove();
            }
        }
    });
}

// Initialize when document is ready
$(document).ready(function () {
    initSiblingValidation();
});