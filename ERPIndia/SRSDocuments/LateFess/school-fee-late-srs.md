# Software Requirements Specification (SRS)
# School Fee Management System with Late Fee Module

## 1. Introduction

### 1.1 Purpose
This Software Requirements Specification (SRS) document details the requirements for developing a School Fee Management System with a comprehensive Late Fee Module. The system will manage student fee structures, apply discounts, and calculate late fees based on configurable rules.

### 1.2 Scope
The system will handle student fee management for educational institutions, with a specific focus on late fee calculation and tracking. It will support both simplified and complex late fee models to accommodate different school policies.

### 1.3 Definitions, Acronyms, and Abbreviations
- **SFS**: Student Fee Structure
- **SFT**: Student Fee Transport
- **SFDH**: Student Fee Discount Head
- **SFDM**: Student Fee Discount Month
- **FMD**: Fee Monthly Due Dates
- **FLP**: Fee Late Period
- **FSLS**: Fee Simplified Late Settings
- **SFLC**: Student Fee Late Charges

## 2. Overall Description

### 2.1 Product Perspective
The School Fee Management System is designed to work as a comprehensive solution for educational institutions to manage their fee structures, collections, and late fee policies. The system will integrate with existing school management systems to retrieve student data and update financial records.

### 2.2 Product Features
1. Student fee structure management
2. Transportation fee management
3. Fee discount management
4. Due date configuration
5. Late fee calculation (simplified and complex models)
6. Late fee tracking and reporting

### 2.3 User Classes and Characteristics
1. **School Administrators**: Configure fee structures, due dates, and late fee policies
2. **Finance Staff**: Process fee payments, generate invoices, and handle late fee collection
3. **Parents/Students**: View fee details and payment history

### 2.4 Operating Environment
The system will operate on a server environment with a SQL Server database. It will be accessible through web and mobile interfaces. The database tables will be designed to work with existing school management systems.

### 2.5 Design and Implementation Constraints
1. Must be compatible with existing fee structure tables
2. Must support academic years that don't align with calendar years
3. Must maintain audit trails for all financial transactions
4. Must support multi-tenancy for school chains with multiple branches

## 3. System Features

### 3.1 Fee Structure Management
The system shall allow administrators to set up fee structures for students, including:
- Basic fee heads (tuition, library, activities, etc.)
- Transportation fees
- Discounts by fee head or month

**Requirements:**
1. The system shall store fee amounts for each month from April to March (academic year).
2. The system shall allow configuration of different fee heads for each student.
3. The system shall calculate net payable fees after applying discounts.

### 3.2 Due Date Configuration
The system shall allow administrators to set due dates for fee payments.

**Requirements:**
1. The system shall allow setting a due date for each month of the academic year.
2. The system shall support different due dates for different academic years.
3. The system shall validate that due dates are valid calendar days.

### 3.3 Simplified Late Fee Model
The system shall support a simplified late fee model with a due date, grace period, and a single fine structure.

**Requirements:**
1. The system shall allow configuring a grace period in days after the due date.
2. The system shall support three types of late fees:
   - Fixed amount (flat fee regardless of days late)
   - Daily amount (per-day charge)
   - Percentage (percentage of the total fee amount)
3. The system shall allow setting a maximum fine amount for daily or percentage-based fines.

### 3.4 Complex Period-Based Late Fee Model
The system shall support a complex period-based late fee model with different fine structures for different periods after the due date.

**Requirements:**
1. The system shall allow defining multiple periods with different start and end days relative to the due date.
2. The system shall support different fine structures for each period:
   - Fixed amounts
   - Daily amounts
   - Percentage-based amounts
3. The system shall allow setting different maximum fine amounts for each period.

### 3.5 Late Fee Calculation
The system shall calculate late fees based on the configured model when a payment is made after the due date.

**Requirements:**
1. The system shall determine if a payment is late by comparing the payment date with the due date.
2. The system shall calculate the number of days late.
3. The system shall apply the appropriate fine based on the configured late fee model.
4. The system shall record the calculated late fee in a separate table.

### 3.6 Late Fee Reporting
The system shall provide reports on late fees.

**Requirements:**
1. The system shall generate reports of outstanding late fees.
2. The system shall generate reports of collected late fees.
3. The system shall generate reports of students with frequent late payments.
4. The system shall allow filtering reports by date range, class, section, and other criteria.

## 4. Database Model

### 4.1 Complete Table List

#### Core Fee Structure Tables (Existing)
1. **StudentFeeStructures**
   - **Purpose**: Stores the core fee amounts for each student, broken down by month and fee head
   - **Primary Key**: Id
   - **Unique Constraint**: TenantID, SchoolCode, StudentId, FeeHeadID
   - **Key Fields**: StudentId, FeeHeadID, April-March (monthly amount fields)
   - **Used For**: Basic fee calculation and tracking what each student should pay

2. **StudentFeeTransports**
   - **Purpose**: Stores transportation fee amounts for each student by month
   - **Primary Key**: Id
   - **Key Fields**: StudentId, April-March (monthly amount fields)
   - **Used For**: Adding transportation charges to student fees

3. **StudentFeeDiscountHeads**
   - **Purpose**: Stores discounts applicable to specific fee heads for each student
   - **Primary Key**: Id
   - **Unique Constraint**: TenantID, SchoolCode, StudentID, FeeHeadID
   - **Key Fields**: StudentID, FeeHeadID, April-March (monthly discount fields)
   - **Used For**: Reducing fee amounts for specific components of the fee

4. **StudentFeeDiscountMonths**
   - **Purpose**: Stores overall monthly discount percentages for each student
   - **Primary Key**: Id
   - **Unique Constraint**: TenantID, SchoolCode, StudentID
   - **Key Fields**: StudentID, April-March (monthly percentage fields)
   - **Used For**: Applying general discounts across all fee heads

#### Due Date Configuration Tables (New)
5. **FeeMonthlyDueDates**
   - **Purpose**: Defines when fees are due for each month of the academic year
   - **Primary Key**: Id
   - **Unique Constraint**: TenantID, SchoolCode, AcademicYearID, MonthNumber
   - **Key Fields**: AcademicYearID, MonthNumber, DueDay, Description
   - **Used For**: Setting the baseline due date for calculating late fees

#### Late Fee Configuration Tables (New)
6. **FeeLatePeriods**
   - **Purpose**: Defines different late fee periods with varying fine structures
   - **Primary Key**: Id
   - **Unique Constraint**: TenantID, SchoolCode, AcademicYearID, MonthNumber, StartDay
   - **Key Fields**: AcademicYearID, MonthNumber, PeriodName, StartDay, EndDay, FineAmount, DailyFineAmount, MaximumFine, IsPercentage, IsFixedAmount
   - **Used For**: Complex late fee scenarios with multiple periods and rates

7. **FeeSimplifiedLateSettings**
   - **Purpose**: Provides a simpler alternative to period-based late fees
   - **Primary Key**: Id
   - **Unique Constraint**: TenantID, SchoolCode, AcademicYearID, MonthNumber
   - **Key Fields**: AcademicYearID, MonthNumber, DueDay, GraceDays, LateChargeType, FixedAmount, DailyAmount, PercentageValue, MaximumFine, UseSimplifiedMode
   - **Used For**: Schools that prefer a simpler due date + grace period + single fine model

#### Late Fee Tracking Tables (New)
8. **StudentFeeLateCharges**
   - **Purpose**: Records actual late fees charged to students
   - **Primary Key**: Id
   - **Key Fields**: StudentID, FeeMonth, AcademicYearID, DueDate, PaymentDate, DaysLate, LatePeriodID, FineAmount, IsPaid, PaymentID
   - **Used For**: Tracking which students have late fees, payment status, and reports

#### Supporting Tables (Assumed to Exist)
9. **Students**
   - **Purpose**: Stores student information
   - **Primary Key**: Id
   - **Key Fields**: FirstName, LastName, ClassID, SectionID
   - **Used For**: Referenced by the student ID in fee tables

10. **Classes**
    - **Purpose**: Stores class information
    - **Primary Key**: Id
    - **Key Fields**: ClassName
    - **Used For**: Reporting and filtering

11. **Sections**
    - **Purpose**: Stores section information within classes
    - **Primary Key**: Id
    - **Key Fields**: SectionName, ClassID
    - **Used For**: Reporting and filtering

12. **AcademicYears**
    - **Purpose**: Stores academic year information
    - **Primary Key**: Id
    - **Key Fields**: AcademicYearName, StartDate, EndDate
    - **Used For**: Critical for proper fee cycle management

13. **FeeHeads**
    - **Purpose**: Stores the different types of fees
    - **Primary Key**: Id
    - **Key Fields**: FeeHeadName, Description
    - **Used For**: Referenced by the fee head ID in the fee structure table

14. **FeeCollections**
    - **Purpose**: Records actual fee payments made by students
    - **Primary Key**: Id
    - **Key Fields**: StudentID, PaymentDate, Amount, PaymentMode
    - **Used For**: Tracking payments and linking to late fees when applicable

## 5. External Interface Requirements

### 5.1 User Interfaces
1. **Fee Configuration Interface**
   - Configure fee structures, discounts, and due dates
   - Set up late fee policies (simplified or complex)

2. **Fee Collection Interface**
   - Process fee payments
   - View calculated late fees
   - Generate receipts

3. **Reporting Interface**
   - Generate and view various reports
   - Export reports to different formats

### 5.2 Software Interfaces
1. **Student Management System**
   - Retrieve student, class, and section data
   - Update student status

2. **Financial System**
   - Update financial records
   - Generate invoices

## 6. Other Non-functional Requirements

### 6.1 Performance Requirements
1. The system shall handle at least 1,000 concurrent users.
2. The system shall process a fee payment transaction within 3 seconds.
3. The system shall generate reports within 5 seconds.

### 6.2 Security Requirements
1. The system shall enforce role-based access control.
2. All financial transactions shall be logged with user details.
3. The system shall encrypt sensitive data.

### 6.3 Data Integrity Requirements
1. The system shall maintain audit trails for all financial transactions.
2. The system shall ensure that fee calculations are accurate.
3. The system shall prevent duplicate entries.

## 7. Use Cases

### 7.1 Key Use Cases

**Use Case 1: Setting Up Monthly Fee Due Dates**
1. Administrator logs into the system
2. Navigates to the "Fee Configuration" section
3. Selects the academic year and month
4. Sets the due date for the month
5. Saves the configuration

**Use Case 2: Configuring a Simplified Late Fee Model**
1. Administrator logs into the system
2. Navigates to the "Late Fee Configuration" section
3. Selects the academic year and month
4. Sets the due day and grace period
5. Chooses the late charge type (Fixed, Daily, or Percentage)
6. Enters the appropriate amounts
7. Sets maximum fine (if applicable)
8. Saves the configuration

**Use Case 3: Configuring a Complex Period-Based Late Fee Model**
1. Administrator logs into the system
2. Navigates to the "Late Fee Configuration" section
3. Selects the academic year and month
4. Sets the due day
5. Adds multiple periods with:
   - Period name
   - Start and end days
   - Fine amount or daily rate
   - Maximum fine (if applicable)
6. Saves the configuration

**Use Case 4: Processing a Late Fee Payment**
1. Finance staff logs into the system
2. Navigates to the "Fee Collection" section
3. Searches for the student
4. Enters the payment amount and date
5. System calculates any late fee based on configuration
6. Finance staff confirms the payment
7. System generates a receipt including late fee details

**Use Case 5: Generating a Late Fee Report**
1. Administrator logs into the system
2. Navigates to the "Reports" section
3. Selects "Late Fee Report"
4. Specifies the date range, class, and other filters
5. System generates the report
6. Administrator exports the report to Excel

## 8. Table Usage by Scenario

### 8.1 Basic Fee Structure Setup
When setting up the basic fee structure for students:
- **StudentFeeStructures**: Used to store fee amounts by fee head
- **StudentFeeTransports**: Used if the student uses school transport
- **StudentFeeDiscountHeads**: Used for discounts on specific fee components
- **StudentFeeDiscountMonths**: Used for overall percentage discounts

### 8.2 Simple Late Fee Model
For schools that prefer a straightforward late fee system:
- **FeeMonthlyDueDates**: Used to record when each month's fees are due
- **FeeSimplifiedLateSettings**: Used to configure the grace period and single fine structure
- **StudentFeeLateCharges**: Used to record actual late fees

### 8.3 Complex Period-Based Late Fee Model
For schools that need different fine amounts for different periods:
- **FeeMonthlyDueDates**: Used to record when each month's fees are due
- **FeeLatePeriods**: Used to define multiple periods with different fine structures
- **StudentFeeLateCharges**: Used to record actual late fees

### 8.4 Fee Collection and Late Fee Calculation
When a student makes a payment and the system needs to check if it's late:
- **FeeMonthlyDueDates**: Used to retrieve the due date
- **FeeSimplifiedLateSettings** OR **FeeLatePeriods**: Used to determine which fine structure applies
- **StudentFeeStructures** and **StudentFeeTransports**: Used if calculating percentage-based late fees
- **StudentFeeLateCharges**: Used to record the calculated late fee
- **FeeCollections**: Used to record the payment

## 9. Conclusion

This Software Requirements Specification provides a comprehensive overview of the requirements for the School Fee Management System with Late Fee Module. The system is designed to be flexible, supporting both simplified and complex late fee models to accommodate different school policies. The database design ensures proper tracking of fees, discounts, and late charges, while the user interfaces provide easy configuration and reporting capabilities.

The implementation of this system will streamline fee management processes, improve late fee collection, and provide valuable insights through comprehensive reporting. Schools can choose the late fee model that best suits their policies, and the system will handle the calculations automatically based on the configuration.

## Appendix A: Database Schema SQL Script

```sql
-- =============================================
-- Complete SQL Script for Late Fee Tables
-- =============================================

-- =============================================
-- STEP 1: Create Due Date Configuration Table
-- =============================================

IF OBJECT_ID (N'dbo.FeeMonthlyDueDates') IS NOT NULL
	DROP TABLE dbo.FeeMonthlyDueDates
GO

CREATE TABLE dbo.FeeMonthlyDueDates
	(
	  Id                UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
	  AcademicYearID    UNIQUEIDENTIFIER NOT NULL, -- Link to academic year
	  MonthNumber       INT NOT NULL, -- 1=January, 2=February, etc.
	  DueDay            INT NOT NULL, -- Day of month when payment is due (e.g., 1st of the month)
	  Description       NVARCHAR(100) NULL, -- Optional description
	  TenantID          UNIQUEIDENTIFIER NOT NULL,
	  SchoolCode        INT NOT NULL,
	  CreatedBy         INT NOT NULL,
	  CreatedDate       DATETIME DEFAULT (getdate()) NOT NULL,
	  ModifiedBy        INT NULL,
	  ModifiedDate      DATETIME NULL,
	  IsActive          BIT DEFAULT ((1)) NOT NULL,
	  IsDeleted         BIT DEFAULT ((0)) NOT NULL,
	  CONSTRAINT PK_FeeMonthlyDueDates PRIMARY KEY (Id),
	  CONSTRAINT UC_FeeMonthlyDueDates UNIQUE (TenantID, SchoolCode, AcademicYearID, MonthNumber)
	)
GO

-- =============================================
-- STEP 2: Create Complex Period-Based Late Fee Table
-- =============================================

IF OBJECT_ID (N'dbo.FeeLatePeriods') IS NOT NULL
	DROP TABLE dbo.FeeLatePeriods
GO

CREATE TABLE dbo.FeeLatePeriods
	(
	  Id                UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
	  AcademicYearID    UNIQUEIDENTIFIER NOT NULL, -- Link to academic year
	  MonthNumber       INT NOT NULL, -- 1=January, 2=February, etc.
	  PeriodName        NVARCHAR(100) NOT NULL, -- Name of the period (e.g., "Early Fine", "Late Fine")
	  StartDay          INT NOT NULL, -- Days after due date when this period starts (0 for grace period)
	  EndDay            INT NOT NULL, -- Days after due date when this period ends
	  FineAmount        DECIMAL (18, 2) DEFAULT ((0)) NOT NULL, -- Fixed amount for the period
	  DailyFineAmount   DECIMAL (18, 2) DEFAULT ((0)) NOT NULL, -- Per day fine amount
	  MaximumFine       DECIMAL (18, 2) DEFAULT ((0)) NOT NULL, -- Maximum fine for this period
	  IsPercentage      BIT DEFAULT ((0)) NOT NULL, -- Whether the fine is a percentage
	  IsFixedAmount     BIT DEFAULT ((1)) NOT NULL, -- Whether it's a fixed amount for the whole period
	  TenantID          UNIQUEIDENTIFIER NOT NULL,
	  SchoolCode        INT NOT NULL,
	  CreatedBy         INT NOT NULL,
	  CreatedDate       DATETIME DEFAULT (getdate()) NOT NULL,
	  ModifiedBy        INT NULL,
	  ModifiedDate      DATETIME NULL,
	  IsActive          BIT DEFAULT ((1)) NOT NULL,
	  IsDeleted         BIT DEFAULT ((0)) NOT NULL,
	  CONSTRAINT PK_FeeLatePeriods PRIMARY KEY (Id),
	  CONSTRAINT UC_FeeLatePeriods UNIQUE (TenantID, SchoolCode, AcademicYearID, MonthNumber, StartDay)
	)
GO

-- =============================================
-- STEP 3: Create Simplified Late Fee Settings Table
-- =============================================

IF OBJECT_ID (N'dbo.FeeSimplifiedLateSettings') IS NOT NULL
	DROP TABLE dbo.FeeSimplifiedLateSettings
GO

CREATE TABLE dbo.FeeSimplifiedLateSettings
	(
	  Id                UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
	  AcademicYearID    UNIQUEIDENTIFIER NOT NULL, -- Link to academic year
	  MonthNumber       INT NOT NULL, -- 1=January, 2=February, etc.
	  DueDay            INT NOT NULL, -- Day of month when payment is due
	  GraceDays         INT DEFAULT (0) NOT NULL, -- Number of grace days after due date
	  LateChargeType    TINYINT NOT NULL, -- 1=Fixed Amount, 2=Per Day Amount, 3=Percentage
	  FixedAmount       DECIMAL (18, 2) DEFAULT ((0)) NOT NULL, -- Fixed amount if Type=1
	  DailyAmount       DECIMAL (18, 2) DEFAULT ((0)) NOT NULL, -- Per day amount if Type=2
	  PercentageValue   DECIMAL (18, 2) DEFAULT ((0)) NOT NULL, -- Percentage if Type=3
	  MaximumFine       DECIMAL (18, 2) DEFAULT ((0)) NOT NULL, -- Maximum fine amount
	  UseSimplifiedMode BIT DEFAULT ((1)) NOT NULL, -- Flag to use simplified instead of period-based
	  TenantID          UNIQUEIDENTIFIER NOT NULL,
	  SchoolCode        INT NOT NULL,
	  CreatedBy         INT NOT NULL,
	  CreatedDate       DATETIME DEFAULT (getdate()) NOT NULL,
	  ModifiedBy        INT NULL,
	  ModifiedDate      DATETIME NULL,
	  IsActive          BIT DEFAULT ((1)) NOT NULL,
	  IsDeleted         BIT DEFAULT ((0)) NOT NULL,
	  CONSTRAINT PK_FeeSimplifiedLateSettings PRIMARY KEY (Id),
	  CONSTRAINT UC_FeeSimplifiedLateSettings UNIQUE (TenantID, SchoolCode, AcademicYearID, MonthNumber)
	)
GO

-- =============================================
-- STEP 4: Create Student Late Fee Charges Table
-- =============================================

IF OBJECT_ID (N'dbo.StudentFeeLateCharges') IS NOT NULL
	DROP TABLE dbo.StudentFeeLateCharges
GO

CREATE TABLE dbo.StudentFeeLateCharges
	(
	  Id                UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
	  StudentID         UNIQUEIDENTIFIER NOT NULL,
	  FeeMonth          INT NOT NULL, -- 1=January, 2=February, etc.
	  AcademicYearID    UNIQUEIDENTIFIER NOT NULL,
	  DueDate           DATE NOT NULL, -- Actual due date with year
	  PaymentDate       DATE NULL, -- When payment was actually made
	  DaysLate          INT DEFAULT (0) NOT NULL, -- Number of days payment was late
	  LatePeriodID      UNIQUEIDENTIFIER NOT NULL, -- Which period rule was applied
	  FineAmount        DECIMAL (18, 2) DEFAULT ((0)) NOT NULL, -- Calculated fine amount
	  IsPaid            BIT DEFAULT ((0)) NOT NULL, -- Whether the fine has been paid
	  PaymentID         UNIQUEIDENTIFIER NULL, -- Reference to the payment record if paid
	  TenantID          UNIQUEIDENTIFIER NOT NULL,
	  SchoolCode        INT NOT NULL,
	  CreatedBy         INT NOT NULL,
	  CreatedDate       DATETIME DEFAULT (getdate()) NOT NULL,
	  ModifiedBy        INT NULL,
	  ModifiedDate      DATETIME NULL,
	  IsActive          BIT DEFAULT ((1)) NOT NULL,
	  IsDeleted         BIT DEFAULT ((0)) NOT NULL,
	  CONSTRAINT PK_StudentFeeLateCharges PRIMARY KEY (Id)
	)
GO

-- =============================================
-- STEP 5: Create Stored Procedures for Late Fee Management
-- =============================================

-- Procedure to set up monthly due dates
CREATE OR ALTER PROCEDURE dbo.SetupMonthlyDueDate
    @AcademicYearID UNIQUEIDENTIFIER,
    @MonthNumber INT,
    @DueDay INT,
    @Description NVARCHAR(100) = NULL,
    @TenantID UNIQUEIDENTIFIER,
    @SchoolCode INT,
    @CreatedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if a record already exists
    IF EXISTS (
        SELECT 1 FROM dbo.FeeMonthlyDueDates
        WHERE AcademicYearID = @AcademicYearID
          AND MonthNumber = @MonthNumber
          AND TenantID = @TenantID
          AND SchoolCode = @SchoolCode
          AND IsDeleted = 0
    )
    BEGIN
        -- Update existing record
        UPDATE dbo.FeeMonthlyDueDates
        SET DueDay = @DueDay,
            Description = @Description,
            ModifiedBy = @CreatedBy,
            ModifiedDate = GETDATE()
        WHERE AcademicYearID = @AcademicYearID
          AND MonthNumber = @MonthNumber
          AND TenantID = @TenantID
          AND SchoolCode = @SchoolCode
          AND IsDeleted = 0
    END
    ELSE
    BEGIN
        -- Insert new record
        INSERT INTO dbo.FeeMonthlyDueDates
            (AcademicYearID, MonthNumber, DueDay, Description, 
             TenantID, SchoolCode, CreatedBy)
        VALUES
            (@AcademicYearID, @MonthNumber, @DueDay, @Description,
             @TenantID, @SchoolCode, @CreatedBy)
    END
    
    RETURN 0
END
GO

-- Procedure to set up a late fee period
CREATE OR ALTER PROCEDURE dbo.SetupLatePeriod
    @AcademicYearID UNIQUEIDENTIFIER,
    @MonthNumber INT,
    @PeriodName NVARCHAR(100),
    @StartDay INT,
    @EndDay INT,
    @FineAmount DECIMAL(18,2),
    @DailyFineAmount DECIMAL(18,2) = 0,
    @MaximumFine DECIMAL(18,2) = 0,
    @IsPercentage BIT = 0,
    @IsFixedAmount BIT = 1,
    @TenantID UNIQUEIDENTIFIER,
    @SchoolCode INT,
    @CreatedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if a record already exists
    IF EXISTS (
        SELECT 1 FROM dbo.FeeLatePeriods
        WHERE AcademicYearID = @AcademicYearID
          AND MonthNumber = @MonthNumber
          AND StartDay = @StartDay
          AND TenantID = @TenantID
          AND SchoolCode = @SchoolCode
          AND IsDeleted = 0
    )
    BEGIN
        -- Update existing record
        UPDATE dbo.FeeLatePeriods
        SET PeriodName = @PeriodName,
            EndDay = @EndDay,
            FineAmount = @FineAmount,
            DailyFineAmount = @DailyFineAmount,
            MaximumFine = @MaximumFine,
            IsPercentage = @IsPercentage,
            IsFixedAmount = @IsFixedAmount,
            ModifiedBy = @CreatedBy,
            ModifiedDate = GETDATE()
        WHERE AcademicYearID = @AcademicYearID
          AND MonthNumber = @MonthNumber
          AND StartDay = @StartDay
          AND TenantID = @TenantID
          AND SchoolCode = @SchoolCode
          AND IsDeleted = 0
    END
    ELSE
    BEGIN
        -- Insert new record
        INSERT INTO dbo.FeeLatePeriods
            (AcademicYearID, MonthNumber, PeriodName, StartDay, EndDay,
             FineAmount, DailyFineAmount, MaximumFine, IsPercentage, IsFixedAmount,
             TenantID, SchoolCode, CreatedBy)
        VALUES
            (@AcademicYearID, @MonthNumber, @PeriodName, @StartDay, @EndDay,
             @FineAmount, @DailyFineAmount, @MaximumFine, @IsPercentage, @IsFixedAmount,
             @TenantID, @SchoolCode, @CreatedBy)
    END
    
    RETURN 0
END
GO

-- Procedure to set up simplified late fee settings
CREATE OR ALTER PROCEDURE dbo.SetupSimplifiedLateFee
    @AcademicYearID UNIQUEIDENTIFIER,
    @MonthNumber INT,
    @DueDay INT,
    @GraceDays INT,
    @LateChargeType TINYINT, -- 1=Fixed, 2=Daily, 3=Percentage
    @FixedAmount DECIMAL(18,2) = 0,
    @DailyAmount DECIMAL(18,2) = 0,
    @PercentageValue DECIMAL(18,2) = 0,
    @MaximumFine DECIMAL(18,2) = 0,
    @TenantID UNIQUEIDENTIFIER,
    @SchoolCode INT,
    @CreatedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate inputs
    IF @LateChargeType NOT IN (1, 2, 3)
    BEGIN
        RAISERROR('Invalid LateChargeType. Must be 1 (Fixed), 2 (Daily), or 3 (Percentage).', 16, 1)
        RETURN -1
    END
    
    IF @LateChargeType = 1 AND @FixedAmount <= 0
    BEGIN
        RAISERROR('Fixed amount must be greater than zero for Fixed charge type.', 16, 1)
        RETURN -1
    END
    
    IF @LateChargeType = 2 AND @DailyAmount <= 0
    BEGIN
        RAISERROR('Daily amount must be greater than zero for Daily charge type.', 16, 1)
        RETURN -1
    END
    
    IF @LateChargeType = 3 AND (@PercentageValue <= 0 OR @PercentageValue > 100)
    BEGIN
        RAISERROR('Percentage value must be between 0 and 100 for Percentage charge type.', 16, 1)
        RETURN -1
    END
    
    -- Check if a record already exists
    IF EXISTS (
        SELECT 1 FROM dbo.FeeSimplifiedLateSettings
        WHERE AcademicYearID = @AcademicYearID
          AND MonthNumber = @MonthNumber
          AND TenantID = @TenantID
          AND SchoolCode = @SchoolCode
          AND IsDeleted = 0
    )
    BEGIN
        -- Update existing record
        UPDATE dbo.FeeSimplifiedLateSettings
        SET DueDay = @DueDay,
            GraceDays = @GraceDays,
            LateChargeType = @LateChargeType,
            FixedAmount = @FixedAmount,
            DailyAmount = @DailyAmount,
            PercentageValue = @PercentageValue,
            MaximumFine = @MaximumFine,
            UseSimplifiedMode = 1,
            ModifiedBy = @CreatedBy,
            ModifiedDate = GETDATE()
        WHERE AcademicYearID = @AcademicYearID
          AND MonthNumber = @MonthNumber
          AND TenantID = @TenantID
          AND SchoolCode = @SchoolCode
          AND IsDeleted = 0
    END
    ELSE
    BEGIN
        -- Insert new record
        INSERT INTO dbo.FeeSimplifiedLateSettings
            (AcademicYearID, MonthNumber, DueDay, GraceDays, 
             LateChargeType, FixedAmount, DailyAmount, PercentageValue, MaximumFine, 
             UseSimplifiedMode, TenantID, SchoolCode, CreatedBy)
        VALUES
            (@AcademicYearID, @MonthNumber, @DueDay, @GraceDays, 
             @LateChargeType, @FixedAmount, @DailyAmount, @PercentageValue, @MaximumFine, 
             1, @TenantID, @SchoolCode, @CreatedBy)
    END
    
    -- Also create/update a record in FeeMonthlyDueDates for compatibility
    EXEC dbo.SetupMonthlyDueDate
        @AcademicYearID = @AcademicYearID,
        @MonthNumber = @MonthNumber,
        @DueDay = @DueDay,
        @Description = 'Automatically created by simplified settings',
        @TenantID = @TenantID,
        @SchoolCode = @SchoolCode,
        @CreatedBy = @CreatedBy
    
    RETURN 0
END
GO

-- Procedure to calculate late fee using period-based approach
CREATE OR ALTER PROCEDURE dbo.CalculateStudentLateFee
    @StudentID UNIQUEIDENTIFIER,
    @FeeMonth INT,
    @AcademicYearID UNIQUEIDENTIFIER,
    @PaymentDate DATE,
    @TenantID UNIQUEIDENTIFIER,
    @SchoolCode INT,
    @CreatedBy INT,
    @LateFeeAmount DECIMAL(18,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get the due date configuration
    DECLARE @DueDay INT, @DueDate DATE
    
    SELECT @DueDay = DueDay
    FROM dbo.FeeMonthlyDueDates
    WHERE MonthNumber = @FeeMonth 
      AND AcademicYearID = @AcademicYearID
      AND TenantID = @TenantID 
      AND SchoolCode = @SchoolCode
      AND IsActive = 1
      AND IsDeleted = 0
    
    IF @DueDay IS NULL
    BEGIN
        -- No due date configured for this month
        SET @LateFeeAmount = 0
        RETURN 0
    END
    
    -- Calculate the actual due date (with year)
    -- Note: We need to handle the case where payment might be in the next year
    DECLARE @PaymentYear INT = YEAR(@PaymentDate)
    DECLARE @PaymentMonth INT = MONTH(@PaymentDate)
    
    -- If the payment month is earlier than the fee month, assume it's for next year
    IF @PaymentMonth < @FeeMonth
        SET @DueDate = DATEFROMPARTS(@PaymentYear - 1, @FeeMonth, @DueDay)
    ELSE
        SET @DueDate = DATEFROMPARTS(@PaymentYear, @FeeMonth, @DueDay)
    
    -- If payment date is before or on due date, no fine
    IF @PaymentDate <= @DueDate
    BEGIN
        SET @LateFeeAmount = 0
        RETURN 0
    END
    
    -- Calculate days late
    DECLARE @DaysLate INT = DATEDIFF(DAY, @DueDate, @PaymentDate)
    
    -- Find the applicable fine period
    DECLARE @LatePeriodID UNIQUEIDENTIFIER, 
            @FineAmount DECIMAL(18,2),
            @DailyFineAmount DECIMAL(18,2), 
            @MaximumFine DECIMAL(18,2),
            @IsFixedAmount BIT,
            @IsPercentage BIT,
            @StartDay INT
    
    SELECT TOP 1 
           @LatePeriodID = Id, 
           @FineAmount = FineAmount, 
           @DailyFineAmount = DailyFineAmount,
           @MaximumFine = MaximumFine,
           @IsFixedAmount = IsFixedAmount,
           @IsPercentage = IsPercentage,
           @StartDay = StartDay
    FROM dbo.FeeLatePeriods
    WHERE MonthNumber = @FeeMonth
      AND AcademicYearID = @AcademicYearID
      AND StartDay <= @DaysLate
      AND EndDay >= @DaysLate
      AND TenantID = @TenantID
      AND SchoolCode = @SchoolCode
      AND IsActive = 1
      AND IsDeleted = 0
    ORDER BY StartDay DESC
    
    IF @LatePeriodID IS NULL
    BEGIN
        -- No applicable late period found
        SET @LateFeeAmount = 0
        RETURN 0
    END
    
    -- Calculate the total fine
    DECLARE @TotalFine DECIMAL(18,2)
    
    IF @IsFixedAmount = 1
    BEGIN
        -- It's a fixed amount for the period
        SET @TotalFine = @FineAmount
    END
    ELSE
    BEGIN
        -- It's a per-day fine
        DECLARE @DaysInPeriod INT = @DaysLate - @StartDay + 1
        
        SET @TotalFine = @DailyFineAmount * @DaysInPeriod
        
        -- Apply maximum fine limit if set
        IF @MaximumFine > 0 AND @TotalFine > @MaximumFine
            SET @TotalFine = @MaximumFine
    END
    
    -- If it's a percentage-based fine, we need to get the fee amount
    IF @IsPercentage = 1
    BEGIN
        -- Get the total fee amount for this student and month
        DECLARE @TotalFeeAmount DECIMAL(18,2) = 0
        
        -- Sum up from StudentFeeStructures
        SELECT @TotalFeeAmount = @TotalFeeAmount + 
            CASE 
                WHEN @FeeMonth = 4 THEN April
                WHEN @FeeMonth = 5 THEN May
                WHEN @FeeMonth = 6 THEN June
                WHEN @FeeMonth = 7 THEN July
                WHEN @FeeMonth = 8 THEN August
                WHEN @FeeMonth = 9 THEN September
                WHEN @FeeMonth = 10 THEN October
                WHEN @FeeMonth = 11 THEN November
                WHEN @FeeMonth = 12 THEN December
                WHEN @FeeMonth = 1 THEN January
                WHEN @FeeMonth = 2 THEN February
                WHEN @FeeMonth = 3 THEN March
            END
        FROM dbo.StudentFeeStructures
        WHERE StudentId = @StudentID
          AND TenantID = @TenantID
          AND SchoolCode = @SchoolCode
          AND IsActive = 1
          AND IsDeleted = 0
        
        -- Add transport fees if any
        SELECT @TotalFeeAmount = @TotalFeeAmount + 
            CASE 
                WHEN @FeeMonth = 4 THEN April
                WHEN @FeeMonth = 5 THEN May
                WHEN @FeeMonth = 6 THEN June
                WHEN @FeeMonth = 7 THEN July
                WHEN @FeeMonth = 8 THEN August
                WHEN @FeeMonth = 9 THEN September
                WHEN @FeeMonth = 10 THEN October
                WHEN @FeeMonth = 11 THEN November
                WHEN @FeeMonth = 12 THEN December
                WHEN @FeeMonth = 1 THEN January
                WHEN @FeeMonth = 2 THEN February
                WHEN @FeeMonth = 3 THEN March
            END
        FROM dbo.StudentFeeTransports
        WHERE StudentId = @StudentID
          AND TenantID = @TenantID
          AND SchoolCode = @SchoolCode
          AND IsActive = 1
          AND IsDeleted = 0
        
        -- Calculate percentage-based fine
        IF @IsFixedAmount = 1
            SET @TotalFine = (@TotalFeeAmount * @FineAmount) / 100
        ELSE
            SET @TotalFine = (@TotalFeeAmount * @DailyFineAmount * @DaysInPeriod) / 100
        
        -- Apply maximum fine limit if set
        IF @MaximumFine > 0 AND @TotalFine > @MaximumFine
            SET @TotalFine = @MaximumFine
    END
    
    -- Record the late charge
    INSERT INTO dbo.StudentFeeLateCharges
        (StudentID, FeeMonth, AcademicYearID, DueDate, PaymentDate, DaysLate,
         LatePeriodID, FineAmount, IsPaid, TenantID, SchoolCode, CreatedBy)
    VALUES
        (@StudentID, @FeeMonth, @AcademicYearID, @DueDate, @PaymentDate, @DaysLate,
         @LatePeriodID, @TotalFine, 0, @TenantID, @SchoolCode, @CreatedBy)
    
    SET @LateFeeAmount = @TotalFine
    RETURN 0
END
GO

-- Procedure to calculate late fee using simplified approach
CREATE OR ALTER PROCEDURE dbo.CalculateStudentLateFeeSimplified
    @StudentID UNIQUEIDENTIFIER,
    @FeeMonth INT,
    @AcademicYearID UNIQUEIDENTIFIER,
    @PaymentDate DATE,
    @TenantID UNIQUEIDENTIFIER,
    @SchoolCode INT,
    @CreatedBy INT,
    @LateFeeAmount DECIMAL(18,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if simplified mode is enabled for this month
    DECLARE @UseSimplifiedMode BIT = 0
    
    SELECT @UseSimplifiedMode = UseSimplifiedMode
    FROM dbo.FeeSimplifiedLateSettings
    WHERE MonthNumber = @FeeMonth 
      AND AcademicYearID = @AcademicYearID
      AND TenantID = @TenantID 
      AND SchoolCode = @SchoolCode
      AND IsActive = 1
      AND IsDeleted = 0
      
    -- If simplified mode is not enabled, use the period-based calculation
    IF @UseSimplifiedMode = 0 OR @UseSimplifiedMode IS NULL
    BEGIN
        EXEC dbo.CalculateStudentLateFee
            @StudentID = @StudentID,
            @FeeMonth = @FeeMonth,
            @AcademicYearID = @AcademicYearID,
            @PaymentDate = @PaymentDate,
            @TenantID = @TenantID,
            @SchoolCode = @SchoolCode,
            @CreatedBy = @CreatedBy,
            @LateFeeAmount = @LateFeeAmount OUTPUT
            
        RETURN 0
    END
    
    -- Simplified mode is enabled, proceed with simplified calculation
    DECLARE @DueDay INT, 
            @GraceDays INT,
            @LateChargeType TINYINT,
            @FixedAmount DECIMAL(18,2),
            @DailyAmount DECIMAL(18,2),
            @PercentageValue DECIMAL(18,2),
            @MaximumFine DECIMAL(18,2),
            @DueDate DATE
    
    -- Get the simplified late fee settings
    SELECT 
        @DueDay = DueDay,
        @GraceDays = GraceDays,
        @LateChargeType = LateChargeType,
        @FixedAmount = FixedAmount,
        @DailyAmount = DailyAmount,
        @PercentageValue = PercentageValue,
        @MaximumFine = MaximumFine
    FROM dbo.FeeSimplifiedLateSettings
    WHERE MonthNumber = @FeeMonth 
      AND AcademicYearID = @AcademicYearID
      AND TenantID = @TenantID 
      AND SchoolCode = @SchoolCode
      AND IsActive = 1
      AND IsDeleted = 0
    
    IF @DueDay IS NULL
    BEGIN
        -- No due date configured for this month
        SET @LateFeeAmount = 0
        RETURN 0
    END
    
    -- Calculate the actual due date (with year)
    -- Note: We need to handle the case where payment might be in the next year
    DECLARE @PaymentYear INT = YEAR(@PaymentDate)
    DECLARE @PaymentMonth INT = MONTH(@PaymentDate)
    
    -- If the payment month is earlier than the fee month, assume it's for next year
    IF @PaymentMonth < @FeeMonth
        SET @DueDate = DATEFROMPARTS(@PaymentYear - 1, @FeeMonth, @DueDay)
    ELSE
        SET @DueDate = DATEFROMPARTS(@PaymentYear, @FeeMonth, @DueDay)
    
    -- If payment date is before or on due date, no fine
    IF @PaymentDate <= @DueDate
    BEGIN
        SET @LateFeeAmount = 0
        RETURN 0
    END
    
    -- Calculate days late
    DECLARE @DaysLate INT = DATEDIFF(DAY, @DueDate, @PaymentDate)
    
    -- Apply grace period
    DECLARE @EffectiveDaysLate INT = @DaysLate - @GraceDays
    
    -- If still within grace period, no fine
    IF @EffectiveDaysLate <= 0
    BEGIN
        SET @LateFeeAmount = 0
        RETURN 0
    END
    
    -- Calculate fine based on charge type
    DECLARE @TotalFine DECIMAL(18,2) = 0
    
    IF @LateChargeType = 1  -- Fixed Amount
    BEGIN
        SET @TotalFine = @FixedAmount
    END
    ELSE IF @LateChargeType = 2  -- Daily Amount
    BEGIN
        SET @TotalFine = @EffectiveDaysLate * @DailyAmount
        
        -- Apply maximum fine limit if set
        IF @MaximumFine > 0 AND @TotalFine > @MaximumFine
            SET @TotalFine = @MaximumFine
    END
    ELSE IF @LateChargeType = 3  -- Percentage
    BEGIN
        -- Get the total fee amount for this student and month
        DECLARE @TotalFeeAmount DECIMAL(18,2) = 0
        
        -- Sum up from StudentFeeStructures
        SELECT @TotalFeeAmount = @TotalFeeAmount + 
            CASE 
                WHEN @FeeMonth = 4 THEN April
                WHEN @FeeMonth = 5 THEN May
                WHEN @FeeMonth = 6 THEN June
                WHEN @FeeMonth = 7 THEN July
                WHEN @FeeMonth = 8 THEN August
                WHEN @FeeMonth = 9 THEN September
                WHEN @FeeMonth = 10 THEN October
                WHEN @FeeMonth = 11 THEN November
                WHEN @FeeMonth = 12 THEN December
                WHEN @FeeMonth = 1 THEN January
                WHEN @FeeMonth = 2 THEN February
                WHEN @FeeMonth = 3 THEN March
            END
        FROM dbo.StudentFeeStructures
        WHERE StudentId = @StudentID
          AND TenantID = @TenantID
          AND SchoolCode = @SchoolCode
          AND IsActive = 1
          AND IsDeleted = 0
        
        -- Add transport fees if any
        SELECT @TotalFeeAmount = @TotalFeeAmount + 
            CASE 
                WHEN @FeeMonth = 4 THEN April
                WHEN @FeeMonth = 5 THEN May
                WHEN @FeeMonth = 6 THEN June
                WHEN @FeeMonth = 7 THEN July
                WHEN @FeeMonth = 8 THEN August
                WHEN @FeeMonth = 9 THEN September
                WHEN @FeeMonth = 10 THEN October
                WHEN @FeeMonth = 11 THEN November
                WHEN @FeeMonth = 12 THEN December
                WHEN @FeeMonth = 1 THEN January
                WHEN @FeeMonth = 2 THEN February
                WHEN @FeeMonth = 3 THEN March
            END
        FROM dbo.StudentFeeTransports
        WHERE StudentId = @StudentID
          AND TenantID = @TenantID
          AND SchoolCode = @SchoolCode
          AND IsActive = 1
          AND IsDeleted = 0
        
        -- Calculate percentage-based fine
        SET @TotalFine = (@TotalFeeAmount * @PercentageValue) / 100
        
        -- Apply maximum fine limit if set
        IF @MaximumFine > 0 AND @TotalFine > @MaximumFine
            SET @TotalFine = @MaximumFine
    END
    
    -- Create a corresponding late period ID (this is just for consistency in the StudentFeeLateCharges table)
    DECLARE @LatePeriodID UNIQUEIDENTIFIER
    
    -- Try to find an existing period that matches our criteria
    SELECT TOP 1 @LatePeriodID = Id
    FROM dbo.FeeLatePeriods
    WHERE MonthNumber = @FeeMonth
      AND AcademicYearID = @AcademicYearID
      AND TenantID = @TenantID
      AND SchoolCode = @SchoolCode
      AND IsActive = 1
      AND IsDeleted = 0
      AND PeriodName = 'Simplified Late Fee'
    
    -- If no existing period, create one
    IF @LatePeriodID IS NULL
    BEGIN
        SET @LatePeriodID = NEWID()
        
        INSERT INTO dbo.FeeLatePeriods
            (Id, AcademicYearID, MonthNumber, PeriodName, StartDay, EndDay,
             FineAmount, DailyFineAmount, MaximumFine, 
             IsPercentage, IsFixedAmount, TenantID, SchoolCode, CreatedBy)
        VALUES
            (@LatePeriodID, @AcademicYearID, @FeeMonth, 'Simplified Late Fee', @GraceDays, 999,
             @FixedAmount, @DailyAmount, @MaximumFine, 
             CASE WHEN @LateChargeType = 3 THEN 1 ELSE 0 END, 
             CASE WHEN @LateChargeType = 1 THEN 1 ELSE 0 END, 
             @TenantID, @SchoolCode, @CreatedBy)
    END
    
    -- Record the late charge
    INSERT INTO dbo.StudentFeeLateCharges
        (StudentID, FeeMonth, AcademicYearID, DueDate, PaymentDate, DaysLate,
         LatePeriodID, FineAmount, IsPaid, TenantID, SchoolCode, CreatedBy)
    VALUES
        (@StudentID, @FeeMonth, @AcademicYearID, @DueDate, @PaymentDate, @DaysLate,
         @LatePeriodID, @TotalFine, 0, @TenantID, @SchoolCode, @CreatedBy)
    
    SET @LateFeeAmount = @TotalFine
    RETURN 0
END
GO

-- Procedure to mark late fee as paid
CREATE OR ALTER PROCEDURE dbo.MarkLateFeeAsPaid
    @StudentLateFeeID UNIQUEIDENTIFIER,
    @PaymentID UNIQUEIDENTIFIER,
    @ModifiedBy INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE dbo.StudentFeeLateCharges
    SET IsPaid = 1,
        PaymentID = @PaymentID,
        ModifiedBy = @ModifiedBy,
        ModifiedDate = GETDATE()
    WHERE Id = @StudentLateFeeID
      AND IsDeleted = 0
    
    RETURN 0
END
GO

-- =============================================
-- STEP 6: Create View for Fee Late Payment Summary
-- =============================================

CREATE OR ALTER VIEW dbo.vw_StudentLateFeesSummary
AS
SELECT 
    lf.Id AS LateFeeID,
    lf.StudentID,
    lf.FeeMonth,
    CASE 
        WHEN lf.FeeMonth = 1 THEN 'January'
        WHEN lf.FeeMonth = 2 THEN 'February'
        WHEN lf.FeeMonth = 3 THEN 'March'
        WHEN lf.FeeMonth = 4 THEN 'April'
        WHEN lf.FeeMonth = 5 THEN 'May'
        WHEN lf.FeeMonth = 6 THEN 'June'
        WHEN lf.FeeMonth = 7 THEN 'July'
        WHEN lf.FeeMonth = 8 THEN 'August'
        WHEN lf.FeeMonth = 9 THEN 'September'
        WHEN lf.FeeMonth = 10 THEN 'October'
        WHEN lf.FeeMonth = 11 THEN 'November'
        WHEN lf.FeeMonth = 12 THEN 'December'
    END AS MonthName,
    lf.AcademicYearID,
    lf.DueDate,
    lf.PaymentDate,
    lf.DaysLate,
    lf.LatePeriodID,
    flp.PeriodName,
    CASE 
        WHEN flp.PeriodName = 'Simplified Late Fee' THEN
            CASE
                WHEN (SELECT LateChargeType FROM dbo.FeeSimplifiedLateSettings 
                     WHERE AcademicYearID = lf.AcademicYearID 
                     AND MonthNumber = lf.FeeMonth
                     AND TenantID = lf.TenantID
                     AND SchoolCode = lf.SchoolCode) = 1 THEN 'Fixed Amount'
                WHEN (SELECT LateChargeType FROM dbo.FeeSimplifiedLateSettings 
                     WHERE AcademicYearID = lf.AcademicYearID 
                     AND MonthNumber = lf.FeeMonth
                     AND TenantID = lf.TenantID
                     AND SchoolCode = lf.SchoolCode) = 2 THEN 'Daily Amount'
                WHEN (SELECT LateChargeType FROM dbo.FeeSimplifiedLateSettings 
                     WHERE AcademicYearID = lf.AcademicYearID 
                     AND MonthNumber = lf.FeeMonth
                     AND TenantID = lf.TenantID
                     AND SchoolCode = lf.SchoolCode) = 3 THEN 'Percentage'
                ELSE 'Unknown'
            END
        ELSE
            CASE
                WHEN flp.IsFixedAmount = 1 THEN 'Fixed Amount'
                WHEN flp.DailyFineAmount > 0 THEN 'Daily Amount'
                ELSE 'Unknown'
            END
    END AS FeeType,
    lf.FineAmount,
    lf.IsPaid,
    CASE WHEN lf.IsPaid = 1 THEN 'Paid' ELSE 'Unpaid' END AS PaymentStatus,
    lf.PaymentID,
    lf.TenantID,
    lf.SchoolCode,
    lf.CreatedDate,
    lf.ModifiedDate
FROM dbo.StudentFeeLateCharges lf
LEFT JOIN dbo.FeeLatePeriods flp ON lf.LatePeriodID = flp.Id
WHERE lf.IsDeleted = 0 AND lf.IsActive = 1
GO

-- =============================================
-- STEP 7: Example usage scenarios
-- =============================================

/*
-- Example 1: Setting up a monthly due date
DECLARE @AcademicYearID UNIQUEIDENTIFIER = 'your-academic-year-guid'
DECLARE @TenantID UNIQUEIDENTIFIER = 'your-tenant-guid'
DECLARE @SchoolCode INT = 123
DECLARE @UserID INT = 1

EXEC dbo.SetupMonthlyDueDate
    @AcademicYearID = @AcademicYearID,
    @MonthNumber = 12,
    @DueDay = 5,
    @Description = 'December 2024 Fees',
    @TenantID = @TenantID,
    @SchoolCode = @SchoolCode,
    @CreatedBy = @UserID

-- Example 2: Setting up a complex period-based late fee
-- First period: Grace Period (days 0-9 after due date)
EXEC dbo.SetupLatePeriod
    @AcademicYearID = @AcademicYearID,
    @MonthNumber = 12,
    @PeriodName = 'Grace Period',
    @StartDay = 0,
    @EndDay = 9,
    @FineAmount = 0,
    @IsFixedAmount = 1,
    @TenantID = @TenantID,
    @SchoolCode = @SchoolCode,
    @CreatedBy = @UserID

-- Second period: Fixed Fine (days 10-19 after due date)
EXEC dbo.SetupLatePeriod
    @AcademicYearID = @AcademicYearID,
    @MonthNumber = 12,
    @PeriodName = 'Fixed Fine',
    @StartDay = 10,
    @EndDay = 19,
    @FineAmount = 50,
    @IsFixedAmount = 1,
    @TenantID = @TenantID,
    @SchoolCode = @SchoolCode,
    @CreatedBy = @UserID

-- Third period: Daily Fine (days 20+ after due date)
EXEC dbo.SetupLatePeriod
    @AcademicYearID = @AcademicYearID,
    @MonthNumber = 12,
    @PeriodName = 'Daily Fine',
    @StartDay = 20,
    @EndDay = 999,
    @FineAmount = 0,
    @DailyFineAmount = 10,
    @MaximumFine = 300,
    @IsFixedAmount = 0,
    @TenantID = @TenantID,
    @SchoolCode = @SchoolCode,
    @CreatedBy = @UserID

-- Example 3: Setting up a simplified late fee
EXEC dbo.SetupSimplifiedLateFee
    @AcademicYearID = @AcademicYearID,
    @MonthNumber = 1,
    @DueDay = 5,
    @GraceDays = 3,
    @LateChargeType = 1,  -- 1=Fixed Amount
    @FixedAmount = 100,
    @TenantID = @TenantID,
    @SchoolCode = @SchoolCode,
    @CreatedBy = @UserID

-- Example 4: Calculating a late fee (complex period-based)
DECLARE @StudentID UNIQUEIDENTIFIER = 'student-guid'
DECLARE @PaymentDate DATE = '2024-12-25'
DECLARE @LateFeeAmount DECIMAL(18,2)

EXEC dbo.CalculateStudentLateFee
    @StudentID = @StudentID,
    @FeeMonth = 12,
    @AcademicYearID = @AcademicYearID,
    @PaymentDate = @PaymentDate,
    @TenantID = @TenantID,
    @SchoolCode = @SchoolCode,
    @CreatedBy = @UserID,
    @LateFeeAmount = @LateFeeAmount OUTPUT

PRINT 'Late Fee Amount: ' + CAST(@LateFeeAmount AS VARCHAR(20))

-- Example 5: Calculating a late fee (simplified approach)
DECLARE @StudentID UNIQUEIDENTIFIER = 'student-guid'
DECLARE @PaymentDate DATE = '2025-01-15'
DECLARE @LateFeeAmount DECIMAL(18,2)

EXEC dbo.CalculateStudentLateFeeSimplified
    @StudentID = @StudentID,
    @FeeMonth = 1,
    @AcademicYearID = @AcademicYearID,
    @PaymentDate = @PaymentDate,
    @TenantID = @TenantID,
    @SchoolCode = @SchoolCode,
    @CreatedBy = @UserID,
    @LateFeeAmount = @LateFeeAmount OUTPUT

PRINT 'Late Fee Amount: ' + CAST(@LateFeeAmount AS VARCHAR(20))

-- Example 6: Marking a late fee as paid
DECLARE @LateFeeID UNIQUEIDENTIFIER = 'late-fee-guid'
DECLARE @PaymentID UNIQUEIDENTIFIER = 'payment-guid'

EXEC dbo.MarkLateFeeAsPaid
    @StudentLateFeeID = @LateFeeID,
    @PaymentID = @PaymentID,
    @ModifiedBy = @UserID

-- Example 7: Querying the late fees summary view
SELECT * FROM dbo.vw_StudentLateFeesSummary
WHERE SchoolCode = 123
  AND TenantID = 'your-tenant-guid'
  AND FeeMonth = 12
  AND IsPaid = 0
*/
```

## Appendix B: Implementation Guidelines

### B.1 Deployment Steps
1. Create the database tables using the SQL script in Appendix A
2. Create stored procedures for fee calculations
3. Set up initial configurations for academic years, due dates, and late fee rules
4. Develop user interfaces for administration and fee collection
5. Implement reporting functionality
6. Test with sample data before going live

### B.2 Integration Considerations
When integrating this late fee system with existing school management systems:
1. Ensure compatibility with existing student and fee data
2. Map existing fee structures to the new tables
3. Establish proper transaction handling for fee payments
4. Implement appropriate security measures for financial data
5. Create automated processes for late fee calculations

### B.3 Maintenance Recommendations
1. Regularly backup the database
2. Review and update late fee policies at the start of each academic year
3. Monitor late fee collection effectiveness
4. Update the system to accommodate policy changes
5. Perform regular audits of fee calculations

## Appendix C: Glossary

- **Academic Year**: A period defining the school year, typically spanning multiple calendar years (e.g., April to March)
- **Due Date**: The date by which a fee payment must be made to avoid late fees
- **Grace Period**: A period after the due date during which no late fee is charged
- **Late Fee**: A financial penalty for paying fees after the due date
- **Fixed Amount**: A set fee charged regardless of how late the payment is
- **Daily Amount**: A per-day charge that increases the longer payment is delayed
- **Percentage-based Fee**: A late fee calculated as a percentage of the total fee amount
- **Maximum Fine**: The upper limit on how much late fee can be charged
- **Period-based Model**: A complex late fee model with different rules for different time periods
- **Simplified Model**: A straightforward late fee model with a single rule after the grace period