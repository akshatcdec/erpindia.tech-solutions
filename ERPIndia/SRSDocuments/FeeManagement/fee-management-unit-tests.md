# Fee Management System - Unit Test Cases

## 1. Fee Structure Setup Tests

### 1.1 Fee Head Management

#### Test Case FH-001: Create Fee Head
**Objective**: Verify that a new fee head can be created successfully.
**Test Steps**:
1. Navigate to Fee Head Management screen
2. Click "Add New Fee Head"
3. Enter fee head details:
   - Name: "Computer Lab Fee"
   - Code: "CLF"
   - Description: "Fee for computer laboratory usage"
   - Display Order: 5
   - Is Transport: No
   - Is Refundable: No
4. Click "Save"
**Expected Result**: 
- Fee head is created successfully and appears in the fee head list
- A success message is displayed
- Database record is created in the FeeHeads table

#### Test Case FH-002: Edit Fee Head
**Objective**: Verify that an existing fee head can be edited successfully.
**Test Steps**:
1. Navigate to Fee Head Management screen
2. Select an existing fee head from the list
3. Click "Edit"
4. Modify the description to "Updated description"
5. Click "Save"
**Expected Result**: 
- Fee head details are updated successfully
- A success message is displayed
- Database record is updated in the FeeHeads table

#### Test Case FH-003: Delete Fee Head
**Objective**: Verify that an existing fee head can be deleted (soft delete).
**Test Steps**:
1. Navigate to Fee Head Management screen
2. Select an existing fee head with no dependencies
3. Click "Delete"
4. Confirm deletion
**Expected Result**: 
- Fee head is marked as deleted (IsDeleted = 1)
- Fee head no longer appears in active fee head lists
- A success message is displayed

#### Test Case FH-004: Validate Duplicate Fee Head
**Objective**: Verify that duplicate fee head names are not allowed.
**Test Steps**:
1. Navigate to Fee Head Management screen
2. Click "Add New Fee Head"
3. Enter details with a name that already exists:
   - Name: "Tuition Fee" (assuming this already exists)
   - Code: "NEW-TF"
4. Click "Save"
**Expected Result**: 
- Error message indicating duplicate fee head name
- Record is not saved

### 1.2 Fee Structure Definition

#### Test Case FS-001: Create Fee Structure
**Objective**: Verify that a new fee structure can be created for a class/section/category combination.
**Test Steps**:
1. Navigate to Fee Structure Management screen
2. Click "Add New Fee Structure"
3. Select:
   - Fee Head: "Tuition Fee"
   - Class: "Class 5"
   - Section: "A"
   - Category: "New Student"
   - Frequency: "Monthly"
   - Amount: 1000
4. Enter monthly amounts:
   - April: 1000
   - May: 1000
   - (Same for all remaining months)
5. Click "Save"
**Expected Result**: 
- Fee structure is created successfully
- A success message is displayed
- Database record is created in the FeeSetups table

#### Test Case FS-002: Edit Fee Structure
**Objective**: Verify that an existing fee structure can be modified.
**Test Steps**:
1. Navigate to Fee Structure Management screen
2. Select an existing fee structure
3. Click "Edit"
4. Modify monthly amounts:
   - January: 1100
   - February: 1100
   - March: 1100
5. Click "Save"
**Expected Result**: 
- Fee structure is updated successfully
- A success message is displayed
- Database record is updated in the FeeSetups table

#### Test Case FS-003: Validate Unique Combination
**Objective**: Verify that duplicate class/section/category/fee head combinations are not allowed.
**Test Steps**:
1. Navigate to Fee Structure Management screen
2. Click "Add New Fee Structure"
3. Select a combination that already exists:
   - Fee Head: "Tuition Fee"
   - Class: "Class 5"
   - Section: "A"
   - Category: "New Student"
4. Click "Save"
**Expected Result**: 
- Error message indicating duplicate combination
- Record is not saved

## 2. Student Fee Assignment Tests

### 2.1 Student Fee Structure Management

#### Test Case SF-001: Assign Fee Structure to Student
**Objective**: Verify that fees can be assigned to a student based on predefined fee structures.
**Test Steps**:
1. Navigate to Student Fee Assignment screen
2. Select student: "John Doe", Class: "Class 5", Section: "A", Category: "New Student"
3. Click "Assign Fees"
4. Confirm assignment
**Expected Result**: 
- Student fees are assigned based on the fee structure defined for Class 5, Section A, New Student category
- Records are created in StudentFeeStructures table for each fee head
- Success message is displayed

#### Test Case SF-002: Manually Edit Student Fee Amounts
**Objective**: Verify that individual student fee amounts can be manually adjusted.
**Test Steps**:
1. Navigate to Student Fee Details screen
2. Select student: "John Doe"
3. Select fee head: "Tuition Fee"
4. Click "Edit"
5. Modify monthly amounts:
   - April: 900
   - May: 900
6. Click "Save"
**Expected Result**: 
- Student fee amounts are updated successfully
- Database records in StudentFeeStructures table are updated
- Success message is displayed

#### Test Case SF-003: Assign Transport Fee
**Objective**: Verify that transport fees can be assigned to a student.
**Test Steps**:
1. Navigate to Student Transport Fee Assignment screen
2. Select student: "Jane Smith"
3. Enter monthly transport amounts:
   - April: 500
   - May: 500
   - (Same for all remaining months)
4. Click "Save"
**Expected Result**: 
- Transport fees are assigned successfully
- Record is created in StudentFeeTransports table
- Success message is displayed

### 2.2 Fee Discount Management

#### Test Case SD-001: Apply Global Monthly Discount
**Objective**: Verify that global monthly discounts can be applied to a student.
**Test Steps**:
1. Navigate to Student Discount Management screen
2. Select student: "Mark Wilson"
3. Enter discount percentages (where 90 means 10% discount):
   - April: 90
   - May: 90
   - June: 90
   - (Other months: 100 - no discount)
4. Click "Save"
**Expected Result**: 
- Discount percentages are saved successfully
- Record is created in StudentFeeDiscounts table
- Success message is displayed

#### Test Case SD-002: Apply Fee Head Specific Discount
**Objective**: Verify that fee head specific discounts can be applied to a student.
**Test Steps**:
1. Navigate to Fee Head Discount Management screen
2. Select student: "Mark Wilson"
3. Select fee head: "Tuition Fee"
4. Enter discount percentages:
   - April: 80 (20% discount)
   - May: 80 (20% discount)
   - (Other months: NULL - use global discount)
5. Click "Save"
**Expected Result**: 
- Fee head specific discount percentages are saved successfully
- Record is created in StudentFeeHeadDiscounts table
- Success message is displayed

#### Test Case SD-003: Validate Discount Calculation
**Objective**: Verify that discounts are correctly calculated in fee collection.
**Test Steps**:
1. Set up test student with:
   - Tuition fee: 1000 per month
   - Global discount: 90% (10% discount) for April
   - Fee head discount: 80% (20% discount) for Tuition Fee in April
2. Navigate to Fee Collection screen
3. Select student and month (April)
4. View calculated amounts
**Expected Result**: 
- System shows Tuition Fee amount as 800 (1000 × 80%)
- Head-specific discount takes precedence over global discount
- Correct discount amount (200) is shown

## 3. Payment Processing Tests

### 3.1 Fee Collection

#### Test Case FC-001: Collect Full Payment
**Objective**: Verify that full fee payment can be collected for a student.
**Test Steps**:
1. Navigate to Fee Collection screen
2. Select student: "John Doe"
3. Select month: "April"
4. View payable amount (sum of all fee heads)
5. Enter payment details:
   - Payment Date: Current date
   - Payment Mode: "Cash"
   - Amount: Full payable amount
6. Click "Collect Payment"
**Expected Result**: 
- Payment is recorded successfully
- Records are created in StudentPayments and PaymentAllocations tables
- Receipt is generated
- Success message is displayed

#### Test Case FC-002: Collect Partial Payment
**Objective**: Verify that partial fee payment can be collected and properly allocated.
**Test Steps**:
1. Navigate to Fee Collection screen
2. Select student: "Jane Smith"
3. Select month: "April"
4. View payable amount
5. Enter payment details:
   - Payment Date: Current date
   - Payment Mode: "Online Transfer"
   - Reference Number: "TXN12345"
   - Amount: 50% of payable amount
6. Click "Collect Payment"
**Expected Result**: 
- Payment is recorded successfully
- System allocates payment to fee heads based on priority
- Remaining due amounts are correctly calculated
- Receipt is generated for partial payment
- Success message is displayed

#### Test Case FC-003: Process Cheque Payment
**Objective**: Verify that cheque payment processing works correctly.
**Test Steps**:
1. Navigate to Fee Collection screen
2. Select student: "Mark Wilson"
3. Select month: "April"
4. Enter payment details:
   - Payment Date: Current date
   - Payment Mode: "Cheque"
   - Bank Name: "ABC Bank"
   - Cheque Number: "123456"
   - Cheque Date: Current date
   - Amount: Full payable amount
5. Click "Collect Payment"
**Expected Result**: 
- Payment is recorded successfully
- Cheque details are saved
- Receipt is generated
- Success message is displayed

### 3.2 Receipt Management

#### Test Case RM-001: Generate Receipt
**Objective**: Verify that receipt is generated correctly after payment.
**Test Steps**:
1. Complete a payment collection transaction
2. View generated receipt
**Expected Result**: 
- Receipt shows correct:
  - Student details
  - Fee breakup
  - Payment details
  - Receipt number
  - Date and time
  - Collected by information

#### Test Case RM-002: Cancel Receipt
**Objective**: Verify that a receipt can be cancelled.
**Test Steps**:
1. Navigate to Receipt Management screen
2. Select an existing receipt
3. Click "Cancel Receipt"
4. Enter cancellation reason: "Payment returned"
5. Confirm cancellation
**Expected Result**: 
- Receipt is marked as cancelled
- Cancellation details are recorded
- Payment is marked as cancelled
- Fee dues are restored
- Success message is displayed

#### Test Case RM-003: Print Duplicate Receipt
**Objective**: Verify that a duplicate receipt can be printed.
**Test Steps**:
1. Navigate to Receipt Management screen
2. Select an existing receipt
3. Click "Print Duplicate"
4. Confirm action
**Expected Result**: 
- Duplicate receipt is generated
- Original receipt details are preserved
- Receipt is marked as "DUPLICATE"

## 4. Late Fee Management Tests

### 4.1 Due Date Configuration

#### Test Case LF-001: Configure Due Dates
**Objective**: Verify that due dates can be configured for fee heads.
**Test Steps**:
1. Navigate to Fee Schedule Management screen
2. Click "Add New Schedule"
3. Enter details:
   - Fee Head: "Tuition Fee"
   - Month: "April"
   - Due Date: 10
   - Grace Period: 5 days
   - Late Fee Type: "PERCENTAGE"
   - Late Fee Value: 5.00
   - Is Compounding: No
4. Click "Save"
**Expected Result**: 
- Fee schedule is created successfully
- Record is created in FeeSchedules table
- Success message is displayed

### 4.2 Late Fee Calculation

#### Test Case LF-002: Calculate Late Fees
**Objective**: Verify that late fees are calculated correctly.
**Test Steps**:
1. Set system date to after due date + grace period
2. Run late fee calculation job
3. View calculated late fees for a student with unpaid dues
**Expected Result**: 
- Late fees are calculated correctly
- Records are created in StudentLateFees table
- Late fee amount equals base fee amount × late fee percentage

#### Test Case LF-003: Collect Payment with Late Fee
**Objective**: Verify that payment collection includes late fees.
**Test Steps**:
1. Navigate to Fee Collection screen
2. Select student with unpaid dues and calculated late fees
3. View payable amount (including late fees)
4. Enter payment details for full amount
5. Complete payment
**Expected Result**: 
- Payment is recorded successfully
- Late fees are marked as paid
- Receipt includes late fee details
- Success message is displayed

## 5. Discount Management Tests

### 5.1 Concession Management

#### Test Case CM-001: Create Concession
**Objective**: Verify that fee concessions can be defined.
**Test Steps**:
1. Navigate to Concession Management screen
2. Click "Add New Concession"
3. Enter details:
   - Concession Type: "Sibling Discount"
   - Class: "Class 5"
   - Section: "A"
   - Fee Head: "Tuition Fee"
   - Frequency: "Monthly"
   - Amount: 200
   - Monthly amounts: 200 for all months
4. Click "Save"
**Expected Result**: 
- Concession is created successfully
- Record is created in FeeConcessions table
- Success message is displayed

#### Test Case CM-002: Apply Concession to Student
**Objective**: Verify that concessions are correctly applied during fee assignment.
**Test Steps**:
1. Configure a concession for sibling discount
2. Mark a student as eligible for sibling discount
3. Assign fees to the student
4. View assigned fee structure
**Expected Result**: 
- Fees are assigned with concession amounts applied
- Concession details are recorded

## 6. Reporting Tests

### 6.1 Collection Reports

#### Test Case CR-001: Generate Daily Collection Report
**Objective**: Verify that daily collection reports can be generated correctly.
**Test Steps**:
1. Navigate to Reports screen
2. Select "Daily Collection Report"
3. Enter date parameters
4. Generate report
**Expected Result**: 
- Report is generated with correct data
- Report shows:
  - Total collection amount
  - Collection by payment mode
  - Collection by fee head
  - Collector-wise summary

#### Test Case CR-002: Generate Class-wise Collection Report
**Objective**: Verify that class-wise collection reports can be generated correctly.
**Test Steps**:
1. Navigate to Reports screen
2. Select "Class-wise Collection Report"
3. Enter date range and select class
4. Generate report
**Expected Result**: 
- Report is generated with correct data
- Report shows collection data organized by class and section

### 6.2 Due Reports

#### Test Case DR-001: Generate Outstanding Dues Report
**Objective**: Verify that outstanding dues reports can be generated correctly.
**Test Steps**:
1. Navigate to Reports screen
2. Select "Outstanding Dues Report"
3. Select class and due status
4. Generate report
**Expected Result**: 
- Report is generated with correct data
- Report shows students with outstanding dues
- Aging information is correctly displayed

#### Test Case DR-002: Generate Student Due Statement
**Objective**: Verify that individual student due statements can be generated.
**Test Steps**:
1. Navigate to Reports screen
2. Select "Student Due Statement"
3. Select student
4. Generate report
**Expected Result**: 
- Report is generated with correct data
- Report shows:
  - Fee amounts by month and head
  - Paid amounts
  - Due amounts
  - Payment history

## 7. Data Validation Tests

### 7.1 Input Validation

#### Test Case IV-001: Validate Numeric Inputs
**Objective**: Verify that amount fields accept only valid numeric data.
**Test Steps**:
1. Navigate to any screen with amount entry
2. Attempt to enter non-numeric data in amount fields
**Expected Result**: 
- System prevents entry of non-numeric data
- Appropriate error message is displayed

#### Test Case IV-002: Validate Required Fields
**Objective**: Verify that required fields are properly enforced.
**Test Steps**:
1. Navigate to any data entry screen
2. Leave required fields blank
3. Attempt to save
**Expected Result**: 
- System prevents saving
- Required field validators highlight missing data
- Appropriate error message is displayed

### 7.2 Business Rule Validation

#### Test Case BV-001: Validate Discount Range
**Objective**: Verify that discount percentages are within valid range.
**Test Steps**:
1. Navigate to Student Discount Management screen
2. Attempt to enter discount percentage values:
   - Below 0
   - Above 100
3. Attempt to save
**Expected Result**: 
- System prevents entry of invalid percentages
- Appropriate error message is displayed

#### Test Case BV-002: Validate Payment Amount
**Objective**: Verify that payment amount validation works correctly.
**Test Steps**:
1. Navigate to Fee Collection screen
2. Select a student with dues
3. Attempt to enter payment amount greater than total dues
4. Attempt to save
**Expected Result**: 
- System either prevents entry or handles excess amount as advance payment
- Appropriate message is displayed

## 8. Integration Tests

### 8.1 Fee Structure to Student Fee Assignment

#### Test Case INT-001: Fee Structure Changes Propagation
**Objective**: Verify that changes to fee structures propagate correctly to unassigned students.
**Test Steps**:
1. Modify a fee structure for a class/section/category
2. Assign fees to a new student in that class/section/category
**Expected Result**: 
- New student receives the updated fee structure
- Existing students with already assigned fees are not affected

### 8.2 Discount and Payment Calculation

#### Test Case INT-002: End-to-End Fee Calculation
**Objective**: Verify that all discount types are correctly applied during payment.
**Test Steps**:
1. Set up a test scenario with:
   - Base fee structure
   - Global monthly discount
   - Fee head specific discount
   - Concession
2. Calculate payable amount
**Expected Result**: 
- Discounts and concessions are applied in the correct order
- Final amount is calculated correctly

## 9. Performance Tests

### 9.1 Bulk Operations

#### Test Case PERF-001: Bulk Fee Assignment
**Objective**: Verify system performance during bulk fee assignment.
**Test Steps**:
1. Select multiple students (50+)
2. Perform bulk fee assignment
3. Measure response time
**Expected Result**: 
- Operation completes within acceptable time limit (under 30 seconds)
- All fee assignments are completed successfully
- System remains responsive

#### Test Case PERF-002: Report Generation Performance
**Objective**: Verify system performance during large report generation.
**Test Steps**:
1. Generate a comprehensive collection report for a full year
2. Measure response time
**Expected Result**: 
- Report generates within acceptable time limit (under 60 seconds)
- All data is correctly included
- System remains responsive

## 10. Security Tests

### 10.1 Access Control

#### Test Case SEC-001: Role-Based Access Control
**Objective**: Verify that role-based access controls function correctly.
**Test Steps**:
1. Create test users with different roles:
   - Administrator
   - Finance Officer
   - Clerk
2. Attempt to access restricted functions with each user
**Expected Result**: 
- Users can only access functions permitted by their role
- Appropriate error messages are displayed for unauthorized access attempts

#### Test Case SEC-002: Data Isolation
**Objective**: Verify that multi-client data isolation functions correctly.
**Test Steps**:
1. Set up test data for multiple clients
2. Log in as a user from one client
3. Attempt to access data from another client
**Expected Result**: 
- User can only see data from their own client
- No data leakage between clients occurs

## 11. Mobile App Tests
(If applicable)

### 11.1 Mobile Fee Payment

#### Test Case MOB-001: Parent Mobile Payment
**Objective**: Verify that parents can pay fees through the mobile app.
**Test Steps**:
1. Log in to parent mobile app
2. View student fee details
3. Initiate payment for due amount
4. Complete payment through payment gateway
**Expected Result**: 
- Payment is processed successfully
- Receipt is generated
- Payment is recorded in the database
- Due amounts are updated

## 12. Recovery and Edge Case Tests

### 12.1 Data Recovery

#### Test Case REC-001: Payment Failure Recovery
**Objective**: Verify system recovery from payment processing failures.
**Test Steps**:
1. Initiate a payment transaction
2. Simulate a system failure during processing
3. Restart the system
4. Check transaction status
**Expected Result**: 
- System correctly identifies incomplete transactions
- Payment is either completed or rolled back
- Data integrity is maintained

### 12.2 Edge Cases

#### Test Case EDGE-001: Fee Structure Mid-Year Changes
**Objective**: Verify handling of mid-year fee structure changes.
**Test Steps**:
1. Assign fees to students based on initial fee structure
2. Change fee structure mid-year
3. Apply the changes to existing students
**Expected Result**: 
- System handles the transition correctly
- Only future months are affected by the change
- Historical data remains intact
