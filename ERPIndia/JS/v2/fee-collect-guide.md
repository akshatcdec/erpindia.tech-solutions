# FeeCollect Implementation Guide

This guide provides detailed instructions for implementing and integrating the FeeCollect library into your educational fee management system.

## Table of Contents

1. [System Overview](#system-overview)
2. [Requirements](#requirements)
3. [Implementation Steps](#implementation-steps)
4. [Data Structure](#data-structure)
5. [Database Integration](#database-integration)
6. [Validation Framework](#validation-framework)
7. [Receipt Generation](#receipt-generation)
8. [Advanced Customization](#advanced-customization)
9. [Security Considerations](#security-considerations)
10. [Troubleshooting](#troubleshooting)

## System Overview

FeeCollect is a comprehensive library for managing educational fee collection with these key features:

- Fee selection with monthly granularity
- Fine and discount handling
- Previous balance management
- Payment processing with multiple payment methods
- Receipt generation
- Validation framework
- Database integration

## Requirements

### Client-Side

- Modern web browser (Chrome, Firefox, Safari, Edge)
- JavaScript enabled
- Minimum screen resolution: 768px width

### Server-Side (for integration)

- Any server environment (Node.js, PHP, Java, etc.)
- Database system (MySQL, PostgreSQL, MongoDB, etc.)
- Ability to handle JSON data

## Implementation Steps

### 1. Install the Library

**Using NPM:**

```bash
npm install feecollect
```

**Manual Installation:**

1. Download `FeeCollect.js` and `FeeCollect.css`
2. Include in your HTML:

```html
<link rel="stylesheet" href="path/to/FeeCollect.css">
<script src="path/to/FeeCollect.js"></script>
```

### 2. Set Up HTML Structure

Create a container element:

```html
<div id="feeCollectContainer"></div>
```

### 3. Prepare Data Structure

Format your fee data according to the required structure:

```javascript
const feeData = {
  feeData: [
    {
      id: 1,
      name: "TUITION FEE",
      amounts: [500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500],
      fines: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],     // Optional
      discounts: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]  // Optional
    },
    // More fee types...
  ],
  studentInfo: {
    name: "Student Name",
    id: "STU001",
    class: "Grade 10",
    section: "A",
    balance: 1500  // Optional: previous balance
  }
};
```

### 4. Initialize FeeCollect

```javascript
const feeCollect = new FeeCollect('#feeCollectContainer', feeData, {
  // Configuration options
  currency: '₹',
  showFineDiscount: true,
  autoUpdateReceived: true,
  // Callbacks
  callbacks: {
    onInit: function() {
      console.log('FeeCollect initialized');
    },
    onPaymentComplete: function(data) {
      // Process payment data
      console.log('Payment completed:', data);
      savePaymentToServer(data);
    }
  }
});
```

### 5. Handle Payment Data

Create an endpoint to receive and process payment data:

```javascript
// Example client-side function to send data to server
function savePaymentToServer(paymentData) {
  fetch('/api/fee-payments', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      payment: paymentData,
      fees: paymentData.fees
    })
  })
  .then(response => response.json())
  .then(result => {
    if (result.success) {
      // Show success message
      alert(`Payment successful! Receipt Number: ${result.receiptNumber}`);
      
      // Open receipt in new window
      if (result.receiptUrl) {
        window.open(result.receiptUrl, '_blank');
      }
      
      // Clear the form for next entry
      feeCollect.clearSelectedFees();
    } else {
      // Show error message
      alert(`Error: ${result.message}`);
    }
  })
  .catch(error => {
    console.error('Error saving payment:', error);
    alert('Connection error. Please try again.');
  });
}
```

## Data Structure

### Fee Data Structure

```javascript
{
  feeData: [
    {
      id: Number,        // Unique identifier for the fee
      name: String,      // Name of the fee
      amounts: Array,    // 12 amounts for each month (April to March)
      fines: Array,      // Optional: 12 fine amounts for each month
      discounts: Array   // Optional: 12 discount amounts for each month
    },
    // More fee items...
  ],
  studentInfo: {
    name: String,        // Student name
    id: String,          // Student ID
    class: String,       // Class/Grade
    section: String,     // Section
    balance: Number      // Optional: Previous balance amount
  }
}
```

### Payment Data Structure

The data returned by the `onPaymentComplete` callback:

```javascript
{
  fees: [                // Array of selected fee items
    {
      id: Number,        // Fee ID
      month: String,     // Month name (e.g., "April")
      name: String,      // Fee name
      amount: Number,    // Base fee amount
      fine: Number,      // Fine amount (if any)
      discount: Number,  // Discount amount (if any)
      netAmount: Number  // Calculated net amount
    },
    // More selected fees...
  ],
  balance: String,       // Previous balance amount
  total: String,         // Total base fee amount
  finalTotal: String,    // Grand total (balance + total)
  concession: String,    // Total concession/discount amount
  lateFee: String,       // Total late fee/fine amount
  received: String,      // Amount received from customer
  remaining: String,     // Remaining balance (if any)
  note: String,          // Optional payment note
  paymentMethod: String, // Selected payment method
  receiptNumber: String, // Generated receipt number
  date: String,          // Formatted payment date
  studentInfo: {         // Student information (from original data)
    name: String,
    id: String,
    class: String,
    section: String
  }
}
```

## Database Integration

### Database Schema

Here's a recommended database schema for storing fee collection data:

```sql
-- Students table
CREATE TABLE students (
    student_id VARCHAR(20) PRIMARY KEY,
    student_name VARCHAR(100) NOT NULL,
    class_name VARCHAR(50) NOT NULL,
    section VARCHAR(10) NOT NULL,
    balance_amount DECIMAL(10, 2) DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Fee types table
CREATE TABLE fee_types (
    id INT AUTO_INCREMENT PRIMARY KEY,
    fee_name VARCHAR(100) NOT NULL,
    fee_description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Student fees table (fee allocation to students)
CREATE TABLE student_fees (
    id INT AUTO_INCREMENT PRIMARY KEY,
    student_id VARCHAR(20) NOT NULL,
    fee_id INT NOT NULL,
    fee_month VARCHAR(20) NOT NULL,
    fee_amount DECIMAL(10, 2) NOT NULL,
    fine_amount DECIMAL(10, 2) DEFAULT 0,
    discount_amount DECIMAL(10, 2) DEFAULT 0,
    payment_status ENUM('PENDING', 'PAID', 'WAIVED') DEFAULT 'PENDING',
    payment_date DATE NULL,
    receipt_number VARCHAR(50) NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (student_id) REFERENCES students(student_id),
    FOREIGN KEY (fee_id) REFERENCES fee_types(id)
);

-- Fee payments table (main payment records)
CREATE TABLE fee_payments (
    receipt_number VARCHAR(50) PRIMARY KEY,
    payment_date DATE NOT NULL,
    student_id VARCHAR(20) NOT NULL,
    total_amount DECIMAL(10, 2) NOT NULL,
    balance_amount DECIMAL(10, 2) DEFAULT 0,
    final_total_amount DECIMAL(10, 2) NOT NULL,
    concession_amount DECIMAL(10, 2) DEFAULT 0,
    late_fee_amount DECIMAL(10, 2) DEFAULT 0,
    received_amount DECIMAL(10, 2) NOT NULL,
    remaining_amount DECIMAL(10, 2) DEFAULT 0,
    payment_method VARCHAR(20) NOT NULL,
    payment_note TEXT,
    payment_status ENUM('COMPLETE', 'PARTIAL') NOT NULL,
    created_by INT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (student_id) REFERENCES students(student_id)
);

-- Fee payment details table (fee items in a payment)
CREATE TABLE fee_payment_details (
    id INT AUTO_INCREMENT PRIMARY KEY,
    receipt_number VARCHAR(50) NOT NULL,
    fee_id INT NOT NULL,
    fee_month VARCHAR(20) NOT NULL,
    fee_name VARCHAR(100) NOT NULL,
    base_amount DECIMAL(10, 2) NOT NULL,
    fine_amount DECIMAL(10, 2) DEFAULT 0,
    discount_amount DECIMAL(10, 2) DEFAULT 0,
    net_amount DECIMAL(10, 2) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (receipt_number) REFERENCES fee_payments(receipt_number),
    FOREIGN KEY (fee_id) REFERENCES fee_types(id)
);
```

### Server-Side Processing

When handling payment data on the server side, follow these steps:

1. **Validate the payment data** using the validation framework
2. **Start a database transaction** to ensure data integrity
3. **Insert the main payment record** into `fee_payments` table
4. **Insert payment details** for each fee item into `fee_payment_details` table
5. **Update student fee status** in `student_fees` table
6. **Update student balance** in `students` table if there's a remaining amount
7. **Commit the transaction** if all operations succeed
8. **Generate a receipt** for the payment
9. **Return success response** with receipt information

See the [Server Integration Example](fee-collect-api.js) for detailed implementation.

## Validation Framework

FeeCollect includes a comprehensive validation framework for both client-side and server-side validation.

### Client-Side Validation

```javascript
// Set up validation rules
const customRules = {
  business: {
    maxConcessionPercent: 20, // Max concession allowed (20%)
    requireNoteForPartialPayment: true
  }
};

// Validate before processing payment
document.getElementById('validateButton').addEventListener('click', function() {
  const paymentData = feeCollect.getPaymentDetails();
  const selectedFees = feeCollect.getSelectedFees();
  
  // Add fees to payment data for validation
  paymentData.fees = selectedFees;
  
  // Use the validation framework
  const validationResult = validateFeePayment(paymentData, customRules);
  
  if (validationResult.valid) {
    // Show success message
    showMessage('Validation successful!');
    enablePaymentButton();
  } else {
    // Show error messages
    showMessage('Validation failed:');
    validationResult.errors.forEach(error => {
      showErrorMessage(error);
    });
  }
});
```

### Server-Side Validation

```javascript
// Server-side validation in API endpoint
app.post('/api/fee-payments', (req, res) => {
  const { payment, fees } = req.body;
  
  // Combine data for validation
  const paymentData = {
    ...payment,
    fees: fees
  };
  
  // Validate with server-side rules
  const serverRules = {
    business: {
      maxConcessionPercent: 20,
      requireApprovalForConcessionAbove: 10,
      balanceThreshold: 5000
    }
  };
  
  const validationResult = validateFeePayment(paymentData, serverRules);
  
  if (!validationResult.valid) {
    return res.status(400).json({
      success: false,
      message: 'Validation failed',
      errors: validationResult.errors
    });
  }
  
  // Process payment if validation passes
  // ...
});
```

See the [Validation Framework](fee-collect-validation.js) for detailed implementation.

## Receipt Generation

FeeCollect provides a flexible receipt generation system.

### Client-Side Receipt Generation

```javascript
// Generate and open receipt in new window
function showReceipt(paymentData) {
  // Configure receipt options
  const receiptOptions = {
    schoolName: 'ABC School',
    schoolLogo: '/images/school-logo.png',
    schoolAddress: '123 Education St, City, State - 12345',
    schoolPhone: '123-456-7890',
    schoolEmail: 'info@abcschool.edu',
    currency: '₹'
  };
  
  // Open receipt in new window
  openReceiptInNewWindow(paymentData, receiptOptions);
}
```

### Server-Side Receipt Generation

For more advanced receipt generation, use server-side PDF generation:

1. Create an API endpoint for receipt generation:

```javascript
app.get('/receipts/:receiptNumber', async (req, res) => {
  const { receiptNumber } = req.params;
  
  // Fetch payment data from database
  const paymentData = await getPaymentDataFromDatabase(receiptNumber);
  
  if (!paymentData) {
    return res.status(404).send('Receipt not found');
  }
  
  // Generate receipt HTML
  const receiptHTML = generateReceiptHTML(paymentData, {
    schoolName: 'ABC School',
    // Other options...
  });
  
  // Option 1: Return HTML directly
  res.send(receiptHTML);
  
  // Option 2: Generate PDF and return it
  // const pdfBuffer = await generatePDF(receiptHTML);
  // res.setHeader('Content-Type', 'application/pdf');
  // res.setHeader('Content-Disposition', `inline; filename="Receipt-${receiptNumber}.pdf"`);
  // res.send(pdfBuffer);
});
```

See the [Receipt Generator](fee-collect-receipt.js) for detailed implementation.

## Advanced Customization

### Modifying Styles

Override default styles by adding custom CSS:

```css
/* Change header color */
.fee-collect-header {
  background-color: #2c3e50;
  color: white;
}

/* Change button colors */
.fee-collect-add-month {
  background-color: #3498db;
}

.fee-collect-receive-button {
  background-color: #2ecc71;
}

/* Change payment row background */
.fee-collect-payment-row {
  background-color: #34495e;
}
```

### Extending Functionality

You can extend FeeCollect by creating wrapper classes or plugins:

```javascript
// Example: Create a wrapper class with additional functionality
class AdvancedFeeCollect {
  constructor(container, data, options) {
    // Create base FeeCollect instance
    this.feeCollect = new FeeCollect(container, data, options);
    
    // Add additional functionality
    this.setupAdvancedFeatures();
  }
  
  setupAdvancedFeatures() {
    // Add custom buttons, features, etc.
    const container = document.querySelector(this.feeCollect.container);
    
    const exportButton = document.createElement('button');
    exportButton.textContent = 'Export Data';
    exportButton.addEventListener('click', () => this.exportData());
    
    container.appendChild(exportButton);
  }
  
  exportData() {
    const data = this.feeCollect.getSelectedFees();
    // Export to CSV, Excel, etc.
    console.log('Exporting data:', data);
  }
  
  // Proxy methods to the original FeeCollect instance
  getSelectedFees() {
    return this.feeCollect.getSelectedFees();
  }
  
  // Add more proxy methods as needed
}
```

## Security Considerations

### Input Validation

Always validate all input data on both client and server side:

- Use the provided validation framework
- Sanitize input to prevent SQL injection and XSS attacks
- Verify numeric values are within acceptable ranges

### Authentication and Authorization

Implement proper authentication and authorization:

- Ensure only authorized users can access fee collection functionality
- Implement role-based permissions for different operations (e.g., cashier, accountant, admin)
- Require additional authorization for certain actions (e.g., giving high concessions)

### Data Protection

Protect sensitive payment information:

- Use HTTPS for all communication
- Encrypt sensitive data in the database
- Implement proper backup procedures
- Comply with relevant data protection regulations

### Transaction Security

Ensure financial transaction security:

- Use database transactions to maintain data integrity
- Implement idempotent API endpoints to prevent duplicate payments
- Add fraud detection measures for unusual payment patterns
- Maintain detailed audit logs of all financial transactions

## Troubleshooting

### Common Issues

1. **UI rendering issues**:
   - Check if you're using the latest version of the library
   - Verify that the CSS file is properly loaded
   - Inspect the console for JavaScript errors

2. **Calculation errors**:
   - Ensure all numeric values are properly parsed (use `parseInt` or `parseFloat`)
   - Check for edge cases with zero values or missing fields

3. **Integration problems**:
   - Verify data structure matches the expected format
   - Check server endpoints for errors in handling the data
   - Enable detailed error logging for debugging

### Debugging Tips

1. Use the browser's developer tools to inspect elements and monitor network requests
2. Add debug logging in key methods to trace data flow
3. Validate data at each step to identify where issues occur
4. Create minimal test cases to isolate problems

### Support Resources

For additional support:

- Check the [API Documentation](api-documentation.md)
- Visit the [GitHub repository](https://github.com/feecollect/feecollect)
- Join the [community forum](https://forum.feecollect.org)
- Contact support at support@feecollect.org
