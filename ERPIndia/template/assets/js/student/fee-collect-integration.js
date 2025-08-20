/**
 * FeeCollect API Integration
 * Example of how to integrate FeeCollect.js with the ASP.NET MVC5 API
 */

// Base API URL - change this to your actual API endpoint
const API_BASE_URL = '/api';

// Student selector dropdown
const studentSelector = document.getElementById('studentSelector');
const feeCollectContainer = document.getElementById('feeCollectContainer');

// Load students when the page loads
document.addEventListener('DOMContentLoaded', function() {
    loadStudents();
});

// Event listener for student selection
if (studentSelector) {
    studentSelector.addEventListener('change', function() {
        const studentId = this.value;
        if (studentId) {
            loadStudentFeeData(studentId);
        }
    });
}

/**
 * Load all students for the demo selector
 */
function loadStudents() {
    fetch(`${API_BASE_URL}/students`)
        .then(response => response.json())
        .then(students => {
            studentSelector.innerHTML = '<option value="">Select a student</option>';
            
            students.forEach(student => {
                const option = document.createElement('option');
                option.value = student.Id;
                option.textContent = `${student.Name} (${student.Class} ${student.Section})`;
                studentSelector.appendChild(option);
            });
        })
        .catch(error => {
            console.error('Error loading students:', error);
            alert('Failed to load students. Please try again.');
        });
}

/**
 * Load fee data for a specific student
 * @param {number} studentId - Student ID
 */
function loadStudentFeeData(studentId) {
    fetch(`${API_BASE_URL}/feestudent`)
        .then(response => response.json())
        .then(data => {
            console.log('Received data:', data);

            // Fix property names that are lowercase
            if (data.feedata && !data.feeData) {
                data.feeData = data.feedata;
                delete data.feedata;
            }

            if (data.studentinfo && !data.studentInfo) {
                data.studentInfo = data.studentinfo;
                delete data.studentinfo;
            }

            // Fix admission number field
            if (data.studentInfo && data.studentInfo.admissionnumber && !data.studentInfo.admissionNumber) {
                data.studentInfo.admissionNumber = data.studentInfo.admissionnumber;
                delete data.studentInfo.admissionnumber;
            }

            console.log('Fixed data structure:', data);

            // Initialize FeeCollect with the properly structured data
            initializeFeeCollect(data, studentId);
        })
        .catch(error => {
            console.error('Error loading student fee data:', error);
            alert('Failed to load student fee data. Please try again.');
        });
}

/**
 * Initialize FeeCollect component with student data
 * @param {Object} data - Student fee data
 * @param {number} studentId - Student ID
 */
function initializeFeeCollect(data, studentId) {
    // Clear any existing instances
    feeCollectContainer.innerHTML = '';
    
    // Configure options for FeeCollect
    const options = {
        dateFormat: 'DD-MM-YYYY',
        currency: '₹',
        autoGenerateReceipt: true,
        showProgressBar: true,
        showLedgerButton: true,
        showFineDiscount: true,
        autoUpdateReceived: true,
        paymentMethods: ['cash', 'bank', 'cheque', 'upi', 'paytm'],
        callbacks: {
            onInit: function() {
                console.log('FeeCollect initialized');
            },
            onFeeSelect: function(fee) {
                console.log('Fee selected:', fee);
            },
            onFeeBulkSelect: function(data) {
                console.log('Bulk fees selected:', data);
            },
            onFeeRemove: function(fee) {
                console.log('Fee removed:', fee);
            },
            onPaymentMethodChange: function(method) {
                console.log('Payment method changed:', method);
            },
            onPaymentComplete: function(paymentData) {
                // Add student ID to the payment data
                paymentData.studentId = studentId;
                
                // Submit the payment to the API
                submitPayment(paymentData);
            },
            onLedgerOpen: function() {
                // Open the ledger view
                openLedger(studentId);
            }
        }
    };
    
    // Create FeeCollect instance
    const feeCollect = new FeeCollect(feeCollectContainer, data, options);
    
    // Store the instance on the window for debugging
    window.feeCollect = feeCollect;
}

/**
 * Submit payment to API
 * @param {Object} paymentData - Payment data
 */
function submitPayment(paymentData) {
    // Show loading indicator
    const loadingOverlay = document.createElement('div');
    loadingOverlay.className = 'loading-overlay';
    loadingOverlay.innerHTML = '<div class="loading-spinner"></div><div>Processing Payment...</div>';
    document.body.appendChild(loadingOverlay);
    
    // Send payment data to API
    fetch(`${API_BASE_URL}/fee/payment`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(paymentData)
    })
    .then(response => response.json())
    .then(result => {
        // Remove loading overlay
        document.body.removeChild(loadingOverlay);
        
        if (result.success) {
            // Show success notification
            alert(`Payment successful! Receipt Number: ${result.receiptNumber}`);
            
            // Offer to open the receipt
            if (confirm('Would you like to view the receipt?')) {
                openReceipt(result.receiptNumber);
            }
            
            // Reload student data to refresh
            loadStudentFeeData(paymentData.studentId);
        } else {
            // Show error notification
            alert(`Payment failed: ${result.message}`);
        }
    })
    .catch(error => {
        // Remove loading overlay
        document.body.removeChild(loadingOverlay);
        
        console.error('Error processing payment:', error);
        alert('Failed to process payment. Please try again.');
    });
}

/**
 * Open student ledger in a new window/tab
 * @param {number} studentId - Student ID
 */
function openLedger(studentId) {
    // In a real application, you might open a modal or navigate to a ledger page
    // For this example, we'll just fetch the ledger data and display it
    
    fetch(`${API_BASE_URL}/fee/ledger/${studentId}`)
        .then(response => response.json())
        .then(ledger => {
            // Open a new window for the ledger view
            const ledgerWindow = window.open('', 'FeeCollectLedger', 'width=800,height=600');
            
            // Create ledger HTML content
            const ledgerContent = `
                <html>
                <head>
                    <title>Student Ledger - ${ledger.student.name}</title>
                    <style>
                        body { font-family: Arial, sans-serif; margin: 20px; }
                        h1 { color: #333; }
                        table { width: 100%; border-collapse: collapse; margin-top: 20px; }
                        th, td { padding: 8px; text-align: left; border-bottom: 1px solid #ddd; }
                        th { background-color: #f2f2f2; }
                        .student-info { margin-bottom: 20px; }
                        .student-info p { margin: 5px 0; }
                        .balance-info { margin-top: 20px; font-weight: bold; }
                    </style>
                </head>
                <body>
                    <h1>Student Ledger</h1>
                    
                    <div class="student-info">
                        <p><strong>Name:</strong> ${ledger.student.name}</p>
                        <p><strong>Class:</strong> ${ledger.student.class} ${ledger.student.section}</p>
                        <p><strong>Admission Number:</strong> ${ledger.student.admissionNumber}</p>
                    </div>
                    
                    <table>
                        <thead>
                            <tr>
                                <th>Date</th>
                                <th>Receipt Number</th>
                                <th>Description</th>
                                <th>Debit</th>
                                <th>Credit</th>
                                <th>Balance</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${ledger.entries.map(entry => `
                                <tr>
                                    <td>${entry.date}</td>
                                    <td>${entry.receiptNumber}</td>
                                    <td>${entry.description}</td>
                                    <td>₹${entry.debit.toFixed(2)}</td>
                                    <td>₹${entry.credit.toFixed(2)}</td>
                                    <td>₹${entry.balance.toFixed(2)}</td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>
                    
                    <div class="balance-info">
                        <p>Opening Balance: ₹${ledger.openingBalance.toFixed(2)}</p>
                        <p>Closing Balance: ₹${ledger.closingBalance.toFixed(2)}</p>
                    </div>
                    
                    <button onclick="window.print()">Print Ledger</button>
                </body>
                </html>
            `;
            
            ledgerWindow.document.write(ledgerContent);
            ledgerWindow.document.close();
        })
        .catch(error => {
            console.error('Error loading ledger:', error);
            alert('Failed to load ledger. Please try again.');
        });
}

/**
 * Open receipt in a new window/tab
 * @param {string} receiptNumber - Receipt number
 */
function openReceipt(receiptNumber) {
    fetch(`${API_BASE_URL}/fee/receipt/${receiptNumber}`)
        .then(response => response.json())
        .then(receipt => {
            // Open a new window for the receipt view
            const receiptWindow = window.open('', 'FeeCollectReceipt', 'width=800,height=600');
            
            // Calculate totals
            let totalAmount = 0;
            receipt.fees.forEach(fee => {
                totalAmount += fee.netAmount;
            });
            
            // Create receipt HTML content
            const receiptContent = `
                <html>
                <head>
                    <title>Fee Receipt - ${receipt.receiptNumber}</title>
                    <style>
                        body { font-family: Arial, sans-serif; margin: 20px; }
                        .receipt { border: 1px solid #333; padding: 20px; max-width: 800px; margin: 0 auto; }
                        .receipt-header { display: flex; justify-content: space-between; border-bottom: 2px solid #333; padding-bottom: 10px; margin-bottom: 20px; }
                        .school-info h1 { margin: 0; color: #333; }
                        .receipt-info { text-align: right; }
                        .student-info { margin-bottom: 20px; }
                        .student-info p { margin: 5px 0; }
                        table { width: 100%; border-collapse: collapse; margin: 20px 0; }
                        th, td { padding: 8px; text-align: left; border-bottom: 1px solid #ddd; }
                        th { background-color: #f2f2f2; }
                        .amount-col { text-align: right; }
                        .total-row { font-weight: bold; border-top: 2px solid #333; }
                        .payment-info { display: flex; justify-content: space-between; margin-top: 20px; }
                        .signature { margin-top: 50px; display: flex; justify-content: space-between; }
                        .signature div { width: 200px; text-align: center; }
                        .signature .line { border-top: 1px solid #333; margin-top: 50px; }
                    </style>
                </head>
                <body>
                    <div class="receipt">
                        <div class="receipt-header">
                            <div class="school-info">
                                <h1>School Name</h1>
                                <p>123 Education Street, City, State - 123456</p>
                                <p>Phone: (123) 456-7890 | Email: info@schoolname.edu</p>
                            </div>
                            <div class="receipt-info">
                                <h2>Fee Receipt</h2>
                                <p><strong>Receipt Number:</strong> ${receipt.receiptNumber}</p>
                                <p><strong>Date:</strong> ${new Date(receipt.date).toLocaleDateString()}</p>
                            </div>
                        </div>
                        
                        <div class="student-info">
                            <p><strong>Student Name:</strong> ${receipt.studentName}</p>
                            <p><strong>Class:</strong> ${receipt.class} ${receipt.section}</p>
                            <p><strong>Admission Number:</strong> ${receipt.admissionNumber}</p>
                        </div>
                        
                        <table>
                            <thead>
                                <tr>
                                    <th>S.No</th>
                                    <th>Fee Month</th>
                                    <th>Fee Name</th>
                                    <th class="amount-col">Amount</th>
                                    <th class="amount-col">Fine</th>
                                    <th class="amount-col">Discount</th>
                                    <th class="amount-col">Net Amount</th>
                                </tr>
                            </thead>
                            <tbody>
                                ${receipt.fees.map((fee, index) => `
                                    <tr>
                                        <td>${index + 1}</td>
                                        <td>${fee.month}</td>
                                        <td>${fee.name}</td>
                                        <td class="amount-col">₹${fee.amount.toFixed(2)}</td>
                                        <td class="amount-col">₹${fee.fine.toFixed(2)}</td>
                                        <td class="amount-col">₹${fee.discount.toFixed(2)}</td>
                                        <td class="amount-col">₹${fee.netAmount.toFixed(2)}</td>
                                    </tr>
                                `).join('')}
                                <tr class="total-row">
                                    <td colspan="3">Total</td>
                                    <td class="amount-col">₹${receipt.total.toFixed(2)}</td>
                                    <td class="amount-col">₹${receipt.lateFee.toFixed(2)}</td>
                                    <td class="amount-col">₹${receipt.concession.toFixed(2)}</td>
                                    <td class="amount-col">₹${receipt.finalAmount.toFixed(2)}</td>
                                </tr>
                            </tbody>
                        </table>
                        
                        <div class="payment-info">
                            <div>
                                <p><strong>Payment Method:</strong> ${receipt.paymentMethod.charAt(0).toUpperCase() + receipt.paymentMethod.slice(1)}</p>
                                <p><strong>Note:</strong> ${receipt.note || 'N/A'}</p>
                            </div>
                            <div>
                                <p><strong>Amount in Words:</strong> ${amountInWords(receipt.finalAmount)}</p>
                            </div>
                        </div>
                        
                        <div class="signature">
                            <div>
                                <div class="line"></div>
                                <p>Student Signature</p>
                            </div>
                            <div>
                                <div class="line"></div>
                                <p>Cashier Signature</p>
                            </div>
                            <div>
                                <div class="line"></div>
                                <p>Principal Signature</p>
                            </div>
                        </div>
                    </div>
                    
                    <div style="text-align: center; margin-top: 20px;">
                        <button onclick="window.print()">Print Receipt</button>
                    </div>
                    
                    <script>
                        // Auto-print when opened
                        window.onload = function() {
                            // Uncomment to automatically print
                            // window.print();
                        };
                    </script>
                </body>
                </html>
            `;
            
            receiptWindow.document.write(receiptContent);
            receiptWindow.document.close();
        })
        .catch(error => {
            console.error('Error loading receipt:', error);
            alert('Failed to load receipt. Please try again.');
        });
}

/**
 * Convert number to words for receipt
 * @param {number} amount - Amount to convert
 * @returns {string} - Amount in words
 */
function amountInWords(amount) {
    const ones = ['', 'One', 'Two', 'Three', 'Four', 'Five', 'Six', 'Seven', 'Eight', 'Nine', 'Ten', 'Eleven', 'Twelve', 'Thirteen', 'Fourteen', 'Fifteen', 'Sixteen', 'Seventeen', 'Eighteen', 'Nineteen'];
    const tens = ['', '', 'Twenty', 'Thirty', 'Forty', 'Fifty', 'Sixty', 'Seventy', 'Eighty', 'Ninety'];
    const scales = ['', 'Thousand', 'Million', 'Billion', 'Trillion'];
    
    if (amount === 0) return 'Zero Rupees Only';
    
    const rupees = Math.floor(amount);
    const paise = Math.round((amount - rupees) * 100);
    
    let words = '';
    
    // Convert rupees to words
    if (rupees > 0) {
        let scaleIndex = 0;
        let remainder = rupees;
        
        while (remainder > 0) {
            const chunk = remainder % 1000;
            
            if (chunk > 0) {
                let chunkWords = '';
                
                // Convert hundreds
                const hundreds = Math.floor(chunk / 100);
                if (hundreds > 0) {
                    chunkWords += ones[hundreds] + ' Hundred';
                    if (chunk % 100 > 0) chunkWords += ' and ';
                }
                
                // Convert tens and ones
                const tensOnes = chunk % 100;
                if (tensOnes > 0) {
                    if (tensOnes < 20) {
                        chunkWords += ones[tensOnes];
                    } else {
                        chunkWords += tens[Math.floor(tensOnes / 10)];
                        if (tensOnes % 10 > 0) chunkWords += '-' + ones[tensOnes % 10];
                    }
                }
                
                words = chunkWords + ' ' + scales[scaleIndex] + ' ' + words;
            }
            
            remainder = Math.floor(remainder / 1000);
            scaleIndex++;
        }
        
        words += 'Rupees';
    }
    
    // Convert paise to words
    if (paise > 0) {
        words += ' and ';
        
        if (paise < 20) {
            words += ones[paise];
        } else {
            words += tens[Math.floor(paise / 10)];
            if (paise % 10 > 0) words += '-' + ones[paise % 10];
        }
        
        words += ' Paise';
    }
    
    return words + ' Only';
}