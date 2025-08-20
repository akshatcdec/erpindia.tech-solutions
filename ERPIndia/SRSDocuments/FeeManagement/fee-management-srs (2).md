# School Fee Management System
## Software Requirements Specification (SRS)

## 1. Introduction

### 1.1 Purpose
This document outlines the requirements for a School Fee Management System designed to manage and track student fee payments, generate receipts, and maintain detailed financial records for educational institutions.

### 1.2 Scope
The School Fee Management System will handle all aspects of fee collection, including recording various types of fees (tuition, exam, transport, etc.), processing payments, maintaining payment history, and generating financial reports. The system will be used by administrative staff to process payments and by management to oversee financial operations.

### 1.3 Definitions, Acronyms and Abbreviations
- **SRS**: Software Requirements Specification
- **UI**: User Interface
- **DB**: Database
- **SFS**: School Fee System

### 1.4 References
- SQL Server database design documentation
- Educational institution fee structure guidelines
- Financial record-keeping compliance standards

### 1.5 Overview
The remainder of this document provides a detailed description of the School Fee Management System functionality, including system features, user interfaces, and database requirements.

## 2. Overall Description

### 2.1 Product Perspective
The School Fee Management System is a standalone application that integrates with the school's student information database. It will be used by the administrative and accounting departments to manage fee collection and financial record-keeping.

### 2.2 Product Functions
The system will provide the following core functions:
- Student fee structure management
- Fee collection and receipt generation
- Payment tracking and history
- Fee defaulter identification
- Financial reporting
- Discount and concession management

### 2.3 User Classes and Characteristics
1. **Administrative Staff**: Daily users who collect fees, generate receipts, and handle student inquiries.
2. **Finance Officers**: Oversee financial operations, process concessions, and manage fee structures.
3. **School Management**: Access reports and dashboards to make financial decisions.

### 2.4 Operating Environment
- The system will operate on Windows desktop environments.
- Backend: Microsoft SQL Server
- Frontend: .NET Framework based application

### 2.5 Design and Implementation Constraints
- The system must integrate with existing student database systems.
- Must comply with financial data retention regulations.
- Must support backup and recovery for financial data.

### 2.6 User Documentation
The system will include:
- User manual for administrative staff
- Technical documentation for IT support
- Help functions within the application

### 2.7 Assumptions and Dependencies
- The system assumes a stable network connection to the database server.
- Depends on accurate student information from the enrollment system.
- Assumes standard fee structures with occasional exceptions.

## 3. Specific Requirements

### 3.1 User Interface Requirements

#### 3.1.1 Student Information Panel
- Display fields:
  - Admission No
  - Student Name
  - Class
  - Section
  - Roll No
  - Father's Name
  - Discount Category
  - Mobile Number
  - Profile Picture (if available)

#### 3.1.2 Fee Structure Panel
- Monthly fee grid showing:
  - Fee types (Admission, Tuition, Computer, Exam, Library, etc.)
  - Monthly breakdown of fees (12 months)
  - Amount payable for each fee type for each month
  - Checkboxes for selecting which fees to pay
  - Total amount row

#### 3.1.3 Payment Processing Panel
- Last Balance display
- Discount field
- Late Fee field
- Subtotal calculation
- Total calculation
- Received amount input
- Remaining balance calculation
- Receipt date picker
- Notes field
- Save Payment button

#### 3.1.4 Receipt Generation
- Generated receipt showing all payment details
- Option to print receipt
- Option to email receipt to parents

### 3.2 Functional Requirements

#### 3.2.1 Student Lookup
- Search for students by:
  - Admission number
  - Name
  - Class and section
  - Mobile number
- Display student details upon selection

#### 3.2.2 Fee Management
1. **Fee Structure Configuration**
   - Set up different fee types
   - Configure monthly/annual fees
   - Set up transport routes and fees
   - Configure exam fees and other special fees

2. **Fee Collection**
   - Select applicable fees for payment
   - Calculate total amount due
   - Process full or partial payments
   - Apply discounts or concessions
   - Record payment details

3. **Receipt Management**
   - Generate receipts with unique receipt numbers
   - Print receipts
   - Store receipt history
   - Allow receipt lookup and reprinting

#### 3.2.3 Transport Fee Management
- Assign transport routes to students
- Calculate transport fees based on routes
- Process transport fee payments

#### 3.2.4 Discount Management
- Apply fixed or percentage-based discounts
- Support category-based automatic discounts
- Allow manual discount adjustments
- Maintain discount history

#### 3.2.5 Late Fee Management
- Configure late fee rules
- Automatically calculate applicable late fees
- Allow manual override of late fees
- Record late fee justifications

#### 3.2.6 Reporting
- Generate student payment history reports
- Create fee collection summaries by period
- Identify fee defaulters
- Generate financial reports for accounting

### 3.3 Data Requirements

#### 3.3.1 Database Structure
The system will utilize the following database tables:

1. **FeeReceivedTbl** (Main receipt table)
   - ReceiptNo (PK)
   - SchoolCode
   - AdmissionNo
   - ConcessinAuto/Manual
   - LastDepositMonth
   - Fee details (FeeAdded, FeeBalance, TotalFee)
   - Payment information (Received, Remain)
   - Late fee information
   - Notes and metadata

2. **FeeMonthlyFeeTbl** (Individual fee items)
   - id (PK)
   - ReceiptNo (FK)
   - SchoolCode
   - AdmissionNo
   - SNo
   - FeeMonth
   - FeeName
   - FeeAmount
   - EntryDate

3. **FeeTransportFeeTbl** (Transport fee records)
   - id (PK)
   - ReceiptNo (FK)
   - SchoolCode
   - AdmissionNo
   - RouteName
   - FeeAmount
   - FeeMonth
   - EntryDate

#### 3.3.2 Data Integrity Requirements
- Foreign key constraints between tables
- Unique constraints on receipt numbers
- Data validation for all monetary values
- Date validation for payment dates
- Student existence validation

### 3.4 Non-Functional Requirements

#### 3.4.1 Performance Requirements
- The system should support up to 100 concurrent users
- Receipt generation should complete within 3 seconds
- Database queries should return results within 2 seconds
- The system should handle a school with up to 10,000 students

#### 3.4.2 Security Requirements
- Role-based access control
- Audit logging for all financial transactions
- Password protection for administrative functions
- Data encryption for sensitive information

#### 3.4.3 Reliability Requirements
- System availability during school operating hours
- Data backup procedures
- Recovery mechanisms for transaction failures
- Handling of network disconnections

#### 3.4.4 Usability Requirements
- Intuitive interface for administrative staff
- Clear error messages
- Confirmation dialogs for critical actions
- Help documentation accessible from the UI

#### 3.4.5 Maintainability Requirements
- Well-documented code
- Modular architecture
- Configuration options for fee structures
- Ability to update without data loss

## 4. Validation Requirements

### 4.1 Student Information Validation
- Verify student exists in the system
- Validate admission number format
- Ensure student is active/enrolled

### 4.2 Fee Amount Validation
- Ensure all fee amounts are non-negative
- Validate against predefined fee structure
- Verify total calculation accuracy
- Prevent duplicate fee payments for the same period

### 4.3 Payment Validation
- Ensure receipt date is valid (not future date)
- Validate payment amount against due amount
- Verify balance calculations
- Prevent duplicate receipt numbers

### 4.4 Month-specific Validation
- Ensure valid month format
- Verify fees are not paid for future months before current dues
- Prevent double payment for the same month/fee type

### 4.5 Transport Fee Validation
- Validate route exists if transport fee is charged
- Verify transport amount matches route charge

### 4.6 Database Integrity Validation
- Ensure all required fields have values
- Validate foreign key relationships
- Check for data type consistency

## 5. System Interface Requirements

### 5.1 User Interfaces
- Windows-based forms application
- Dashboard for overview of collections
- Student fee management screens
- Payment processing interface
- Report generation interface

### 5.2 Hardware Interfaces
- Support for barcode scanners for student ID cards
- Support for receipt printers
- Support for standard printers for reports

### 5.3 Software Interfaces
- Interface with SQL Server database
- Potential integration with school management system
- PDF generation for receipts and reports
- Email interface for sending digital receipts

### 5.4 Communication Interfaces
- Network connectivity to database server
- Optional SMS gateway for payment notifications
- Email service integration for receipts and notices

## 6. Business Rules

### 6.1 Fee Collection Rules
- Fees are typically collected monthly
- Some fees (like admission) are one-time
- Some fees (like exam) are term-based
- Transport fees depend on route distance

### 6.2 Discount Rules
- Siblings may get discounts
- Merit-based scholarships reduce fees
- Staff children may have special fee structures
- Management can approve special concessions

### 6.3 Late Fee Rules
- Late fees apply after a specified date each month
- Late fee may be a fixed amount or percentage
- Some students may be exempt from late fees
- Late fees can be waived with proper authorization

### 6.4 Payment Priority Rules
- Earlier months must be paid before later months
- Essential fees must be paid before optional fees
- Old balances are settled before current dues

## 7. Appendices

### Appendix A: Fee Structure Example
| Fee Type | Frequency | Amount |
|----------|-----------|--------|
| Admission Fee | One-time | 500.00 |
| Tuition Fee | Monthly | 800.00 |
| Computer Fee | Monthly | 50.00 |
| Exam Fee | Term-based | 350.00 |
| Library Fee | Monthly | 20.00 |
| Generator Fee | Monthly | 70.00 |
| Transport Fee | Monthly | Varies by route |

### Appendix B: Database Schema Diagram
[Placeholder for database schema diagram]

### Appendix C: UI Mockups
[Placeholder for UI mockups]

### Appendix D: Error Messages and Codes
| Code | Message | Resolution |
|------|---------|------------|
| E001 | Invalid admission number | Verify student exists in system |
| E002 | Negative fee amount | Check fee structure configuration |
| E003 | Receipt date in future | Enter valid current or past date |
| E004 | Duplicate receipt number | System will generate new receipt number |
| E005 | Previous months unpaid | Collect earlier months' fees first |

## 8. Revision History

| Version | Date | Description | Author |
|---------|------|-------------|--------|
| 1.0 | 2025-03-23 | Initial SRS document | System Analyst |
