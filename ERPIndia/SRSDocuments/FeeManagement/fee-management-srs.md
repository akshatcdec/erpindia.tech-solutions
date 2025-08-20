# Fee Management System
# Software Requirements Specification

## 1. Introduction

### 1.1 Purpose
This document outlines the requirements for a comprehensive Fee Management System designed for educational institutions. The system will facilitate the management of student fees, including fee structure definition, fee collection, concession management, and reporting.

### 1.2 Scope
The Fee Management System will handle all aspects of fee administration, from defining fee structures to processing payments and generating reports. It will support various fee types, student categories, payment modes, and discount mechanisms.

### 1.3 Definitions, Acronyms, and Abbreviations
- **SRS**: Software Requirements Specification
- **UI**: User Interface
- **DB**: Database
- **FMS**: Fee Management System
- **Fee Head**: A specific type of fee (e.g., tuition, development, laboratory)
- **Concession**: A reduction in fees based on specific criteria
- **Discount**: A percentage or fixed amount reduction in fees

## 2. Overall Description

### 2.1 Product Perspective
The Fee Management System will be a standalone application with a modular architecture. It will interface with existing student information systems to obtain student data. The system will operate on a client-server model, with a central database storing all fee-related information.

### 2.2 Product Functions
The main functions of the Fee Management System include:
- Defining fee structures for different classes, sections, and student categories
- Assigning fees to individual students
- Managing concessions and discounts on both global and individual levels
- Processing fee payments through various payment modes
- Tracking outstanding dues and generating late fee penalties
- Generating receipts and reports
- Providing analytical insights on fee collection and pending dues

### 2.3 User Classes and Characteristics
The system will cater to the following user classes:
- **Administrators**: Set up system parameters, define fee structures, and manage user access
- **Finance Staff**: Process payments, manage concessions, and generate reports
- **Teachers/Class Teachers**: View fee status of students in their classes
- **Parents/Students**: View fee structure, payment history, and outstanding dues

### 2.4 Operating Environment
The system will operate on standard desktop/laptop computers with internet connectivity. It will be compatible with major web browsers and operating systems.

### 2.5 Design and Implementation Constraints
- The system will use SQL Server for database management
- The application will be developed using a modern web framework
- The system must adhere to relevant data protection regulations
- The database design must support efficient querying for large datasets

### 2.6 User Documentation
The system will include:
- Administrator manual
- User guides for finance staff
- Quick reference guides for teachers
- Help documentation for parents/students
- Database schema documentation

## 3. System Features and Requirements

### 3.1 Data Management

#### 3.1.1 Fee Structure Definition
The system shall allow administrators to define fee structures based on:
- Fee types (tuition, development, transport, etc.)
- Class and section
- Student category (new, old, scholar, etc.)
- Academic year or term
- Monthly distribution of fees

**Database Tables:**
```sql
CREATE TABLE dbo.FeeSetups (
  Id           INT IDENTITY NOT NULL,
  ClientID     INT NOT NULL,
  FeeHeadID    INT NOT NULL,
  ClassID      INT NOT NULL,
  SectionID    INT NOT NULL,
  CategoryID   INT NOT NULL,
  Frequency    NVARCHAR(50) NOT NULL,
  Amount       DECIMAL(10,2) NOT NULL,
  April        DECIMAL(10,2) NOT NULL,
  May          DECIMAL(10,2) NOT NULL,
  June         DECIMAL(10,2) NOT NULL,
  July         DECIMAL(10,2) NOT NULL,
  August       DECIMAL(10,2) NOT NULL,
  September    DECIMAL(10,2) NOT NULL,
  October      DECIMAL(10,2) NOT NULL,
  November     DECIMAL(10,2) NOT NULL,
  December     DECIMAL(10,2) NOT NULL,
  January      DECIMAL(10,2) NOT NULL,
  February     DECIMAL(10,2) NOT NULL,
  March        DECIMAL(10,2) NOT NULL,
  CreatedDate  DATETIME DEFAULT(getdate()) NOT NULL,
  ModifiedDate DATETIME DEFAULT(getdate()) NOT NULL,
  PRIMARY KEY(Id)
)
```

#### 3.1.2 Student Fee Assignment
The system shall assign fees to individual students based on the defined fee structure, with support for student-specific adjustments.

**Database Tables:**
```sql
CREATE TABLE dbo.StudentFeeStructures (
  Id           INT IDENTITY NOT NULL,
  ClientID     INT NOT NULL,
  StudentID    INT NOT NULL,
  FeeHeadID    INT NOT NULL,
  April        DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  May          DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  June         DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  July         DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  August       DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  September    DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  October      DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  November     DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  December     DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  January      DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  February     DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  March        DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  CreatedBy    INT NOT NULL,
  CreatedDate  DATETIME DEFAULT(getdate()) NOT NULL,
  ModifiedBy   INT NULL,
  ModifiedDate DATETIME NULL,
  IsActive     BIT DEFAULT((1)) NOT NULL,
  IsDeleted    BIT DEFAULT((0)) NOT NULL,
  CONSTRAINT PK_StudentFeeHeads PRIMARY KEY(Id)
)

CREATE TABLE dbo.StudentFeeTransports (
  Id           INT IDENTITY NOT NULL,
  ClientID     INT NOT NULL,
  StudentID    INT NOT NULL,
  April        DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  May          DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  June         DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  July         DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  August       DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  September    DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  October      DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  November     DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  December     DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  January      DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  February     DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  March        DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  CreatedBy    INT NOT NULL,
  CreatedDate  DATETIME DEFAULT(getdate()) NOT NULL,
  ModifiedBy   INT NULL,
  ModifiedDate DATETIME NULL,
  IsActive     BIT DEFAULT((1)) NOT NULL,
  IsDeleted    BIT DEFAULT((0)) NOT NULL,
  CONSTRAINT PK_StudentTransportFee PRIMARY KEY(Id)
)
```

#### 3.1.3 Discount Management
The system shall support two types of discounts:
1. General monthly discounts that apply to all fee types for a student
2. Fee head-specific discounts that apply to individual fee types for a student

**Database Tables:**
```sql
CREATE TABLE dbo.StudentFeeDiscounts (
  Id           INT IDENTITY NOT NULL,
  ClientID     INT NOT NULL,
  StudentID    INT NOT NULL,
  April        DECIMAL(5,2) DEFAULT(100) NOT NULL,
  May          DECIMAL(5,2) DEFAULT(100) NOT NULL,
  June         DECIMAL(5,2) DEFAULT(100) NOT NULL,
  July         DECIMAL(5,2) DEFAULT(100) NOT NULL,
  August       DECIMAL(5,2) DEFAULT(100) NOT NULL,
  September    DECIMAL(5,2) DEFAULT(100) NOT NULL,
  October      DECIMAL(5,2) DEFAULT(100) NOT NULL,
  November     DECIMAL(5,2) DEFAULT(100) NOT NULL,
  December     DECIMAL(5,2) DEFAULT(100) NOT NULL,
  January      DECIMAL(5,2) DEFAULT(100) NOT NULL,
  February     DECIMAL(5,2) DEFAULT(100) NOT NULL,
  March        DECIMAL(5,2) DEFAULT(100) NOT NULL,
  CreatedBy    INT NOT NULL,
  CreatedDate  DATETIME DEFAULT(getdate()) NOT NULL,
  ModifiedBy   INT NULL,
  ModifiedDate DATETIME NULL,
  IsActive     BIT DEFAULT((1)) NOT NULL,
  IsDeleted    BIT DEFAULT((0)) NOT NULL,
  CONSTRAINT PK_StudentMonthlyDiscountPercentages PRIMARY KEY(Id),
  CONSTRAINT UC_StudentMonthlyDiscountPercentages UNIQUE(ClientID, StudentID)
)

CREATE TABLE dbo.StudentFeeHeadDiscounts (
  Id           INT IDENTITY NOT NULL,
  ClientID     INT NOT NULL,
  StudentID    INT NOT NULL,
  FeeHeadID    INT NOT NULL,
  April        DECIMAL(5,2) NULL,
  May          DECIMAL(5,2) NULL,
  June         DECIMAL(5,2) NULL,
  July         DECIMAL(5,2) NULL,
  August       DECIMAL(5,2) NULL,
  September    DECIMAL(5,2) NULL,
  October      DECIMAL(5,2) NULL,
  November     DECIMAL(5,2) NULL,
  December     DECIMAL(5,2) NULL,
  January      DECIMAL(5,2) NULL,
  February     DECIMAL(5,2) NULL,
  March        DECIMAL(5,2) NULL,
  CreatedBy    INT NOT NULL,
  CreatedDate  DATETIME DEFAULT(getdate()) NOT NULL,
  ModifiedBy   INT NULL,
  ModifiedDate DATETIME NULL,
  IsActive     BIT DEFAULT((1)) NOT NULL,
  IsDeleted    BIT DEFAULT((0)) NOT NULL,
  CONSTRAINT PK_StudentFeeHeadDiscounts PRIMARY KEY(Id),
  CONSTRAINT UC_StudentFeeHeadDiscounts UNIQUE(ClientID, StudentID, FeeHeadID)
)
```

#### 3.1.4 Concession Management
The system shall support fee concessions based on different criteria, such as:
- Sibling discount
- Staff child discount
- Merit scholarship
- Financial need-based concession

```sql
CREATE TABLE dbo.FeeConcessions (
  Id               INT IDENTITY NOT NULL,
  ClientId         INT NOT NULL,
  ConcessionTypeID INT NOT NULL,
  ClassID          INT NOT NULL,
  SectionID        INT NOT NULL,
  FeeHeadID        INT NOT NULL,
  Frequency        NVARCHAR(20) NOT NULL,
  Amount           DECIMAL(18,2) NOT NULL,
  April            DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  May              DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  June             DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  July             DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  August           DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  September        DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  October          DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  November         DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  December         DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  January          DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  February         DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  March            DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  CreatedBy        INT NULL,
  CreatedDate      DATETIME DEFAULT(getdate()) NULL,
  ModifiedBy       INT NULL,
  ModifiedDate     DATETIME NULL,
  IsActive         BIT DEFAULT((1)) NULL,
  IsDeleted        BIT DEFAULT((0)) NULL,
  PRIMARY KEY(Id),
  CONSTRAINT UC_FeeConcessions_1 UNIQUE(ClientId, ConcessionTypeID, ClassID, SectionID, FeeHeadID)
)
```

### 3.2 Payment Processing

#### 3.2.1 Fee Collection
The system shall support multiple payment methods:
- Cash
- Check/Cheque
- Credit/Debit Card
- Online Transfer
- Mobile Payment

```sql
CREATE TABLE dbo.StudentPayments (
  Id              INT IDENTITY NOT NULL,
  ClientID        INT NOT NULL,
  StudentID       INT NOT NULL,
  PaymentDate     DATETIME NOT NULL,
  PaymentAmount   DECIMAL(18,2) NOT NULL,
  PaymentMode     NVARCHAR(50) NOT NULL,
  ReferenceNumber NVARCHAR(100) NULL,
  BankName        NVARCHAR(100) NULL,
  ChequeDate      DATETIME NULL,
  Remarks         NVARCHAR(500) NULL,
  CreatedBy       INT NOT NULL,
  CreatedDate     DATETIME DEFAULT(getdate()) NOT NULL,
  ModifiedBy      INT NULL,
  ModifiedDate    DATETIME NULL,
  IsActive        BIT DEFAULT((1)) NOT NULL,
  IsDeleted       BIT DEFAULT((0)) NOT NULL,
  CONSTRAINT PK_StudentPayments PRIMARY KEY(Id)
)
```

#### 3.2.2 Payment Allocation
The system shall allocate payments to specific fee heads and months:

```sql
CREATE TABLE dbo.PaymentAllocations (
  Id               INT IDENTITY NOT NULL,
  PaymentID        INT NOT NULL,
  FeeHeadID        INT NOT NULL,
  MonthName        NVARCHAR(10) NOT NULL,
  AllocatedAmount  DECIMAL(18,2) NOT NULL,
  LateFeeAmount    DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  DiscountAmount   DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  CreatedBy        INT NOT NULL,
  CreatedDate      DATETIME DEFAULT(getdate()) NOT NULL,
  ModifiedBy       INT NULL,
  ModifiedDate     DATETIME NULL,
  CONSTRAINT PK_PaymentAllocations PRIMARY KEY(Id),
  CONSTRAINT FK_PaymentAllocations_StudentPayments FOREIGN KEY(PaymentID) 
    REFERENCES dbo.StudentPayments(Id)
)
```

#### 3.2.3 Receipt Generation
The system shall generate receipts for each payment:

```sql
CREATE TABLE dbo.FeeReceiptDetails (
  Id                  INT IDENTITY NOT NULL,
  ClientID            INT NOT NULL,
  ReceiptNumber       NVARCHAR(50) NOT NULL,
  StudentID           INT NOT NULL,
  PaymentID           INT NOT NULL,
  ReceiptDate         DATETIME NOT NULL,
  TotalAmount         DECIMAL(18,2) NOT NULL,
  DiscountAmount      DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  LateFeeAmount       DECIMAL(18,2) DEFAULT((0)) NOT NULL,
  NetAmount           DECIMAL(18,2) NOT NULL,
  CreatedBy           INT NOT NULL,
  CreatedDate         DATETIME DEFAULT(getdate()) NOT NULL,
  ModifiedBy          INT NULL,
  ModifiedDate        DATETIME NULL,
  IsCancelled         BIT DEFAULT((0)) NOT NULL,
  CancelledBy         INT NULL,
  CancelledDate       DATETIME NULL,
  CancellationReason  NVARCHAR(500) NULL,
  CONSTRAINT PK_FeeReceiptDetails PRIMARY KEY(Id),
  CONSTRAINT UC_FeeReceiptDetails UNIQUE(ClientID, ReceiptNumber),
  CONSTRAINT FK_FeeReceiptDetails_StudentPayments FOREIGN KEY(PaymentID) 
    REFERENCES dbo.StudentPayments(Id)
)
```

### 3.3 Late Fee Management

#### 3.3.1 Due Date Configuration
The system shall allow configuration of due dates for each fee head and month:

```sql
CREATE TABLE dbo.FeeSchedules (
  Id              INT IDENTITY NOT NULL,
  ClientID        INT NOT NULL,
  FeeHeadID       INT NOT NULL,
  MonthName       NVARCHAR(10) NOT NULL,
  DueDate         INT NOT NULL,
  GracePeriod     INT DEFAULT((0)) NOT NULL,
  LateFeeType     NVARCHAR(20) NOT NULL,
  LateFeeValue    DECIMAL(18,2) NOT NULL,
  IsCompounding   BIT DEFAULT((0)) NOT NULL,
  CreatedBy       INT NOT NULL,
  CreatedDate     DATETIME DEFAULT(getdate()) NOT NULL,
  ModifiedBy      INT NULL,
  ModifiedDate    DATETIME NULL,
  IsActive        BIT DEFAULT((1)) NOT NULL,
  IsDeleted       BIT DEFAULT((0)) NOT NULL,
  CONSTRAINT PK_FeeSchedules PRIMARY KEY(Id)
)
```

#### 3.3.2 Late Fee Calculation
The system shall calculate late fees based on configurable rules:
- Percentage of outstanding amount
- Fixed amount
- Progressive (increasing with delay)
- Compounding or non-compounding

```sql
CREATE TABLE dbo.StudentLateFees (
  Id              INT IDENTITY NOT NULL,
  ClientID        INT NOT NULL,
  StudentID       INT NOT NULL,
  FeeHeadID       INT NOT NULL,
  MonthName       NVARCHAR(10) NOT NULL,
  LateFeeAmount   DECIMAL(18,2) NOT NULL,
  CalculatedDate  DATETIME NOT NULL,
  IsPaid          BIT DEFAULT((0)) NOT NULL,
  PaidDate        DATETIME NULL,
  CreatedBy       INT NOT NULL,
  CreatedDate     DATETIME DEFAULT(getdate()) NOT NULL,
  ModifiedBy      INT NULL,
  ModifiedDate    DATETIME NULL,
  IsActive        BIT DEFAULT((1)) NOT NULL,
  IsDeleted       BIT DEFAULT((0)) NOT NULL,
  CONSTRAINT PK_StudentLateFees PRIMARY KEY(Id)
)
```

### 3.4 Reporting and Analytics

#### 3.4.1 Fee Collection Reports
The system shall generate reports on:
- Daily collection summary
- Monthly collection by fee head
- Class-wise collection status
- Payment mode analysis

#### 3.4.2 Outstanding Dues Reports
The system shall generate reports on:
- Student-wise outstanding dues
- Class-wise dues summary
- Fee head-wise pending amounts
- Aging analysis of dues

#### 3.4.3 Discount and Concession Reports
The system shall generate reports on:
- Concession type summary
- Student-wise discount details
- Financial impact of concessions

#### 3.4.4 Audit and Transaction Reports
The system shall generate reports on:
- Receipt cancellation log
- User activity audit
- Payment allocation details

### 3.5 Administrative Functions

#### 3.5.1 Academic Year Management
The system shall support:
- Defining academic year start and end dates
- Rolling over fee structures to new academic years
- Managing fee structures across multiple academic years

#### 3.5.2 User Management
The system shall support:
- Role-based access control
- User creation and permission assignment
- Activity logging and audit trails

#### 3.5.3 Master Data Management
The system shall manage:
- Fee head definitions
- Student categories
- Concession types
- Class and section information

```sql
CREATE TABLE dbo.FeeHeads (
  Id              INT IDENTITY NOT NULL,
  ClientID        INT NOT NULL,
  FeeHeadName     NVARCHAR(100) NOT NULL,
  FeeHeadCode     NVARCHAR(20) NOT NULL,
  Description     NVARCHAR(500) NULL,
  DisplayOrder    INT DEFAULT((0)) NOT NULL,
  IsTransport     BIT DEFAULT((0)) NOT NULL,
  IsRefundable    BIT DEFAULT((0)) NOT NULL,
  CreatedBy       INT NOT NULL,
  CreatedDate     DATETIME DEFAULT(getdate()) NOT NULL,
  ModifiedBy      INT NULL,
  ModifiedDate    DATETIME NULL,
  IsActive        BIT DEFAULT((1)) NOT NULL,
  IsDeleted       BIT DEFAULT((0)) NOT NULL,
  CONSTRAINT PK_FeeHeads PRIMARY KEY(Id),
  CONSTRAINT UC_FeeHeads_Name UNIQUE(ClientID, FeeHeadName),
  CONSTRAINT UC_FeeHeads_Code UNIQUE(ClientID, FeeHeadCode)
)

CREATE TABLE dbo.StudentCategories (
  Id                  INT IDENTITY NOT NULL,
  ClientID            INT NOT NULL,
  CategoryName        NVARCHAR(100) NOT NULL,
  Description         NVARCHAR(500) NULL,
  DisplayOrder        INT DEFAULT((0)) NOT NULL,
  CreatedBy           INT NOT NULL,
  CreatedDate         DATETIME DEFAULT(getdate()) NOT NULL,
  ModifiedBy          INT NULL,
  ModifiedDate        DATETIME NULL,
  IsActive            BIT DEFAULT((1)) NOT NULL,
  IsDeleted           BIT DEFAULT((0)) NOT NULL,
  CONSTRAINT PK_StudentCategories PRIMARY KEY(Id),
  CONSTRAINT UC_StudentCategories_Name UNIQUE(ClientID, CategoryName)
)
```

## 4. Non-Functional Requirements

### 4.1 Performance Requirements
- The system shall support at least 100 concurrent users
- Response time for basic operations shall not exceed 3 seconds
- Report generation shall complete within 30 seconds for standard reports
- The system shall handle data for at least 10,000 students

### 4.2 Security Requirements
- All user access shall require authentication
- Sensitive financial data shall be encrypted
- Payment processing shall comply with relevant security standards
- User actions shall be logged for audit purposes
- Password policies shall enforce strong passwords

### 4.3 Usability Requirements
- The user interface shall be intuitive and require minimal training
- The system shall provide contextual help and tooltips
- Error messages shall be clear and suggest corrective actions
- The system shall be accessible on various screen sizes
- Reports shall be exportable in multiple formats (PDF, Excel, CSV)

### 4.4 Reliability Requirements
- The system shall have 99.5% uptime during school operational hours
- Database backups shall be performed daily
- The system shall implement data validation to prevent incorrect entries
- Recovery from failures shall be automated where possible

### 4.5 Maintainability Requirements
- The system shall use a modular architecture to facilitate updates
- Configuration parameters shall be externalized
- Database schema changes shall be versioned
- The system shall include comprehensive logging for troubleshooting

## 5. Implementation Plan

### 5.1 Development Phases
1. **Phase 1: Core Fee Structure and Student Fee Assignment**
   - Fee head definition
   - Fee structure setup
   - Student fee assignment
   - Basic reporting

2. **Phase 2: Payment Processing and Receipts**
   - Payment collection
   - Receipt generation
   - Payment allocation
   - Payment reports

3. **Phase 3: Discount and Concession Management**
   - Discount setup
   - Concession management
   - Discount application
   - Concession reports

4. **Phase 4: Late Fee and Due Management**
   - Due date configuration
   - Late fee calculation
   - Outstanding dues tracking
   - Due reports

5. **Phase 5: Advanced Features and Integration**
   - Dashboard and analytics
   - External system integration
   - Mobile app integration
   - Advanced reporting

### 5.2 Testing Strategy
- Unit testing for individual components
- Integration testing for module interactions
- System testing for end-to-end scenarios
- User acceptance testing with actual staff
- Performance testing under load conditions

### 5.3 Deployment Strategy
- Initial deployment in a controlled environment
- Parallel run with existing system
- Phased rollout by department
- Training sessions for all user groups
- Post-deployment support and monitoring

## 6. Conclusion

This Software Requirements Specification outlines the comprehensive requirements for the Fee Management System. The system is designed to streamline fee management processes for educational institutions, providing flexibility, accuracy, and efficiency. The phased implementation approach will ensure smooth adoption while minimizing disruption to existing operations.

## Appendix A: Glossary

- **Fee Head**: A specific category of fee charged to students (e.g., tuition fee, development fee)
- **Concession**: A reduction in fees based on predefined criteria
- **Discount**: A percentage reduction applied to fees
- **Late Fee**: Additional charges applied when fees are paid after the due date
- **Receipt**: A document acknowledging payment of fees
- **Due Date**: The date by which a fee payment must be made to avoid late fees