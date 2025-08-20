# School Fee Management System - Late Fee Module
# Part 3: Simplified Late Fee and Mode Selection Tests

## 1. Introduction

This file contains unit tests for the simplified late fee calculation functionality and mode selection mechanism of the Late Fee Module. These tests verify that:
- The simplified late fee calculation works correctly with different fee types (fixed, daily, percentage)
- The system chooses the correct fee calculation mode based on configuration

## 2. Test Environment Setup

### 2.1 Prerequisites
- SQL Server instance (version 2016 or later)
- Test database with all late fee tables and stored procedures created
- tSQLt testing framework installed

### 2.2 Test Data Setup

```sql
-- Sample test data setup script for simplified late fee tests
-- Create test student with fee structure
DECLARE @TestStudentID UNIQUEIDENTIFIER = '98765432-1234-1234-1234-123456789012'
DECLARE @TestTenantID UNIQUEIDENTIFIER = 'ABCDEF12-1234-1234-1234-123456789012'
DECLARE @TestSchoolCode INT = 123

-- Insert fee structure records for testing percentage-based fees
INSERT INTO StudentFeeStructures (Id, StudentId, FeeHeadID, 
                                 January, February, March, April, May, June,
                                 July, August, September, October, November, December,
                                 TenantID, SchoolCode, CreatedBy)
VALUES (NEWID(), @TestStudentID, NEWID(), 
        2000, 2000, 2000, 2000, 2000, 2000,
        2000, 2000, 2000, 2000, 2000, 2000,
        @TestTenantID, @TestSchoolCode, 1)
```

## 3. Simplified Late Fee Calculation Tests

```sql
EXEC tSQLt.NewTestClass 'SimplifiedLateFeeCalculationTests'
GO

CREATE PROCEDURE SimplifiedLateFeeCalculationTests.[test fixed amount late fee with simplified model]
AS
BEGIN
    -- Arrange
    DECLARE @TestAcademicYearID UNIQUEIDENTIFIER = '12345678-1234-1234-1234-123456789012'
    DECLARE @TestTenantID UNIQUEIDENTIFIER = 'ABCDEF12-1234-1234-1234-123456789012'
    DECLARE @TestSchoolCode INT = 123
    DECLARE @TestStudentID UNIQUEIDENTIFIER = '98765432-1234-1234-1234-123456789012'
    DECLARE @TestMonth INT = 5 -- May
    DECLARE @TestUserID INT = 1
    DECLARE @LateFeeAmount DECIMAL(18,2)
    
    -- Set up simplified late fee with fixed amount
    EXEC dbo.SetupSimplifiedLateFee
        @AcademicYearID = @TestAcademicYearID,
        @MonthNumber = @TestMonth,
        @DueDay = 5,
        @GraceDays = 3,
        @LateChargeType = 1, -- Fixed Amount
        @FixedAmount = 100,
        @TenantID = @TestTenantID,
        @SchoolCode = @TestSchoolCode,
        @CreatedBy = @TestUserID
    
    -- Act - Calculate late fee for payment on 15th (10 days late, after 3-day grace period)
    EXEC dbo.CalculateStudentLateFeeSimplified
        @StudentID = @TestStudentID,
        @FeeMonth = @TestMonth,
        @AcademicYearID = @TestAcademicYearID,
        @PaymentDate = '2025-05-15',
        @TenantID = @TestTenantID,
        @SchoolCode = @TestSchoolCode,
        @CreatedBy = @TestUserID,
        @LateFeeAmount = @LateFeeAmount OUTPUT
    
    -- Assert
    EXEC tSQLt.AssertEquals 100, @LateFeeAmount, 'Late fee should be the fixed amount of 100'
END
GO

CREATE PROCEDURE SimplifiedLateFeeCalculationTests.[test daily amount late fee with simplified model]
AS
BEGIN
    -- Arrange
    DECLARE @TestAcademicYearID UNIQUEIDENTIFIER = '12345678-1234-1234-1234-123456789012'
    DECLARE @TestTenantID UNIQUEIDENTIFIER = 'ABCDEF12-1234-1234-1234-123456789012'
    DECLARE @TestSchoolCode INT = 123
    DECLARE @TestStudentID UNIQUEIDENTIFIER = '98765432-1234-1234-1234-123456789012'
    DECLARE @TestMonth INT = 6 -- June
    DECLARE @TestUserID INT = 1
    DECLARE @LateFeeAmount DECIMAL(18,2)
    
    -- Set up simplified late fee with daily amount
    EXEC dbo.SetupSimplifiedLateFee
        @AcademicYearID = @TestAcademicYearID,
        @MonthNumber = @TestMonth,
        @DueDay = 5,
        @GraceDays = 3,
        @LateChargeType = 2, -- Daily Amount
        @DailyAmount = 10,
        @MaximumFine = 200,
        @TenantID = @TestTenantID,
        @SchoolCode = @TestSchoolCode,
        @CreatedBy = @TestUserID
    
    -- Act - Calculate late fee for payment on 15th (10 days late, 7 days after grace period)
    EXEC dbo.CalculateStudentLateFeeSimplified
        @StudentID = @TestStudentID,
        @FeeMonth = @TestMonth,
        @AcademicYearID = @TestAcademicYearID,
        @PaymentDate = '2025-06-15',
        @TenantID = @TestTenantID,
        @SchoolCode = @TestSchoolCode,
        @CreatedBy = @TestUserID,
        @LateFeeAmount = @LateFeeAmount OUTPUT
    
    -- Assert
    -- 7 days after grace period * 10 per day = 70
    EXEC tSQLt.AssertEquals 70, @LateFeeAmount, 'Late fee should be 7 days * 10 per day = 70'
END
GO

CREATE PROCEDURE SimplifiedLateFeeCalculationTests.[test percentage late fee with simplified model]
AS
BEGIN
    -- Arrange
    DECLARE @TestAcademicYearID UNIQUEIDENTIFIER = '12345678-1234-1234-1234-123456789012'
    DECLARE @TestTenantID UNIQUEIDENTIFIER = 'ABCDEF12-1234-1234-1234-123456789012'
    DECLARE @TestSchoolCode INT = 123
    DECLARE @TestStudentID UNIQUEIDENTIFIER = '98765432-1234-1234-1234-123456789012'
    DECLARE @TestMonth INT = 7 -- July
    DECLARE @TestUserID INT = 1
    DECLARE @LateFeeAmount DECIMAL(18,2)
    
    -- Create student fee structure (2000 for July)
    INSERT INTO StudentFeeStructures (Id, StudentId, FeeHeadID, July, TenantID, SchoolCode, CreatedBy)
    VALUES (NEWID(), @TestStudentID, NEWID(), 2000, @TestTenantID, @TestSchoolCode, @TestUserID)
    
    -- Set up simplified late fee with percentage
    EXEC dbo.SetupSimplifiedLateFee
        @AcademicYearID = @TestAcademicYearID,
        @MonthNumber = @TestMonth,
        @DueDay = 5,
        @GraceDays = 3,
        @LateChargeType = 3, -- Percentage
        @PercentageValue = 2, -- 2% of fee amount
        @TenantID = @TestTenantID,
        @SchoolCode = @TestSchoolCode,
        @CreatedBy = @TestUserID
    
    -- Act - Calculate late fee for payment on 15th (after grace period)
    EXEC dbo.CalculateStudentLateFeeSimplified
        @StudentID = @TestStudentID,
        @FeeMonth = @TestMonth,
        @AcademicYearID = @TestAcademicYearID,
        @PaymentDate = '2025-07-15',
        @TenantID = @TestTenantID,
        @SchoolCode = @TestSchoolCode,
        @CreatedBy = @TestUserID,
        @LateFeeAmount = @LateFeeAmount OUTPUT
    
    -- Assert
    -- 2% of 2000 = 40
    EXEC tSQLt.AssertEquals 40, @LateFeeAmount, 'Late fee should be 2% of 2000 = 40'
END
GO

CREATE PROCEDURE SimplifiedLateFeeCalculationTests.[test no late fee within grace period with simplified model]
AS
BEGIN
    -- Arrange
    DECLARE @TestAcademicYearID UNIQUEIDENTIFIER = '12345678-1234-1234-1234-123456789012'
    DECLARE @TestTenantID UNIQUEIDENTIFIER = 'ABCDEF12-1234-1234-1234-123456789012'
    DECLARE @TestSchoolCode INT = 123
    DECLARE @TestStudentID UNIQUEIDENTIFIER = '98765432-1234-1234-1234-123456789012'
    DECLARE @TestMonth INT = 8 -- August
    DECLARE @TestUserID INT = 1
    DECLARE @LateFeeAmount DECIMAL(18,2)
    
    -- Set up simplified late fee with 5-day grace period
    EXEC dbo.SetupSimplifiedLateFee
        @AcademicYearID = @TestAcademicYearID,
        @MonthNumber = @TestMonth,
        @DueDay = 5,
        @GraceDays = 5, -- 5-day grace period
        @LateChargeType = 1,
        @FixedAmount = 100,
        @TenantID = @TestTenantID,
        @SchoolCode = @TestSchoolCode,
        @CreatedBy = @TestUserID
    
    -- Act - Calculate late fee for payment on 10th (5 days late, within grace period)
    EXEC dbo.CalculateStudentLateFeeSimplified
        @StudentID = @TestStudentID,
        @FeeMonth = @TestMonth,
        @AcademicYearID = @TestAcademicYearID,
        @PaymentDate = '2025-08-10',
        @TenantID = @TestTenantID,
        @SchoolCode = @TestSchoolCode,
        @CreatedBy = @TestUserID,
        @LateFeeAmount = @LateFeeAmount OUTPUT
    
    -- Assert
    EXEC tSQLt.AssertEquals 0, @LateFeeAmount, 'Late fee should be 0 within grace period'
END
GO

CREATE PROCEDURE SimplifiedLateFeeCalculationTests.[test maximum fine limit with simplified model]
AS
BEGIN
    -- Arrange
    DECLARE @TestAcademicYearID UNIQUEIDENTIFIER = '12345678-1234-1234-1234-123456789012'
    DECLARE @TestTenantID UNIQUEIDENTIFIER = 'ABCDEF12-1234-1234-1234-123456789012'
    DECLARE @TestSchoolCode INT = 123
    DECLARE @TestStudentID UNIQUEIDENTIFIER = '98765432-1234-1234-1234-123456789012'
    DECLARE @TestMonth INT = 9 -- September
    DECLARE @TestUserID INT = 1
    DECLARE @LateFeeAmount DECIMAL(18,2)
    
    -- Set up simplified late fee with daily amount and maximum limit
    EXEC dbo.SetupSimplifiedLateFee
        @AcademicYearID = @TestAcademicYearID,
        @MonthNumber = @TestMonth,
        @DueDay = 5,
        @GraceDays = 0,
        @LateChargeType = 2, -- Daily Amount
        @DailyAmount = 20,
        @MaximumFine = 150, -- Maximum fine of 150
        @TenantID = @TestTenantID,
        @SchoolCode = @TestSchoolCode,
        @CreatedBy = @TestUserID
    
    -- Act - Calculate late fee for payment on 25th (20 days late)
    -- Normal calculation would be 20 days * 20 per day = 400, exceeding the max
    EXEC dbo.CalculateStudentLateFeeSimplified
        @StudentID = @TestStudentID,
        @FeeMonth = @TestMonth,
        @AcademicYearID = @TestAcademicYearID,
        @PaymentDate = '2025-09-25',
        @TenantID = @TestTenantID,
        @SchoolCode = @TestSchoolCode,
        @CreatedBy = @TestUserID,
        @LateFeeAmount = @LateFeeAmount OUTPUT
    
    -- Assert
    EXEC tSQLt.AssertEquals 150, @LateFeeAmount, 'Late fee should be capped at the maximum fine of 150'
END
GO
```

## 4. Mode Selection Tests

```sql
EXEC tSQLt.NewTestClass 'ModeSelectionTests'
GO

CREATE PROCEDURE ModeSelectionTests.[test simplified mode is used when available]
AS
BEGIN
    -- Arrange
    DECLARE @TestAcademicYearID UNIQUEIDENTIFIER = '12345678-1234-1234-1234-123456789012'
    DECLARE @TestTenantID UNIQUEIDENTIFIER = 'ABCDEF12-1234-1234-1234-123456789012'
    DECLARE @TestSchoolCode INT = 123
    DECLARE @TestStudentID UNIQUEIDENTIFIER = '98765432-1234-1234-1234-123456789012'
    DECLARE @TestMonth INT = 9 -- September
    DECLARE @TestUserID INT = 1
    DECLARE @LateFeeAmount DECIMAL(18,2)
    
    -- Set up complex period-based late fee (100 fixed amount)
    EXEC dbo.SetupMonthlyDueDate
        @AcademicYearID = @TestAcademicYearID,
        @MonthNumber = @TestMonth,
        @DueDay = 5,
        @TenantID = @TestTenantID,
        @SchoolCode = @TestSchoolCode,
        @CreatedBy = @TestUserID
        
    EXEC dbo.SetupLatePeriod
        @AcademicYearID = @TestAcademicYearID,
        @MonthNumber = @TestMonth,
        @PeriodName = 'Fixed Fine',
        @StartDay = 0,
        @EndDay = 999,
        @FineAmount = 100,
        @TenantID = @TestTenantID,
        @SchoolCode = @TestSchoolCode,
        @CreatedBy = @TestUserID
    
    -- Set up simplified late fee (200 fixed amount)
    EXEC dbo.SetupSimplifiedLateFee
        @AcademicYearID = @TestAcademicYearID,
        @MonthNumber = @TestMonth,
        @DueDay = 5,
        @GraceDays = 0,
        @LateChargeType = 1,
        @FixedAmount = 200,
        @TenantID = @TestTenantID,
        @SchoolCode = @TestSchoolCode,
        @CreatedBy = @TestUserID
    
    -- Make sure UseSimplifiedMode is set to 1
    UPDATE dbo.FeeSimplifiedLateSettings
    SET UseSimplifiedMode = 1
    WHERE AcademicYearID = @TestAcademicYearID
      AND MonthNumber = @TestMonth
      AND TenantID = @TestTenantID
      AND SchoolCode = @TestSchoolCode
    
    -- Act - Calculate late fee
    EXEC dbo.CalculateStudentLateFeeSimplified
        @StudentID = @TestStudentID,
        @FeeMonth = @TestMonth,
        @AcademicYearID = @TestAcademicYearID,
        @PaymentDate = '2025-09-15',
        @TenantID = @TestTenantID,
        @SchoolCode = @TestSchoolCode,
        @CreatedBy = @TestUserID,
        @LateFeeAmount = @LateFeeAmount OUTPUT
    
    -- Assert
    -- If simplified mode is used, amount should be 200; if complex mode, amount would be 100
    EXEC tSQLt.AssertEquals 200, @LateFeeAmount, 'Simplified mode should be used (200 instead of 100)'
END
GO

CREATE PROCEDURE ModeSelectionTests.[test complex mode is used when simplified not available]
AS
BEGIN
    -- Arrange
    DECLARE @TestAcademicYearID UNIQUEIDENTIFIER = '12345678-1234-1234-1234-123456789012'
    DECLARE @TestTenantID UNIQUEIDENTIFIER = 'ABCDEF12-1234-1234-1234-123456789012'
    DECLARE @TestSchoolCode INT = 123
    DECLARE @TestStudentID UNIQUEIDENTIFIER = '98765432-1234-1234-1234-123456789012'
    DECLARE @TestMonth INT = 10 -- October
    DECLARE @TestUserID INT = 1
    DECLARE @LateFeeAmount DECIMAL(18,2)
    
    -- Set up complex period-based late fee only
    EXEC dbo.SetupMonthlyDueDate
        @AcademicYearID = @TestAcademicYearID,
        @MonthNumber = @TestMonth,
        @DueDay = 5,
        @TenantID = @TestTenantID,
        @SchoolCode = @TestSchoolCode,
        @CreatedBy = @TestUserID
        
    EXEC dbo.SetupLatePeriod
        @AcademicYearID = @TestAcademicYearID,
        @MonthNumber = @TestMonth,
        @PeriodName = 'Fixed Fine',
        @StartDay = 0,
        @EndDay = 999,
        @FineAmount = 150,
        @TenantID = @TestTenantID,
        @SchoolCode = @TestSchoolCode,
        @CreatedBy = @TestUserID
    
    -- Make sure no simplified settings exist
    DELETE FROM dbo.FeeSimplifiedLateSettings
    WHERE AcademicYearID = @TestAcademicYearID
      AND MonthNumber = @TestMonth
      AND TenantID = @TestTenantID
      AND SchoolCode = @TestSchoolCode
    
    -- Act - Calculate late fee using the simplified entry point
    EXEC dbo.CalculateStudentLateFeeSimplified
        @StudentID = @TestStudentID,
        @FeeMonth = @TestMonth,
        @AcademicYearID = @TestAcademicYearID,
        @PaymentDate = '2025-10-15',
        @TenantID = @TestTenantID,
        @SchoolCode = @TestSchoolCode,
        @CreatedBy = @TestUserID,
        @LateFeeAmount = @LateFeeAmount OUTPUT
    
    -- Assert
    -- Complex mode should be used, resulting in a fine of 150
    EXEC tSQLt.AssertEquals 150, @LateFeeAmount, 'Complex mode should be used when simplified not available'
END
GO

CREATE PROCEDURE ModeSelectionTests.[test simplified mode is disabled when UseSimplifiedMode=0]
AS
BEGIN
    -- Arrange
    DECLARE @TestAcademicYearID UNIQUEIDENTIFIER = '12345678-1234-1234-1234-123456789012'
    DECLARE @TestTenantID UNIQUEIDENTIFIER = 'ABCDEF12-1234-1234-1234-123456789012'
    DECLARE @TestSchoolCode INT = 123
    DECLARE @TestStudentID UNIQUEIDENTIFIER = '98765432-1234-1234-1234-123456789012'
    DECLARE @TestMonth INT = 11 -- November
    DECLARE @TestUserID INT = 1
    DECLARE @LateFeeAmount DECIMAL(18,2)
    
    -- Set up complex period-based late fee (100 fixed amount)
    EXEC dbo.SetupMonthlyDueDate
        @AcademicYearID = @TestAcademicYearID,
        @MonthNumber = @TestMonth,
        @DueDay = 5,
        @TenantID = @TestTenantID,
        @SchoolCode = @TestSchoolCode,
        @CreatedBy = @TestUserID
        
    EXEC dbo.SetupLatePeriod
        @AcademicYearID = @TestAcademicYearID,
        @MonthNumber = @TestMonth,
        @PeriodName = 'Fixed Fine',
        @StartDay = 0,
        @EndDay = 999,
        @FineAmount = 125,
        @TenantID = @TestTenantID,
        @SchoolCode = @TestSchoolCode,
        @CreatedBy = @TestUserID
    
    -- Set up simplified late fee (250 fixed amount) but explicitly disable it
    EXEC dbo.SetupSimplifiedLateFee
        @AcademicYearID = @TestAcademicYearID,
        @MonthNumber = @TestMonth,
        @DueDay = 5,
        @GraceDays = 0,
        @LateChargeType = 1,
        @FixedAmount = 250,
        @TenantID = @TestTenantID,
        @SchoolCode = @TestSchoolCode,
        @CreatedBy = @TestUserID
    
    -- Explicitly set UseSimplifiedMode to 0
    UPDATE dbo.FeeSimplifiedLateSettings
    SET UseSimplifiedMode = 0
    WHERE AcademicYearID = @TestAcademicYearID
      AND MonthNumber = @TestMonth
      AND TenantID = @TestTenantID
      AND SchoolCode = @TestSchoolCode
    
    -- Act - Calculate late fee
    EXEC dbo.CalculateStudentLateFeeSimplified
        @StudentID = @TestStudentID,
        @FeeMonth = @TestMonth,
        @AcademicYearID = @TestAcademicYearID,
        @PaymentDate = '2025-11-15',
        @TenantID = @TestTenantID,
        @SchoolCode = @TestSchoolCode,
        @CreatedBy = @TestUserID,
        @LateFeeAmount = @LateFeeAmount OUTPUT
    
    -- Assert
    -- If simplified mode is disabled, complex mode should be used (125 instead of 250)
    EXEC tSQLt.AssertEquals 125, @LateFeeAmount, 'Complex mode should be used when UseSimplifiedMode=0'
END
GO
```

## 5. Additional Tests

```sql
EXEC tSQLt.NewTestClass 'MiscLateFeeTests'
GO

CREATE PROCEDURE MiscLateFeeTests.[test mark late fee as paid]
AS
BEGIN
    -- Arrange
    DECLARE @TestAcademicYearID UNIQUEIDENTIFIER = '12345678-1234-1234-1234-123456789012'
    DECLARE @TestTenantID UNIQUEIDENTIFIER = 'ABCDEF12-1234-1234-1234-123456789012'
    DECLARE @TestSchoolCode INT = 123
    DECLARE @TestStudentID UNIQUEIDENTIFIER = '98765432-1234-1234-1234-123456789012'
    DECLARE @TestUserID INT = 1
    DECLARE @TestPaymentID UNIQUEIDENTIFIER = NEWID()
    DECLARE @LateFeeID UNIQUEIDENTIFIER
    
    -- Create a test late fee record
    INSERT INTO dbo.StudentFeeLateCharges
        (StudentID, FeeMonth, AcademicYearID, DueDate, PaymentDate, DaysLate,
         LatePeriodID, FineAmount, IsPaid, TenantID, SchoolCode, CreatedBy)
    VALUES
        (@TestStudentID, 12, @TestAcademicYearID, '2025-12-05', '2025-12-15', 10,
         NEWID(), 50, 0, @TestTenantID, @TestSchoolCode, @TestUserID)
    
    -- Get the ID of the inserted record
    SELECT @LateFeeID = Id
    FROM dbo.StudentFeeLateCharges
    WHERE StudentID = @TestStudentID
      AND AcademicYearID = @TestAcademicYearID
      AND FeeMonth = 12
      AND TenantID = @TestTenantID
      AND SchoolCode = @TestSchoolCode
    
    -- Act - Mark the late fee as paid
    EXEC dbo.MarkLateFeeAsPaid
        @StudentLateFeeID = @LateFeeID,
        @PaymentID = @TestPaymentID,
        @ModifiedBy = @TestUserID
    
    -- Assert
    DECLARE @IsPaid BIT, @RecordedPaymentID UNIQUEIDENTIFIER
    
    SELECT 
        @IsPaid = IsPaid,
        @RecordedPaymentID = PaymentID
    FROM dbo.StudentFeeLateCharges
    WHERE Id = @LateFeeID
    
    EXEC tSQLt.AssertEquals 1, @IsPaid, 'Late fee should be marked as paid'
    EXEC tSQLt.AssertEquals CONVERT(VARCHAR(36), @TestPaymentID), CONVERT(VARCHAR(36), @RecordedPaymentID), 'Payment ID should be recorded'
END
GO

CREATE PROCEDURE MiscLateFeeTests.[test summary view retrieves correct data]
AS
BEGIN
    -- Arrange
    DECLARE @TestAcademicYearID UNIQUEIDENTIFIER = '12345678-1234-1234-1234-123456789012'
    DECLARE @TestTenantID UNIQUEIDENTIFIER = 'ABCDEF12-1234-1234-1234-123456789012'
    DECLARE @TestSchoolCode INT = 123
    DECLARE @TestStudentID UNIQUEIDENTIFIER = '98765432-1234-1234-1234-123456789012'
    DECLARE @TestUserID INT = 1
    DECLARE @TestLatePeriodID UNIQUEIDENTIFIER = NEWID()
    
    -- Create a test period
    INSERT INTO dbo.FeeLatePeriods
        (Id, AcademicYearID, MonthNumber, PeriodName, StartDay, EndDay, 
         FineAmount, IsFixedAmount, TenantID, SchoolCode, CreatedBy)
    VALUES
        (@TestLatePeriodID, @TestAcademicYearID, 12, 'Test Period', 0, 999,
         75, 1, @TestTenantID, @TestSchoolCode, @TestUserID)
    
    -- Create a test late fee record
    INSERT INTO dbo.StudentFeeLateCharges
        (StudentID, FeeMonth, AcademicYearID, DueDate, PaymentDate, DaysLate,
         LatePeriodID, FineAmount, IsPaid, TenantID, SchoolCode, CreatedBy)
    VALUES
        (@TestStudentID, 12, @TestAcademicYearID, '2025-12-05', '2025-12-15', 10,
         @TestLatePeriodID, 75, 0, @TestTenantID, @TestSchoolCode, @TestUserID)
    
    -- Act - Query the summary view
    DECLARE @MonthName NVARCHAR(20), @FineAmount DECIMAL(18,2), @PeriodName NVARCHAR(100)
    
    SELECT 
        @MonthName = MonthName,
        @FineAmount = FineAmount,
        @PeriodName = PeriodName
    FROM dbo.vw_StudentLateFeesSummary
    WHERE StudentID = @TestStudentID
      AND AcademicYearID = @TestAcademicYearID
      AND FeeMonth = 12
      AND TenantID = @TestTenantID
      AND SchoolCode = @TestSchoolCode
    
    -- Assert
    EXEC tSQLt.AssertEquals 'December', @MonthName, 'Month name should be December'
    EXEC tSQLt.AssertEquals 75, @FineAmount, 'Fine amount should be 75'
    EXEC tSQLt.AssertEquals 'Test Period', @PeriodName, 'Period name should be Test Period'
END
GO
```
Upon review, I've identified several areas that would benefit from additional test cases. Here's a list of test cases we should add for complete coverage:
Additional Unit Test Cases
A. Academic Year Boundary Tests

Test late fee calculation across academic year boundary - Ensure fees due in March but paid in April are calculated correctly
Test fee due date at academic year transition - Verify proper handling of due dates at year boundaries

B. Error Handling Tests

Test invalid month number rejection - Verify system rejects month numbers outside 1-12
Test invalid due day rejection - Verify system validates due days against month length

C. Data Integrity Tests

Test tenant data isolation - Ensure tenant A can't access tenant B's fee configurations
Test soft deletion - Verify IsDeleted=1 records are ignored in calculations

D. Special Calculation Tests

Test transport fee inclusion - Verify transport fees are included in percentage calculations
Test multiple fee heads calculation - Ensure percentage calculations include all fee heads
Test zero fee amount - Verify percentage calculations work correctly with zero fee amount

E. API Sequence Tests

Test update after calculation - Verify updating settings doesn't affect already calculated fines
Test payment date before due date - Verify early payments never generate fines

F. Performance Tests

Test batch calculation performance - Measure performance when calculating fines for many students
Test large period count performance - Verify performance with many different period configurations

G. Integration Tests

Test late fee integration with fee collection - Verify late fees are correctly added to total amount due
Test late fee reporting accuracy - Ensure summary reports reflect actual charges

These additional test cases would provide more comprehensive coverage of the Late Fee Module functionality, especially for edge cases and potential integration points with other system components.