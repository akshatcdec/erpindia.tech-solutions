/**
 * Complete fee-collection.js with all the improvements:
 * - Modal window for receipt viewing
 * - Loading indicator for iframe
 * - Confirmation dialog before saving payment
 * - Redirection after closing modal
 */

// Application state
const feeManager = {
    state: {
        addedFees: [],         // Tracks fees added to the payment
        feeCounter: 1,         // Counter for generating unique IDs for added fees
        schoolCode: 1,         // Default school code (should be set from server)
        hasUserModifiedReceived: false  // Flag to track if user manually changed the received amount
    },

    // Configuration constants
    config: {
        months: [
            { short: "Apr", long: "April" },
            { short: "May", long: "May" },
            { short: "Jun", long: "June" },
            { short: "Jul", long: "July" },
            { short: "Aug", long: "August" },
            { short: "Sep", long: "September" },
            { short: "Oct", long: "October" },
            { short: "Nov", long: "November" },
            { short: "Dec", long: "December" },
            { short: "Jan", long: "January" },
            { short: "Feb", long: "February" },
            { short: "Mar", long: "March" }
        ]
    },

    // Initialize the application
    init: function () {
        this.setupEventListeners();

        // Initialize Last Year Balance from the DOM if it exists
        const previousBalanceInput = document.getElementById('previous-balance');
        if (previousBalanceInput) {
            // Ensure it's treated as a number
            previousBalanceInput.value = parseFloat(previousBalanceInput.value) || 0;

            // Run initial calculation of totals to include the balance
            this.updateTotals();
        }

        // We don't need to check for paid months here since the CSHTML already 
        // adds the 'disabled' class and attribute to buttons with paid amounts

        console.log('Fee Manager initialized');
    },

    /**
     * Set up all event listeners for the UI
     */
    setupEventListeners: function () {
        // Add month buttons
        document.querySelectorAll('.add-month-btn').forEach(btn => {
            btn.addEventListener('click', this.handleAddMonthClick.bind(this));
        });

        // Click cell for adding all monthly fees
        const clickCell = document.querySelector('.click-cell');
        if (clickCell) {
            clickCell.addEventListener('click', this.handleAddAllMonthsClick.bind(this));
        }

        // Previous Balance input
        const previousBalanceInput = document.getElementById('previous-balance');
        if (previousBalanceInput) {
            previousBalanceInput.addEventListener('input', () => {
                previousBalanceInput.value = parseFloat(previousBalanceInput.value) || 0;
                this.updateTotals();
            });
        }

        // Discount amount input
        const discountInput = document.getElementById('discount-amount');
        if (discountInput) {
            discountInput.addEventListener('input', () => {
                discountInput.value = parseFloat(discountInput.value) || 0;
                this.updateTotals();
            });
        }

        // Late fee amount input
        const lateFeeInput = document.getElementById('late-fee-amount');
        if (lateFeeInput) {
            lateFeeInput.addEventListener('input', () => {
                lateFeeInput.value = parseFloat(lateFeeInput.value) || 0;
                this.updateTotals();
            });
        }

        // Received amount input
        const receivedInput = document.getElementById('received-amount');
        if (receivedInput) {
            receivedInput.addEventListener('input', () => {
                this.state.hasUserModifiedReceived = true;
                this.updateRemaining();
            });
        }

        // Additional Discount (Manual Concession) input
        const additionalDiscountInput = document.getElementById('additional-discount');
        if (additionalDiscountInput) {
            additionalDiscountInput.addEventListener('input', () => {
                additionalDiscountInput.value = parseFloat(additionalDiscountInput.value) || 0;
                this.updateTotals();
            });
        }

        // Other Charges input
        const otherChargesInput = document.getElementById('other-charges');
        if (otherChargesInput) {
            otherChargesInput.addEventListener('input', () => {
                otherChargesInput.value = parseFloat(otherChargesInput.value) || 0;
                this.updateTotals();
            });
        }

        // Payment method select
        const paymentMethodSelect = document.getElementById('paymentMethod');
        if (paymentMethodSelect) {
            paymentMethodSelect.addEventListener('change', () => {
                this.validatePayment(
                    parseFloat(document.getElementById('total-amount').value) || 0,
                    parseFloat(document.getElementById('received-amount').value) || 0
                );
            });
        }

        // Save payment button
        const saveBtn = document.getElementById('save-payment-btn');
        if (saveBtn) {
            saveBtn.addEventListener('click', this.handlePaymentSubmission.bind(this));
        }
    },

    /**
     * Handle adding a specific month's fees
     * FIXED: Properly calculate and apply the monthly discount
     */
    handleAddMonthClick: function (event) {
        event.preventDefault();

        const monthBtn = event.currentTarget;
        const month = monthBtn.dataset.month;

        // Skip if button is disabled
        if (monthBtn.disabled || monthBtn.classList.contains('disabled')) {
            this.showNotification(`Fees for ${month} have already been added or paid.`, 'warning');
            return;
        }

        // IMPORTANT: Get the correct values from the button data attributes
        const totalAmount = parseFloat(monthBtn.dataset.amount || 0);
        const totalDiscount = parseFloat(monthBtn.dataset.discount || 0);
        const finalAmount = parseFloat(monthBtn.dataset.final || 0);

        // Check if month has already been paid
        const paidAmount = parseFloat(monthBtn.dataset.paid || 0);
        if (paidAmount > 0) {
            this.showNotification(`Fees for ${month} have already been paid.`, 'warning');
            return;
        }

        // Select fee rows, excluding special rows
        const feeRows = document.querySelectorAll('#fee-details-body tr:not(.monthly-totals-row)');
        let feesAdded = 0;
        let totalRegularAmount = 0;

        // First pass: calculate the total regular amount to determine discount proportions
        feeRows.forEach(row => {
            const nameCell = row.querySelector('td:nth-child(2)');
            if (!nameCell) return;

            const feeName = nameCell.textContent.trim();
            if (!feeName || feeName === "Discount Monthly" || feeName === "Fixed Monthly Discount") return;

            const monthCell = row.querySelector(`td[data-month="${month}"]`);
            if (monthCell) {
                const feeAmountElement = monthCell.querySelector('.fee-amount');
                if (feeAmountElement) {
                    const amount = parseFloat(feeAmountElement.textContent);
                    if (amount > 0) {
                        totalRegularAmount += amount;
                    }
                }
            }
        });

        // Second pass: add fees with proportional discounts
        feeRows.forEach(row => {
            const nameCell = row.querySelector('td:nth-child(2)');
            if (!nameCell) return;

            const feeName = nameCell.textContent.trim();
            if (!feeName || feeName === "Discount Monthly" || feeName === "Fixed Monthly Discount") return;

            const feeHeadId = row.getAttribute('data-fee-id');
            const monthCell = row.querySelector(`td[data-month="${month}"]`);

            if (monthCell) {
                const feeAmountElement = monthCell.querySelector('.fee-amount');

                if (feeAmountElement) {
                    const regularAmount = parseFloat(feeAmountElement.textContent);

                    // Check if fee already added
                    const isDuplicate = this.state.addedFees.some(fee =>
                        fee.name === feeName && fee.month === month
                    );

                    if (!isDuplicate && regularAmount > 0) {
                        // Calculate proportional discount for this fee
                        let proportionalDiscount = 0;

                        if (totalRegularAmount > 0 && totalDiscount > 0) {
                            proportionalDiscount = (regularAmount / totalRegularAmount) * totalDiscount;
                        }

                        // Always ensure discount is not more than the fee amount
                        proportionalDiscount = Math.min(proportionalDiscount, regularAmount);

                        // Calculate the final amount after discount
                        const finalFeeAmount = regularAmount - proportionalDiscount;

                        // Add fee to the table with the calculated discount
                        this.addFeeToTable(feeName, month, regularAmount, proportionalDiscount, finalFeeAmount, feeHeadId);
                        feesAdded++;
                    }
                }
            }
        });

        if (feesAdded === 0) {
            this.showNotification(`No fees to add for ${month} or all fees already added.`, 'warning');
        } else {
            this.showNotification(`Added ${feesAdded} fees for ${month}.`, 'success');

            // Check if payment button should be enabled after adding fees
            this.validatePayment(
                parseFloat(document.getElementById('total-amount').value) || 0,
                parseFloat(document.getElementById('received-amount').value) || 0
            );
        }
    },

    /**
     * Handle adding all available months' fees
     */
    handleAddAllMonthsClick: function (event) {
        event.preventDefault();

        // Get all available month buttons (not disabled)
        const availableMonthButtons = Array.from(
            document.querySelectorAll('.add-month-btn:not(.disabled)')
        );

        if (availableMonthButtons.length === 0) {
            this.showNotification('All fees have already been added or paid.', 'warning');
            return;
        }

        // Click each available month button
        let totalAdded = 0;
        availableMonthButtons.forEach(button => {
            // Skip months with zero final amount
            const finalAmount = parseFloat(button.dataset.final || 0);
            if (finalAmount <= 0) return;

            // Skip months that have been paid
            const paidAmount = parseFloat(button.dataset.paid || 0);
            if (paidAmount > 0) return;

            // Simulate a click on this month button
            button.click();
            totalAdded++;
        });

        if (totalAdded === 0) {
            this.showNotification('No additional fees available to add.', 'info');
        }

        // Check if payment button should be enabled after adding fees
        this.validatePayment(
            parseFloat(document.getElementById('total-amount').value) || 0,
            parseFloat(document.getElementById('received-amount').value) || 0
        );
    },

    /**
     * Add a fee to the added fees table
     * FIXED: Added missing parameters (discount, finalAmount) and correct table row creation
     */
    addFeeToTable: function (feeName, month, regularAmount, discount, finalAmount, feeHeadId) {
        const tableBody = document.getElementById('added-fees-body');
        if (!tableBody) {
            console.error('added-fees-body element not found');
            return;
        }

        // Check for duplicates
        const isDuplicate = this.state.addedFees.some(fee =>
            fee.name === feeName && fee.month === month
        );

        if (isDuplicate) {
            this.showNotification(`${feeName} for ${month} is already added.`, 'warning');
            return;
        }

        // Create a new row
        const newRow = document.createElement('tr');
        // Important: Use data-fee-id attribute for selection
        newRow.setAttribute('data-fee-id', this.state.feeCounter);
        newRow.innerHTML = `
            <td>${this.state.feeCounter}</td>
            <td>${month}</td>
            <td>${feeName}</td>
            <td>${regularAmount.toFixed(2)}</td>
            <td>
                <button type="button" class="btn-cancel" data-id="${this.state.feeCounter}" data-month="${month}" aria-label="Remove fee">
                    <i class="fas fa-times"></i>
                </button>
            </td>
        `;

        // Store the feeHeadId as a data attribute
        newRow.setAttribute('data-fee-head-id', feeHeadId);

        // Add to the table
        tableBody.appendChild(newRow);

        // Add to the state
        this.state.addedFees.push({
            id: this.state.feeCounter,
            month: month,
            name: feeName,
            regularAmount: regularAmount,
            discount: discount,
            finalAmount: finalAmount,
            feeHeadId: feeHeadId
        });

        // Increment counter
        this.state.feeCounter++;

        // Set up the cancel button
        const cancelBtn = newRow.querySelector('.btn-cancel');
        cancelBtn.addEventListener('click', this.handleRemoveFee.bind(this));

        // Update totals
        this.updateTotals();

        // Initialize received amount only when adding the first fee (if user hasn't modified it)
        if (this.state.addedFees.length === 1 && !this.state.hasUserModifiedReceived) {
            this.initializeReceivedAmount();
        }

        // Check if all fees for this month are added
        this.checkMonthCompletionStatus(month);
    },

    /**
     * Handle removing a fee
     */
    handleRemoveFee: function (event) {
        const button = event.currentTarget;
        const month = button.dataset.month;

        // Remove all fees for this month
        this.removeAllFeesForMonth(month);
    },

    /**
     * Remove all fees for a specific month
     */
    removeAllFeesForMonth: function (month) {
        console.log(`Removing all fees for month: ${month}`);

        // Find all fees for this month
        const monthFees = this.state.addedFees.filter(fee => fee.month === month);
        console.log(`Found ${monthFees.length} fees to remove for month ${month}:`, monthFees);

        if (monthFees.length === 0) {
            console.warn(`No fees found for month ${month}`);
            return;
        }

        // Get all the IDs that need to be removed
        const feeIdsToRemove = monthFees.map(fee => fee.id);
        console.log(`Fee IDs to remove: ${feeIdsToRemove.join(', ')}`);

        // Remove all rows from the DOM
        feeIdsToRemove.forEach(id => {
            // Find the row using a more specific selector
            const rowToRemove = document.querySelector(`#added-fees-body tr[data-fee-id="${id}"]`);
            if (rowToRemove) {
                console.log(`Removing row for fee ID ${id}`);
                rowToRemove.remove();
            } else {
                console.warn(`Row for fee ID ${id} not found in the DOM`);
            }
        });

        // Filter the state to remove these fees
        const originalLength = this.state.addedFees.length;
        this.state.addedFees = this.state.addedFees.filter(fee => fee.month !== month);

        // Check if fees were actually removed
        const newLength = this.state.addedFees.length;
        const removedCount = originalLength - newLength;
        console.log(`Removed ${removedCount} fees from state. Original: ${originalLength}, New: ${newLength}`);

        // Update totals
        this.updateTotals();

        // Re-enable the month button
        this.enableMonthButton(month);

        // Update row numbers after deletion
        this.updateRowNumbers();

        // Validate payment after removing fees
        this.validatePayment(
            parseFloat(document.getElementById('total-amount').value) || 0,
            parseFloat(document.getElementById('received-amount').value) || 0
        );

        // Show notification
        this.showNotification(`All fees for ${month} have been removed.`, 'info');
    },

    /**
     * Update row numbers in the table after deletion
     */
    updateRowNumbers: function () {
        const rows = document.querySelectorAll('#added-fees-body tr');

        rows.forEach((row, index) => {
            const numberCell = row.querySelector('td:first-child');
            if (numberCell) {
                numberCell.textContent = index + 1;
            }
        });
    },

    /**
     * Initialize the received amount field when adding the first fee
     */
    initializeReceivedAmount: function () {
        if (this.state.hasUserModifiedReceived) return;

        const receivedElement = document.getElementById('received-amount');
        const totalElement = document.getElementById('total-amount');

        if (receivedElement && totalElement) {
            const totalValue = parseFloat(totalElement.value) || 0;
            receivedElement.value = totalValue.toFixed(2);
            this.updateRemaining();
        }
    },

    /**
     * Check if all fees for a month are added and update UI accordingly
     */
    checkMonthCompletionStatus: function (month) {
        // FIX: Improve the fee row selection to exclude special rows
        const feeRows = Array.from(document.querySelectorAll('#fee-details-body tr:not(.monthly-totals-row)'))
            .filter(row => {
                const nameCell = row.querySelector('td:nth-child(2)');
                return nameCell && nameCell.textContent.trim() !== "Discount Monthly" && nameCell.textContent.trim() !== "Fixed Monthly Discount";
            });

        let availableFees = 0;
        let addedFees = 0;

        // Count available and added fees
        feeRows.forEach(row => {
            const nameCell = row.querySelector('td:nth-child(2)');
            if (!nameCell) return;

            const feeName = nameCell.textContent.trim();
            const monthCell = row.querySelector(`td[data-month="${month}"]`);

            if (monthCell) {
                const feeAmountElement = monthCell.querySelector('.fee-amount');
                if (feeAmountElement && parseFloat(feeAmountElement.textContent) > 0) {
                    availableFees++;

                    // Check if already added
                    if (this.state.addedFees.some(fee => fee.name === feeName && fee.month === month)) {
                        addedFees++;
                    }
                }
            }
        });

        // Update button state
        if (availableFees > 0 && addedFees >= availableFees) {
            this.disableMonthButton(month);
        }
    },

    /**
     * Disable the add button for a month
     */
    disableMonthButton: function (month) {
        const button = document.querySelector(`.add-month-btn[data-month="${month}"]`);
        if (button) {
            button.disabled = true;
            button.classList.add('disabled');

            const icon = button.querySelector('i');
            if (icon) {
                icon.classList.remove('fa-plus-square');
                icon.classList.add('fa-check-square');
            }
        }
    },

    /**
     * Enable the add button for a month
     */
    enableMonthButton: function (month) {
        // Don't enable if the month was already paid
        const button = document.querySelector(`.add-month-btn[data-month="${month}"]`);
        if (button) {
            // Check if this month has a payment already
            const paidAmount = parseFloat(button.dataset.paid || 0);
            if (paidAmount > 0) {
                // Don't enable paid months
                return;
            }

            button.disabled = false;
            button.classList.remove('disabled');

            const icon = button.querySelector('i');
            if (icon) {
                icon.classList.remove('fa-check-square');
                icon.classList.add('fa-plus-square');
            }
        }
    },

    /**
     * Update all total calculations
     * FIXED: Properly sum and display ALL discounts in the main discount field
     * ADDED: Update span elements with values from text boxes
     * ADDED: Include additional discount and other charges in calculations
     */
    updateTotals: function () {
        // Sum of all added fees (original amount before any discounts)
        const feeOriginalTotal = this.state.addedFees.reduce((sum, fee) => sum + fee.regularAmount, 0);

        // Sum of ALL individual discounts applied to specific fees
        const individualDiscounts = this.state.addedFees.reduce((sum, fee) => sum + fee.discount, 0);
        console.log("Total individual discounts calculated: " + individualDiscounts);

        // Sum of all added fees (final amount after individual discounts)
        const feeTotal = this.state.addedFees.reduce((sum, fee) => sum + fee.finalAmount, 0);

        // Get previous balance
        const previousBalanceElement = document.getElementById('previous-balance');
        const previousBalance = previousBalanceElement ? parseFloat(previousBalanceElement.value) || 0 : 0;

        // Get late fee
        const lateFeeElement = document.getElementById('late-fee-amount');
        const lateFee = lateFeeElement ? parseFloat(lateFeeElement.value) || 0 : 0;

        // Get additional discount (manual concession)
        const additionalDiscountElement = document.getElementById('additional-discount');
        const additionalDiscount = additionalDiscountElement ? parseFloat(additionalDiscountElement.value) || 0 : 0;

        // Get other charges
        const otherChargesElement = document.getElementById('other-charges');
        const otherCharges = otherChargesElement ? parseFloat(otherChargesElement.value) || 0 : 0;

        // Get the discount field
        const discountElement = document.getElementById('discount-amount');

        // CRITICAL FIX: Always update the discount amount field with the total of all individual discounts
        // This ensures the discount field always shows the correct total discount amount
        if (discountElement) {
            // Set the discount field to show the total of all individual discounts
            discountElement.value = individualDiscounts.toFixed(2);
        }

        // Get the final discount value for calculations (auto + additional)
        const totalDiscount = individualDiscounts + additionalDiscount;

        // Calculate subtotal (original fees + previous balance + late fee + other charges)
        const subtotal = feeOriginalTotal + previousBalance + lateFee + otherCharges;

        // Calculate total (subtotal - total discount)
        const total = Math.max(0, subtotal - totalDiscount);

        // Update UI elements
        this.updateElementValue('subtotal-amount', subtotal.toFixed(2));
        this.updateElementValue('total-amount', total.toFixed(2));

        // If user hasn't manually modified the received amount, update it to match total
        if (!this.state.hasUserModifiedReceived) {
            this.updateElementValue('received-amount', total.toFixed(2));
        }

        // Update span elements with values from text boxes
        this.updateSpanElement('spnOldBalance', previousBalance.toFixed(2));
        this.updateSpanElement('spnDiscount', individualDiscounts.toFixed(2)); // Auto discount
        this.updateSpanElement('spnAddDiscount', additionalDiscount.toFixed(2)); // Added manual concession
        this.updateSpanElement('spnOtherCharges', otherCharges.toFixed(2)); // Other charges
        this.updateSpanElement('spnLateFee', lateFee.toFixed(2));
        this.updateSpanElement('spnSubTotal', subtotal.toFixed(2));
        this.updateSpanElement('spnTotal', total.toFixed(2));

        // Update remaining calculation
        this.updateRemaining();
    },

    /**
     * Update remaining balance calculation
     */
    updateRemaining: function () {
        const totalElement = document.getElementById('total-amount');
        const receivedElement = document.getElementById('received-amount');
        const remainElement = document.getElementById('remain-amount');

        if (totalElement && receivedElement && remainElement) {
            const total = parseFloat(totalElement.value) || 0;
            const received = parseFloat(receivedElement.value) || 0;

            // Check if received is greater than total
            if (received > total) {
                this.showNotification('Received amount cannot exceed total amount.', 'warning');
                // Set received to equal total
                receivedElement.value = total.toFixed(2);
                remainElement.value = "0.00";
            } else {
                // Calculate remaining normally
                const remaining = Math.max(0, total - received);
                remainElement.value = remaining.toFixed(2);
            }

            // Validate if the payment is valid
            this.validatePayment(total, parseFloat(receivedElement.value) || 0);
        }
    },

    /**
     * Validate payment amounts and enable/disable save button accordingly
     * FIXED: Enable the button properly when fees are added and payment method is selected
     */
    validatePayment: function (totalAmount, receivedAmount) {
        const saveButton = document.getElementById('save-payment-btn');
        if (!saveButton) return false;

        const previousBalanceElement = document.getElementById('previous-balance');
        const previousBalance = previousBalanceElement ? parseFloat(previousBalanceElement.value) || 0 : 0;
        const remainingElement = document.getElementById('remain-amount');
        const remaining = remainingElement ? parseFloat(remainingElement.value) || 0 : 0;
        const paymentMethodElement = document.getElementById('paymentMethod');
        const paymentMethod = paymentMethodElement ? paymentMethodElement.value : '';

        console.log("Validating payment: Fees:", this.state.addedFees.length, "Total:", totalAmount, "Received:", receivedAmount, "Method:", paymentMethod);

        // If there's nothing to pay yet (no fees added and no old balance), disable the button
        if (totalAmount === 0 && this.state.addedFees.length === 0 && previousBalance === 0) {
            saveButton.disabled = true;
            console.log("Payment button disabled: No fees or balance");
            return false;
        }

        // If we have added fees or previous balance
        if (this.state.addedFees.length > 0 || previousBalance > 0) {
            // If received amount is 0, we can save (this will just save the fees without payment)
            if (receivedAmount === 0) {
                saveButton.disabled = false;
                console.log("Payment button enabled: Has fees but no payment (saving fee record only)");
                return true;
            }

            // If received amount is greater than 0, require payment method
            if (receivedAmount > 0) {
                if (!paymentMethod) {
                    saveButton.disabled = true;
                    console.log("Payment button disabled: Received amount but no payment method");
                    return false;
                } else {
                    // If we have both received amount and payment method, enable button
                    saveButton.disabled = false;
                    console.log("Payment button enabled: Has fees, received amount, and payment method");
                    return true;
                }
            }
        }

        // Default to disable if none of the conditions are met
        saveButton.disabled = true;
        console.log("Payment button disabled: Default case");
        return false;
    },

    /**
     * Handle payment submission with confirmation
     */
    handlePaymentSubmission: function (event) {
        event.preventDefault();

        // Get payment details
        const totalAmount = parseFloat(document.getElementById('total-amount').value) || 0;
        const receivedAmount = parseFloat(document.getElementById('received-amount').value) || 0;
        const paymentMethod = document.getElementById('paymentMethod').value;

        // Basic validations
        if (this.state.addedFees.length === 0 && receivedAmount <= 0) {
            this.showNotification('Please add at least one fee or provide a payment amount.', 'error');
            return;
        }

        if (receivedAmount > 0 && !paymentMethod) {
            this.showNotification('Please select a payment method.', 'warning');
            return;
        }

        // Show confirmation dialog before proceeding
        this.showConfirmationDialog(
            'Save Fee Payment',
            'Are you sure you want to save this fee payment?',
            () => {
                // User confirmed - proceed with payment
                this.processPayment();
            }
        );
    },

    /**
     * Process payment after confirmation
     */
    processPayment: function () {
        // Collect all payment details
        const paymentDetails = this.collectPaymentDetails();

        // Show processing state
        const submitButton = document.getElementById('save-payment-btn');
        if (submitButton) {
            submitButton.disabled = true;
            submitButton.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i> Processing...';
        }

        // Submit payment to server
        this.submitPaymentToServer(paymentDetails, submitButton);
    },

    /**
     * Show confirmation dialog
     */
    showConfirmationDialog: function (title, message, confirmCallback) {
        // Create modal elements
        const modalOverlay = document.createElement('div');
        modalOverlay.className = 'modal-overlay';

        const modalContainer = document.createElement('div');
        modalContainer.className = 'modal-container';

        // Create modal content
        modalContainer.innerHTML = `
            <div class="modal-header">
                <h5>${title}</h5>
            </div>
            <div class="modal-body">
                <p>${message}</p>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" id="cancel-btn">Cancel</button>
                <button type="button" class="btn btn-primary" id="confirm-btn">Yes, Save</button>
            </div>
        `;

        // Add modal to document
        modalOverlay.appendChild(modalContainer);
        document.body.appendChild(modalOverlay);

        // Add event listeners
        document.getElementById('confirm-btn').addEventListener('click', () => {
            // Remove modal
            document.body.removeChild(modalOverlay);

            // Execute the callback function
            if (typeof confirmCallback === 'function') {
                confirmCallback();
            }
        });

        document.getElementById('cancel-btn').addEventListener('click', () => {
            // Just close the modal without doing anything
            document.body.removeChild(modalOverlay);
        });
    },

    /**
     * Collect all payment details for submission
     */
    collectPaymentDetails: function () {
        // Get the auto discount amount (sum of all individual discounts)
        const autoDiscount = this.state.addedFees.reduce((sum, fee) => sum + fee.discount, 0);

        // Get additional discount (manual concession)
        const additionalDiscount = parseFloat(document.getElementById('additional-discount').value) || 0;

        // Get other charges
        const otherCharges = parseFloat(document.getElementById('other-charges').value) || 0;

        // Calculate transport related fields
        const transportFees = this.state.addedFees.filter(fee =>
            fee.name.toLowerCase().includes('transport'));

        const totalTransport = transportFees.reduce((sum, fee) => sum + fee.finalAmount, 0);

        // Find the last month for which fees are being paid
        const months = this.config.months.map(m => m.short);
        const addedMonths = this.state.addedFees.map(fee => fee.month);
        const lastDepositMonth = addedMonths.length > 0 ?
            addedMonths.sort((a, b) => months.indexOf(b) - months.indexOf(a))[0] : "";

        // Get SMS preferences
        const sendHindiSMS = document.getElementById('chkHindi').checked;
        const sendEnglishSMS = document.getElementById('chkEng').checked;

        // Get payment method
        const paymentMethod = document.getElementById('paymentMethod').value;

        return {
            studentId: document.getElementById('hiddenStudentId').value,
            SessionID: document.getElementById('hiddenSessionID').value,
            TenantID: document.getElementById('hiddenTenantID').value,
            TenantCode: document.getElementById('hiddenTenantCode').value,
            SessionYear: document.getElementById('hiddenSessionYear').value,
            admissionNo: document.getElementById('hiddenAdmNo').value,
            schoolCode: this.state.schoolCode,
            addedFees: this.state.addedFees.map(fee => ({
                id: fee.id,
                month: fee.month,
                name: fee.name,
                regularAmount: fee.regularAmount,
                // Don't send individual discount to database, only for display
                finalAmount: fee.finalAmount,
                feeHeadId: fee.feeHeadId
            })),
            previousBalance: parseFloat(document.getElementById('previous-balance').value) || 0,
            autoDiscount: autoDiscount,
            additionalDiscount: additionalDiscount,
            otherCharges: otherCharges,
            subtotal: parseFloat(document.getElementById('subtotal-amount').value) || 0,
            lateFee: parseFloat(document.getElementById('late-fee-amount').value) || 0,
            total: parseFloat(document.getElementById('total-amount').value) || 0,
            received: parseFloat(document.getElementById('received-amount').value) || 0,
            remaining: parseFloat(document.getElementById('remain-amount').value) || 0,
            lastDepositMonth: lastDepositMonth,
            totalTransport: totalTransport,
            transportRoute: document.getElementById('hiddenpickupname').value, // Could be populated if you have this data available
            transportAmount: totalTransport,
            note: document.getElementById('note').value,
            receiptDate: document.getElementById('receipt-date').value,
            paymentMethod: paymentMethod,
            sendHindiSMS: sendHindiSMS,
            sendEnglishSMS: sendEnglishSMS
        };
    },

    /**
     * Submit payment data to the server
     */
    submitPaymentToServer: function (paymentDetails, submitButton) {
        // Prepare API-friendly fee items without individual discounts
        const formattedFees = paymentDetails.addedFees.map(fee => ({
            Month: fee.month,
            FeeName: fee.name,
            RegularAmount: fee.regularAmount,
            // Skip sending individual Discount to database
            FinalAmount: fee.finalAmount,
            FeeHeadId: fee.feeHeadId
        }));

        // Prepare the data for our SQL database structure - matching server model fields
        const paymentData = {
            // Main receipt data for FeeReceivedTbl
            SessionID: paymentDetails.SessionID,
            TenantID: paymentDetails.TenantID,
            TenantCode: paymentDetails.TenantCode,
            SessionYear: paymentDetails.SessionYear,
            StudentID: paymentDetails.studentId,
            SchoolCode: paymentDetails.TenantCode, // Using TenantCode as SchoolCode per your example
            AdmissionNo: paymentDetails.admissionNo,
            OldBalance: paymentDetails.previousBalance,

            // FIXED: Map properly to C# model fields
            LateFee: paymentDetails.otherCharges, // Using other-charges for LateFee
            LateFeeAuto: paymentDetails.lateFee,  // Using late-fee-amount for LateFeeAuto

            // Fix FeeAdded calculation
            FeeAdded: paymentDetails.subtotal - paymentDetails.previousBalance - paymentDetails.lateFee - paymentDetails.otherCharges,

            ConcessinAuto: paymentDetails.autoDiscount, // Automatic discount from fee structure
            ConcessinMannual: paymentDetails.additionalDiscount, // Manual concession added by user

            TotalFee: paymentDetails.total,
            Received: paymentDetails.received,
            Remain: paymentDetails.remaining,
            LastDepositMonth: paymentDetails.lastDepositMonth,
            TotalTransport: paymentDetails.totalTransport,
            TransportRoute: paymentDetails.transportRoute,
            TransportAmount: paymentDetails.transportAmount,
            Note1: paymentDetails.note,
            Note2: '',
            EntryTime: paymentDetails.receiptDate,
            PaymentMode: paymentDetails.paymentMethod,
            UserId: 1, // Default user ID as shown in the C# model
            SendHindiSMS: paymentDetails.sendHindiSMS,
            SendEnglishSMS: paymentDetails.sendEnglishSMS,

            // Fees array for FeeMonthlyFeeTbl (without individual discounts)
            MonthlyFees: formattedFees,

            // Transport fees array for FeeTransportFeeTbl (if applicable)
            TransportFees: formattedFees.filter(fee => fee.FeeName.toLowerCase().includes('transport'))
        };

        console.log("Submitting payment data:", paymentData);

        // For debugging
        console.log("Added Fees:", this.state.addedFees);
        console.log("Auto Discount:", paymentData.ConcessinAuto);
        console.log("Manual Concession:", paymentData.ConcessinMannual);
        console.log("Last Deposit Month:", paymentData.LastDepositMonth);
        console.log("Total Transport:", paymentData.TotalTransport);
        console.log("LateFee (other-charges):", paymentData.LateFee);
        console.log("LateFeeAuto (late-fee-amount):", paymentData.LateFeeAuto);

        // Send data to server
        fetch('/CollectFee/SubmitPayment', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify(paymentData)
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Network response was not ok');
                }
                return response.json();
            })
            .then(data => {
                // Reset button state
                if (submitButton) {
                    submitButton.disabled = false;
                    submitButton.innerHTML = '<i class="fas fa-save me-1"></i> Save Payment';
                }

                if (data.success) {
                    // Show success and prompt for receipt
                    this.showNotification('Payment successfully processed! Receipt: ' + data.receiptNumber, 'success');
                    this.promptToPrintReceiptAndRefresh(data.receiptNumber, paymentData.TenantCode);
                } else {
                    // Show error message
                    this.showNotification('Error: ' + (data.message || 'Unknown error'), 'error');
                }
            })
            .catch(error => {
                console.error('Error submitting payment:', error);

                // Reset button state
                if (submitButton) {
                    submitButton.disabled = false;
                    submitButton.innerHTML = '<i class="fas fa-save me-1"></i> Save Payment';
                }

                // Show error message
                this.showNotification('An error occurred while processing the payment. Please try again.', 'error');
            });
    },

    /**
     * Show a modal to ask user about printing the receipt
     */
    promptToPrintReceiptAndRefresh: function (receiptNumber, TenantCode) {
        // Create modal elements
        const modalOverlay = document.createElement('div');
        modalOverlay.className = 'modal-overlay';

        const modalContainer = document.createElement('div');
        modalContainer.className = 'modal-container';

        // Create modal content
        modalContainer.innerHTML = `
            <div class="modal-header">
                <h5>Payment Processed Successfully</h5>
            </div>
            <div class="modal-body">
                <p>Receipt ${receiptNumber} has been generated.</p>
                <p>Would you like to view the receipt?</p>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" id="print-no-btn">No</button>
                <button type="button" class="btn btn-primary" id="print-yes-btn">Yes, View Receipt</button>
            </div>
        `;

        // Add modal to document
        modalOverlay.appendChild(modalContainer);
        document.body.appendChild(modalOverlay);

        // Add event listeners
        document.getElementById('print-yes-btn').addEventListener('click', () => {
            // Open receipt in modal instead of new tab
            this.openReceiptInModal(receiptNumber, TenantCode);

            // Remove confirmation modal
            document.body.removeChild(modalOverlay);
        });

        document.getElementById('print-no-btn').addEventListener('click', () => {
            // Close the modal and refresh page
            document.body.removeChild(modalOverlay);
            window.location.href = "/CollectFee/Index";
        });
    },

    /**
     * Open receipt in a modal window instead of new tab
     */
    openReceiptInModal: function (receiptNumber, TenantCode) {
        // Create modal for the receipt
        const receiptModalOverlay = document.createElement('div');
        receiptModalOverlay.className = 'receipt-modal-overlay';
        receiptModalOverlay.id = 'receipt-modal-overlay';

        // Create modal container with iframe and loading indicator - NO FOOTER
        receiptModalOverlay.innerHTML = `
            <div class="receipt-modal-container">
                <div style="padding:10px;">
                    <h4>Receipt ${receiptNumber}</h4>
                    <button type="button" class="btn-close-receipt" style="position: absolute; top: 10px; right: 10px; color: red;" aria-label="Close receipt">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
                <div class="receipt-modal-body">
                    <button type="button" class="btn-close-receipt" aria-label="Close receipt">
                        <i class="fas fa-times"></i>
                    </button>
                    <div id="iframe-loading" class="iframe-loading">
                        <div class="spinner-border text-primary" role="status">
                            <span class="visually-hidden">Loading...</span>
                        </div>
                        <p>Loading receipt...</p>
                    </div>
                    <iframe src="/CollectFee/Receipt1?receiptNumber=${receiptNumber}&code=${TenantCode}" 
                            id="receipt-iframe" 
                            frameborder="0" 
                            width="100%" 
                            height="100%"
                            style="opacity: 0; transition: opacity 0.3s;">
                    </iframe>
                </div>
            </div>
        `;

        // Add to document
        document.body.appendChild(receiptModalOverlay);

        // Prevent body scrolling when modal is open
        document.body.style.overflow = 'hidden';

        // Get the iframe element
        const iframe = document.getElementById('receipt-iframe');

        // Add loading indicator handling
        if (iframe) {
            iframe.onload = function () {
                // Hide the loading indicator and show the iframe
                document.getElementById('iframe-loading').style.display = 'none';
                iframe.style.opacity = '1';
            };
        }

        // Add event listener with proper binding
        const closeButton = document.querySelector('.btn-close-receipt');
        if (closeButton) {
            closeButton.addEventListener('click', this.closeReceiptModal.bind(this));
        }
    },

    /**
     * Close receipt modal and redirect to fee collection page
     */
    closeReceiptModal: function () {
        // Find the modal overlay
        const modal = document.getElementById('receipt-modal-overlay');

        if (modal) {
            // Add a fade-out class
            modal.classList.add('modal-closing');

            // Wait for animation to complete then remove and redirect
            setTimeout(() => {
                if (document.body.contains(modal)) {
                    document.body.removeChild(modal);
                }
                // Restore scrolling
                document.body.style.overflow = '';

                // Redirect to fee collection page
                window.location.href = "/CollectFee/Index";
            }, 300);
        } else {
            // Fallback if modal not found
            document.body.style.overflow = '';
            // Redirect to fee collection page
            window.location.href = "/CollectFee/Index";
        }
    },

    /**
     * Helper function to update an element's value
     */
    updateElementValue: function (elementId, value) {
        const element = document.getElementById(elementId);
        if (element) {
            element.value = value;
        }
    },

    /**
     * Helper function to update a span element's text content
     */
    updateSpanElement: function (elementId, value) {
        const element = document.getElementById(elementId);
        if (element) {
            element.textContent = value;
        }
    },

    /**
     * Show a notification message
     */
    showNotification: function (message, type = 'info') {
        // Create notification element
        const notificationContainer = document.createElement('div');
        notificationContainer.className = `notification notification-${type}`;

        // Set icon based on notification type
        let icon = 'info-circle';
        if (type === 'success') icon = 'check-circle';
        if (type === 'warning') icon = 'exclamation-triangle';
        if (type === 'error') icon = 'times-circle';

        // Set notification content
        notificationContainer.innerHTML = `
            <div class="notification-icon">
                <i class="fas fa-${icon}"></i>
            </div>
            <div class="notification-message">${message}</div>
            <button class="notification-close">
                <i class="fas fa-times"></i>
            </button>
        `;

        // Add notification to document
        document.body.appendChild(notificationContainer);

        // Add event listener to close button
        const closeButton = notificationContainer.querySelector('.notification-close');
        if (closeButton) {
            closeButton.addEventListener('click', function () {
                document.body.removeChild(notificationContainer);
            });
        }

        // Auto-remove notification after a delay
        setTimeout(() => {
            if (document.body.contains(notificationContainer)) {
                document.body.removeChild(notificationContainer);
            }
        }, 5000);
    }
};

/**
 * Global updateTotals function to handle the inline 'onblur' attribute in the HTML
 * This is needed because your HTML has a direct reference to updateTotals
 */
function updateTotals() {
    // Call the feeManager's updateTotals method
    feeManager.updateTotals();
}

// Initialize when the DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    feeManager.init();
});