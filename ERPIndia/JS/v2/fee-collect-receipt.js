/**
 * Fee Receipt Generator
 * 
 * This file provides functionality to generate printable fee receipts
 * from FeeCollect payment data.
 */

/**
 * Generate a printable receipt HTML from payment data
 * @param {Object} paymentData - Payment data from FeeCollect
 * @param {Object} options - Optional settings for receipt generation
 * @returns {String} - HTML string for receipt
 */
function generateReceiptHTML(paymentData, options = {}) {
  // Default options
  const defaultOptions = {
    schoolName: 'School Name',
    schoolLogo: null, // URL to school logo
    schoolAddress: 'School Address, City, State - Pincode',
    schoolPhone: '123-456-7890',
    schoolEmail: 'school@example.com',
    schoolWebsite: 'www.school.edu',
    receiptTitle: 'FEE RECEIPT',
    footerText: 'Thank you for your payment!',
    showWatermark: true,
    watermarkText: 'PAID',
    currency: '₹',
    locale: 'en-IN',
    dateFormat: { year: 'numeric', month: 'short', day: 'numeric' },
    printButton: true,
    downloadButton: true
  };
  
  // Merge options
  const settings = { ...defaultOptions, ...options };
  
  // Format date
  const receiptDate = formatDate(paymentData.date, settings.dateFormat);
  
  // Generate receipt HTML
  const html = `
    <!DOCTYPE html>
    <html lang="en">
    <head>
      <meta charset="UTF-8">
      <meta name="viewport" content="width=device-width, initial-scale=1.0">
      <title>Receipt ${paymentData.receiptNumber}</title>
      <style>
        @media print {
          .no-print {
            display: none !important;
          }
          
          body {
            -webkit-print-color-adjust: exact;
            print-color-adjust: exact;
          }
          
          .receipt {
            box-shadow: none;
            border: 1px solid #ddd;
          }
        }
        
        body {
          font-family: Arial, sans-serif;
          margin: 0;
          padding: 20px;
          background-color: #f5f5f5;
          color: #333;
        }
        
        .receipt-container {
          max-width: 800px;
          margin: 0 auto;
        }
        
        .receipt {
          background-color: white;
          box-shadow: 0 1px 3px rgba(0,0,0,0.1);
          border-radius: 5px;
          padding: 15px;
          position: relative;
          overflow: hidden;
        }
        
        ${settings.showWatermark ? `
        .watermark {
          position: absolute;
          top: 50%;
          left: 50%;
          transform: translate(-50%, -50%) rotate(-45deg);
          font-size: 80px;
          color: rgba(0, 128, 0, 0.1);
          font-weight: bold;
          pointer-events: none;
          z-index: 1;
        }
        ` : ''}
        
        .receipt-header {
          display: flex;
          justify-content: space-between;
          align-items: center;
          border-bottom: 2px solid #333;
          padding-bottom: 10px;
          margin-bottom: 15px;
        }
        
        .receipt-logo {
          max-width: 80px;
          max-height: 80px;
        }
        
        .school-info {
          text-align: center;
          flex-grow: 1;
        }
        
        .school-name {
          font-size: 20px;
          font-weight: bold;
          margin-bottom: 5px;
        }
        
        .school-address, .school-contact {
          font-size: 12px;
          color: #666;
          margin: 3px 0;
        }
        
        .receipt-title {
          font-size: 18px;
          font-weight: bold;
          text-align: center;
          margin: 15px 0;
          text-transform: uppercase;
        }
        
        .receipt-details {
          display: flex;
          justify-content: space-between;
          margin-bottom: 15px;
        }
        
        .receipt-detail {
          flex: 1;
        }
        
        .detail-label {
          font-weight: bold;
          font-size: 12px;
          color: #666;
          margin-bottom: 3px;
        }
        
        .detail-value {
          font-size: 14px;
        }
        
        .student-details {
          border: 1px solid #ddd;
          padding: 10px;
          margin-bottom: 15px;
          border-radius: 4px;
        }
        
        .student-details-title {
          font-weight: bold;
          text-transform: uppercase;
          font-size: 14px;
          margin-bottom: 10px;
          border-bottom: 1px solid #eee;
          padding-bottom: 5px;
        }
        
        .student-detail-row {
          display: flex;
          flex-wrap: wrap;
          margin-bottom: 5px;
        }
        
        .student-detail-item {
          flex: 1;
          min-width: 200px;
          font-size: 13px;
          margin-bottom: 5px;
        }
        
        .student-detail-label {
          font-weight: bold;
          display: inline-block;
          margin-right: 5px;
        }
        
        .fee-table {
          width: 100%;
          border-collapse: collapse;
          margin: 15px 0;
        }
        
        .fee-table th, .fee-table td {
          border: 1px solid #ddd;
          padding: 8px;
          text-align: left;
          font-size: 13px;
        }
        
        .fee-table th {
          background-color: #f5f5f5;
          font-weight: bold;
          text-transform: uppercase;
          font-size: 12px;
        }
        
        .fee-table tr:nth-child(even) {
          background-color: #f9f9f9;
        }
        
        .fee-amount, .fee-fine, .fee-discount, .fee-net {
          text-align: right;
        }
        
        .fee-fine {
          color: #e74c3c;
        }
        
        .fee-discount {
          color: #27ae60;
        }
        
        .payment-summary {
          display: flex;
          justify-content: flex-end;
          margin: 15px 0;
        }
        
        .payment-totals {
          width: 300px;
          border: 1px solid #ddd;
          border-radius: 4px;
          overflow: hidden;
        }
        
        .payment-row {
          display: flex;
          justify-content: space-between;
          padding: 8px 10px;
          font-size: 13px;
        }
        
        .payment-row:nth-child(even) {
          background-color: #f9f9f9;
        }
        
        .payment-label {
          font-weight: bold;
        }
        
        .payment-value {
          text-align: right;
        }
        
        .payment-row.payment-total {
          background-color: #333;
          color: white;
          font-weight: bold;
        }
        
        .payment-method {
          border: 1px solid #ddd;
          padding: 10px;
          margin: 15px 0;
          border-radius: 4px;
          display: flex;
          justify-content: space-between;
          align-items: center;
        }
        
        .payment-method-label {
          font-weight: bold;
          text-transform: uppercase;
          font-size: 14px;
        }
        
        .payment-method-value {
          text-transform: uppercase;
          font-weight: bold;
        }
        
        .payment-note {
          font-size: 12px;
          font-style: italic;
          color: #666;
          margin-top: 5px;
        }
        
        .receipt-footer {
          margin-top: 30px;
          text-align: center;
          font-size: 12px;
          color: #666;
          border-top: 1px solid #ddd;
          padding-top: 10px;
        }
        
        .signatures {
          display: flex;
          justify-content: space-between;
          margin-top: 50px;
        }
        
        .signature {
          text-align: center;
          width: 150px;
        }
        
        .signature-line {
          border-top: 1px solid #333;
          margin-bottom: 5px;
        }
        
        .signature-name {
          font-size: 12px;
          font-weight: bold;
        }
        
        .action-buttons {
          margin-top: 20px;
          text-align: center;
        }
        
        .receipt-button {
          padding: 10px 15px;
          margin: 0 5px;
          border: none;
          border-radius: 4px;
          background-color: #3498db;
          color: white;
          font-weight: bold;
          cursor: pointer;
        }
        
        .download-button {
          background-color: #2ecc71;
        }
      </style>
    </head>
    <body>
      <div class="receipt-container">
        <div class="receipt">
          ${settings.showWatermark ? `<div class="watermark">${settings.watermarkText}</div>` : ''}
          
          <div class="receipt-header">
            ${settings.schoolLogo ? `<img src="${settings.schoolLogo}" alt="School Logo" class="receipt-logo">` : ''}
            
            <div class="school-info">
              <div class="school-name">${settings.schoolName}</div>
              <div class="school-address">${settings.schoolAddress}</div>
              <div class="school-contact">
                ${settings.schoolPhone} | ${settings.schoolEmail} | ${settings.schoolWebsite}
              </div>
            </div>
          </div>
          
          <div class="receipt-title">${settings.receiptTitle}</div>
          
          <div class="receipt-details">
            <div class="receipt-detail">
              <div class="detail-label">Receipt No:</div>
              <div class="detail-value">${paymentData.receiptNumber}</div>
            </div>
            
            <div class="receipt-detail">
              <div class="detail-label">Date:</div>
              <div class="detail-value">${receiptDate}</div>
            </div>
          </div>
          
          <div class="student-details">
            <div class="student-details-title">Student Information</div>
            
            <div class="student-detail-row">
              <div class="student-detail-item">
                <span class="student-detail-label">Name:</span>
                <span>${paymentData.studentInfo?.name || 'Student Name'}</span>
              </div>
              
              <div class="student-detail-item">
                <span class="student-detail-label">ID:</span>
                <span>${paymentData.studentInfo?.id || 'Student ID'}</span>
              </div>
            </div>
            
            <div class="student-detail-row">
              <div class="student-detail-item">
                <span class="student-detail-label">Class:</span>
                <span>${paymentData.studentInfo?.class || 'Class'}</span>
              </div>
              
              <div class="student-detail-item">
                <span class="student-detail-label">Section:</span>
                <span>${paymentData.studentInfo?.section || 'Section'}</span>
              </div>
            </div>
          </div>
          
          <table class="fee-table">
            <thead>
              <tr>
                <th style="width: 5%;">S.No</th>
                <th style="width: 15%;">Month</th>
                <th style="width: 30%;">Fee Type</th>
                <th style="width: 12.5%;" class="fee-amount">Amount</th>
                <th style="width: 12.5%;" class="fee-fine">Fine</th>
                <th style="width: 12.5%;" class="fee-discount">Discount</th>
                <th style="width: 12.5%;" class="fee-net">Net Amount</th>
              </tr>
            </thead>
            <tbody>
              ${paymentData.fees.map((fee, index) => `
                <tr>
                  <td>${index + 1}</td>
                  <td>${fee.month}</td>
                  <td>${fee.name}</td>
                  <td class="fee-amount">${settings.currency}${formatCurrency(fee.amount, settings.locale)}</td>
                  <td class="fee-fine">${fee.fine ? `${settings.currency}${formatCurrency(fee.fine, settings.locale)}` : '-'}</td>
                  <td class="fee-discount">${fee.discount ? `${settings.currency}${formatCurrency(fee.discount, settings.locale)}` : '-'}</td>
                  <td class="fee-net">${settings.currency}${formatCurrency(fee.netAmount, settings.locale)}</td>
                </tr>
              `).join('')}
            </tbody>
          </table>
          
          <div class="payment-summary">
            <div class="payment-totals">
              <div class="payment-row">
                <div class="payment-label">Previous Balance:</div>
                <div class="payment-value">${settings.currency}${formatCurrency(paymentData.balance, settings.locale)}</div>
              </div>
              
              <div class="payment-row">
                <div class="payment-label">Current Fees:</div>
                <div class="payment-value">${settings.currency}${formatCurrency(paymentData.total, settings.locale)}</div>
              </div>
              
              <div class="payment-row">
                <div class="payment-label">Total Due:</div>
                <div class="payment-value">${settings.currency}${formatCurrency(paymentData.finalTotal, settings.locale)}</div>
              </div>
              
              <div class="payment-row">
                <div class="payment-label">Concession:</div>
                <div class="payment-value">${settings.currency}${formatCurrency(paymentData.concession, settings.locale)}</div>
              </div>
              
              <div class="payment-row">
                <div class="payment-label">Late Fee:</div>
                <div class="payment-value">${settings.currency}${formatCurrency(paymentData.lateFee, settings.locale)}</div>
              </div>
              
              <div class="payment-row payment-total">
                <div class="payment-label">Received Amount:</div>
                <div class="payment-value">${settings.currency}${formatCurrency(paymentData.received, settings.locale)}</div>
              </div>
              
              <div class="payment-row">
                <div class="payment-label">Remaining Balance:</div>
                <div class="payment-value">${settings.currency}${formatCurrency(paymentData.remaining, settings.locale)}</div>
              </div>
            </div>
          </div>
          
          <div class="payment-method">
            <div class="payment-method-label">Payment Method:</div>
            <div class="payment-method-value">${paymentData.paymentMethod}</div>
          </div>
          
          ${paymentData.note ? `
            <div class="payment-note">
              <strong>Note:</strong> ${paymentData.note}
            </div>
          ` : ''}
          
          <div class="signatures">
            <div class="signature">
              <div class="signature-line"></div>
              <div class="signature-name">Cashier's Signature</div>
            </div>
            
            <div class="signature">
              <div class="signature-line"></div>
              <div class="signature-name">Authorized Signature</div>
            </div>
          </div>
          
          <div class="receipt-footer">
            ${settings.footerText}
          </div>
        </div>
        
        ${(settings.printButton || settings.downloadButton) ? `
          <div class="action-buttons no-print">
            ${settings.printButton ? `
              <button class="receipt-button print-button" onclick="window.print()">Print Receipt</button>
            ` : ''}
            
            ${settings.downloadButton ? `
              <button class="receipt-button download-button" onclick="downloadReceipt()">Download PDF</button>
            ` : ''}
          </div>
        ` : ''}
      </div>
      
      ${settings.downloadButton ? `
        <script>
          function downloadReceipt() {
            // Implementation depends on which PDF library you're using
            alert('PDF download functionality needs to be implemented with a library like jsPDF or using server-side PDF generation');
          }
        </script>
      ` : ''}
    </body>
    </html>
  `;
  
  return html;
}

/**
 * Format a currency value
 * @param {Number|String} value - Value to format
 * @param {String} locale - Locale for formatting (e.g., 'en-US', 'en-IN')
 * @returns {String} - Formatted currency value without currency symbol
 */
function formatCurrency(value, locale = 'en-IN') {
  const numValue = parseFloat(value);
  
  if (isNaN(numValue)) {
    return '0.00';
  }
  
  return new Intl.NumberFormat(locale, {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  }).format(numValue);
}

/**
 * Format a date string
 * @param {String} dateString - Date string (e.g., '31-12-2023')
 * @param {Object} options - Date formatting options
 * @returns {String} - Formatted date
 */
function formatDate(dateString, options) {
  // Handle DD-MM-YYYY format
  if (/^\d{2}-\d{2}-\d{4}$/.test(dateString)) {
    const [day, month, year] = dateString.split('-').map(part => parseInt(part, 10));
    const date = new Date(year, month - 1, day); // Month is 0-indexed
    
    if (!isNaN(date.getTime())) {
      return date.toLocaleDateString(undefined, options);
    }
  }
  
  // Fallback to original string if parsing fails
  return dateString;
}

/**
 * Generate and open a receipt in a new window
 * @param {Object} paymentData - Payment data from FeeCollect
 * @param {Object} options - Receipt options
 */
function openReceiptInNewWindow(paymentData, options = {}) {
  const receiptHTML = generateReceiptHTML(paymentData, options);
  
  // Open new window
  const receiptWindow = window.open('', '_blank');
  
  // Write receipt HTML to the new window
  receiptWindow.document.write(receiptHTML);
  receiptWindow.document.close();
  
  // Focus the new window
  receiptWindow.focus();
}

/**
 * Generate and download a receipt PDF
 * @param {Object} paymentData - Payment data from FeeCollect
 * @param {Object} options - Receipt options
 */
function downloadReceiptPDF(paymentData, options = {}) {
  // This function would use a PDF library to generate a PDF from the receipt HTML
  // Example implementation using jsPDF and html2canvas:
  
  /*
  // Include these libraries in your project:
  // <script src="https://cdnjs.cloudflare.com/ajax/libs/jspdf/2.5.1/jspdf.umd.min.js"></script>
  // <script src="https://cdnjs.cloudflare.com/ajax/libs/html2canvas/1.4.1/html2canvas.min.js"></script>
  
  const receiptHTML = generateReceiptHTML(paymentData, { ...options, printButton: false, downloadButton: false });
  
  // Create temporary container
  const container = document.createElement('div');
  container.innerHTML = receiptHTML;
  container.style.position = 'absolute';
  container.style.left = '-9999px';
  document.body.appendChild(container);
  
  const receiptElement = container.querySelector('.receipt');
  
  // Use html2canvas to create an image of the receipt
  html2canvas(receiptElement).then(canvas => {
    const imgData = canvas.toDataURL('image/png');
    const pdf = new jspdf.jsPDF({
      orientation: 'portrait',
      unit: 'mm',
      format: 'a4'
    });
    
    // Calculate dimensions to fit the receipt in the PDF
    const imgWidth = 210; // A4 width in mm
    const pageHeight = 297; // A4 height in mm
    const imgHeight = (canvas.height * imgWidth) / canvas.width;
    
    pdf.addImage(imgData, 'PNG', 0, 0, imgWidth, imgHeight);
    
    // Download the PDF
    pdf.save(`Receipt-${paymentData.receiptNumber}.pdf`);
    
    // Clean up
    document.body.removeChild(container);
  });
  */
  
  console.log('PDF download functionality needs to be implemented with a library like jsPDF');
  alert('PDF download functionality needs to be implemented with a library like jsPDF or using server-side PDF generation');
}

// Export the receipt functions if in a CommonJS environment
if (typeof module !== 'undefined' && module.exports) {
  module.exports = {
    generateReceiptHTML,
    openReceiptInNewWindow,
    downloadReceiptPDF
  };
}
