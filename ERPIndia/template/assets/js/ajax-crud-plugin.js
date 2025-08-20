/**
 * ajaxCRUD.js - A Plug and Play AJAX CRUD Library
 * 
 * This library provides a simple way to implement CRUD operations using AJAX
 * against any RESTful API endpoint.
 * 
 * Usage: 
 * const crud = new AjaxCRUD({
 *   apiUrl: 'https://api.example.com/users',
 *   tableId: 'userTable',
 *   formId: 'userForm',
 *   idField: 'userId',
 *   fields: ['name', 'email', 'phone', 'address'],
 *   onSuccess: (message) => console.log(message),
 *   onError: (error) => console.error(error)
 * });
 * 
 * crud.init();
 */

class AjaxCRUD {
  /**
   * Create a new instance of AjaxCRUD
   * @param {Object} config - Configuration object
   * @param {string} config.apiUrl - The API endpoint URL
   * @param {string} config.tableId - The ID of the table element to display data
   * @param {string} config.formId - The ID of the form element for creating/updating data
   * @param {string} config.idField - The ID of the hidden input field that stores the record ID
   * @param {Array<string>} config.fields - Array of field IDs that should be collected from the form
   * @param {Object} [config.fieldMap] - Optional mapping from API field names to display names
   * @param {Function} [config.onSuccess] - Callback function when operations succeed
   * @param {Function} [config.onError] - Callback function when operations fail
   * @param {Function} [config.beforeRender] - Function to process data before rendering
   * @param {Function} [config.renderRow] - Custom row rendering function
   * @param {boolean} [config.showLoader] - Whether to show a loader during AJAX calls
   * @param {Object} [config.validations] - Validation rules for each field
   */
  constructor(config) {
    // Required configuration
    this.apiUrl = config.apiUrl;
    this.tableId = config.tableId;
    this.formId = config.formId;
    this.idField = config.idField;
    this.fields = config.fields;
    
    // Optional configuration with defaults
    this.fieldMap = config.fieldMap || {};
    this.onSuccess = config.onSuccess || this._defaultSuccessCallback;
    this.onError = config.onError || this._defaultErrorCallback;
    this.beforeRender = config.beforeRender || ((data) => data);
    this.renderRow = config.renderRow || this._defaultRenderRow;
    this.showLoader = config.showLoader !== undefined ? config.showLoader : true;
    this.validations = config.validations || {};
    
    // DOM elements
    this.form = null;
    this.table = null;
    this.submitBtn = null;
    this.cancelBtn = null;
    this.idInput = null;
    this.fieldElements = {};
    
    // Internal state
    this.isEditing = false;
    this.currentEditId = null;
  }
  
  /**
   * Initialize the CRUD functionality
   */
  init() {
    // Get DOM elements
    this.form = document.getElementById(this.formId);
    //this.table = document.getElementById(this.tableId).querySelector('tbody') || document.getElementById(this.tableId);
    this.idInput = document.getElementById(this.idField);
    
    // Get field elements
    this.fields.forEach(field => {
      this.fieldElements[field] = document.getElementById(field);
    });
    
    // Find submit and cancel buttons
    this.submitBtn = this.form.querySelector('button[type="submit"]') || 
                     this.form.querySelector('input[type="submit"]');
    this.cancelBtn = this.form.querySelector('.cancel') || 
                     document.createElement('button');
    
    if (!this.submitBtn) {
      throw new Error('Submit button not found in the form');
    }
    
    // Set up cancel button if it doesn't exist
    if (this.cancelBtn.tagName !== 'BUTTON') {
      this.cancelBtn = document.createElement('button');
      this.cancelBtn.type = 'button';
      this.cancelBtn.className = 'cancel';
      this.cancelBtn.textContent = 'Cancel';
      this.cancelBtn.style.display = 'none';
      this.submitBtn.parentNode.insertBefore(this.cancelBtn, this.submitBtn.nextSibling);
    }
    
    // Create loader if needed
    if (this.showLoader) {
      this._createLoader();
    }
    
    // Setup validation
    this._setupValidation();
    
    // Add event listeners
    this.form.addEventListener('submit', this._handleSubmit.bind(this));
    this.cancelBtn.addEventListener('click', this._resetForm.bind(this));
    
    // Load initial data
    //this.loadData();
  }
  
  /**
   * Set up field validation
   * @private
   */
  _setupValidation() {
    // Create error message elements for each field
    this.fields.forEach(field => {
      const fieldElement = this.fieldElements[field];
      if (fieldElement && this.validations[field]) {
        // Create error message container if it doesn't exist
        let errorContainer = fieldElement.nextElementSibling;
        if (!errorContainer || !errorContainer.classList.contains('error-message')) {
          errorContainer = document.createElement('div');
          errorContainer.className = 'error-message';
          errorContainer.style.cssText = `
            color: #f44336;
            font-size: 12px;
            margin-top: 4px;
            display: none;
          `;
          fieldElement.parentNode.insertBefore(errorContainer, fieldElement.nextElementSibling);
        }
        
        // Add input event listener for real-time validation
        fieldElement.addEventListener('input', () => {
          this._validateField(field);
        });
        
        // Add blur event listener for validation when field loses focus
        fieldElement.addEventListener('blur', () => {
          this._validateField(field);
        });
      }
    });
    
    // Add form styles for invalid fields
    const style = document.createElement('style');
    style.textContent = `
      .crud-field-invalid {
        border-color: #f44336 !important;
      }
    `;
    document.head.appendChild(style);
  }
  
  /**
   * Load data from the API and render it in the table
   */
  loadData() {
    if (this.showLoader) this._showLoader();
    
    fetch(this.apiUrl)
      .then(response => {
        if (!response.ok) {
          throw new Error(`HTTP error! Status: ${response.status}`);
        }
        return response.json();
      })
      .then(data => {
        // Clear existing table data
        this.table.innerHTML = '';
        
        // Process data if needed
        const processedData = this.beforeRender(data);
        
        // Add each item to the table
        processedData.forEach(item => {
          this._addToTable(item);
        });
        
        if (this.showLoader) this._hideLoader();
        this.onSuccess('Data loaded successfully');
      })
      .catch(error => {
        if (this.showLoader) this._hideLoader();
        this.onError(`Error loading data: ${error.message}`);
      });
  }
  
  /**
   * Add an item to the table
   * @param {Object} item - The item to add
   * @private
   */
  _addToTable(item) {
    const row = this.table.insertRow();
    row.dataset.id = item.id;
    
    // Render the row using the custom renderer or default renderer
    row.innerHTML = this.renderRow(item, this.fields, this.fieldMap);
    
    // Add event listeners to action buttons
    const editBtn = row.querySelector('.edit');
    const deleteBtn = row.querySelector('.delete');
    
    if (editBtn) {
      editBtn.addEventListener('click', () => this.editItem(item));
    }
    
    if (deleteBtn) {
      deleteBtn.addEventListener('click', () => this.deleteItem(item.id));
    }
  }
  
  /**
   * Handle form submission
   * @param {Event} e - Form submit event
   * @private
   */
  _handleSubmit(e) {
    e.preventDefault();
    
    // Validate all fields first
    if (!this._validateForm()) {
      return; // Stop submission if validation fails
    }
    
    // Collect form data
    const formData = {};
    this.fields.forEach(field => {
      formData[field] = this.fieldElements[field].value;
    });
    
    // Check if we're editing or creating
    if (this.isEditing && this.currentEditId) {
      this.updateItem(this.currentEditId, formData);
    } else {
      this.createItem(formData);
    }
  }
  
  /**
   * Validate all form fields
   * @returns {boolean} Whether the form is valid
   * @private
   */
  _validateForm() {
    let isValid = true;
    
    this.fields.forEach(field => {
      if (this.validations[field]) {
        const fieldIsValid = this._validateField(field);
        if (!fieldIsValid) {
          isValid = false;
        }
      }
    });
    
    return isValid;
  }
  
  /**
   * Validate a single field
   * @param {string} field - The field to validate
   * @returns {boolean} Whether the field is valid
   * @private
   */
  _validateField(field) {
    const fieldElement = this.fieldElements[field];
    const value = fieldElement.value;
    const validations = this.validations[field];
    const errorContainer = fieldElement.nextElementSibling;
    
    if (!validations) {
      return true; // No validation rules
    }
    
    // Check required
    if (validations.required && (!value || value.trim() === '')) {
      this._showFieldError(fieldElement, errorContainer, validations.required.message || 'This field is required');
      return false;
    }
    
    // Check minimum length
    if (validations.minLength && value.length < validations.minLength.value) {
      this._showFieldError(fieldElement, errorContainer, validations.minLength.message || 
        `Minimum length is ${validations.minLength.value} characters`);
      return false;
    }
    
    // Check maximum length
    if (validations.maxLength && value.length > validations.maxLength.value) {
      this._showFieldError(fieldElement, errorContainer, validations.maxLength.message || 
        `Maximum length is ${validations.maxLength.value} characters`);
      return false;
    }
    
    // Check pattern
    if (validations.pattern && !validations.pattern.value.test(value)) {
      this._showFieldError(fieldElement, errorContainer, validations.pattern.message || 'Invalid format');
      return false;
    }
    
    // Check email format
    if (validations.email && value && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
      this._showFieldError(fieldElement, errorContainer, validations.email.message || 'Invalid email format');
      return false;
    }
    
    // Check custom validation
    if (validations.custom && typeof validations.custom.validator === 'function') {
      const customResult = validations.custom.validator(value);
      if (customResult !== true) {
        this._showFieldError(fieldElement, errorContainer, 
          typeof customResult === 'string' ? customResult : validations.custom.message || 'Invalid value');
        return false;
      }
    }
    
    // All validations passed
    this._hideFieldError(fieldElement, errorContainer);
    return true;
  }
  
  /**
   * Show field error
   * @param {HTMLElement} fieldElement - The field element
   * @param {HTMLElement} errorContainer - The error container element
   * @param {string} message - The error message
   * @private
   */
  _showFieldError(fieldElement, errorContainer, message) {
    fieldElement.classList.add('crud-field-invalid');
    if (errorContainer) {
      errorContainer.textContent = message;
      errorContainer.style.display = 'block';
    }
  }
  
  /**
   * Hide field error
   * @param {HTMLElement} fieldElement - The field element
   * @param {HTMLElement} errorContainer - The error container element
   * @private
   */
  _hideFieldError(fieldElement, errorContainer) {
    fieldElement.classList.remove('crud-field-invalid');
    if (errorContainer) {
      errorContainer.style.display = 'none';
    }
  }
  
  /**
   * Create a new item
   * @param {Object} data - The data to create
   */
  createItem(data) {
    if (this.showLoader) this._showLoader();
    
    fetch(this.apiUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data)
    })
      .then(response => {
        if (!response.ok) {
          throw new Error(`HTTP error! Status: ${response.status}`);
        }
        return response.json();
      })
      .then(newItem => {
        // For APIs that don't return the created item with ID
        if (!newItem.id) {
          newItem = { ...data, id: Date.now() };
        }
        
        //this._addToTable(newItem);
        this._resetForm();
        
        if (this.showLoader) this._hideLoader();
        this.onSuccess('Item created successfully');
      })
      .catch(error => {
        if (this.showLoader) this._hideLoader();
        this.onError(`Error creating item: ${error.message}`);
      });
  }
  
  /**
   * Edit an item (prepare form for editing)
   * @param {Object} item - The item to edit
   */
  editItem(item) {
    // Set form values
    this.idInput.value = item.id;
    this.fields.forEach(field => {
      if (this.fieldElements[field] && item[field] !== undefined) {
        this.fieldElements[field].value = item[field];
      }
    });
    
    // Update UI for editing mode
    this.submitBtn.textContent = 'Update';
    this.cancelBtn.style.display = 'inline-block';
    
    // Update internal state
    this.isEditing = true;
    this.currentEditId = item.id;
    
    // Focus the first field
    if (this.fields.length > 0 && this.fieldElements[this.fields[0]]) {
      this.fieldElements[this.fields[0]].focus();
    }
  }
  
  /**
   * Update an item
   * @param {string|number} id - The ID of the item to update
   * @param {Object} data - The updated data
   */
  updateItem(id, data) {
    if (this.showLoader) this._showLoader();
    
    fetch(`${this.apiUrl}/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data)
    })
      .then(response => {
        if (!response.ok) {
          throw new Error(`HTTP error! Status: ${response.status}`);
        }
        return response.json();
      })
      .then(updatedItem => {
        // For APIs that don't return the updated item
        if (!updatedItem.id) {
          updatedItem = { ...data, id: id };
        }
        
        // Find and update the row
        const rows = this.table.querySelectorAll('tr');
        for (let i = 0; i < rows.length; i++) {
          if (rows[i].dataset.id == id) {
            rows[i].innerHTML = this.renderRow(updatedItem, this.fields, this.fieldMap);
            
            // Re-add event listeners
            const editBtn = rows[i].querySelector('.edit');
            const deleteBtn = rows[i].querySelector('.delete');
            
            if (editBtn) {
              editBtn.addEventListener('click', () => this.editItem(updatedItem));
            }
            
            if (deleteBtn) {
              deleteBtn.addEventListener('click', () => this.deleteItem(id));
            }
            
            break;
          }
        }
        
        this._resetForm();
        
        if (this.showLoader) this._hideLoader();
        this.onSuccess('Item updated successfully');
      })
      .catch(error => {
        if (this.showLoader) this._hideLoader();
        this.onError(`Error updating item: ${error.message}`);
      });
  }
  
  /**
   * Delete an item
   * @param {string|number} id - The ID of the item to delete
   */
  deleteItem(id) {
    if (!confirm('Are you sure you want to delete this item?')) {
      return;
    }
    
    if (this.showLoader) this._showLoader();
    
    fetch(`${this.apiUrl}/${id}`, {
      method: 'DELETE'
    })
      .then(response => {
        if (!response.ok) {
          throw new Error(`HTTP error! Status: ${response.status}`);
        }
        
        // Try to parse response (some APIs return empty response for DELETE)
        return response.text().then(text => {
          return text ? JSON.parse(text) : {};
        });
      })
      .then(() => {
        // Remove row from table
        const rows = this.table.querySelectorAll('tr');
        for (let i = 0; i < rows.length; i++) {
          if (rows[i].dataset.id == id) {
            this.table.removeChild(rows[i]);
            break;
          }
        }
        
        if (this.showLoader) this._hideLoader();
        this.onSuccess('Item deleted successfully');
      })
      .catch(error => {
        if (this.showLoader) this._hideLoader();
        this.onError(`Error deleting item: ${error.message}`);
      });
  }
  
  /**
   * Reset the form to its initial state
   * @private
   */
  _resetForm() {
    this.form.reset();
    this.idInput.value = '';
    this.submitBtn.textContent = 'Add';
    this.cancelBtn.style.display = 'none';
    
    // Reset validation errors
    this.fields.forEach(field => {
      const fieldElement = this.fieldElements[field];
      if (fieldElement) {
        const errorContainer = fieldElement.nextElementSibling;
        if (errorContainer && errorContainer.classList.contains('error-message')) {
          this._hideFieldError(fieldElement, errorContainer);
        }
      }
    });
    
    // Reset internal state
    this.isEditing = false;
    this.currentEditId = null;
  }
  
  /**
   * Create a loader element
   * @private
   */
  _createLoader() {
    // Create overlay
    this.overlay = document.createElement('div');
    this.overlay.style.cssText = `
      display: none;
      position: fixed;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      background-color: rgba(0, 0, 0, 0.5);
      z-index: 999;
    `;
    
    // Create loader
    this.loader = document.createElement('div');
    this.loader.style.cssText = `
      display: none;
      border: 5px solid #f3f3f3;
      border-top: 5px solid #3498db;
      border-radius: 50%;
      width: 50px;
      height: 50px;
      animation: spin 1s linear infinite;
      position: fixed;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      z-index: 1000;
    `;
    
    // Create animation
    const style = document.createElement('style');
    style.textContent = `
      @keyframes spin {
        0% { transform: translate(-50%, -50%) rotate(0deg); }
        100% { transform: translate(-50%, -50%) rotate(360deg); }
      }
    `;
    
    // Append to body
    document.head.appendChild(style);
    document.body.appendChild(this.overlay);
    document.body.appendChild(this.loader);
  }
  
  /**
   * Show the loader
   * @private
   */
  _showLoader() {
    if (this.overlay && this.loader) {
      this.overlay.style.display = 'block';
      this.loader.style.display = 'block';
    }
  }
  
  /**
   * Hide the loader
   * @private
   */
  _hideLoader() {
    if (this.overlay && this.loader) {
      this.overlay.style.display = 'none';
      this.loader.style.display = 'none';
    }
  }
  
  /**
   * Default success callback
   * @param {string} message - Success message
   * @private
   */
  _defaultSuccessCallback(message) {
    console.log(message);
  }
  
  /**
   * Default error callback
   * @param {string} error - Error message
   * @private
   */
  _defaultErrorCallback(error) {
    console.error(error);
  }
  
  /**
   * Default row renderer
   * @param {Object} item - The item to render
   * @param {Array<string>} fields - The fields to display
   * @param {Object} fieldMap - Field name mapping
   * @returns {string} HTML for the row
   * @private
   */
  _defaultRenderRow(item, fields, fieldMap) {
    let html = '';
    
    // Add data cells
    fields.forEach(field => {
      const displayValue = item[field] !== undefined ? item[field] : '';
      html += `<td>${displayValue}</td>`;
    });
    
    // Add action buttons
    html += `
      <td class="actions">
        <button class="edit" data-id="${item.id}">Edit</button>
        <button class="delete" data-id="${item.id}">Delete</button>
      </td>
    `;
    
    return html;
  }
}

// Optional notification system
class CRUDNotification {
  /**
   * Create a notification system
   * @param {Object} [config] - Configuration object
   * @param {string} [config.position='top-right'] - Position of notifications
   * @param {number} [config.duration=3000] - Duration in milliseconds
   */
  constructor(config = {}) {
    this.position = config.position || 'top-right';
    this.duration = config.duration || 3000;
    this.container = null;
    
    this._initialize();
  }
  
  /**
   * Initialize the notification system
   * @private
   */
  _initialize() {
    // Create container
    this.container = document.createElement('div');
    this.container.className = 'crud-notification-container';
    this.container.style.cssText = `
      position: fixed;
      z-index: 9999;
      display: flex;
      flex-direction: column;
      max-width: 300px;
    `;
    
    // Set position
    switch (this.position) {
      case 'top-right':
        this.container.style.top = '20px';
        this.container.style.right = '20px';
        break;
      case 'top-left':
        this.container.style.top = '20px';
        this.container.style.left = '20px';
        break;
      case 'bottom-right':
        this.container.style.bottom = '20px';
        this.container.style.right = '20px';
        this.container.style.flexDirection = 'column-reverse';
        break;
      case 'bottom-left':
        this.container.style.bottom = '20px';
        this.container.style.left = '20px';
        this.container.style.flexDirection = 'column-reverse';
        break;
      default:
        this.container.style.top = '20px';
        this.container.style.right = '20px';
    }
    
    // Add to document
    document.body.appendChild(this.container);
    
    // Add styles
    const style = document.createElement('style');
    style.textContent = `
      .crud-notification {
        padding: 15px;
        margin-bottom: 10px;
        border-radius: 4px;
        color: white;
        font-family: Arial, sans-serif;
        font-size: 14px;
        box-shadow: 0 2px 5px rgba(0, 0, 0, 0.2);
        animation: crud-notification-fade 0.3s ease-out;
        cursor: pointer;
      }
      
      .crud-notification.success {
        background-color: #4CAF50;
      }
      
      .crud-notification.error {
        background-color: #f44336;
      }
      
      .crud-notification.info {
        background-color: #2196F3;
      }
      
      .crud-notification.warning {
        background-color: #ff9800;
      }
      
      @keyframes crud-notification-fade {
        from {
          opacity: 0;
          transform: translateY(-20px);
        }
        to {
          opacity: 1;
          transform: translateY(0);
        }
      }
    `;
    
    document.head.appendChild(style);
  }
  
  /**
   * Show a notification
   * @param {string} message - The message to display
   * @param {string} type - The type of notification ('success', 'error', 'info', 'warning')
   */
  show(message, type = 'info') {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `crud-notification ${type}`;
    notification.textContent = message;
    
    // Add click event to dismiss
    notification.addEventListener('click', () => {
      this.container.removeChild(notification);
    });
    
    // Add to container
    this.container.appendChild(notification);
    
    // Auto-remove after duration
    setTimeout(() => {
      if (notification.parentNode === this.container) {
        this.container.removeChild(notification);
      }
    }, this.duration);
  }
  
  /**
   * Show a success notification
   * @param {string} message - The message to display
   */
  success(message) {
    this.show(message, 'success');
  }
  
  /**
   * Show an error notification
   * @param {string} message - The message to display
   */
  error(message) {
    this.show(message, 'error');
  }
  
  /**
   * Show an info notification
   * @param {string} message - The message to display
   */
  info(message) {
    this.show(message, 'info');
  }
  
  /**
   * Show a warning notification
   * @param {string} message - The message to display
   */
  warning(message) {
    this.show(message, 'warning');
  }
}
