document.addEventListener('DOMContentLoaded', function() {
    // Initialize state
    const state = {
        studentId: document.getElementById('hiddenStudentId').value, // This would come from URL parameters or previous page
        feeDetails: [],
        feeSummary: {},
        addedFees: [],
        feeCounter: 1
    };

    // Initialize the page
    initializePage();

    // Initialize the page by loading all required data
    async function initializePage() {
        try {
            // Set today's date in the receipt date field
            document.getElementById('receipt-date').valueAsDate = new Date();
            
            // Load all data asynchronously
            await Promise.all([
                loadStudentInfo(),
                loadFeeDetails(),
                loadFeeSummary(),
                loadPaymentMethods(),
               loadReceiptTemplates()
            ]);
            
            // Update the UI
            updateTotals();
            
            // Add event listeners
            //setupEventListeners();
        } catch (error) {
            console.error('Error initializing page:', error);
            alert('Failed to load data. Please try again later.');
        }
    }

    // Load student information
    async function loadStudentInfo() {
        try {
            const studentInfo = await API.getStudentInfo(state.studentId);
            // Update student info in the UI
            const studentInfoContainer = document.getElementById('student-info');

            studentInfoContainer.innerHTML = `
              <div class="row mb-4" id="personalInfoCard" style="">
    <div class="col-md-12">
        <div class="card">
            <div class="card-header bg-light">
                <div class="d-flex align-items-center">
                    <span class="bg-white avatar avatar-sm me-2 text-gray-7 flex-shrink-0">
                        <i class="ti ti-info-square-rounded fs-16"></i>
                    </span>
                    <h5 class="text-dark">Student Information</h5>
                </div>
            </div>
            <div class="card-body">
                <div class="row">
                    <!-- First column for image -->
                    <div class="col-md-3 text-center mb-2" id="studentPhotoContainer">
                        <div class="profile-image d-flex align-items-center justify-content-center bg-light" style="height: 200px;">
                            <i class="ti ti-user fs-1 text-muted"></i>
                        </div>
                    </div>

                    <!-- Second column for all student data -->
                    <div class="col-md-9">
                        <!-- Row 1: Admission No -->
                        <div class="row mb-2">
                            <div class="col-md-3">
                                <div class="label-text"><b>Admission No</b></div>
                            </div>
                            <div class="col-md-8">
                                <div class="value-text" id="studentAdmissionNo">${studentInfo.id}</div>
                            </div>
                        </div>
                        
                        <!-- Row 2: Name -->
                        <div class="row mb-2">
                            <div class="col-md-3">
                                <div class="label-text"><b>Name</b></div>
                            </div>
                            <div class="col-md-8">
                                <div class="value-text" id="studentName">${studentInfo.name}</div>
                            </div>
                        </div>
                        
                        <!-- Row 3: Class -->
                        <div class="row mb-2">
                            <div class="col-md-3">
                                <div class="label-text"><b>Class</b></div>
                            </div>
                            <div class="col-md-8">
                                <div class="value-text" id="studentClass">${studentInfo.class}</div>
                            </div>
                        </div>
                        
                        <!-- Row 4: Section -->
                        <div class="row mb-2">
                            <div class="col-md-3">
                                <div class="label-text"><b>Section</b></div>
                            </div>
                            <div class="col-md-8">
                                <div class="value-text" id="studentSection">${studentInfo.section}</div>
                            </div>
                        </div>
                        
                        <!-- Row 5: Roll No -->
                        <div class="row mb-2">
                            <div class="col-md-3">
                                <div class="label-text"><b>Roll No</b></div>
                            </div>
                            <div class="col-md-8">
                                <div class="value-text" id="studentRollNo">1001</div>
                            </div>
                        </div>
                        
                        <!-- Row 6: Father's Name -->
                        <div class="row mb-2">
                            <div class="col-md-3">
                                <div class="label-text"><b>Father's Name</b></div>
                            </div>
                            <div class="col-md-8">
                                <div class="value-text" id="studentFatherName">${studentInfo.father}</div>
                            </div>
                        </div>
                        
                        <!-- Row 7: Mobile -->
                        <div class="row mb-2">
                            <div class="col-md-3">
                                <div class="label-text"><b>Mobile</b></div>
                            </div>
                            <div class="col-md-8">
                                <div class="value-text" id="studentMobile">${studentInfo.contact}</div>
                            </div>
                        </div>
                        
                        <!-- Row 8: Discount Category -->
                        <div class="row mb-2">
                            <div class="col-md-3">
                                <div class="label-text"><b>Discount Category</b></div>
                            </div>
                            <div class="col-md-8">
                                <div class="value-text" id="studentDiscountCategory">${studentInfo.discountCategory}</div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
            `;
        } catch (error) {
            console.error('Error loading student info:', error);
            throw error;
        }
    }

    // Load fee details
    async function loadFeeDetails() {
        try {
            // Get fee details from API
            state.feeDetails = await API.getFeeDetails(state.studentId);
           // Calculate monthly totals
            const monthlyTotals = {};
            CONFIG.MONTHS.forEach(month => {
                monthlyTotals[month.short] = 0;
            });
            
            state.feeDetails.forEach(fee => {
                CONFIG.MONTHS.forEach(month => {
                    monthlyTotals[month.short] += fee.months[month.short];
                });
            });
            
            // Update fee details in the UI
            const feeDetailsBody = document.getElementById('fee-details-body');
            feeDetailsBody.innerHTML = '';
            let srNo = 1;
            // Add rows for each fee type
            state.feeDetails.forEach(fee => {
                const row = document.createElement('tr');
                row.dataset.feeId = fee.id;
                row.innerHTML = `
                    <td>${srNo++}</td>
                    <td>${fee.name}</td>
                `;
                
                // Add cells for each month
                CONFIG.MONTHS.forEach(month => {
                    const amount = fee.months[month.short];
                    const discount = fee.discounts[month.short];
                    
                    row.innerHTML += `
                        <td class="month-cell" data-month="${month.short}">
                            ${amount > 0 ? `
                                <div class="fee-amount">${amount}</div>
                                ${discount > 0 ? `
                                    <div class="discount-value">-${discount}</div>
                                ` : ''}
                            ` : '0'}
                        </td>
                    `;
                });
                
                feeDetailsBody.appendChild(row);
            });
            
            // Add a totals row at the bottom with "Click" and plus buttons
            const totalsRow = document.createElement('tr');
            totalsRow.className = 'monthly-totals-row';
            
            // Add Click column with icon
            totalsRow.innerHTML = `
                <td colspan="1"></td>
                <td class="click-cell">
                    Click <i class="fas fa-plus-square"></i>
                </td>
            `;
            
            // Add total for each month with plus buttons and amount bubbles
            CONFIG.MONTHS.forEach(month => {
                const total = monthlyTotals[month.short];
                
                totalsRow.innerHTML += `
                    <td class="month-cell">
                        <button class="add-month-btn" data-month="${month.short}" data-amount="${total}">
                            <i class="fas fa-plus-square"></i>
                        </button>
                        <div class="amount-bubble">${total}</div>
                    </td>
                `;
            });
            
            feeDetailsBody.appendChild(totalsRow);
            
            // Add click event listeners to the add buttons
            const addButtons = document.querySelectorAll('.add-month-btn');
            addButtons.forEach(button => {
                button.addEventListener('click', handleTotalMonthSelection);
            });
        } catch (error) {
            console.error('Error loading fee details:', error);
            throw error;
        }
    }

    // Load fee summary
    async function loadFeeSummary() {
        try {
            // Get fee summary from API
            state.feeSummary = await API.getFeeSummary(state.studentId);
            
            // Update old balance in the UI
            document.getElementById('old-year-bal').textContent = `Old Year Bal: ${state.feeSummary.oldBalance}`;
            
            // Update summary section without transport name
            const feeSummary = document.getElementById('fee-summary');
            feeSummary.innerHTML = `
                <div class="summary-item">
                    <span class="summary-icon subtract"><i class="fas fa-minus"></i></span>
                    <span>Discount</span>
                    <span class="amount-circle">${state.feeSummary.discount}</span>
                </div>
                <div class="summary-item">
                    <span class="summary-icon add"><i class="fas fa-plus"></i></span>
                    <span>Late Fee</span>
                    <span class="amount-circle">${state.feeSummary.lateFee}</span>
                </div>
                <div class="summary-item">
                    <span class="summary-icon add"><i class="fas fa-plus"></i></span>
                    <span>Old Bal.</span>
                    <span class="amount-circle">${state.feeSummary.oldBalance}</span>
                </div>
            `;
            
            // Update input fields
            document.getElementById('discount-amount').value = state.feeSummary.discount;
            document.getElementById('late-fee-amount').value = state.feeSummary.lateFee;
        } catch (error) {
            console.error('Error loading fee summary:', error);
            throw error;
        }
    }

    // Load payment methods
    async function loadPaymentMethods() {
        try {
            // Get payment methods from API
            const paymentMethods = await API.getPaymentMethods();
            
            // Update payment methods in the UI
            const paymentMethodsContainer = document.getElementById('payment-methods');
            paymentMethodsContainer.innerHTML = '';
            
            // Add each payment method
            paymentMethods.forEach(method => {
                const label = document.createElement('label');
                label.className = 'payment-method-option';
                label.innerHTML = `
                    <input type="radio" name="payment-method" value="${method.id}" ${method.default ? 'checked' : ''}>
                    <span>${method.name}</span>
                `;
                paymentMethodsContainer.appendChild(label);
            });
        } catch (error) {
            console.error('Error loading payment methods:', error);
            throw error;
        }
    }

    // Load receipt templates
    async function loadReceiptTemplates() {
        try {
            // Get receipt templates from API
            const receiptTemplates = await API.getReceiptTemplates();
            
            // Update receipt templates in the UI
            const receiptTemplatesContainer = document.getElementById('receipt-templates');
            receiptTemplatesContainer.innerHTML = '';
            
            // Add each receipt template
            receiptTemplates.forEach(template => {
                const label = document.createElement('label');
                label.className = 'receipt-template-option';
                label.innerHTML = `
                    <input type="radio" name="receipt-template" value="${template.id}" ${template.default ? 'checked' : ''}>
                    <span>${template.content}</span>
                `;
                receiptTemplatesContainer.appendChild(label);
            });
            
            // Add No SMS option
            const noSmsLabel = document.createElement('label');
            noSmsLabel.className = 'receipt-template-option';
            noSmsLabel.innerHTML = `
                <input type="checkbox" id="no-sms" value="no-sms">
                <span>No SMS</span>
            `;
            receiptTemplatesContainer.appendChild(noSmsLabel);
        } catch (error) {
            console.error('Error loading receipt templates:', error);
            throw error;
        }
    }

    // Setup event listeners
    function setupEventListeners() {
        // Handle discount amount changes
        document.getElementById('discount-amount').addEventListener('input', function() {
            const value = parseFloat(this.value) || 0;
            this.value = value;
            updateTotals();
        });
        
        // Handle late fee amount changes
        document.getElementById('late-fee-amount').addEventListener('input', function() {
            const value = parseFloat(this.value) || 0;
            this.value = value;
            updateTotals();
        });
        
        // Handle received amount changes
        document.getElementById('received-amount').addEventListener('input', function() {
            const value = parseFloat(this.value) || 0;
            this.value = value;
            updateRemaining();
        });
        
        // "Received Fee" button click handler
        document.getElementById('received-fee-btn').addEventListener('click', handlePaymentSubmission);
        
        // "Show Ledger" button click handler
        document.getElementById('show-ledger-btn').addEventListener('click', function() {
            alert('Ledger view will be implemented in the future.');
        });
    }
    
    // Handle total month selection (add all fees for a month)
    function handleTotalMonthSelection(event) {
        event.preventDefault();
        event.stopPropagation();
        
        // Get month from data attributes
        const button = event.currentTarget;
        const month = button.dataset.month;
        
        // Find all fees for this month and add them
        let feesAdded = 0;
        
        state.feeDetails.forEach(fee => {
            const amount = fee.months[month];
            if (amount > 0) {
                // Check if this fee is already added
                const isDuplicate = state.addedFees.some(addedFee => 
                    addedFee.name === fee.name && addedFee.month === month
                );
                
                if (!isDuplicate) {
                    // Get regular amount and discount
                    const regularAmount = fee.regularAmounts ? fee.regularAmounts[month] : amount;
                    const discount = fee.discounts ? fee.discounts[month] : 0;
                    
                    addFeeToTable(fee.name, month, regularAmount, discount, amount);
                    feesAdded++;
                }
            }
        });
        
        if (feesAdded === 0) {
            alert(`All fees for ${month} are already added.`);
        }
    }
    
    // Function to add a fee to the added fees table
    function addFeeToTable(feeName, month, regularAmount, discount, finalAmount) {
        // Check if fee is already added
        const isDuplicate = state.addedFees.some(fee => 
            fee.name === feeName && fee.month === month
        );
        
        if (isDuplicate) {
            alert(`${feeName} for ${month} is already added.`);
            return;
        }
        
        const tableBody = document.getElementById('added-fees-body');
        
        // Create new row
        const newRow = document.createElement('tr');
        newRow.innerHTML = `
            <td>${state.feeCounter}</td>
            <td>${month}</td>
            <td>${feeName}</td>
            <td>${regularAmount.toFixed(2)}</td>
            <td class="discount-value">-${discount.toFixed(2)}</td>
            <td>${finalAmount.toFixed(2)}</td>
            <td><button class="btn-cancel" data-id="${state.feeCounter}">✕</button></td>
        `;
        
        // Add to table
        tableBody.appendChild(newRow);
        
        // Add to our tracking array
        state.addedFees.push({
            id: state.feeCounter,
            month: month,
            name: feeName,
            regularAmount: regularAmount,
            discount: discount,
            finalAmount: finalAmount
        });
        
        // Increment counter for next fee
        state.feeCounter++;
        
        // Add event listener to the cancel button
        const cancelBtn = newRow.querySelector('.btn-cancel');
        cancelBtn.addEventListener('click', function() {
            const feeId = parseInt(this.getAttribute('data-id'));
            removeFee(feeId, newRow, month);
        });
        
        // Update totals
        updateTotals();
        
        // Ensure the table doesn't exceed maximum height
        enforceTableHeight();
        
        // Disable the plus button for this month
        disableMonthButton(month);
    }
    
    // Disable the plus button for a specific month
    function disableMonthButton(month) {
        // Find the add button for this month
        const addButton = document.querySelector(`.add-month-btn[data-month="${month}"]`);
        if (addButton) {
            // Disable the button
            addButton.disabled = true;
            addButton.classList.add('disabled');
            
            // Change the icon to indicate it's disabled
            const icon = addButton.querySelector('i');
            if (icon) {
                icon.classList.remove('fa-plus-square');
                icon.classList.add('fa-check-square');
            }
        }
    }

    // Enable the plus button for a specific month
    function enableMonthButton(month) {
        // Find the add button for this month
        const addButton = document.querySelector(`.add-month-btn[data-month="${month}"]`);
        if (addButton) {
            // Enable the button
            addButton.disabled = false;
            addButton.classList.remove('disabled');
            
            // Change the icon back to plus
            const icon = addButton.querySelector('i');
            if (icon) {
                icon.classList.remove('fa-check-square');
                icon.classList.add('fa-plus-square');
            }
        }
    }
    
    // Function to update total amounts
    function updateTotals() {
        // Calculate the sum of all fees (using finalAmount after individual discounts)
        const feeTotal = state.addedFees.reduce((sum, fee) => sum + fee.finalAmount, 0);
        
        // Calculate the sum of all regular amounts
        const regularTotal = state.addedFees.reduce((sum, fee) => sum + fee.regularAmount, 0);

        // Calculate the sum of all individual discounts
        const individualDiscounts = state.addedFees.reduce((sum, fee) => sum + fee.discount, 0);
        
        // Get old balance
        const oldBalance = state.feeSummary.oldBalance || 0;
        
        // Get late fee
        const lateFee = parseFloat(document.getElementById('late-fee-amount').value) || 0;
        
        // Calculate subtotal (before additional discount)
        const subtotal = feeTotal + oldBalance + lateFee;
        
        // Get additional discount from input
        const additionalDiscount = parseFloat(document.getElementById('discount-amount').value) || 0;
        
        // Calculate total (after additional discount)
        const total = subtotal - additionalDiscount;
        
        // Update fields
        document.getElementById('subtotal-amount').value = subtotal.toFixed(2);
        document.getElementById('total-amount').value = total.toFixed(2);
        
        // Update received amount to match total (can be changed by user)
        document.getElementById('received-amount').value = total.toFixed(2);
        
        // Update remaining amount (should be 0 if received matches total)
        updateRemaining();
        
        // Update discount display in summary
        updateDiscountSummary(individualDiscounts + additionalDiscount);
    }
    
    // Update the discount summary display
    function updateDiscountSummary(totalDiscount) {
        const feeSummary = document.getElementById('fee-summary');
        const discountItem = feeSummary.querySelector('.summary-item:first-child .amount-circle');
        if (discountItem) {
            discountItem.textContent = Math.round(totalDiscount);
        }
    }
    
    // Function to update remaining amount and adjust fees if needed
    function updateRemaining() {
        const total = parseFloat(document.getElementById('total-amount').value) || 0;
        const received = parseFloat(document.getElementById('received-amount').value) || 0;
        const remaining = total - received;
        
        document.getElementById('remain-amount').value = remaining.toFixed(2);
        
        // If received amount is less than total, adjust fees based on head-wise distribution
        if (received < total && received > 0 && state.addedFees.length > 0) {
            // Group fees by fee name (head)
            const feesByHead = {};
            state.addedFees.forEach(fee => {
                if (!feesByHead[fee.name]) {
                    feesByHead[fee.name] = [];
                }
                feesByHead[fee.name].push(fee);
            });
            
            // Calculate total by head
            const headTotals = {};
            Object.keys(feesByHead).forEach(head => {
                headTotals[head] = feesByHead[head].reduce((sum, fee) => sum + fee.finalAmount, 0);
            });
            
            // Sort heads by priority (we'll use the order they appear in the original fee details)
            const headsByPriority = Object.keys(headTotals).sort((a, b) => {
                const aIndex = state.feeDetails.findIndex(f => f.name === a);
                const bIndex = state.feeDetails.findIndex(f => f.name === b);
                return aIndex - bIndex;
            });
            
            // Allocate received amount to heads in priority order
            let remainingReceived = received;
            const adjustedHeads = {};
            let allHeadsAdjusted = true;
            
            headsByPriority.forEach(head => {
                if (remainingReceived >= headTotals[head]) {
                    // Can fully cover this head
                    adjustedHeads[head] = headTotals[head];
                    remainingReceived -= headTotals[head];
                } else if (remainingReceived > 0) {
                    // Can partially cover this head
                    adjustedHeads[head] = remainingReceived;
                    remainingReceived = 0;
                    allHeadsAdjusted = false;
                } else {
                    // Can't cover this head at all
                    adjustedHeads[head] = 0;
                    allHeadsAdjusted = false;
                }
            });
            
            // If we can't cover all heads, remove fees that won't be paid
            if (!allHeadsAdjusted) {
                // Start by removing all fees from the table
                document.getElementById('added-fees-body').innerHTML = '';
                
                // Reset the fees array and counter
                const oldFees = [...state.addedFees];
                state.addedFees = [];
                state.feeCounter = 1;
                
                // Store the months that had fees removed completely
                const removedMonths = new Set();
                const keptMonths = new Set();
                
                // Re-add fees based on adjusted amounts
                headsByPriority.forEach(head => {
                    if (adjustedHeads[head] > 0) {
                        let headRemaining = adjustedHeads[head];
                        const headFees = feesByHead[head].sort((a, b) => {
                            // Sort by month (using the month order from CONFIG)
                            const aMonthIndex = CONFIG.MONTHS.findIndex(m => m.short === a.month);
                            const bMonthIndex = CONFIG.MONTHS.findIndex(m => m.short === b.month);
                            return aMonthIndex - bMonthIndex;
                        });
                        
                        // Add fees until we reach the adjusted amount
                        for (const fee of headFees) {
                            if (headRemaining >= fee.finalAmount) {
                                // Add this fee
                                const tableBody = document.getElementById('added-fees-body');
                                const newRow = document.createElement('tr');
                                newRow.innerHTML = `
                                    <td>${state.feeCounter}</td>
                                    <td>${fee.month}</td>
                                    <td>${fee.name}</td>
                                    <td>${fee.regularAmount.toFixed(2)}</td>
                                    <td class="discount-value">-${fee.discount.toFixed(2)}</td>
                                    <td>${fee.finalAmount.toFixed(2)}</td>
                                    <td><button class="btn-cancel" data-id="${state.feeCounter}">✕</button></td>
                                `;
                                
                                // Add to table
                                tableBody.appendChild(newRow);
                                
                                // Add to tracking array
                                state.addedFees.push({
                                    id: state.feeCounter,
                                    month: fee.month,
                                    name: fee.name,
                                    regularAmount: fee.regularAmount,
                                    discount: fee.discount,
                                    finalAmount: fee.finalAmount
                                });
                                
                                // Keep track of months we're keeping
                                keptMonths.add(fee.month);
                                
                                // Add event listener to the cancel button
                                const cancelBtn = newRow.querySelector('.btn-cancel');
                                cancelBtn.addEventListener('click', function() {
                                    const feeId = parseInt(this.getAttribute('data-id'));
                                    removeFee(feeId, newRow, fee.month);
                                });
                                
                                // Update counter and remaining amount
                                state.feeCounter++;
                                headRemaining -= fee.finalAmount;
                            } else {
                                // Can't add this fee, note that this month had fees removed
                                removedMonths.add(fee.month);
                                break;
                            }
                        }
                    } else {
                        // Can't cover this head at all, note that all months for this head had fees removed
                        feesByHead[head].forEach(fee => {
                            removedMonths.add(fee.month);
                        });
                    }
                });
                
                // Re-enable buttons for months that had all fees removed
                // and weren't kept in any other fee
                removedMonths.forEach(month => {
                    if (!keptMonths.has(month)) {
                        enableMonthButton(month);
                    }
                });
                
                // Update the totals without triggering a loop
                updateTotals();
                
                // Ensure the table scrolls if needed
                enforceTableHeight();
            }
        }
    }
    
    // Function to remove a fee
    function removeFee(id, rowElement, month) {
        // Remove from DOM
        rowElement.remove();
        
        // Remove from array
        state.addedFees = state.addedFees.filter(fee => fee.id !== id);
        
        // Update totals
        updateTotals();
        
        // Re-enable the button for this month if all fees for this month are removed
        const stillHasFeeForMonth = state.addedFees.some(fee => fee.month === month);
        if (!stillHasFeeForMonth) {
            enableMonthButton(month);
        }
    }
    
    // Function to enforce the height of the added fees table
    function enforceTableHeight() {
        const tableContainer = document.querySelector('.added-fees-container');
        const rows = document.querySelectorAll('#added-fees-body tr');
        
        // If we have more rows than the max visible, add scrolling class
        if (rows.length > CONFIG.MAX_VISIBLE_ROWS) {
            tableContainer.style.overflowY = 'auto';
        } else {
            tableContainer.style.overflowY = 'visible';
        }
    }
    
    // Handle payment submission
    async function handlePaymentSubmission() {
        try {
            // Validate that there are added fees
            if (state.addedFees.length === 0) {
                alert('Please add at least one fee before proceeding.');
                return;
            }
            
            // Get payment values
            const subtotal = parseFloat(document.getElementById('subtotal-amount').value) || 0;
            const discount = parseFloat(document.getElementById('discount-amount').value) || 0;
            const lateFee = parseFloat(document.getElementById('late-fee-amount').value) || 0;
            const total = parseFloat(document.getElementById('total-amount').value) || 0;
            const received = parseFloat(document.getElementById('received-amount').value) || 0;
            const remaining = parseFloat(document.getElementById('remain-amount').value) || 0;
            const note = document.getElementById('note').value || '';
            
            // Get selected payment method
            const paymentMethodElement = document.querySelector('input[name="payment-method"]:checked');
            if (!paymentMethodElement) {
                alert('Please select a payment method.');
                return;
            }
            const paymentMethod = paymentMethodElement.value;
            
            // Get selected receipt template
            const receiptTemplateElement = document.querySelector('input[name="receipt-template"]:checked');
            if (!receiptTemplateElement) {
                alert('Please select a receipt template.');
                return;
            }
            const receiptTemplate = receiptTemplateElement.value;
            
            // Get SMS preference
            const noSms = document.getElementById('no-sms').checked;
            
            // Validate received amount
            if (received <= 0) {
                alert('Please enter a valid received amount.');
                return;
            }
            
            // Prepare payment data
            const paymentData = {
                studentId: state.studentId,
                fees: state.addedFees,
                subtotalAmount: subtotal,
                discountAmount: discount,
                lateFeeAmount: lateFee,
                totalAmount: total,
                receivedAmount: received,
                remainAmount: remaining,
                paymentMethod: paymentMethod,
                receiptTemplate: receiptTemplate,
                sendSms: !noSms,
                note: note,
                date: document.getElementById('receipt-date').value
            };
            
            // Display loading state
            document.getElementById('received-fee-btn').disabled = true;
            document.getElementById('received-fee-btn').textContent = 'Processing...';
            
            // Submit payment to API
            const result = await API.submitPayment(paymentData);
            
            // Show success message
            alert(`Payment received successfully! Receipt number: ${result.receiptNumber}`);
            
            // Reset the form
            resetForm();
        } catch (error) {
            console.error('Error processing payment:', error);
            alert('Failed to process payment. Please try again.');
        } finally {
            // Reset button state
            document.getElementById('received-fee-btn').disabled = false;
            document.getElementById('received-fee-btn').textContent = 'Received Fee';
        }
    }
    
    // Function to reset the form after successful submission
    function resetForm() {
        // Clear added fees
        document.getElementById('added-fees-body').innerHTML = '';
        state.addedFees = [];
        state.feeCounter = 1;
        
        // Reset input fields
        document.getElementById('subtotal-amount').value = '0';
        document.getElementById('discount-amount').value = '0';
        document.getElementById('late-fee-amount').value = '0';
        document.getElementById('total-amount').value = '0';
        document.getElementById('received-amount').value = '0';
        document.getElementById('remain-amount').value = '0';
        document.getElementById('note').value = '';
        
        // Reset payment method to default
        const defaultPaymentMethod = document.querySelector('input[name="payment-method"][value="cash"]');
        if (defaultPaymentMethod) {
            defaultPaymentMethod.checked = true;
        }
        
        // Reset receipt template to default
        const defaultReceiptTemplate = document.querySelector('input[name="receipt-template"][value="template1"]');
        if (defaultReceiptTemplate) {
            defaultReceiptTemplate.checked = true;
        }
        
        // Reset SMS checkbox
        document.getElementById('no-sms').checked = false;
        
        // Update today's date in the receipt date field
        document.getElementById('receipt-date').valueAsDate = new Date();
        
        // Re-enable all month buttons
        CONFIG.MONTHS.forEach(month => {
            enableMonthButton(month.short);
        });
        
        // Update the discount summary back to zero
        updateDiscountSummary(0);
    }
});