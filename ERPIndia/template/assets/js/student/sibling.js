// Combined sibling.js - Merges functionality from both files and fixes issues

// Global variables
let siblingCount = 1;
const MAX_SIBLINGS = 3;

function validateFatherAadharMatchesSibling() {
    // Only validate if siblings checkbox is checked
    if (!$('#hasSiblings').is(':checked')) {
        return true;
    }

    // Get father's Aadhar from the main form
    const fatherAadhar = $('#Family_FAadhar').val();
    if (!fatherAadhar) {
        // If father's Aadhar is empty, show error
        $('#Family_FAadhar').addClass('is-invalid');

        // Add error message if it doesn't exist
        if ($('#father-aadhar-required-error').length === 0) {
            $('#Family_FAadhar').after(
                '<div id="father-aadhar-required-error" class="text-danger validation-error">' +
                'Father\'s Aadhar is required when adding siblings.' +
                '</div>'
            );
        }

        // Scroll to the error
        $('html, body').animate({
            scrollTop: $('#Family_FAadhar').offset().top - 100
        }, 500);

        return false;
    }

    // Find all visible sibling sections with details showing
    let allValid = true;
    $('.sibling-details:visible').each(function () {
        const sectionIndex = $(this).attr('id').replace('siblingDetails-', '');
        const siblingFatherAadhar = $(`#siblingFatherAadharNo-${sectionIndex}`).val();

        if (!siblingFatherAadhar) {
            // If sibling's father Aadhar is empty, this is handled in validateSiblingFatherInfo
            return;
        }

        // Compare father's Aadhar with sibling's father Aadhar
        if (fatherAadhar !== siblingFatherAadhar) {
            allValid = false;

            // Mark fields as invalid
            $('#Family_FAadhar').addClass('is-invalid');
            $(`#siblingFatherAadharNo-${sectionIndex}`).addClass('is-invalid');

            // Add error message to sibling section if it doesn't exist
            if ($(`#sibling-section-${sectionIndex} .aadhar-mismatch-error`).length === 0) {
                $(`#sibling-section-${sectionIndex}`).prepend(
                    '<div class="alert alert-danger aadhar-mismatch-error mb-3">' +
                    'Sibling\'s father Aadhar does not match the student\'s father Aadhar.' +
                    '</div>'
                );
            }
        } else {
            // Clear any mismatch errors if Aadhars match
            $(`#sibling-section-${sectionIndex} .aadhar-mismatch-error`).remove();
            $(`#siblingFatherAadharNo-${sectionIndex}`).removeClass('is-invalid');
        }
    });

    // If there's a mismatch, make sure the main father Aadhar is also marked as invalid
    if (!allValid) {
        // Add error message to the main father Aadhar field if it doesn't exist
        if ($('#father-aadhar-mismatch-error').length === 0) {
            $('#Family_FAadhar').after(
                '<div id="father-aadhar-mismatch-error" class="text-danger validation-error">' +
                'Father\'s Aadhar must match with all siblings\' father Aadhar.' +
                '</div>'
            );
        }

        // Scroll to the first error
        $('html, body').animate({
            scrollTop: $('.aadhar-mismatch-error:first').length ?
                $('.aadhar-mismatch-error:first').offset().top - 100 :
                $('#Family_FAadhar').offset().top - 100
        }, 500);
    } else {
        // Clear any mismatch errors if all Aadhars match
        $('#father-aadhar-mismatch-error').remove();
        $('#Family_FAadhar').removeClass('is-invalid');
    }

    return allValid;
}

// Function to update sibling's father Aadhar when main father Aadhar changes
function validateFatherAadharOnChange() {
    // Only validate if siblings checkbox is checked
    if (!$('#hasSiblings').is(':checked')) {
        return;
    }

    // Run validation when Aadhar changes
    setTimeout(validateFatherAadharMatchesSibling, 300);
}

// Function to initialize siblingCount based on existing siblings
function initializeExistingSiblings() {
    // Count how many sibling sections exist
    siblingCount = $('.sibling-section').length;
    console.log("Initializing with " + siblingCount + " existing siblings");

    // Update addSiblingBtn state if needed
    if (siblingCount >= MAX_SIBLINGS) {
        $('#addSiblingBtn').prop('disabled', true);
        console.log("Maximum siblings reached, disabling add button");
    }

    // Check hasSiblings checkbox state
    if ($('#hasSiblings').is(':checked')) {
        $('#siblingsContainer').show();
    } else {
        $('#siblingsContainer').hide();
    }

    // Initialize each sibling section
    $('.sibling-section').each(function (index) {
        initializeSiblingSearch(index);
        enhanceSiblingSection(index);

        // Check if this sibling has data
        const siblingId = $(`#siblingId-${index}`).val();
        if (siblingId && siblingId !== "00000000-0000-0000-0000-000000000000") {
            console.log(`Sibling ${index} already has data:`, siblingId);
            $(`#siblingDetails-${index}`).show();
        }
    });
}
// Main initialization function
document.addEventListener('DOMContentLoaded', function () {
    console.log("Initializing sibling functionality");

    // Initialize sibling container visibility
    $("#hasSiblings").change(function () {
        if ($(this).is(":checked")) {
            $("#siblingsContainer").show();
        } else {
            $("#siblingsContainer").hide();
        }
    });

    // Initially hide siblings container if checkbox is unchecked
    if (!$("#hasSiblings").is(":checked")) {
        $("#siblingsContainer").hide();
    }

    // Set initial sibling count based on existing sibling sections
    siblingCount = $('.sibling-section').length || 1;
    console.log("Initial sibling count:", siblingCount);
    initializeExistingSiblings();
    // Add sibling button click handler
    $("#addSiblingBtn").off('click').on('click', function () {
        addSibling();
    });

    // Initialize existing sibling searches
    $('.sibling-section').each(function (index) {
        initializeSiblingSearch(index);
        enhanceSiblingSection(index);
    });

    // Setup sibling removal functionality
    setupSiblingRemoval();
});

// Function to add a new sibling section
function addSibling() {
    console.log("Add sibling requested, current count:", siblingCount);

    // First validate existing siblings
    if (validateSiblingFatherInfo()) {
        if (siblingCount < MAX_SIBLINGS) {
            const newIndex = siblingCount;

            const newSiblingSection = `
                <div class="sibling-section mb-4" id="sibling-section-${newIndex}">
                    <div class="d-flex justify-content-between align-items-center mb-3">
                        <h5 class="mb-0">Sibling ${newIndex + 1}</h5>
                        <button type="button" class="btn btn-sm btn-outline-danger remove-sibling-btn">
                            <i class="ti ti-trash"></i> Remove
                        </button>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <div class="mb-3">
                                <label class="form-label">Search Sibling</label>
                                <div class="student-search-container" id="search-container-${newIndex}">
                                    <div class="search-box">
                                        <input type="text" class="search-input sibling-search" autocomplete="off" id="siblingSearchInput-${newIndex}"
                                               data-index="${newIndex}" placeholder="Search by name, class, father, roll no...">
                                        <button type="button" class="toggle-btn" id="dropdownToggle-${newIndex}" data-index="${newIndex}">▼</button>
                                    </div>
                                    <div class="results-container" id="resultsContainer-${newIndex}">
                                        <table class="results-table">
                                            <thead>
                                                <tr>
                                                    <th>Name</th>
                                                    <th>Class</th>
                                                    <th>Father Name</th>
                                                    <th>Father Aadhar</th>
                                                    <th>Roll No</th>
                                                    <th>SR</th>
                                                </tr>
                                            </thead>
                                            <tbody id="resultsTableBody-${newIndex}">
                                                <!-- Results will be populated here -->
                                            </tbody>
                                        </table>
                                        <div id="noResultsMessage-${newIndex}" class="message hidden">No students found</div>
                                        <div id="loadingMessage-${newIndex}" class="message hidden">Loading...</div>
                                        <div id="errorMessage-${newIndex}" class="message hidden">Error loading students</div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="row sibling-details" id="siblingDetails-${newIndex}" style="display: none;">
                        <div class="col-lg-4 col-md-6">
                            <div class="mb-3">
                                <label class="form-label">Sibling Name</label>
                                <input type="text" class="form-control sibling-name" id="siblingName-${newIndex}" readonly />
                            </div>
                        </div>
                        <div class="col-lg-4 col-md-6">
                            <div class="mb-3">
                                <label class="form-label">Roll No</label>
                                <input type="text" class="form-control sibling-rollno" id="siblingRollNo-${newIndex}" readonly />
                            </div>
                        </div>
                        <div class="col-lg-4 col-md-6">
                            <div class="mb-3">
                                <label class="form-label">Admission No</label>
                                <input type="text" class="form-control sibling-admno" id="siblingAdmNo-${newIndex}" readonly />
                            </div>
                        </div>
                        <div class="col-lg-4 col-md-6">
                            <div class="mb-3">
                                <label class="form-label">Class</label>
                                <input type="text" class="form-control sibling-class" id="siblingClass-${newIndex}" readonly />
                            </div>
                        </div>
                        <div class="col-lg-4 col-md-6">
                            <div class="mb-3">
                                <label class="form-label">Father Name</label>
                                <input type="text" class="form-control sibling-fathername" id="siblingFatherName-${newIndex}" readonly />
                            </div>
                        </div>
                        <div class="col-lg-4 col-md-6">
                            <div class="mb-3">
                                <label class="form-label">Father AadharNo</label>
                                <input type="text" class="form-control sibling-fatheraadhar" id="siblingFatherAadharNo-${newIndex}" readonly />
                            </div>
                        </div>
                        <div class="col-md-12 sibling-hidden-fields">
                            <!-- Hidden fields for form submission -->
                            <input type="hidden" name="Family.Siblings[${newIndex}].SiblingId" id="siblingId-${newIndex}" class="sibling-id-hidden" />
                            <input type="hidden" name="Family.Siblings[${newIndex}].Name" id="siblingName-hidden-${newIndex}" />
                            <input type="hidden" name="Family.Siblings[${newIndex}].RollNo" id="siblingRollNo-hidden-${newIndex}" />
                            <input type="hidden" name="Family.Siblings[${newIndex}].AdmissionNo" id="siblingAdmissionNo-hidden-${newIndex}" />
                            <input type="hidden" name="Family.Siblings[${newIndex}].Class" id="siblingClass-hidden-${newIndex}" />
                            <input type="hidden" name="Family.Siblings[${newIndex}].FatherName" id="siblingFatherName-hidden-${newIndex}" />
                            <input type="hidden" name="Family.Siblings[${newIndex}].FatherAadharNo" id="siblingFatherAadharNo-hidden-${newIndex}" />
                        </div>
                    </div>
                </div>
            `;

            // Insert new sibling section before the Add button row
            $('#addSiblingBtn').closest('.row').before(newSiblingSection);

            // Initialize the search functionality for the new sibling
            initializeSiblingSearch(newIndex);

            // Increment the sibling count
            siblingCount++;
            console.log("Added new sibling section, new count:", siblingCount);

            // Disable the Add button if we've reached the maximum
            if (siblingCount >= MAX_SIBLINGS) {
                $('#addSiblingBtn').prop('disabled', true);
                console.log("Max siblings reached, disabling add button");
            }
        } else {
            console.log("Maximum siblings reached, cannot add more");
            // Show a message that max siblings reached
            if ($('.max-siblings-message').length === 0) {
                $('#addSiblingBtn').before(
                    '<div class="alert alert-warning max-siblings-message mb-3">' +
                    'Maximum of ' + MAX_SIBLINGS + ' siblings allowed.' +
                    '</div>'
                );

                // Auto-remove message after 3 seconds
                setTimeout(function () {
                    $('.max-siblings-message').fadeOut(function () {
                        $(this).remove();
                    });
                }, 3000);
            }
        }
    }
}

// Function to initialize sibling search functionality
function initializeSiblingSearch(index) {
    console.log(`Initializing sibling search for index ${index}`);

    const searchInput = document.getElementById(`siblingSearchInput-${index}`);
    const dropdownToggle = document.getElementById(`dropdownToggle-${index}`);
    const resultsContainer = document.getElementById(`resultsContainer-${index}`);
    const resultsTableBody = document.getElementById(`resultsTableBody-${index}`);
    const noResultsMessage = document.getElementById(`noResultsMessage-${index}`);
    const loadingMessage = document.getElementById(`loadingMessage-${index}`);
    const errorMessage = document.getElementById(`errorMessage-${index}`);

    if (!searchInput || !dropdownToggle || !resultsContainer || !resultsTableBody) {
        console.error(`Missing elements for sibling search ${index}`);
        return;
    }

    let searchTimeout = null;
    const SEARCH_DELAY = 300; // milliseconds
    let currentSelectedRow = -1; // Track currently selected row
    let searchResults = []; // Store search results for selection

    // Search function for sibling dropdown
    function searchSiblings(searchTerm) {
        // Show loading state
        hideAllMessages();
        loadingMessage.classList.remove('hidden');
        resultsTableBody.innerHTML = '';
        currentSelectedRow = -1; // Reset selected row

        // Clear any existing timeout
        if (searchTimeout) {
            clearTimeout(searchTimeout);
        }

        // Set timeout for API call
        searchTimeout = setTimeout(() => {
            if (searchTerm.length < 1) {
                loadingMessage.classList.add('hidden');
                return;
            }

            console.log(`Searching for sibling with term: ${searchTerm}`);

            // Make the API call
            fetch(`${window.location.origin}/Student/GetAllStudentsBySchholCode?searchTerm=${encodeURIComponent(searchTerm)}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`API error: ${response.status}`);
                    }
                    return response.json();
                })
                .then(data => {
                    console.log(`Search results received for index ${index}:`, data);
                    displayResults(data);
                })
                .catch(error => {
                    console.error('Error fetching siblings:', error);
                    hideAllMessages();
                    errorMessage.classList.remove('hidden');
                });
        }, SEARCH_DELAY);
    }

    // Display results for sibling dropdown
    function displayResults(data) {
        // Clear previous results
        resultsTableBody.innerHTML = '';
        currentSelectedRow = -1;
        searchResults = [];

        // Hide all messages initially
        hideAllMessages();

        // Check if we have valid data
        if (!data) {
            noResultsMessage.classList.remove('hidden');
            return;
        }

        // Extract the students array from the response
        let studentsArray = [];

        if (data.student && Array.isArray(data.student)) {
            studentsArray = data.student;
        } else if (Array.isArray(data)) {
            studentsArray = data;
        } else if (data.success && data.data && Array.isArray(data.data)) {
            // Handle case where data might be wrapped in a result object
            studentsArray = data.data;
        } else {
            console.error('Unexpected data format:', data);
            noResultsMessage.classList.remove('hidden');
            return;
        }

        // Check if we have any students
        if (studentsArray.length === 0) {
            noResultsMessage.classList.remove('hidden');
            return;
        }

        // Add each student to the table (limit to 10 for performance)
        const limitedStudents = studentsArray.slice(0, 10);
        searchResults = limitedStudents;

        console.log(`Displaying ${limitedStudents.length} sibling search results`);

        limitedStudents.forEach((student, rowIndex) => {
            const row = document.createElement('tr');
            row.setAttribute('data-index', rowIndex);
            row.setAttribute('data-id', student.StudentId || student.Id);

            row.innerHTML = `
                <td>${student.FirstName || ''}</td>
                <td>${student.Class || ''}</td>
                <td>${student.FatherName || ''}</td>
                <td>${student.FatherAadhar || ''}</td>
                <td>${student.RollNo || ''}</td>
                <td>${student.AdmsnNo || ''}</td>
            `;

            row.addEventListener('click', () => {
                selectSibling(student);
            });

            // Also highlight the row when mouse hovers over it
            row.addEventListener('mouseenter', () => {
                currentSelectedRow = rowIndex;
                updateSelection();
            });

            resultsTableBody.appendChild(row);
        });
    }

    // Sibling selection handler
    function selectSibling(student) {
        console.log(`Selecting sibling for index ${index}:`, student);

        // Set the search input value to selected sibling's name
        searchInput.value = student.FirstName || '';

        // Hide dropdown
        resultsContainer.classList.remove('active');
        dropdownToggle.textContent = '▼';

        // Get the ID for the sibling
        const siblingId = '';

        // Update the visible display fields
        document.getElementById(`siblingName-${index}`).value = student.FirstName || '';
        document.getElementById(`siblingRollNo-${index}`).value = student.RollNo || '';
        document.getElementById(`siblingAdmNo-${index}`).value = student.AdmsnNo || '';
        document.getElementById(`siblingClass-${index}`).value = student.ClassName || student.Class || '';
        document.getElementById(`siblingFatherName-${index}`).value = student.FatherName || '';
        document.getElementById(`siblingFatherAadharNo-${index}`).value = student.FatherAadhar || '';

        // Update the main hidden ID field
        document.getElementById(`siblingId-${index}`).value = siblingId;

        // Ensure the hidden fields exist and update them
        createOrUpdateHiddenField(index, "Name", student.FirstName || '');
        createOrUpdateHiddenField(index, "RollNo", student.RollNo || '');
        createOrUpdateHiddenField(index, "AdmissionNo", student.AdmsnNo || '');
        createOrUpdateHiddenField(index, "Class", student.ClassName || student.Class || '');
        createOrUpdateHiddenField(index, "FatherName", student.FatherName || '');
        createOrUpdateHiddenField(index, "FatherAadharNo", student.FatherAadhar || '');

        // Show the details section
        document.getElementById(`siblingDetails-${index}`).style.display = 'flex';

        // Remove any error messages in this section
        $(`#sibling-section-${index} .sibling-error, #sibling-section-${index} .sibling-error-message`).remove();
        $(`#siblingSearchInput-${index}`).removeClass('is-invalid');

        console.log(`Sibling ${index} selected with ID: ${siblingId}`);
    }

    // Helper function to create or update hidden fields
    function createOrUpdateHiddenField(index, fieldName, value) {
        const fieldId = `sibling${fieldName}-hidden-${index}`;
        const fieldSelector = document.getElementById(fieldId);

        // If field doesn't exist, create it
        if (!fieldSelector) {
            console.log(`Creating missing hidden field: ${fieldId}`);

            const hiddenField = document.createElement('input');
            hiddenField.type = 'hidden';
            hiddenField.id = fieldId;
            hiddenField.name = `Family.Siblings[${index}].${fieldName}`;
            hiddenField.value = value || '';

            const container = document.querySelector(`#siblingDetails-${index} .sibling-hidden-fields`);
            if (container) {
                container.appendChild(hiddenField);
            } else {
                console.error(`Hidden fields container not found for sibling ${index}`);
            }
        } else {
            // Otherwise update the existing field
            fieldSelector.value = value || '';
        }
    }

    // Hide all messages
    function hideAllMessages() {
        noResultsMessage.classList.add('hidden');
        loadingMessage.classList.add('hidden');
        errorMessage.classList.add('hidden');
    }

    // Update the visual selection in the table
    function updateSelection() {
        // Remove highlight from all rows
        const rows = resultsTableBody.querySelectorAll('tr');
        rows.forEach(row => {
            row.classList.remove('bg-primary', 'text-white');
            row.style.backgroundColor = '';
            row.style.color = '';
        });

        // Add highlight to the selected row
        if (currentSelectedRow >= 0 && currentSelectedRow < rows.length) {
            rows[currentSelectedRow].classList.add('bg-primary', 'text-white');
            rows[currentSelectedRow].style.backgroundColor = '#4a86e8';
            rows[currentSelectedRow].style.color = 'white';
            // Ensure the selected row is visible
            rows[currentSelectedRow].scrollIntoView({ block: 'nearest', behavior: 'auto' });
        }
    }

    // Event listeners for sibling dropdown
    searchInput.addEventListener('input', function () {
        // Show dropdown
        resultsContainer.classList.add('active');
        dropdownToggle.textContent = '▲';

        const searchTerm = this.value.trim();
        searchSiblings(searchTerm);
    });

    searchInput.addEventListener('focus', function () {
        // On focus, if there's text, show the dropdown
        if (this.value.trim()) {
            resultsContainer.classList.add('active');
            dropdownToggle.textContent = '▲';

            // Perform search if we have a value
            const searchTerm = this.value.trim();
            searchSiblings(searchTerm);
        }
    });

    // Key event handling for keyboard navigation
    searchInput.addEventListener('keydown', function (e) {
        // Only process keyboard navigation if dropdown is visible and has results
        if (!resultsContainer.classList.contains('active')) return;

        const rows = resultsTableBody.querySelectorAll('tr');
        if (rows.length === 0) return;

        switch (e.key) {
            case 'ArrowDown':
                e.preventDefault();
                if (currentSelectedRow < rows.length - 1) {
                    currentSelectedRow++;
                } else {
                    currentSelectedRow = 0; // Wrap to first item
                }
                updateSelection();
                break;

            case 'ArrowUp':
                e.preventDefault();
                if (currentSelectedRow > 0) {
                    currentSelectedRow--;
                } else {
                    currentSelectedRow = rows.length - 1; // Wrap to last item
                }
                updateSelection();
                break;

            case 'Enter':
                e.preventDefault();
                if (currentSelectedRow >= 0 && currentSelectedRow < searchResults.length) {
                    selectSibling(searchResults[currentSelectedRow]);
                }
                break;

            case 'Escape':
                e.preventDefault();
                resultsContainer.classList.remove('active');
                dropdownToggle.textContent = '▼';
                break;
        }
    });

    dropdownToggle.addEventListener('click', function () {
        resultsContainer.classList.toggle('active');
        this.textContent = resultsContainer.classList.contains('active') ? '▲' : '▼';

        // If opening and we have text, search
        if (resultsContainer.classList.contains('active') && searchInput.value.trim()) {
            searchSiblings(searchInput.value.trim());
        }
    });

    // Close when clicking outside
    document.addEventListener('click', function (event) {
        if (!event.target.closest(`#search-container-${index}`)) {
            resultsContainer.classList.remove('active');
            dropdownToggle.textContent = '▼';
        }
    });
}

// Function to set up sibling removal
function setupSiblingRemoval() {
    // Remove existing event handlers for sibling removal buttons to avoid duplicates
    $(document).off('click', '.remove-sibling-btn');

    // Add new event handler for sibling removal
    $(document).on('click', '.remove-sibling-btn', function () {
        const siblingSection = $(this).closest('.sibling-section');
        const index = siblingSection.attr('id').replace('sibling-section-', '');

        console.log(`Removing sibling with index: ${index}`);

        // If this is not the only/first sibling section, remove it completely
        if (index > 0) {
            siblingSection.remove();
            siblingCount--;

            // Re-enable the Add button if we're below max
            if (siblingCount < MAX_SIBLINGS) {
                $('#addSiblingBtn').prop('disabled', false);
            }

            // Renumber the remaining siblings
            $('.sibling-section').each(function (newIndex) {
                // Update heading
                $(this).find('h5').text(`Sibling ${newIndex + 1}`);

                // Update the section ID
                $(this).attr('id', `sibling-section-${newIndex}`);

                // Update all input IDs and names
                $(this).find('input, select').each(function () {
                    const oldId = $(this).attr('id');
                    if (oldId) {
                        const newId = oldId.replace(/\d+$/, newIndex);
                        $(this).attr('id', newId);
                    }

                    const oldName = $(this).attr('name');
                    if (oldName && oldName.includes('Siblings')) {
                        const newName = oldName.replace(/\[\d+\]/, `[${newIndex}]`);
                        $(this).attr('name', newName);
                    }
                });

                // Update search container
                const searchContainer = $(this).find('.student-search-container');
                if (searchContainer.length) {
                    searchContainer.attr('id', `search-container-${newIndex}`);

                    // Update search input
                    const searchInput = searchContainer.find('.sibling-search');
                    searchInput.attr('id', `siblingSearchInput-${newIndex}`);
                    searchInput.attr('data-index', newIndex);

                    // Update toggle button
                    const toggleBtn = searchContainer.find('.toggle-btn');
                    toggleBtn.attr('id', `dropdownToggle-${newIndex}`);
                    toggleBtn.attr('data-index', newIndex);

                    // Update results container and related elements
                    const resultsContainer = searchContainer.find('.results-container');
                    resultsContainer.attr('id', `resultsContainer-${newIndex}`);
                    resultsContainer.find('tbody').attr('id', `resultsTableBody-${newIndex}`);
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
                    detailsContainer.attr('id', `siblingDetails-${newIndex}`);
                }
            });
        } else {
            // If it's the first/only sibling, just clear the fields and hide details
            siblingSection.find('.sibling-error-message').remove();
            siblingSection.find('.is-invalid').removeClass('is-invalid');
            siblingSection.find('#siblingSearchInput-0').val('');
            siblingSection.find('#siblingDetails-0').hide();
            siblingSection.find('#siblingDetails-0 input').val('');

            // Also clear hidden fields
            siblingSection.find('.sibling-hidden-fields input').val('');
        }

        // Remove any validation errors
        $('.sibling-validation-error').remove();
    });
}

// Function to enhance existing sibling sections with remove buttons
function enhanceSiblingSection(index) {
    const siblingSection = $(`#sibling-section-${index}`);
    const siblingHeading = siblingSection.find('h5');

    // Check if remove button already exists to avoid duplicates
    if (siblingSection.find('.remove-sibling-btn').length === 0) {
        // If the heading is not already in a wrapper
        if (!siblingHeading.parent().hasClass('d-flex')) {
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
}

// Function to validate that father's name and Aadhar are filled for all visible siblings
function validateSiblingFatherInfo() {
    let isValid = true;
    let errorMessage = '';

    // Check each visible sibling section with details showing
    $('.sibling-details:visible').each(function () {
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
            `<div class="alert alert-danger sibling-validation-error mb-3">${errorMessage} Please edit the sibling student.</div>`
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

// Main validation function for the form submission
// Modified validation function for the form submission
function validateSiblingsOnSubmit() {
    // Clear previous error messages
    $('.sibling-validation-error').remove();

    // If siblings aren't enabled, no validation needed
    if (!$('#hasSiblings').is(':checked')) {
        return true;
    }

    // Check if there are any siblings with data in the hidden fields
    const siblingInputs = $('input[name^="Family.Siblings"][name$=".Name"]');
    const siblingCount = siblingInputs.length;
    console.log("Sibling inputs count:", siblingCount);

    // If no siblings are added but siblings are enabled, show error
    if (siblingCount === 0) {
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

    // Check if all siblings have valid data
    let allValid = true;

    // Loop through each sibling by index
    for (let i = 0; i < siblingCount; i++) {
        // Check both hidden and visible fields
        const nameHidden = $(`#siblingName-hidden-${i}`).val();
        const fatherNameHidden = $(`#siblingFatherName-hidden-${i}`).val();
        const fatherAadharHidden = $(`#siblingFatherAadharNo-hidden-${i}`).val();
        const admNoHidden = $(`#siblingAdmissionNo-hidden-${i}`).val();
        const classHidden = $(`#siblingClass-hidden-${i}`).val();

        // Corresponding visible fields (useful for highlighting errors)
        const nameVisible = $(`#siblingName-${i}`).val();
        const fatherNameVisible = $(`#siblingFatherName-${i}`).val();
        const fatherAadharVisible = $(`#siblingFatherAadharNo-${i}`).val();
        const admNoVisible = $(`#siblingAdmNo-${i}`).val();
        const classVisible = $(`#siblingClass-${i}`).val();

        console.log(`Validating sibling ${i}:`, {
            nameHidden,
            fatherNameHidden,
            fatherAadharHidden,
            admNoHidden,
            classHidden,
            nameVisible,
            fatherNameVisible,
            fatherAadharVisible,
            admNoVisible,
            classVisible
        });

        // Ensure hidden fields are populated
        if (!nameHidden || !admNoHidden || !classHidden || !fatherNameHidden || !fatherAadharHidden) {
            $(`#sibling-section-${i} .sibling-error-message`).remove(); // Remove existing message

            $(`#sibling-section-${i}`).prepend(
                '<div class="alert alert-danger sibling-error-message mb-3">' +
                'This sibling is missing required information. Please make sure all siblings have valid data.' +
                '</div>'
            );

            // Highlight search field if basic data is missing
            if (!nameHidden || !admNoHidden || !classHidden) {
                $(`#siblingSearchInput-${i}`).addClass('is-invalid');
            }

            // Highlight father fields specifically if those are missing
            if (!fatherNameHidden) {
                $(`#siblingFatherName-${i}`).addClass('is-invalid');
            }
            if (!fatherAadharHidden) {
                $(`#siblingFatherAadharNo-${i}`).addClass('is-invalid');
            }

            allValid = false;
        } else {
            // Clear any error indications
            $(`#sibling-section-${i} .sibling-error-message`).remove();
            $(`#siblingSearchInput-${i}`).removeClass('is-invalid');
            $(`#siblingFatherName-${i}`).removeClass('is-invalid');
            $(`#siblingFatherAadharNo-${i}`).removeClass('is-invalid');
        }

        // Ensure visible fields match hidden fields
        if ((nameVisible !== nameHidden) ||
            (fatherNameVisible !== fatherNameHidden) ||
            (fatherAadharVisible !== fatherAadharHidden) ||
            (admNoVisible !== admNoHidden) ||
            (classVisible !== classHidden)) {

            // Synchronize visible and hidden fields
            $(`#siblingName-${i}`).val(nameHidden);
            $(`#siblingRollNo-${i}`).val($(`#siblingRollNo-hidden-${i}`).val());
            $(`#siblingAdmNo-${i}`).val(admNoHidden);
            $(`#siblingClass-${i}`).val(classHidden);
            $(`#siblingFatherName-${i}`).val(fatherNameHidden);
            $(`#siblingFatherAadharNo-${i}`).val(fatherAadharHidden);

            console.log(`Synchronized visible fields for sibling ${i}`);
        }

        // Make sure details section is visible if data exists
        if (nameHidden && admNoHidden) {
            $(`#siblingDetails-${i}`).css('display', 'flex');
        }
    }

    if (!allValid) {
        // Scroll to the first error
        $('html, body').animate({
            scrollTop: $('.sibling-error-message:first').offset().top - 100
        }, 500);
    }

    console.log("Sibling validation result:", allValid);
    return allValid;
}

// Debug function to check sibling data before form submission
function debugSiblingData() {
    console.log('Checking sibling data before submission:');

    // Log whether siblings are enabled
    console.log('Siblings checkbox checked:', $('#hasSiblings').is(':checked'));

    // Log all sibling input fields
    console.log('Sibling fields found:');
    $('input[name^="Family.Siblings"]').each(function () {
        console.log(`${$(this).attr('name')}: ${$(this).val()}`);
    });

    // Group sibling data by index for better readability
    const siblingData = {};
    $('input[name^="Family.Siblings"]').each(function () {
        const name = $(this).attr('name');
        const matches = name.match(/Family\.Siblings\[(\d+)\]\.(.+)/);

        if (matches && matches.length === 3) {
            const index = matches[1];
            const property = matches[2];

            if (!siblingData[index]) {
                siblingData[index] = {};
            }

            siblingData[index][property] = $(this).val();
        }
    });

    console.log('Grouped sibling data:', siblingData);
}
// Function to validate all sibling data based on hidden fields
function validateSiblingHiddenFields() {
    // Clear previous error messages
    $('.sibling-validation-error').remove();

    // If siblings aren't enabled, no validation needed
    if (!$('#hasSiblings').is(':checked')) {
        return true;
    }

    // Find all sibling hidden fields containers
    const siblingContainers = $('.sibling-hidden-fields');
    console.log(`Found ${siblingContainers.length} sibling hidden field containers`);

    // If no sibling data but siblings are enabled, show error
    if (siblingContainers.length === 0) {
        $('#siblingsContainer').prepend(
            '<div class="alert alert-danger sibling-validation-error mb-3">' +
            'You have enabled siblings but haven\'t added any. Please add a sibling or uncheck the siblings option.' +
            '</div>'
        );
        return false;
    }

    let allValid = true;

    // Validate each sibling's hidden fields
    siblingContainers.each(function (index) {
        const container = $(this);
        const siblingSection = container.closest('.sibling-section');
        const sectionIndex = siblingSection.attr('id').replace('sibling-section-', '');

        // Get the values from hidden fields
        const siblingId = container.find('.sibling-id-hidden').val();
        const name = container.find('input[name$=".Name"]').val();
        const rollNo = container.find('input[name$=".RollNo"]').val();
        const admNo = container.find('input[name$=".AdmissionNo"]').val();
        const className = container.find('input[name$=".Class"]').val();
        const fatherName = container.find('input[name$=".FatherName"]').val();
        const fatherAadhar = container.find('input[name$=".FatherAadharNo"]').val();

        console.log(`Validating sibling ${sectionIndex} hidden fields:`, {
            name, rollNo, admNo, className, fatherName, fatherAadhar
        });

        // Check for required fields
        const isDataComplete = name && admNo && className && fatherName && fatherAadhar;

        if (!isDataComplete) {
            // Remove existing error message
            siblingSection.find('.sibling-error-message').remove();

            // Add error message
            siblingSection.prepend(
                '<div class="alert alert-danger sibling-error-message mb-3">' +
                'This sibling is missing required information. Please ensure all data is complete.' +
                '</div>'
            );

            // Highlight specific missing fields in the visible form
            if (!name || !admNo || !className) {
                siblingSection.find('.sibling-search').addClass('is-invalid');
            }
            if (!fatherName) {
                siblingSection.find('.sibling-fathername').addClass('is-invalid');
            }
            if (!fatherAadhar) {
                siblingSection.find('.sibling-fatheraadhar').addClass('is-invalid');
            }

            allValid = false;
        } else {
            // Clear any error indicators
            siblingSection.find('.sibling-error-message').remove();
            siblingSection.find('.is-invalid').removeClass('is-invalid');

            // Make sure visible fields match hidden fields
            siblingSection.find('.sibling-name').val(name);
            siblingSection.find('.sibling-rollno').val(rollNo);
            siblingSection.find('.sibling-admno').val(admNo);
            siblingSection.find('.sibling-class').val(className);
            siblingSection.find('.sibling-fathername').val(fatherName);
            siblingSection.find('.sibling-fatheraadhar').val(fatherAadhar);

            // Ensure details section is visible
            siblingSection.find('.sibling-details').css('display', 'flex');
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

// Function to collect all sibling data from hidden fields
function collectSiblingData() {
    const siblingData = [];

    $('.sibling-hidden-fields').each(function (index) {
        const container = $(this);

        // Create an object with all sibling data
        const sibling = {
            siblingId: container.find('.sibling-id-hidden').val(),
            name: container.find('input[name$=".Name"]').val(),
            rollNo: container.find('input[name$=".RollNo"]').val(),
            admissionNo: container.find('input[name$=".AdmissionNo"]').val(),
            class: container.find('input[name$=".Class"]').val(),
            fatherName: container.find('input[name$=".FatherName"]').val(),
            fatherAadharNo: container.find('input[name$=".FatherAadharNo"]').val()
        };

        siblingData.push(sibling);
    });

    console.log('Collected sibling data:', siblingData);
    return siblingData;
}

// Function to ensure hidden fields are properly indexed
function reindexSiblingHiddenFields() {
    $('.sibling-section').each(function (newIndex) {
        const container = $(this).find('.sibling-hidden-fields');

        // Update each hidden input name with correct index
        container.find('input').each(function () {
            const input = $(this);
            const name = input.attr('name');

            if (name && name.includes('Siblings[')) {
                // Update the index part of the name
                const newName = name.replace(/Siblings\[\d+\]/, `Siblings[${newIndex}]`);
                input.attr('name', newName);
            }
        });
    });

    console.log('Reindexed sibling hidden fields');
}

// Function to update a form before submission
function prepareFormForSubmission() {
    // Only process if siblings are enabled
    if ($('#hasSiblings').is(':checked')) {
        // First reindex all hidden fields to ensure proper ordering
        reindexSiblingHiddenFields();

        // Then validate the data
        return validateSiblingHiddenFields();
    }

    return true;
}