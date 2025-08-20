//* StudentSearch - A customizable student search component for MVC 5
// * @version 1.1.0
// */
(function (window) {
    /**
     * StudentSearch constructor
     * @param {string|HTMLElement} selector - CSS selector or DOM element
     * @param {Object} options - Configuration options
     */
    function StudentSearch(selector, options) {
        // Default options
        this.options = {
            placeholder: 'Search for students...',
            apiUrl: '/API/SearchStudents',
            maxResults: 5,
            minLength: 2,
            searchDelay: 300,
            onSelect: null,
            onClear: null,
            onSearch: null
        };

        // Merge options
        if (options) {
            for (var key in options) {
                if (options.hasOwnProperty(key)) {
                    this.options[key] = options[key];
                }
            }
        }

        // Get container
        this.container = typeof selector === 'string' ?
            document.querySelector(selector) : selector;

        if (!this.container) {
            console.error('StudentSearch: Container not found');
            return;
        }

        // State variables
        this.timeout = null;
        this.currentFocus = -1;
        this.isOpen = false;
        this.selectedStudent = null;

        // Initialize
        this.init();
    }

    // Initialize component
    StudentSearch.prototype.init = function () {
        this.container.classList.add('student-search');
        this.createElements();
        this.setupEvents();
    };

    // Create DOM elements
    StudentSearch.prototype.createElements = function () {
        // Clear container
        this.container.innerHTML = '';

        // Create wrapper
        this.wrapper = document.createElement('div');
        this.wrapper.className = 'ss-wrapper';

        // Create search container
        this.searchContainer = document.createElement('div');
        this.searchContainer.className = 'ss-search-container';

        // Create search icon
        this.searchIcon = document.createElement('div');
        this.searchIcon.className = 'ss-search-icon';
        this.searchIcon.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="11" cy="11" r="8"></circle><line x1="21" y1="21" x2="16.65" y2="16.65"></line></svg>';

        // Create clear icon
        this.clearIcon = document.createElement('div');
        this.clearIcon.className = 'ss-clear-icon';
        this.clearIcon.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="18" y1="6" x2="6" y2="18"></line><line x1="6" y1="6" x2="18" y2="18"></line></svg>';

        // Create input
        this.input = document.createElement('input');
        this.input.type = 'text';
        this.input.className = 'ss-input';
        this.input.placeholder = this.options.placeholder;

        // Create hidden fields
        this.hiddenAdmsnNo = document.createElement('input');
        this.hiddenAdmsnNo.type = 'hidden';
        this.hiddenAdmsnNo.name = this.container.id + '_admsnNo';

        this.hiddenSchoolCode = document.createElement('input');
        this.hiddenSchoolCode.type = 'hidden';
        this.hiddenSchoolCode.name = this.container.id + '_schoolCode';

        this.hiddenName = document.createElement('input');
        this.hiddenName.type = 'hidden';
        this.hiddenName.name = this.container.id + '_name';

        this.hiddenClass = document.createElement('input');
        this.hiddenClass.type = 'hidden';
        this.hiddenClass.name = this.container.id + '_class';

        // Add additional hidden fields
        this.hiddenSection = document.createElement('input');
        this.hiddenSection.type = 'hidden';
        this.hiddenSection.name = this.container.id + '_section';

        this.hiddenRollNo = document.createElement('input');
        this.hiddenRollNo.type = 'hidden';
        this.hiddenRollNo.name = this.container.id + '_rollNo';

        this.hiddenFatherName = document.createElement('input');
        this.hiddenFatherName.type = 'hidden';
        this.hiddenFatherName.name = this.container.id + '_fatherName';

        this.hiddenGender = document.createElement('input');
        this.hiddenGender.type = 'hidden';
        this.hiddenGender.name = this.container.id + '_gender';

        this.hiddenDiscountCategory = document.createElement('input');
        this.hiddenDiscountCategory.type = 'hidden';
        this.hiddenDiscountCategory.name = this.container.id + '_discountCategory';

        // Create dropdown
        this.dropdown = document.createElement('div');
        this.dropdown.className = 'ss-dropdown-container';

        // Create spinner
        this.spinner = document.createElement('div');
        this.spinner.className = 'ss-spinner';
        this.spinner.innerHTML = '<div class="ss-spinner-border"></div>';

        // Create results container
        this.results = document.createElement('div');
        this.results.className = 'ss-results';

        // Create selected display with updated structure
        this.selectedDisplay = document.createElement('div');
        this.selectedDisplay.className = 'ss-selected-display';
        this.selectedDisplay.style.display = 'none';

        // Create photo container for selected student display
        var photoContainer = document.createElement('div');
        photoContainer.className = 'ss-selected-photo';

        // Create info container for selected student
        var infoContainer = document.createElement('div');
        infoContainer.className = 'ss-selected-info';

        var mainInfo = document.createElement('div');
        mainInfo.className = 'ss-selected-main';

        var detailsInfo = document.createElement('div');
        detailsInfo.className = 'ss-selected-details';

        infoContainer.appendChild(mainInfo);
        infoContainer.appendChild(detailsInfo);

        this.selectedDisplay.appendChild(photoContainer);
        this.selectedDisplay.appendChild(infoContainer);

        // Assemble the component
        this.searchContainer.appendChild(this.searchIcon);
        this.searchContainer.appendChild(this.input);
        this.searchContainer.appendChild(this.clearIcon);

        this.dropdown.appendChild(this.spinner);
        this.dropdown.appendChild(this.results);

        this.wrapper.appendChild(this.searchContainer);
        this.wrapper.appendChild(this.dropdown);
        this.wrapper.appendChild(this.selectedDisplay);

        // Append all hidden fields
        this.wrapper.appendChild(this.hiddenAdmsnNo);
        this.wrapper.appendChild(this.hiddenSchoolCode);
        this.wrapper.appendChild(this.hiddenName);
        this.wrapper.appendChild(this.hiddenClass);
        this.wrapper.appendChild(this.hiddenSection);
        this.wrapper.appendChild(this.hiddenRollNo);
        this.wrapper.appendChild(this.hiddenFatherName);
        this.wrapper.appendChild(this.hiddenGender);
        this.wrapper.appendChild(this.hiddenDiscountCategory);

        this.container.appendChild(this.wrapper);
    };
    // Add a helper function to create the selected display with photo
    StudentSearch.prototype.createSelectedDisplay = function (student) {
        var photoHtml = '';

        if (student.photo) {
            photoHtml = '<div class="ss-selected-photo"><img src="' + student.photo + '" alt="' + student.name + '" class="ss-selected-img" /></div>';
        } else {
            photoHtml = '<div class="ss-selected-photo"><img src="/template/assets/img/user.png" alt="Default" class="ss-selected-img" /></div>';
        }

        // Get ID value from various potential properties
        var idValue = student.admsnNo || student.srNo || student.id || '';

        // Get section - either directly or from class
        var classValue = student.class || '';
        var sectionValue = student.section || '';

        // If class contains section (e.g., "10th-A"), extract it
        var classOnly = classValue;
        if (classValue && classValue.indexOf('-') > 0) {
            var classParts = classValue.split('-');
            classOnly = classParts[0];
            if (!sectionValue) {
                sectionValue = classParts[1];
            }
        }

        var html =
            '<div class="ss-selected-layout">' +
            photoHtml +
            '<div class="ss-selected-info">' +
            '<div class="ss-selected-main">' +
            '<strong>Name:</strong> ' + (student.name || '') +
            ' | <strong>Father:</strong> ' + (student.fatherName || student.father || 'Not Available') +
            ' | <strong>Class:</strong> ' + classOnly +
            ' | <strong>Section:</strong> ' + sectionValue +
            '</div>' +
            '<div class="ss-selected-details">' +
            '<strong>Admission No:</strong> ' + idValue +
            ' | <strong>Roll No:</strong> ' + (student.rollNo || student.roll || '') +
            ' | <strong>SR No:</strong> ' + (student.srNo || '') +
            '<br><strong>Mobile:</strong> ' + (student.mobNo || student.mobile || '') +
            ' | <strong>Gender:</strong> ' + (student.gWender || '') +
            ' | <strong>Discount Category:</strong> ' + (student.discountCategory || student.category || '') +
            '</div>' +
            '</div>' +
            '</div>';
       
        return html;
    };

   
    // Set up event listeners
    StudentSearch.prototype.setupEvents = function () {
        var self = this;

        // Input event
        this.input.addEventListener('input', function () {
            clearTimeout(self.timeout);

            var query = this.value.trim();
            if (query.length < self.options.minLength) {
                self.closeDropdown();
                return;
            }

            self.timeout = setTimeout(function () {
                self.search(query);
            }, self.options.searchDelay);
        });

        // Focus event
        this.input.addEventListener('focus', function () {
            // Clear input if there was a previously selected student
            if (self.selectedStudent) {
                this.value = '';
                self.selectedStudent = null;
            }

            var query = this.value.trim();
            if (query.length >= self.options.minLength) {
                self.search(query);
            }
        });

        // Clear button click
        this.clearIcon.addEventListener('click', function () {
            self.input.value = '';
            self.closeDropdown();
            self.clearSelection();

            // Call onClear callback
            if (typeof self.options.onClear === 'function') {
                self.options.onClear.call(self);
            }
        });

        // Keyboard navigation
        this.input.addEventListener('keydown', function (e) {
            var items = self.results.querySelectorAll('.ss-item');
            if (!items || items.length === 0) return;

            // Down arrow
            if (e.key === 'ArrowDown') {
                e.preventDefault();
                self.currentFocus++;
                self.setActiveItem(items);
            }
            // Up arrow
            else if (e.key === 'ArrowUp') {
                e.preventDefault();
                self.currentFocus--;
                self.setActiveItem(items);
            }
            // Enter key
            else if (e.key === 'Enter') {
                e.preventDefault();
                if (self.currentFocus > -1 && items[self.currentFocus]) {
                    items[self.currentFocus].click();
                }
            }
            // Escape key
            else if (e.key === 'Escape') {
                self.closeDropdown();
            }
        });

        // Click outside to close
        document.addEventListener('click', function (e) {
            if (!self.wrapper.contains(e.target)) {
                self.closeDropdown();
            }
        });
    };

    // Search for students
    StudentSearch.prototype.search = function (query) {
        var self = this;

        this.openDropdown();
        this.spinner.style.display = 'block';
        this.results.innerHTML = '';
        this.currentFocus = -1;

        // Call onSearch callback
        if (typeof this.options.onSearch === 'function') {
            this.options.onSearch.call(this, query);
        }

        // Prepare the URL with query parameters
        var url = this.options.apiUrl + '?query=' + encodeURIComponent(query) +
            '&maxResults=' + this.options.maxResults;

        // Create an AJAX request
        var xhr = new XMLHttpRequest();
        xhr.open('GET', url, true);
        xhr.setRequestHeader('Content-Type', 'application/json');
        xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');

        xhr.onload = function () {
            self.spinner.style.display = 'none';

            if (xhr.status === 200) {
                try {
                    var data = JSON.parse(xhr.responseText);
                    self.renderResults(data);
                } catch (e) {
                    console.error('Error parsing JSON response:', e);
                    self.results.innerHTML = '<div class="ss-no-results">Error loading results</div>';
                }
            } else {
                console.error('Request failed. Status:', xhr.status);
                self.results.innerHTML = '<div class="ss-no-results">Error loading results</div>';
            }
        };

        xhr.onerror = function () {
            console.error('Network error');
            self.spinner.style.display = 'none';
            self.results.innerHTML = '<div class="ss-no-results">Network error</div>';
        };

        xhr.send();
    };

   // Update the renderResults function to handle potential issues with the JSON response
   
    // Function to render search results
    StudentSearch.prototype.renderResults = function (students) {
        var self = this;
        this.results.innerHTML = '';

        if (!students || students.length === 0) {
            this.results.innerHTML = '<div class="ss-no-results">No students found</div>';
            return;
        }

        // Debug the received data
        console.log('Received students data:', JSON.stringify(students));

        students.forEach(function (student, index) {
            var item = document.createElement('div');
            item.className = 'ss-item';

            // Store all student data as properties
            var allProps = Object.keys(student);
            allProps.forEach(function (prop) {
                if (student[prop]) {
                    item.dataset[prop.toLowerCase()] = student[prop];
                }
            });

            // Create content
            var content = document.createElement('div');
            content.className = 'ss-item-content';

            var photo = document.createElement('div');
            photo.className = 'ss-item-photo';

            // Check if photo URL is available
            if (student.photo) {
                photo.innerHTML = '<img src="' + student.photo + '" alt="' + student.name + '" class="ss-item-img" />';
            } else {
                // Default photo image
                photo.innerHTML = '<img src="/template/assets/img/user.png" alt="Default" class="ss-item-img" />';
            }

            var info = document.createElement('div');
            info.className = 'ss-item-info';

            var name = document.createElement('div');
            name.className = 'ss-item-name';
            name.textContent = student.name || '';

            // Create details with consistent property checking
            var details = document.createElement('div');
            details.className = 'ss-item-details';

            // Create the first row with name, father name, class, and section
            var firstRow = document.createElement('div');
            firstRow.className = 'ss-item-row';
            firstRow.innerHTML =
                '<strong>Name:</strong> ' + (student.name || '') +
                ' | <strong>Father:</strong> ' + (student.fatherName || student.father || 'Not Available') +
                ' | <strong>Class:</strong> ' + (student.class ? student.class.split('-')[0] : '') +
                ' | <strong>Section:</strong> ' + (student.class ? student.class.split('-')[1] : '');

            // Create the second row with IDs
            var secondRow = document.createElement('div');
            secondRow.className = 'ss-item-row';
            secondRow.innerHTML =
                '<strong>Sr No:</strong> ' + (student.srNo || '') +
                ' | <strong>Admission No:</strong> ' + (student.admsnNo || '') +
                ' | <strong>Roll No:</strong> ' + (student.rollNo || '');

            // Create the third row with other details
            var thirdRow = document.createElement('div');
            thirdRow.className = 'ss-item-row';
            thirdRow.innerHTML =
                '<strong>Mobile:</strong> ' + (student.mobNo || student.mobile || '') +
                ' | <strong>Category:</strong> ' + (student.discountCategory || student.category || '') +
                ' | <strong>Gender:</strong> ' + (student.gender || '');

            // Set first item as active
            if (index === 0) {
                item.classList.add('ss-item-active');
                self.currentFocus = 0;
            }

            // Add click handler with all student data
            item.addEventListener('click', function () {
                self.selectStudent(student);
            });

            // Assemble item
            details.appendChild(firstRow);
            details.appendChild(secondRow);
            details.appendChild(thirdRow);
            info.appendChild(details);
            content.appendChild(photo);
            content.appendChild(info);
            item.appendChild(content);

            self.results.appendChild(item);
        });

        // Log the resulting HTML for debugging
        console.log('Rendered search results:', self.results.innerHTML);
    };


    // Select a student with comprehensive information display
    StudentSearch.prototype.selectStudent = function (student) {
        // Store the selected student
        this.selectedStudent = student;

        console.log('Selected student data:', student);

        // Update hidden fields with better property fallbacks
        this.hiddenAdmsnNo.value = student.admsnNo || student.srNo || student.id || '';
        this.hiddenSchoolCode.value = student.schoolCode || '';
        this.hiddenName.value = student.name || '';
        this.hiddenClass.value = student.class || '';

        // Get section - either directly from section property or extract from class
        var section = student.section || '';
        if (!section && student.class && student.class.indexOf('-') > 0) {
            section = student.class.split('-')[1];
        }
        this.hiddenSection.value = section;

        // Handle roll number with fallbacks
        this.hiddenRollNo.value = student.rollNo || student.roll || '';

        // Handle father name with fallbacks
        this.hiddenFatherName.value = student.fatherName || student.father || '';

        // Handle gender
        this.hiddenGender.value = student.gender || '';

        // Handle discount category with fallbacks
        this.hiddenDiscountCategory.value = student.discountCategory || student.category || '';

        // Update input text to show the selected student
        this.input.value = student.name || '';

        // Update selected display
        var selectedDisplay = this.createSelectedDisplay(student);
        this.selectedDisplay.innerHTML = selectedDisplay;

        // Show selected display
        this.selectedDisplay.style.display = 'block';

        // Close dropdown
        this.closeDropdown();

        // Call onSelect callback
        if (typeof this.options.onSelect === 'function') {
            this.options.onSelect.call(this, student);
        }
    };


    // Clear selection
    StudentSearch.prototype.clearSelection = function () {
        this.selectedStudent = null;

        // Clear all hidden fields
        this.hiddenAdmsnNo.value = '';
        this.hiddenSchoolCode.value = '';
        this.hiddenName.value = '';
        this.hiddenClass.value = '';
        this.hiddenSection.value = '';
        this.hiddenRollNo.value = '';
        this.hiddenFatherName.value = '';
        this.hiddenGender.value = '';
        this.hiddenDiscountCategory.value = '';

        this.selectedDisplay.style.display = 'none';

        // Clear photo container
        var photoContainer = this.selectedDisplay.querySelector('.ss-selected-photo');
        if (photoContainer) {
            photoContainer.innerHTML = '';
        }
    };

    // Set active item for keyboard navigation
    StudentSearch.prototype.setActiveItem = function (items) {
        if (!items || items.length === 0) return;

        // Remove active class from all items
        for (var i = 0; i < items.length; i++) {
            items[i].classList.remove('ss-item-active');
        }

        // Adjust current focus
        if (this.currentFocus >= items.length) this.currentFocus = 0;
        if (this.currentFocus < 0) this.currentFocus = items.length - 1;

        // Add active class to current item
        if (items[this.currentFocus]) {
            items[this.currentFocus].classList.add('ss-item-active');
        }
    };

    // Open dropdown
    StudentSearch.prototype.openDropdown = function () {
        if (this.isOpen) return;
        this.dropdown.classList.add('ss-dropdown-open');
        this.isOpen = true;
    };

    // Close dropdown
    StudentSearch.prototype.closeDropdown = function () {
        if (!this.isOpen) return;
        this.dropdown.classList.remove('ss-dropdown-open');
        this.isOpen = false;
    };

    // Public method: Set config
    StudentSearch.prototype.setConfig = function (options) {
        if (options) {
            for (var key in options) {
                if (options.hasOwnProperty(key)) {
                    this.options[key] = options[key];
                }
            }
        }
    };

    // Public method: Reset
    StudentSearch.prototype.reset = function () {
        this.input.value = '';
        this.clearSelection();
        this.closeDropdown();
    };

    // Public method: Get selected student
    StudentSearch.prototype.getSelected = function () {
        return this.selectedStudent;
    };

    // Public method: Destroy
    StudentSearch.prototype.destroy = function () {
        // Clear the container
        this.container.innerHTML = '';
        this.container.classList.remove('student-search');
    };

    // Add to window
    window.StudentSearch = StudentSearch;
})(window);