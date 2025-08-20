// Server-side implementation examples for FeeCollect integration

/**
 * Example Node.js/Express API endpoint for handling fee payment submission
 * This shows how to process, validate and store data from FeeCollect
 */

// Express route for payment processing
app.post('/api/fee-payments', async (req, res) => {
  try {
    // Get payment data from request body
    const { payment, fees } = req.body;
    
    // Validate request data
    const validationResult = validatePaymentRequest(payment, fees);
    if (!validationResult.valid) {
      return res.status(400).json({
        success: false,
        message: 'Validation failed',
        errors: validationResult.errors
      });
    }
    
    // Start database transaction
    const connection = await getDbConnection();
    try {
      await connection.beginTransaction();
      
      // 1. Insert main payment record
      const paymentRecord = {
        receipt_number: payment.receiptNumber,
        payment_date: formatDateForDB(payment.date),
        student_id: payment.studentInfo.id,
        total_amount: parseFloat(payment.total),
        balance_amount: parseFloat(payment.balance),
        final_total_amount: parseFloat(payment.finalTotal),
        concession_amount: parseFloat(payment.concession),
        late_fee_amount: parseFloat(payment.lateFee),
        received_amount: parseFloat(payment.received),
        remaining_amount: parseFloat(payment.remaining),
        payment_method: payment.paymentMethod,
        payment_note: payment.note,
        payment_status: parseFloat(payment.remaining) > 0 ? 'PARTIAL' : 'COMPLETE',
        created_by: req.user.id,
        created_at: new Date()
      };
      
      const [paymentResult] = await connection.query(
        'INSERT INTO fee_payments SET ?', 
        paymentRecord
      );
      
      // 2. Insert payment detail records for each fee
      for (const fee of fees) {
        const feeDetail = {
          receipt_number: payment.receiptNumber,
          fee_id: fee.id,
          fee_month: fee.month,
          fee_name: fee.name,
          base_amount: fee.amount,
          fine_amount: fee.fine || 0,
          discount_amount: fee.discount || 0,
          net_amount: fee.netAmount,
          created_at: new Date()
        };
        
        await connection.query(
          'INSERT INTO fee_payment_details SET ?',
          feeDetail
        );
        
        // 3. Update student fee status in separate table (if using that approach)
        await connection.query(
          `UPDATE student_fees 
           SET payment_status = 'PAID', 
               payment_date = ?, 
               receipt_number = ? 
           WHERE student_id = ? 
             AND fee_id = ? 
             AND fee_month = ?`,
          [formatDateForDB(payment.date), payment.receiptNumber, payment.studentInfo.id, fee.id, fee.month]
        );
      }
      
      // 4. Update student balance if there's a remaining amount
      if (parseFloat(payment.remaining) > 0) {
        await connection.query(
          `UPDATE students 
           SET balance_amount = ? 
           WHERE student_id = ?`,
          [parseFloat(payment.remaining), payment.studentInfo.id]
        );
      } else {
        // Clear balance if payment is complete
        await connection.query(
          `UPDATE students 
           SET balance_amount = 0 
           WHERE student_id = ?`,
          [payment.studentInfo.id]
        );
      }
      
      // Commit the transaction
      await connection.commit();
      
      // Generate receipt for download/print
      const receiptUrl = generateReceiptUrl(payment.receiptNumber);
      
      // Return success response
      return res.status(200).json({
        success: true,
        message: 'Payment processed successfully',
        receiptNumber: payment.receiptNumber,
        receiptUrl: receiptUrl
      });
      
    } catch (error) {
      // Rollback transaction on error
      await connection.rollback();
      console.error('Database error:', error);
      
      return res.status(500).json({
        success: false,
        message: 'Server error processing payment',
        error: error.message
      });
    } finally {
      // Release connection
      if (connection) connection.release();
    }
    
  } catch (error) {
    console.error('Payment processing error:', error);
    return res.status(500).json({
      success: false,
      message: 'Server error',
      error: error.message
    });
  }
});

/**
 * Express route for getting student fee data
 * This prepares data in the format expected by FeeCollect
 */
app.get('/api/students/:studentId/fees', async (req, res) => {
  try {
    const { studentId } = req.params;
    
    // Get student information
    const [studentRows] = await connection.query(
      'SELECT * FROM students WHERE student_id = ?',
      [studentId]
    );
    
    if (studentRows.length === 0) {
      return res.status(404).json({
        success: false,
        message: 'Student not found'
      });
    }
    
    const student = studentRows[0];
    
    // Get student's fee structure
    const [feeRows] = await connection.query(
      `SELECT sf.fee_id, f.fee_name, sf.fee_month, sf.fee_amount, 
              sf.fine_amount, sf.discount_amount, sf.payment_status
       FROM student_fees sf
       JOIN fee_types f ON sf.fee_id = f.id
       WHERE sf.student_id = ?
       ORDER BY sf.fee_id, sf.fee_month`,
      [studentId]
    );
    
    // Transform database data to FeeCollect format
    const feeData = transformToFeeCollectFormat(feeRows);
    
    // Return data in the format expected by FeeCollect
    return res.status(200).json({
      success: true,
      name: student.student_name,
      id: student.student_id,
      class: student.class_name,
      section: student.section,
      balance: student.balance_amount || 0,
      fees: feeData
    });
    
  } catch (error) {
    console.error('Error fetching student fees:', error);
    return res.status(500).json({
      success: false,
      message: 'Server error',
      error: error.message
    });
  }
});

/**
 * Transform database fee rows to FeeCollect format
 * @param {Array} feeRows - Fee rows from database
 * @returns {Object} - Data in FeeCollect format
 */
function transformToFeeCollectFormat(feeRows) {
  // Group by fee type
  const feeGroups = {};
  
  // Map of month names to array indices (April = 0, May = 1, etc.)
  const monthMap = {
    'April': 0, 'May': 1, 'June': 2, 'July': 3, 'August': 4, 'September': 5,
    'October': 6, 'November': 7, 'December': 8, 'January': 9, 'February': 10, 'March': 11
  };
  
  // Process each fee row
  feeRows.forEach(row => {
    if (!feeGroups[row.fee_id]) {
      feeGroups[row.fee_id] = {
        id: row.fee_id,
        name: row.fee_name,
        amounts: Array(12).fill(0),
        fines: Array(12).fill(0),
        discounts: Array(12).fill(0)
      };
    }
    
    // Only include unpaid fees
    if (row.payment_status !== 'PAID') {
      const monthIndex = monthMap[row.fee_month];
      if (monthIndex !== undefined) {
        feeGroups[row.fee_id].amounts[monthIndex] = row.fee_amount;
        feeGroups[row.fee_id].fines[monthIndex] = row.fine_amount || 0;
        feeGroups[row.fee_id].discounts[monthIndex] = row.discount_amount || 0;
      }
    }
  });
  
  // Convert to array format
  return Object.values(feeGroups);
}

/**
 * Validate payment request data
 * @param {Object} payment - Payment data
 * @param {Array} fees - Fee items
 * @returns {Object} - Validation result
 */
function validatePaymentRequest(payment, fees) {
  const errors = [];
  
  // Check required fields
  if (!payment.receiptNumber) {
    errors.push('Receipt number is required');
  }
  
  if (!payment.date) {
    errors.push('Payment date is required');
  }
  
  if (!payment.studentInfo || !payment.studentInfo.id) {
    errors.push('Student information is missing');
  }
  
  // Validate amounts
  if (isNaN(parseFloat(payment.received)) || parseFloat(payment.received) <= 0) {
    errors.push('Received amount must be greater than zero');
  }
  
  // Validate fee items
  if (!Array.isArray(fees) || fees.length === 0) {
    errors.push('No fee items selected');
  }
  
  // Validate fee items have required fields
  if (Array.isArray(fees)) {
    fees.forEach((fee, index) => {
      if (!fee.id || !fee.month || !fee.name || isNaN(parseFloat(fee.amount))) {
        errors.push(`Invalid fee item at position ${index + 1}`);
      }
    });
  }
  
  // Business logic validations
  
  // Ensure fees aren't already paid
  if (Array.isArray(fees)) {
    // This would involve checking against your database
    // Example code only - you'd need to implement actual check
    const duplicatePaidFees = checkForDuplicatePaidFees(payment.studentInfo.id, fees);
    if (duplicatePaidFees.length > 0) {
      errors.push(`Some fees have already been paid: ${duplicatePaidFees.join(', ')}`);
    }
  }
  
  // Validate concession limit if applicable
  const maxConcessionPercentage = 0.2; // 20% max concession
  const totalAmount = parseFloat(payment.total);
  const concessionAmount = parseFloat(payment.concession);
  
  if (!isNaN(totalAmount) && !isNaN(concessionAmount) && 
      totalAmount > 0 && concessionAmount > totalAmount * maxConcessionPercentage) {
    errors.push(`Concession exceeds maximum allowed (${maxConcessionPercentage * 100}%)`);
  }
  
  return {
    valid: errors.length === 0,
    errors: errors
  };
}

/**
 * Format date for database storage
 * @param {String} dateString - Date string in format DD-MM-YYYY
 * @returns {String} - Date in YYYY-MM-DD format
 */
function formatDateForDB(dateString) {
  // Convert from DD-MM-YYYY to YYYY-MM-DD
  const parts = dateString.split('-');
  if (parts.length === 3) {
    return `${parts[2]}-${parts[1]}-${parts[0]}`;
  }
  return dateString; // Return as is if format doesn't match
}

/**
 * Generate URL for receipt download/print
 * @param {String} receiptNumber - Receipt number
 * @returns {String} - Receipt URL
 */
function generateReceiptUrl(receiptNumber) {
  return `/receipts/${receiptNumber}`;
}

// This is a stub function - in a real app, you'd check against your database
function checkForDuplicatePaidFees(studentId, fees) {
  // Example implementation
  return []; // No duplicates in this example
}

/**
 * Database schema for fee payment system
 * 
 * This shows the database structure needed to store data from FeeCollect
 */

// SQL Schema for fee payments
`
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
`
