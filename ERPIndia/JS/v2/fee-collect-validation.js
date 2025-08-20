/**
 * FeeCollect Validation Framework
 * 
 * This file provides a comprehensive validation system for FeeCollect
 * that can be used both on the client and server side.
 */

/**
 * Validation rules for fee payment
 */
const ValidationRules = {
  // Student validation rules
  student: {
    required: true,
    id: { required: true, minLength: 3 },
    name: { required: true, minLength: 2 },
    class: { required: true }
  },
  
  // Fee item validation rules
  feeItem: {
    id: { required: true, isNumeric: true },
    month: { required: true, isValidMonth: true },
    name: { required: true },
    amount: { required: true, isNumeric: true, min: 0 },
    fine: { isNumeric: true, min: 0 },
    discount: { isNumeric: true, min: 0, maxPercent: 100 }
  },
  
  // Payment validation rules
  payment: {
    balance: { isNumeric: true, min: 0 },
    total: { required: true, isNumeric: true, min: 0 },
    finalTotal: { required: true, isNumeric: true, min: 0 },
    concession: { isNumeric: true, min: 0 },
    lateFee: { isNumeric: true, min: 0 },
    received: { required: true, isNumeric: true, min: 0 },
    remaining: { isNumeric: true, min: 0 },
    paymentMethod: { required: true, isValidPaymentMethod: true },
    receiptNumber: { required: true },
    date: { required: true, isValidDate: true }
  },
  
  // Business rules
  business: {
    minSelectedFees: 1,
    maxConcessionPercent: 20, // Maximum concession percentage allowed
    requireApprovalForConcessionAbove: 10, // Require approval for concession > 10%
    requireNoteForPartialPayment: true, // Require note when payment is partial
    balanceThreshold: 5000 // Warn when balance exceeds this threshold
  }
};

/**
 * Validate fee payment data
 * @param {Object} paymentData - Payment data from FeeCollect
 * @param {Object} customRules - Custom validation rules (optional)
 * @returns {Object} - Validation result { valid: boolean, errors: Array, warnings: Array }
 */
function validateFeePayment(paymentData, customRules = {}) {
  // Merge default rules with custom rules
  const rules = mergeRules(ValidationRules, customRules);
  
  const errors = [];
  const warnings = [];
  
  // Validate fees array
  if (!Array.isArray(paymentData.fees) || paymentData.fees.length === 0) {
    errors.push('No fees selected for payment');
  } else if (paymentData.fees.length < rules.business.minSelectedFees) {
    errors.push(`At least ${rules.business.minSelectedFees} fee must be selected`);
  } else {
    // Validate each fee item
    paymentData.fees.forEach((fee, index) => {
      validateFeeItem(fee, rules.feeItem, index, errors);
    });
  }
  
  // Validate payment details
  validatePaymentDetails(paymentData, rules.payment, errors);
  
  // Validate business rules
  validateBusinessRules(paymentData, rules.business, errors, warnings);
  
  return {
    valid: errors.length === 0,
    errors: errors,
    warnings: warnings
  };
}

/**
 * Validate a fee item
 * @param {Object} feeItem - Fee item to validate
 * @param {Object} rules - Validation rules for fee items
 * @param {Number} index - Index of the fee item
 * @param {Array} errors - Array to collect errors
 */
function validateFeeItem(feeItem, rules, index, errors) {
  // Required fields
  if (rules.id.required && !feeItem.id) {
    errors.push(`Fee at position ${index + 1} is missing an ID`);
  }
  
  if (rules.name.required && !feeItem.name) {
    errors.push(`Fee at position ${index + 1} is missing a name`);
  }
  
  if (rules.month.required && !feeItem.month) {
    errors.push(`Fee at position ${index + 1} is missing a month`);
  }
  
  if (rules.amount.required && (feeItem.amount === undefined || feeItem.amount === null)) {
    errors.push(`Fee at position ${index + 1} is missing an amount`);
  }
  
  // Type validations
  if (rules.isNumeric && feeItem.id && isNaN(parseInt(feeItem.id))) {
    errors.push(`Fee ID at position ${index + 1} must be a number`);
  }
  
  if (rules.amount.isNumeric && feeItem.amount !== undefined && isNaN(parseFloat(feeItem.amount))) {
    errors.push(`Fee amount at position ${index + 1} must be a number`);
  }
  
  if (rules.fine?.isNumeric && feeItem.fine !== undefined && isNaN(parseFloat(feeItem.fine))) {
    errors.push(`Fine amount at position ${index + 1} must be a number`);
  }
  
  if (rules.discount?.isNumeric && feeItem.discount !== undefined && isNaN(parseFloat(feeItem.discount))) {
    errors.push(`Discount amount at position ${index + 1} must be a number`);
  }
  
  // Range validations
  if (rules.amount.min !== undefined && feeItem.amount < rules.amount.min) {
    errors.push(`Fee amount at position ${index + 1} must be at least ${rules.amount.min}`);
  }
  
  if (rules.fine?.min !== undefined && feeItem.fine && feeItem.fine < rules.fine.min) {
    errors.push(`Fine amount at position ${index + 1} must be at least ${rules.fine.min}`);
  }
  
  if (rules.discount?.min !== undefined && feeItem.discount && feeItem.discount < rules.discount.min) {
    errors.push(`Discount amount at position ${index + 1} must be at least ${rules.discount.min}`);
  }
  
  // Month validation
  if (rules.month.isValidMonth && feeItem.month) {
    const validMonths = [
      'April', 'May', 'June', 'July', 'August', 'September', 
      'October', 'November', 'December', 'January', 'February', 'March'
    ];
    
    if (!validMonths.includes(feeItem.month)) {
      errors.push(`Invalid month "${feeItem.month}" at position ${index + 1}`);
    }
  }
  
  // Ensure discount isn't higher than amount if maxPercent is set
  if (rules.discount?.maxPercent !== undefined && feeItem.discount && feeItem.amount) {
    const discountPercent = (feeItem.discount / feeItem.amount) * 100;
    if (discountPercent > rules.discount.maxPercent) {
      errors.push(`Discount at position ${index + 1} exceeds maximum allowed (${rules.discount.maxPercent}%)`);
    }
  }
}

/**
 * Validate payment details
 * @param {Object} paymentData - Payment data
 * @param {Object} rules - Validation rules for payment
 * @param {Array} errors - Array to collect errors
 */
function validatePaymentDetails(paymentData, rules, errors) {
  // Required fields
  if (rules.total.required && (paymentData.total === undefined || paymentData.total === '')) {
    errors.push('Total amount is required');
  }
  
  if (rules.finalTotal.required && (paymentData.finalTotal === undefined || paymentData.finalTotal === '')) {
    errors.push('Final total amount is required');
  }
  
  if (rules.received.required && (paymentData.received === undefined || paymentData.received === '')) {
    errors.push('Received amount is required');
  }
  
  if (rules.paymentMethod.required && !paymentData.paymentMethod) {
    errors.push('Payment method is required');
  }
  
  if (rules.receiptNumber.required && !paymentData.receiptNumber) {
    errors.push('Receipt number is required');
  }
  
  if (rules.date.required && !paymentData.date) {
    errors.push('Payment date is required');
  }
  
  // Type validations
  if (rules.balance?.isNumeric && paymentData.balance !== undefined && isNaN(parseFloat(paymentData.balance))) {
    errors.push('Balance must be a number');
  }
  
  if (rules.total.isNumeric && paymentData.total !== undefined && isNaN(parseFloat(paymentData.total))) {
    errors.push('Total amount must be a number');
  }
  
  if (rules.finalTotal.isNumeric && paymentData.finalTotal !== undefined && isNaN(parseFloat(paymentData.finalTotal))) {
    errors.push('Final total amount must be a number');
  }
  
  if (rules.concession?.isNumeric && paymentData.concession !== undefined && isNaN(parseFloat(paymentData.concession))) {
    errors.push('Concession amount must be a number');
  }
  
  if (rules.lateFee?.isNumeric && paymentData.lateFee !== undefined && isNaN(parseFloat(paymentData.lateFee))) {
    errors.push('Late fee amount must be a number');
  }
  
  if (rules.received.isNumeric && paymentData.received !== undefined && isNaN(parseFloat(paymentData.received))) {
    errors.push('Received amount must be a number');
  }
  
  if (rules.remaining?.isNumeric && paymentData.remaining !== undefined && isNaN(parseFloat(paymentData.remaining))) {
    errors.push('Remaining amount must be a number');
  }
  
  // Range validations
  if (rules.balance?.min !== undefined && parseFloat(paymentData.balance) < rules.balance.min) {
    errors.push(`Balance must be at least ${rules.balance.min}`);
  }
  
  if (rules.total.min !== undefined && parseFloat(paymentData.total) < rules.total.min) {
    errors.push(`Total amount must be at least ${rules.total.min}`);
  }
  
  if (rules.finalTotal.min !== undefined && parseFloat(paymentData.finalTotal) < rules.finalTotal.min) {
    errors.push(`Final total amount must be at least ${rules.finalTotal.min}`);
  }
  
  if (rules.concession?.min !== undefined && parseFloat(paymentData.concession) < rules.concession.min) {
    errors.push(`Concession amount must be at least ${rules.concession.min}`);
  }
  
  if (rules.lateFee?.min !== undefined && parseFloat(paymentData.lateFee) < rules.lateFee.min) {
    errors.push(`Late fee amount must be at least ${rules.lateFee.min}`);
  }
  
  if (rules.received.min !== undefined && parseFloat(paymentData.received) < rules.received.min) {
    errors.push(`Received amount must be at least ${rules.received.min}`);
  }
  
  if (rules.remaining?.min !== undefined && parseFloat(paymentData.remaining) < rules.remaining.min) {
    errors.push(`Remaining amount must be at least ${rules.remaining.min}`);
  }
  
  // Payment method validation
  if (rules.paymentMethod.isValidPaymentMethod && paymentData.paymentMethod) {
    const validMethods = ['cash', 'bank', 'cheque', 'upi', 'paytm'];
    if (!validMethods.includes(paymentData.paymentMethod.toLowerCase())) {
      errors.push(`Invalid payment method "${paymentData.paymentMethod}"`);
    }
    
    // Special validations for specific payment methods
    if (paymentData.paymentMethod.toLowerCase() === 'cheque' && !paymentData.chequeNumber) {
      errors.push('Cheque number is required for cheque payments');
    }
    
    if (paymentData.paymentMethod.toLowerCase() === 'upi' && !paymentData.transactionId) {
      errors.push('Transaction ID is required for UPI payments');
    }
  }
  
  // Date validation
  if (rules.date.isValidDate && paymentData.date) {
    if (!isValidDateFormat(paymentData.date)) {
      errors.push('Invalid date format. Expected DD-MM-YYYY');
    }
  }
  
  // Ensure amounts add up correctly
  if (paymentData.finalTotal && paymentData.total && paymentData.balance) {
    const expectedFinalTotal = parseFloat(paymentData.total) + parseFloat(paymentData.balance);
    const actualFinalTotal = parseFloat(paymentData.finalTotal);
    
    if (Math.abs(expectedFinalTotal - actualFinalTotal) > 0.01) { // Allow for minor floating point differences
      errors.push('Final total does not match total + balance');
    }
  }
  
  if (paymentData.received && paymentData.finalTotal && paymentData.concession && paymentData.lateFee && paymentData.remaining) {
    const expectedRemaining = parseFloat(paymentData.finalTotal) - parseFloat(paymentData.concession) + parseFloat(paymentData.lateFee) - parseFloat(paymentData.received);
    const actualRemaining = parseFloat(paymentData.remaining);
    
    if (Math.abs(expectedRemaining - actualRemaining) > 0.01) { // Allow for minor floating point differences
      errors.push('Remaining amount does not match calculation');
    }
  }
}

/**
 * Validate business rules
 * @param {Object} paymentData - Payment data
 * @param {Object} rules - Business rules
 * @param {Array} errors - Array to collect errors
 * @param {Array} warnings - Array to collect warnings
 */
function validateBusinessRules(paymentData, rules, errors, warnings) {
  // Check concession percentage
  if (rules.maxConcessionPercent && paymentData.concession && paymentData.total) {
    const concessionPercent = (parseFloat(paymentData.concession) / parseFloat(paymentData.total)) * 100;
    
    if (concessionPercent > rules.maxConcessionPercent) {
      errors.push(`Concession exceeds maximum allowed (${rules.maxConcessionPercent}%)`);
    } else if (rules.requireApprovalForConcessionAbove && concessionPercent > rules.requireApprovalForConcessionAbove) {
      warnings.push(`Concession above ${rules.requireApprovalForConcessionAbove}% requires approval`);
    }
  }
  
  // Ensure note is provided for partial payments
  if (rules.requireNoteForPartialPayment && parseFloat(paymentData.remaining) > 0 && (!paymentData.note || paymentData.note.trim() === '')) {
    errors.push('Note is required for partial payments');
  }
  
  // Balance threshold warning
  if (rules.balanceThreshold && parseFloat(paymentData.balance) > rules.balanceThreshold) {
    warnings.push(`Student balance (${paymentData.balance}) exceeds threshold (${rules.balanceThreshold})`);
  }
  
  // Verify received amount is sufficient
  if (parseFloat(paymentData.received) <= 0) {
    errors.push('Received amount must be greater than zero');
  }
}

/**
 * Merge default rules with custom rules
 * @param {Object} defaultRules - Default validation rules
 * @param {Object} customRules - Custom validation rules
 * @returns {Object} - Merged rules
 */
function mergeRules(defaultRules, customRules) {
  const merged = JSON.parse(JSON.stringify(defaultRules)); // Deep clone
  
  // Merge top-level sections
  for (const section in customRules) {
    if (merged[section]) {
      // Merge existing section
      merged[section] = { ...merged[section], ...customRules[section] };
    } else {
      // Add new section
      merged[section] = customRules[section];
    }
  }
  
  return merged;
}

/**
 * Check if a string is a valid date format (DD-MM-YYYY)
 * @param {String} dateString - Date string to validate
 * @returns {Boolean} - Whether the date is valid
 */
function isValidDateFormat(dateString) {
  // Basic format check
  const regex = /^\d{2}-\d{2}-\d{4}$/;
  if (!regex.test(dateString)) {
    return false;
  }
  
  // Parse date parts
  const parts = dateString.split('-');
  const day = parseInt(parts[0], 10);
  const month = parseInt(parts[1], 10) - 1; // Months are 0-based
  const year = parseInt(parts[2], 10);
  
  // Create date object and verify parts
  const date = new Date(year, month, day);
  return date.getDate() === day && 
         date.getMonth() === month && 
         date.getFullYear() === year;
}

/**
 * Format validation errors for UI display
 * @param {Object} validationResult - Result from validateFeePayment
 * @returns {String} - HTML formatted error/warning messages
 */
function formatValidationMessages(validationResult) {
  let message = '';
  
  if (!validationResult.valid) {
    message += '<div class="validation-errors">';
    message += '<h3>Validation Errors:</h3>';
    message += '<ul>';
    validationResult.errors.forEach(error => {
      message += `<li>${error}</li>`;
    });
    message += '</ul>';
    message += '</div>';
  }
  
  if (validationResult.warnings && validationResult.warnings.length > 0) {
    message += '<div class="validation-warnings">';
    message += '<h3>Warnings:</h3>';
    message += '<ul>';
    validationResult.warnings.forEach(warning => {
      message += `<li>${warning}</li>`;
    });
    message += '</ul>';
    message += '</div>';
  }
  
  return message;
}

// Export the validation functions
if (typeof module !== 'undefined' && module.exports) {
  module.exports = {
    validateFeePayment,
    formatValidationMessages,
    ValidationRules
  };
}
