/*!
 * FeeCollect.js v1.1.1
 * A comprehensive library for educational fee collection management
 * 
 * Released under the MIT License
 */

(function(global, factory) {
  typeof exports === 'object' && typeof module !== 'undefined' ? module.exports = factory() :
  typeof define === 'function' && define.amd ? define(factory) :
  (global = global || self, global.FeeCollect = factory());
}(this, function() {
  'use strict';
  
  /**
   * Default configuration options
   */
  const DEFAULT_CONFIG = {
    dateFormat: 'DD-MM-YYYY',
    currency: '',
    autoGenerateReceipt: true,
    showProgressBar: true,
    showLedgerButton: true,
    showFineDiscount: true,
    autoUpdateReceived: true,
    paymentMethods: ['cash', 'bank', 'cheque', 'upi', 'paytm'],
    monthNames: ['April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December', 'January', 'February', 'March'],
    monthShortNames: ['Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec', 'Jan', 'Feb', 'Mar'],
    labels: {
      title: 'Fee Collect',
      oldYearBalance: 'Old Year Bal',
      receiptPrefix: 'Receipt No ',
      receiptAuto: 'Auto Generated',
      monthlyTotal: 'Monthly Total:',
      clickAdd: 'Click',
      summaryTitle: 'Fee Summary',
      serialNo: 'S.No.',
      feeMonth: 'Fee Month',
      feeName: 'Fee Name',
      feeAmount: 'Fee Amount',
      fine: 'Fine',
      discount: 'Discount',
      netAmount: 'Net Amount',
      cancel: 'Cancel',
      balance: 'Previous Balance',
      total: 'Total',
      finalTotal: 'Grand Total',
      concession: '- Concession',
      lateFee: '+ Late Fee',
      received: 'Received',
      remain: 'Remain',
      note: 'Note',
      receivedFee: 'Received Fee',
      showLedger: 'Show Ledger'
    },
    callbacks: {
      onInit: null,
      onFeeSelect: null,
      onFeeBulkSelect: null,
      onFeeRemove: null,
      onPaymentMethodChange: null,
      onPaymentComplete: null,
      onLedgerOpen: null,
      onReceiptGenerate: null
    }
  };
  
  /**
   * FeeCollect main class
   */
  class FeeCollect {
    /**
     * Constructor for the FeeCollect component
     * @param {HTMLElement|String} container - Container element or selector
     * @param {Object} data - Fee data object
     * @param {Object} options - Configuration options
     */
    constructor(container, data, options = {}) {
      if (typeof container === 'string') {
        this.container = document.querySelector(container);
      } else {
        this.container = container;
      }
      
      if (!this.container) {
        throw new Error('FeeCollect: Container element not found');
      }
      
      this.data = data || { feeData: [], studentInfo: {} };
      this.options = this._mergeOptions(DEFAULT_CONFIG, options);
      this.selectedFees = [];
      this.receiptNumber = this._generateReceiptNumber();
      this.currentDate = this._formatDate(new Date());
      this._previousNetAmount = 0; // Track previous net amount for calculation
      this.balance = this.data.studentInfo?.balance || 0;
      
      this._init();
    }
    
    /**
     * Initialize the component
     * @private
     */
    _init() {
      // Create container with class
      this.container.classList.add('fee-collect-container');
      
      // Render the component
      this._render();
      
      // Set initial balance if provided in studentInfo
      if (this.data.studentInfo && this.data.studentInfo.balance) {
        this.setBalance(this.data.studentInfo.balance);
      }
      
      // Trigger onInit callback
      if (typeof this.options.callbacks.onInit === 'function') {
        this.options.callbacks.onInit.call(this);
      }
    }
    
    /**
     * Merge default and user options
     * @private
     * @param {Object} defaults - Default options
     * @param {Object} options - User options
     * @returns {Object} - Merged options
     */
    _mergeOptions(defaults, options) {
      const result = Object.assign({}, defaults);
      
      // Deep merge the labels and callbacks objects
      if (options.labels) {
        result.labels = Object.assign({}, defaults.labels, options.labels);
      }
      
      if (options.callbacks) {
        result.callbacks = Object.assign({}, defaults.callbacks, options.callbacks);
      }
      
      // Merge the rest of the options
      for (const key in options) {
        if (key !== 'labels' && key !== 'callbacks') {
          result[key] = options[key];
        }
      }
      
      return result;
    }
    
    /**
     * Generate receipt number
     * @private
     * @returns {String} - Receipt number
     */
    _generateReceiptNumber() {
      if (!this.options.autoGenerateReceipt) {
        return this.options.labels.receiptAuto;
      }
      
      // Generate a receipt number based on timestamp and random value
      const timestamp = new Date().getTime();
      const random = Math.floor(Math.random() * 1000);
      return `${this.options.labels.receiptPrefix}${timestamp}${random}`;
    }
    
    /**
     * Format date according to the specified format
     * @private
     * @param {Date} date - Date object
     * @returns {String} - Formatted date string
     */
    _formatDate(date) {
      // Simple formatting function, replace with more sophisticated one if needed
      const day = String(date.getDate()).padStart(2, '0');
      const month = String(date.getMonth() + 1).padStart(2, '0');
      const year = date.getFullYear();
      
      return this.options.dateFormat
        .replace('DD', day)
        .replace('MM', month)
        .replace('YYYY', year);
    }
    
    /**
     * Render the component
     * @private
     */
    _render() {
      // Clear any existing event listeners before re-rendering
      this._cleanupEventListeners();
      
      this.container.innerHTML = this._generateHTML();
      
      // Initialize fee table
      this._loadFeeData();
      this._loadSummaryTable();
      this._checkMonthStatus();
      this._updateTotalAmount();
      
      // Attach event listeners to the newly rendered elements
      this._attachEvents();
    }
    
    /**
     * Clean up event listeners before re-rendering
     * @private
     */
    _cleanupEventListeners() {
      // This method intentionally left empty
      // In a more sophisticated implementation, you might want to
      // keep track of attached listeners and remove them here
    }
    
    /**
     * Generate HTML for the component
     * @private
     * @returns {String} - Component HTML
     */
    _generateHTML() {
      return `
        <div class="fee-collect">
          <!-- Header section -->
          <div class="fee-collect-header">
            <h1>${this.options.labels.title}</h1>
            <div class="fee-collect-balance">
              <span>${this.options.labels.oldYearBalance}: 0</span>
              <button class="fee-collect-add-button">+</button>
            </div>
          </div>
          
          <!-- Receipt section -->
          <div class="fee-collect-receipt">
            <div class="fee-collect-receipt-box">${this.receiptNumber}</div>
            <div class="fee-collect-date-box">
              <span>${this.currentDate}</span>
              <span class="fee-collect-calendar-icon">📅</span>
            </div>
          </div>
          
          <!-- Cursor highlight element -->
          <div class="fee-collect-cursor-highlight" id="feeCollectCursor" style="display: none;"></div>
          <div id="feeCollectNotification" class="fee-collect-notification" style="display: none;"></div>
          
          <!-- Fee table -->
          <div class="fee-collect-table-container">
            <table class="fee-collect-table" id="feeCollectTable">
              <thead>
                <tr>
                  <th style="width: 5%;">SN</th>
                  <th style="width: 15%; text-align: left;">${this.options.labels.feeName}</th>
                  ${this.options.monthShortNames.map(month => 
                    `<th style="width: 6.66%;">${month}</th>`
                  ).join('')}
                </tr>
              </thead>
              <tbody id="feeCollectTableBody">
                <!-- Fee data will be inserted here by JavaScript -->
              </tbody>
            </table>
          </div>
          
          <!-- Monthly totals row -->
          <table class="fee-collect-table" style="margin-bottom: 0;">
            <tr class="fee-collect-monthly-total">
              <td style="width: 5%;"></td>
              <td style="width: 15%; text-align: right; font-weight: bold;">${this.options.labels.monthlyTotal}</td>
              ${this.options.monthShortNames.map(month => 
                `<td style="width: 6.66%;" id="feeCollectTotal-${month}">0</td>`
              ).join('')}
            </tr>
          </table>
          
          <!-- Click row with + buttons -->
          <table class="fee-collect-table fee-collect-add-row">
            <tr>
              <td style="width: 5%;"></td>
              <td style="width: 15%; text-align: left; color: #0078d7; font-weight: bold;">${this.options.labels.clickAdd} <span style="color: #8064a2; font-size: 20px;">➕</span></td>
              ${this.options.monthShortNames.map(month => 
                `<td style="width: 6.66%;"><button class="fee-collect-add-month" data-month="${month}">+</button></td>`
              ).join('')}
            </tr>
          </table>
          
          <!-- Fee Summary Section -->
          <div class="fee-collect-summary" id="feeCollectSummary">
            <table class="fee-collect-summary-table">
              <thead>
                <tr>
                  <th>${this.options.labels.serialNo}</th>
                  <th>${this.options.labels.feeMonth}</th>
                  <th>${this.options.labels.feeName}</th>
                  <th>${this.options.labels.feeAmount}</th>
                  ${this.options.showFineDiscount ? `
                  <th>${this.options.labels.fine || 'Fine'}</th>
                  <th>${this.options.labels.discount || 'Discount'}</th>
                  <th>${this.options.labels.netAmount || 'Net Amount'}</th>
                  ` : ''}
                  <th>${this.options.labels.cancel}</th>
                </tr>
              </thead>
              <tbody id="feeCollectSummaryBody">
                <!-- Summary data will be inserted here by JavaScript -->
              </tbody>
            </table>
            
            ${this.options.showProgressBar ? 
              `<div class="fee-collect-progress-bar">
                <div class="fee-collect-progress-indicator" id="feeCollectProgressIndicator"></div>
              </div>` : ''}
          </div>
          
          <!-- Payment Summary Section -->
          <div class="fee-collect-payment">
            <div class="fee-collect-payment-row">
              <div class="fee-collect-payment-item">
                <div class="fee-collect-payment-label">${this.options.labels.balance}</div>
                <input type="text" class="fee-collect-payment-input" id="feeCollectBalanceAmount" value="${this.balance}">
              </div>
              <div class="fee-collect-payment-item">
                <div class="fee-collect-payment-label">${this.options.labels.total}</div>
                <input type="text" class="fee-collect-payment-input" id="feeCollectTotalAmount" value="0" readonly>
              </div>
              <div class="fee-collect-payment-item">
                <div class="fee-collect-payment-label">${this.options.labels.finalTotal}</div>
                <input type="text" class="fee-collect-payment-input fee-collect-final-total" id="feeCollectFinalTotalAmount" value="0" readonly>
              </div>
              <div class="fee-collect-payment-item">
                <div class="fee-collect-payment-label">${this.options.labels.concession}</div>
                <input type="text" class="fee-collect-payment-input" id="feeCollectConcessionAmount" value="0">
              </div>
              <div class="fee-collect-payment-item">
                <div class="fee-collect-payment-label">${this.options.labels.lateFee}</div>
                <input type="text" class="fee-collect-payment-input" id="feeCollectLateFeeAmount" value="0">
              </div>
              <div class="fee-collect-payment-item fee-collect-highlight">
                <div class="fee-collect-payment-label">${this.options.labels.received}</div>
                <input type="text" class="fee-collect-payment-input" id="feeCollectReceivedAmount" value="0">
              </div>
              <div class="fee-collect-payment-item">
                <div class="fee-collect-payment-label">${this.options.labels.remain}</div>
                <input type="text" class="fee-collect-payment-input" id="feeCollectRemainAmount" value="0" readonly>
              </div>
              <div class="fee-collect-payment-item">
                <div class="fee-collect-payment-label">${this.options.labels.note}</div>
                <input type="text" class="fee-collect-payment-input" id="feeCollectPaymentNote">
              </div>
            </div>
            
            <div class="fee-collect-payment-options">
              ${this.options.paymentMethods.map(method => 
                `<div class="fee-collect-payment-method">
                  <label class="fee-collect-radio-container">
                    <input type="radio" name="feeCollectPaymentMethod" value="${method}" ${method === 'cash' ? 'checked' : ''}>
                    <span class="fee-collect-radio-label">${method.charAt(0).toUpperCase() + method.slice(1)}</span>
                  </label>
                </div>`
              ).join('')}
            </div>
            
            <div class="fee-collect-action-buttons">
              <button class="fee-collect-action-button fee-collect-receive-button">${this.options.labels.receivedFee}</button>
              ${this.options.showLedgerButton ? 
                `<button class="fee-collect-action-button fee-collect-ledger-button">${this.options.labels.showLedger}</button>` : ''}
            </div>
          </div>
        </div>
      `;
    }
    
    /**
     * Load fee data into the table
     * @private
     */
    _loadFeeData() {
      const tableBody = document.getElementById('feeCollectTableBody');
      if (!tableBody) return;
      
      tableBody.innerHTML = '';
      
      this.data.feeData.forEach(fee => {
        const row = document.createElement('tr');
        
        // Add SN column
        const snCell = document.createElement('td');
        snCell.textContent = fee.id;
        snCell.style.width = '5%';
        row.appendChild(snCell);
        
        // Add Fee Name column
        const nameCell = document.createElement('td');
        nameCell.textContent = fee.name;
        nameCell.style.width = '15%';
        nameCell.style.textAlign = 'left';
        row.appendChild(nameCell);
        
        // Add monthly amount columns
        fee.amounts.forEach((amount, index) => {
          this._addMonthlyAmountCell(row, fee, index);
        });
        
        tableBody.appendChild(row);
      });
      
      // Calculate and update monthly totals
      this._updateMonthlyTotals();
    }
    
    /**
     * Add a cell to the monthly amount columns
     * @private
     * @param {HTMLElement} row - The row element to add the cell to
     * @param {Object} fee - Fee data object
     * @param {Number} index - Month index
     */
    _addMonthlyAmountCell(row, fee, index) {
      const amountCell = document.createElement('td');
      const amount = fee.amounts[index] || 0;
      // Always get fine and discount values, but apply them conditionally
      const fine = fee.fines && fee.fines[index] ? fee.fines[index] : 0;
      const discount = fee.discounts && fee.discounts[index] ? fee.discounts[index] : 0;
      
      if (this.options.showFineDiscount && (fine > 0 || discount > 0)) {
        // Create a container for the amount and fine/discount
        const cellContainer = document.createElement('div');
        cellContainer.className = 'fee-collect-cell-container';
        
        // Add main amount
        const amountDiv = document.createElement('div');
        amountDiv.className = 'fee-collect-amount';
        amountDiv.textContent = amount;
        cellContainer.appendChild(amountDiv);
        
        // Check if there's a fine for this month
        if (fine > 0) {
          const fineDiv = document.createElement('div');
          fineDiv.className = 'fee-collect-fine';
          fineDiv.textContent = `+${fine}`;
          cellContainer.appendChild(fineDiv);
        }
        
        // Check if there's a discount for this month
        if (discount > 0) {
          const discountDiv = document.createElement('div');
          discountDiv.className = 'fee-collect-discount';
          discountDiv.textContent = `-${discount}`;
          cellContainer.appendChild(discountDiv);
        }
        
        amountCell.appendChild(cellContainer);
      } else {
        // Simple display if showFineDiscount is false or no fines/discounts
        amountCell.textContent = amount;
      }
      
      amountCell.style.width = '6.66%';
      amountCell.dataset.month = this.options.monthShortNames[index];
      amountCell.dataset.feeId = fee.id;
      amountCell.dataset.feeName = fee.name;
      amountCell.dataset.monthFull = this.options.monthNames[index];
      // Always store fine and discount values, even if not displayed
      amountCell.dataset.fine = fine;
      amountCell.dataset.discount = discount;
      
      // Make the cell clickable if it has an amount > 0
      if (amount > 0) {
        amountCell.style.cursor = 'pointer';
        amountCell.classList.add('fee-collect-clickable');
        // Use a bound function with the correct arguments for the event listener
        amountCell.addEventListener('click', (event) => {
          this._handleFeeClick(fee.id, this.options.monthNames[index], fee.name, amount, fine, discount, event);
        });
      }
      
      row.appendChild(amountCell);
    }
    
    /**
     * Calculate and update monthly totals
     * @private
     */
    _updateMonthlyTotals() {
      // Calculate total for each month
      this.options.monthShortNames.forEach((month, index) => {
        let baseTotal = 0;
        let fineTotal = 0;
        let discountTotal = 0;
        
        this.data.feeData.forEach(fee => {
          // Add base amount
          baseTotal += fee.amounts[index] || 0;
          
          // Add fines if available
          if (fee.fines && fee.fines[index]) {
            fineTotal += fee.fines[index];
          }
          
          // Subtract discounts if available
          if (fee.discounts && fee.discounts[index]) {
            discountTotal += fee.discounts[index];
          }
        });
        
        // Update the total display
        const totalElement = document.getElementById(`feeCollectTotal-${month}`);
        if (totalElement) {
          this._displayMonthlyTotal(totalElement, baseTotal, fineTotal, discountTotal);
        }
      });
    }
    
    /**
     * Display monthly total with optional fine/discount details
     * @private
     * @param {HTMLElement} totalElement - Element to update with total
     * @param {Number} baseTotal - Base fee amount
     * @param {Number} fineTotal - Fine amount
     * @param {Number} discountTotal - Discount amount
     */
    _displayMonthlyTotal(totalElement, baseTotal, fineTotal, discountTotal) {
      // Calculate net total
      const netTotal = baseTotal + fineTotal - discountTotal;
      
      // Store data attributes for reference
      totalElement.dataset.baseTotal = baseTotal;
      totalElement.dataset.fineTotal = fineTotal;
      totalElement.dataset.discountTotal = discountTotal;
      totalElement.dataset.netTotal = netTotal;
      
      // Display according to options and values
      if (this.options.showFineDiscount && (fineTotal > 0 || discountTotal > 0)) {
        const totalContainer = document.createElement('div');
        totalContainer.className = 'fee-collect-total-container';
        
        // Main total
        const totalAmount = document.createElement('div');
        totalAmount.className = 'fee-collect-total-amount';
        totalAmount.textContent = baseTotal;
        totalContainer.appendChild(totalAmount);
        
        // Fine indicator
        if (fineTotal > 0) {
          const totalFine = document.createElement('div');
          totalFine.className = 'fee-collect-total-fine';
          totalFine.textContent = `+${fineTotal}`;
          totalContainer.appendChild(totalFine);
        }
        
        // Discount indicator
        if (discountTotal > 0) {
          const totalDiscount = document.createElement('div');
          totalDiscount.className = 'fee-collect-total-discount';
          totalDiscount.textContent = `-${discountTotal}`;
          totalContainer.appendChild(totalDiscount);
        }
        
        // Net amount
        const netAmount = document.createElement('div');
        netAmount.className = 'fee-collect-total-net';
        netAmount.textContent = `=${netTotal}`;
        totalContainer.appendChild(netAmount);
        
        // Clear existing content and append the container
        totalElement.innerHTML = '';
        totalElement.appendChild(totalContainer);
      } else {
        // Simple display if showFineDiscount is false or if no fines/discounts
        totalElement.textContent = netTotal; // Always show net total for consistency
      }
    }
    
    /**
     * Handle fee click event
     * @private
     * @param {Number} id - Fee ID
     * @param {String} month - Fee month
     * @param {String} name - Fee name
     * @param {Number} amount - Fee amount
     * @param {Number} fine - Fine amount
     * @param {Number} discount - Discount amount
     * @param {Event} event - Click event
     */
    _handleFeeClick(id, month, name, amount, fine = 0, discount = 0, event) {
      if (event && event.currentTarget) {
        this._showCursorHighlight(event.currentTarget);
      }
      
      // Apply fine and discount based on showFineDiscount option
      const effectiveFine = this.options.showFineDiscount ? fine : 0;
      const effectiveDiscount = this.options.showFineDiscount ? discount : 0;
      
      this._addFeeToSummary(id, month, name, amount, effectiveFine, effectiveDiscount);
    }
    
    /**
     * Show cursor highlight effect
     * @private
     * @param {HTMLElement} element - Target element
     */
    _showCursorHighlight(element) {
      let cursor = document.getElementById('feeCollectCursor');
      
      if (!cursor) {
        // Create cursor highlight element if it doesn't exist
        cursor = document.createElement('div');
        cursor.id = 'feeCollectCursor';
        cursor.className = 'fee-collect-cursor-highlight';
        cursor.style.display = 'none';
        document.body.appendChild(cursor);
      }
      
      const rect = element.getBoundingClientRect();
      
      cursor.style.display = 'block';
      cursor.style.left = (rect.left + rect.width/2) + 'px';
      cursor.style.top = (rect.top + rect.height/2) + 'px';
      
      setTimeout(() => {
        cursor.style.display = 'none';
      }, 1500);
    }
    
    /**
     * Show notification message
     * @private
     * @param {String} message - Notification message
     */
    _showNotification(message) {
      const notification = document.getElementById('feeCollectNotification');
      if (!notification) {
        // Create notification element if it doesn't exist
        const notif = document.createElement('div');
        notif.id = 'feeCollectNotification';
        notif.className = 'fee-collect-notification';
        notif.style.display = 'none';
        document.body.appendChild(notif);
      }
      
      const notificationElement = document.getElementById('feeCollectNotification');
      
      // Set message and show notification
      notificationElement.textContent = message;
      notificationElement.style.display = 'block';
      
      // Hide notification after 3 seconds
      setTimeout(() => {
        notificationElement.style.display = 'none';
      }, 3000);
    }
    
    /**
     * Add a fee to the summary table
     * @private
     * @param {Number} id - Fee ID
     * @param {String} month - Fee month
     * @param {String} name - Fee name
     * @param {Number} amount - Fee amount
     * @param {Number} fine - Fine amount
     * @param {Number} discount - Discount amount
     */
    _addFeeToSummary(id, month, name, amount, fine = 0, discount = 0) {
      // Check if fee already exists in selected fees
      const existingIndex = this.selectedFees.findIndex(fee => 
        fee.id === id && fee.month === month && fee.name === name
      );
      
      if (existingIndex !== -1) {
        // Show notification that fee is already added
        this._showNotification(`${name} for ${month} is already in the list`);
        return; // Exit to prevent duplicate addition
      }
      
      // Calculate net amount based on effective fine and discount
      const netAmount = amount + fine - discount;
      
      this.selectedFees.push({
        id: id,
        month: month,
        name: name,
        amount: amount,
        fine: fine,
        discount: discount,
        netAmount: netAmount
      });
      
      // Trigger onFeeSelect callback
      if (typeof this.options.callbacks.onFeeSelect === 'function') {
        this.options.callbacks.onFeeSelect.call(this, {
          id: id,
          month: month,
          name: name,
          amount: amount,
          fine: fine,
          discount: discount,
          netAmount: netAmount
        });
      }
      
      this._loadSummaryTable();
    }
    
    /**
     * Add all fees for a specific month
     * @private
     * @param {String} month - Short month name
     */
    _addMonthFees(month) {
      const monthIndex = this.options.monthShortNames.indexOf(month);
      if (monthIndex === -1) return;
      
      const monthFull = this.options.monthNames[monthIndex];
      let addedCount = 0;
      
      // Add all fees for this month to the summary
      this.data.feeData.forEach(fee => {
        const amount = fee.amounts[monthIndex];
        
        if (amount > 0) {
          // Always get fine and discount values, but apply conditionally
          const fine = fee.fines && fee.fines[monthIndex] ? fee.fines[monthIndex] : 0;
          const discount = fee.discounts && fee.discounts[monthIndex] ? fee.discounts[monthIndex] : 0;
          
          // Apply fine and discount based on showFineDiscount option
          const effectiveFine = this.options.showFineDiscount ? fine : 0;
          const effectiveDiscount = this.options.showFineDiscount ? discount : 0;
          
          // Check if fee already exists
          const existingIndex = this.selectedFees.findIndex(selectedFee => 
            selectedFee.id === fee.id && 
            selectedFee.month === monthFull && 
            selectedFee.name === fee.name
          );
          
          if (existingIndex === -1) {
            this.selectedFees.push({
              id: fee.id,
              month: monthFull,
              name: fee.name,
              amount: amount,
              fine: effectiveFine,
              discount: effectiveDiscount,
              netAmount: amount + effectiveFine - effectiveDiscount
            });
            addedCount++;
          }
        }
      });
      
      if (addedCount > 0) {
        // Trigger onFeeBulkSelect callback
        if (typeof this.options.callbacks.onFeeBulkSelect === 'function') {
          this.options.callbacks.onFeeBulkSelect.call(this, {
            month: monthFull,
            count: addedCount
          });
        }
        
        this._loadSummaryTable();
      } else {
        this._showNotification(`All fees for ${month} are already added`);
      }
    }
    
    /**
     * Remove a fee from the summary
     * @private
     * @param {Number} index - Fee index in selectedFees array
     */
    _removeFeeFromSummary(index) {
      if (index >= 0 && index < this.selectedFees.length) {
        const removed = this.selectedFees.splice(index, 1)[0];
        
        // Trigger onFeeRemove callback
        if (typeof this.options.callbacks.onFeeRemove === 'function') {
          this.options.callbacks.onFeeRemove.call(this, removed);
        }
        
        this._loadSummaryTable();
      }
    }
    
    /**
     * Load summary table with selected fees
     * @private
     */
    _loadSummaryTable() {
      const summaryBody = document.getElementById('feeCollectSummaryBody');
      if (!summaryBody) return;
      
      summaryBody.innerHTML = '';
      
      this.selectedFees.forEach((fee, index) => {
        const row = document.createElement('tr');
        
        // S.No cell
        const snCell = document.createElement('td');
        snCell.textContent = index + 1;
        row.appendChild(snCell);
        
        // Fee Month cell
        const monthCell = document.createElement('td');
        monthCell.textContent = fee.month;
        row.appendChild(monthCell);
        
        // Fee Name cell
        const nameCell = document.createElement('td');
        nameCell.textContent = fee.name;
        row.appendChild(nameCell);
        
        // Fee Amount cell
        const amountCell = document.createElement('td');
        amountCell.textContent = fee.amount;
        row.appendChild(amountCell);
        
        // Only add fine/discount/net cells if showFineDiscount is true
        if (this.options.showFineDiscount) {
          // Fine cell
          const fineCell = document.createElement('td');
          fineCell.textContent = fee.fine || 0;
          if (fee.fine > 0) {
            fineCell.className = 'fee-collect-fine-cell';
          }
          row.appendChild(fineCell);
          
          // Discount cell
          const discountCell = document.createElement('td');
          discountCell.textContent = fee.discount || 0;
          if (fee.discount > 0) {
            discountCell.className = 'fee-collect-discount-cell';
          }
          row.appendChild(discountCell);
          
          // Net Amount cell
          const netAmountCell = document.createElement('td');
          const netAmount = (fee.amount || 0) + (fee.fine || 0) - (fee.discount || 0);
          netAmountCell.textContent = netAmount;
          netAmountCell.className = 'fee-collect-net-amount-cell';
          row.appendChild(netAmountCell);
        }
        
        // Cancel cell
        const cancelCell = document.createElement('td');
        const cancelBtn = document.createElement('button');
        cancelBtn.className = 'fee-collect-cancel-btn';
        cancelBtn.innerHTML = '&times;';
        cancelBtn.dataset.index = index;
        cancelBtn.addEventListener('click', () => this._removeFeeFromSummary(index));
        cancelCell.appendChild(cancelBtn);
        row.appendChild(cancelCell);
        
        summaryBody.appendChild(row);
      });
      
      // Update progress indicator
      if (this.options.showProgressBar) {
        this._updateProgressIndicator();
      }
      
      // Update month buttons status
      this._checkMonthStatus();
      
      // Update total amount in the payment summary
      this._updateTotalAmount();
    }
    
    /**
     * Update the progress indicator
     * @private
     */
    _updateProgressIndicator() {
      const progressIndicator = document.getElementById('feeCollectProgressIndicator');
      if (!progressIndicator) return;
      
      const totalPossibleFees = this.data.feeData.reduce((total, fee) => {
        return total + fee.amounts.filter(amount => amount > 0).length;
      }, 0);
      
      if (totalPossibleFees > 0) {
        const percentComplete = (this.selectedFees.length / totalPossibleFees) * 100;
        progressIndicator.style.width = `${percentComplete}%`;
      } else {
        progressIndicator.style.width = '0%';
      }
    }
    
    /**
     * Check if all fees for a month are already added
     * @private
     */
    _checkMonthStatus() {
      this.options.monthShortNames.forEach((month, monthIndex) => {
        const monthButton = this.container.querySelector(`.fee-collect-add-month[data-month="${month}"]`);
        if (!monthButton) return;
        
        let allFeesAdded = true;
        
        // Check if all fees for this month are in the summary
        this.data.feeData.forEach(fee => {
          const amount = fee.amounts[monthIndex];
          if (amount > 0) {
            const feeExists = this.selectedFees.some(selectedFee => 
              selectedFee.id === fee.id && 
              selectedFee.month === this.options.monthNames[monthIndex] && 
              selectedFee.name === fee.name
            );
            
            if (!feeExists) {
              allFeesAdded = false;
            }
          }
        });
        
        // Get totals from the monthly total cell
        const totalElement = document.getElementById(`feeCollectTotal-${month}`);
        let baseTotal = 0, fineTotal = 0, discountTotal = 0;
        
        if (totalElement) {
          baseTotal = parseInt(totalElement.dataset.baseTotal || 0);
          fineTotal = parseInt(totalElement.dataset.fineTotal || 0);
          discountTotal = parseInt(totalElement.dataset.discountTotal || 0);
        }
        
        // Update button appearance and text based on status
        if (allFeesAdded) {
          monthButton.textContent = "✓";
          monthButton.style.backgroundColor = "#999";
          monthButton.disabled = true;
          monthButton.title = "All fees for this month are already added";
        } else {
          // Show amount info on the button when not all fees are added
          if (baseTotal > 0) {
            // Check if there are fines or discounts and if they should be displayed
            const hasFineDiscount = (fineTotal > 0 || discountTotal > 0) && this.options.showFineDiscount;
            
            // If there are fines or discounts and showFineDiscount is true, show a badge
            if (hasFineDiscount) {
              monthButton.innerHTML = `+<span class="fee-collect-button-badge">${hasFineDiscount ? '*' : ''}</span>`;
            } else {
              monthButton.textContent = "+";
            }
            
            // Update tooltip to show more details
            let tooltipText = `Add all fees for ${month}: ${baseTotal}`;
            
            // Only include fine/discount in tooltip if showFineDiscount is true
            if (this.options.showFineDiscount) {
              if (fineTotal > 0) tooltipText += ` +${fineTotal}`;
              if (discountTotal > 0) tooltipText += ` -${discountTotal}`;
              tooltipText += ` = ${baseTotal + fineTotal - discountTotal}`;
            }
            
            monthButton.title = tooltipText;
          } else {
            monthButton.textContent = "+";
            monthButton.title = "Add all fees for this month";
          }
          
          monthButton.style.backgroundColor = "#00b050";
          monthButton.disabled = false;
        }
      });
    }
    
    /**
     * Calculate payment amounts
     * @private
     */
    _calculateAmounts() {
      // Get values from inputs
      const balance = parseInt(document.getElementById('feeCollectBalanceAmount').value) || 0;
      const total = parseInt(document.getElementById('feeCollectTotalAmount').value) || 0;
      const concession = parseInt(document.getElementById('feeCollectConcessionAmount').value) || 0;
      const lateFee = parseInt(document.getElementById('feeCollectLateFeeAmount').value) || 0;
      
      // Calculate final total (including balance)
      const finalTotal = total + balance;
      
      // Update final total display
      const finalTotalInput = document.getElementById('feeCollectFinalTotalAmount');
      if (finalTotalInput) {
        finalTotalInput.value = finalTotal;
      }
      
      // Calculate net amount and remaining
      const netAmount = finalTotal - concession + lateFee;
      
      // Get the received amount
      const receivedInput = document.getElementById('feeCollectReceivedAmount');
      const remainingInput = document.getElementById('feeCollectRemainAmount');
      
      // Check if we need to auto-update the received amount
      const shouldAutoUpdate = this.options.autoUpdateReceived && (
        // First time initialization (0 or empty)
        receivedInput.value === '0' || 
        receivedInput.value === '' || 
        // Or if the received value equals the previous net amount calculation
        // This ensures changing concession/late fee updates the received amount
        parseInt(receivedInput.value) === this._previousNetAmount ||
        // Or if remaining is 0, we always keep it at 0 by adjusting received
        parseInt(remainingInput.value) === 0
      );
      
      // Store current netAmount for future reference
      this._previousNetAmount = netAmount;
      
      if (shouldAutoUpdate) {
        // Update received to match the net amount
        receivedInput.value = netAmount;
        remainingInput.value = 0;
      } else {
        // Calculate remaining based on user-entered received value
        const received = parseInt(receivedInput.value) || 0;
        const remaining = netAmount - received;
        remainingInput.value = remaining;
      }
    }
    
    /**
     * Update total fee amount
     * @private
     */
    _updateTotalAmount() {
      let total = 0;
      let totalFines = 0;
      let totalDiscounts = 0;
      
      this.selectedFees.forEach(fee => {
        total += parseInt(fee.amount || 0);
        totalFines += parseInt(fee.fine || 0);
        totalDiscounts += parseInt(fee.discount || 0);
      });
      
      const totalInput = document.getElementById('feeCollectTotalAmount');
      if (totalInput) {
        totalInput.value = total;
      }
      
      // Update late fee field with total fines
      const lateFeeInput = document.getElementById('feeCollectLateFeeAmount');
      if (lateFeeInput) {
        lateFeeInput.value = totalFines;
      }
      
      // Update concession field with total discounts
      const concessionInput = document.getElementById('feeCollectConcessionAmount');
      if (concessionInput) {
        concessionInput.value = totalDiscounts;
      }
      
      // Store current values before recalculation
      const receivedInput = document.getElementById('feeCollectReceivedAmount');
      this._previousNetAmount = parseInt(receivedInput.value) || 0;
      
      // Calculate amounts - this will handle received amount logic and update final total
      this._calculateAmounts();
    }
    
    /**
     * Attach event listeners
     * @private
     */
    _attachEvents() {
      // Add click event for the add month buttons
      this.container.querySelectorAll('.fee-collect-add-month').forEach(button => {
        button.addEventListener('click', (event) => {
          if (button.disabled) {
            this._showNotification("All fees for this month are already added");
            return;
          }
          
          this._showCursorHighlight(button);
          this._addMonthFees(button.dataset.month);
        });
      });
      
      // Add click event for the header add button
      const headerAddButton = this.container.querySelector('.fee-collect-add-button');
      if (headerAddButton) {
        headerAddButton.addEventListener('click', (event) => {
          this._showCursorHighlight(headerAddButton);
          this._showNotification("Add old year balance functionality");
        });
      }
      
      // Add click event for the calendar icon
      const calendarIcon = this.container.querySelector('.fee-collect-calendar-icon');
      if (calendarIcon) {
        calendarIcon.addEventListener('click', (event) => {
          this._showCursorHighlight(calendarIcon);
          this._showNotification("Calendar functionality");
        });
      }
      
      // Add click event for payment method options
      this.container.querySelectorAll('input[name="feeCollectPaymentMethod"]').forEach(radio => {
        radio.addEventListener('change', (event) => {
          // Trigger onPaymentMethodChange callback
          if (typeof this.options.callbacks.onPaymentMethodChange === 'function') {
            this.options.callbacks.onPaymentMethodChange.call(this, radio.value);
          }
        });
      });
      
      // Set up payment amount calculations
      const balanceInput = document.getElementById('feeCollectBalanceAmount');
      if (balanceInput) {
        balanceInput.addEventListener('input', () => {
          this.balance = parseInt(balanceInput.value) || 0;
          this._calculateAmounts();
        });
      }
      
      const concessionInput = document.getElementById('feeCollectConcessionAmount');
      if (concessionInput) {
        concessionInput.addEventListener('input', () => this._calculateAmounts());
      }
      
      const lateFeeInput = document.getElementById('feeCollectLateFeeAmount');
      if (lateFeeInput) {
        lateFeeInput.addEventListener('input', () => this._calculateAmounts());
      }
      
      const receivedInput = document.getElementById('feeCollectReceivedAmount');
      if (receivedInput) {
        receivedInput.addEventListener('input', () => this._calculateAmounts());
      }
      
      // Add click events for action buttons
      const receiveButton = this.container.querySelector('.fee-collect-receive-button');
      if (receiveButton) {
        receiveButton.addEventListener('click', (event) => {
          this._showCursorHighlight(receiveButton);
          this._processPayment();
        });
      }
      
      const ledgerButton = this.container.querySelector('.fee-collect-ledger-button');
      if (ledgerButton) {
        ledgerButton.addEventListener('click', (event) => {
          this._showCursorHighlight(ledgerButton);
          this._showNotification("Opening ledger view...");
          
          // Trigger onLedgerOpen callback
          if (typeof this.options.callbacks.onLedgerOpen === 'function') {
            this.options.callbacks.onLedgerOpen.call(this);
          }
        });
      }
    }
    
    /**
     * Process payment
     * @private
     */
    _processPayment() {
      // Get all payment details
      const balanceAmount = document.getElementById('feeCollectBalanceAmount').value;
      const totalAmount = document.getElementById('feeCollectTotalAmount').value;
      const finalTotalAmount = document.getElementById('feeCollectFinalTotalAmount').value;
      const concession = document.getElementById('feeCollectConcessionAmount').value;
      const lateFee = document.getElementById('feeCollectLateFeeAmount').value;
      const received = document.getElementById('feeCollectReceivedAmount').value;
      const remaining = document.getElementById('feeCollectRemainAmount').value;
      const note = document.getElementById('feeCollectPaymentNote').value;
      
      const paymentMethodRadio = this.container.querySelector('input[name="feeCollectPaymentMethod"]:checked');
      const paymentMethod = paymentMethodRadio ? paymentMethodRadio.value : 'cash';
      
      // Prepare data for submission
      const paymentData = {
        fees: this.selectedFees,
        balance: balanceAmount,
        total: totalAmount,
        finalTotal: finalTotalAmount,
        concession: concession,
        lateFee: lateFee,
        received: received,
        remaining: remaining,
        note: note,
        paymentMethod: paymentMethod,
        receiptNumber: this.receiptNumber,
        date: this.currentDate
      };
      
      // Trigger onPaymentComplete callback
      if (typeof this.options.callbacks.onPaymentComplete === 'function') {
        this.options.callbacks.onPaymentComplete.call(this, paymentData);
      }
      
      this._showNotification("Fee received successfully!");
    }
    
    /**
     * Clear all selected fees
     * @public
     */
    clearSelectedFees() {
      this.selectedFees = [];
      this._loadSummaryTable();
      return this;
    }
    
    /**
     * Update fee data
     * @public
     * @param {Object} newData - New fee data
     */
    updateData(newData) {
      this.data = newData;
      this._loadFeeData();
      
      // Update balance if provided in new data
      if (newData.studentInfo && newData.studentInfo.balance !== undefined) {
        this.setBalance(newData.studentInfo.balance);
      }
      
      return this;
    }
    
    /**
     * Get current selected fees
     * @public
     * @returns {Array} - Selected fees
     */
    getSelectedFees() {
      return [...this.selectedFees];
    }
    
    /**
     * Get current payment details
     * @public
     * @returns {Object} - Payment details
     */
    getPaymentDetails() {
      const balanceAmount = document.getElementById('feeCollectBalanceAmount').value;
      const totalAmount = document.getElementById('feeCollectTotalAmount').value;
      const finalTotalAmount = document.getElementById('feeCollectFinalTotalAmount').value;
      const concession = document.getElementById('feeCollectConcessionAmount').value;
      const lateFee = document.getElementById('feeCollectLateFeeAmount').value;
      const received = document.getElementById('feeCollectReceivedAmount').value;
      const remaining = document.getElementById('feeCollectRemainAmount').value;
      const note = document.getElementById('feeCollectPaymentNote').value;
      
      const paymentMethodRadio = this.container.querySelector('input[name="feeCollectPaymentMethod"]:checked');
      const paymentMethod = paymentMethodRadio ? paymentMethodRadio.value : 'cash';
      
      return {
        balance: balanceAmount,
        total: totalAmount,
        finalTotal: finalTotalAmount,
        concession: concession,
        lateFee: lateFee,
        received: received,
        remaining: remaining,
        note: note,
        paymentMethod: paymentMethod,
        receiptNumber: this.receiptNumber,
        date: this.currentDate
      };
    }
    
    /**
     * Set the balance amount
     * @public
     * @param {Number} amount - Balance amount to set
     */
    setBalance(amount) {
      const balanceInput = document.getElementById('feeCollectBalanceAmount');
      if (balanceInput) {
        this.balance = parseInt(amount) || 0;
        balanceInput.value = this.balance;
        this._calculateAmounts();
      }
      return this;
    }
    
    /**
     * Get the balance amount
     * @public
     * @returns {Number} - Current balance amount
     */
    getBalance() {
      return this.balance;
    }
    
    /**
     * Set options
     * @public
     * @param {Object} options - New options
     */
    setOptions(options) {
      const prevShowFineDiscount = this.options.showFineDiscount;
      this.options = this._mergeOptions(this.options, options);
      
      // If showFineDiscount option changed, do more processing
      if (options.showFineDiscount !== undefined && options.showFineDiscount !== prevShowFineDiscount) {
        // Update selected fees to match the new option
        this.selectedFees.forEach(fee => {
          // If turning off fine/discount, reset those values in fee entries
          if (!this.options.showFineDiscount) {
            fee.fine = 0;
            fee.discount = 0;
            fee.netAmount = fee.amount;
          } else {
            // Try to retrieve original fine/discount from fee data
            const feeData = this.data.feeData.find(f => f.id === fee.id);
            if (feeData) {
              const monthIndex = this.options.monthNames.indexOf(fee.month);
              if (monthIndex >= 0) {
                // Restore original values from data
                const fine = (feeData.fines && feeData.fines[monthIndex]) || 0;
                const discount = (feeData.discounts && feeData.discounts[monthIndex]) || 0;
                fee.fine = fine;
                fee.discount = discount;
                fee.netAmount = fee.amount + fine - discount;
              }
            }
          }
        });
        
        // Re-render everything to reflect changes
        this._render();
      }
      
      return this;
    }
    
    /**
     * Destroy the component and clean up
     * @public
     */
    destroy() {
      // Clean up event listeners
      this._cleanupEventListeners();
      
      // Clear container
      this.container.innerHTML = '';
      this.container.classList.remove('fee-collect-container');
      
      return this;
    }
    
    /**
     * Validate payment data
     * @public
     * @param {Object} customRules - Custom validation rules
     * @returns {Object} - Validation result {valid: boolean, errors: Array}
     */
    validate(customRules = {}) {
      const errors = [];
      const paymentDetails = this.getPaymentDetails();
      
      // Check if any fees are selected
      if (this.selectedFees.length === 0) {
        errors.push('No fees selected');
      }
      
      // Check received amount
      const received = parseFloat(paymentDetails.received);
      if (isNaN(received) || received <= 0) {
        errors.push('Invalid received amount');
      }
      
      // Check payment method
      if (!paymentDetails.paymentMethod) {
        errors.push('Payment method is required');
      }
      
      // Check custom validation rules
      if (customRules.maxConcession) {
        const total = parseFloat(paymentDetails.total);
        const concession = parseFloat(paymentDetails.concession);
        if (concession > (total * customRules.maxConcession)) {
          errors.push(`Concession exceeds maximum allowed (${customRules.maxConcession * 100}%)`);
        }
      }
      
      return {
        valid: errors.length === 0,
        errors: errors
      };
    }
  }
  
  return FeeCollect;
}));
