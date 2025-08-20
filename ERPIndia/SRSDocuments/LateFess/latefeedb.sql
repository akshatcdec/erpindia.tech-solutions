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
*

/*
this complete SQL script provides everything you need to implement the late fee system for your school fee management solution. Let me explain what each part does:
Tables Created

FeeMonthlyDueDates: Defines when payments are due for each month
FeeLatePeriods: Supports complex late fee scenarios with multiple periods
FeeSimplifiedLateSettings: Provides a simpler late fee model option
StudentFeeLateCharges: Tracks all late fees applied to students

Stored Procedures

SetupMonthlyDueDate: Configures monthly due dates
SetupLatePeriod: Creates different late fee periods with various fine structures
SetupSimplifiedLateFee: Sets up the simplified late fee model
CalculateStudentLateFee: Calculates late fees using the complex period approach
CalculateStudentLateFeeSimplified: Entry point that decides which calculation method to use
MarkLateFeeAsPaid: Updates the payment status of a late fee

Views

vw_StudentLateFeesSummary: Provides a comprehensive view of all late fees with user-friendly details

Example Usage
The script includes commented example code that shows how to:

Set up due dates
Configure complex period-based late fees
Configure simplified late fees
Calculate late fees using both approaches
Mark late fees as paid
Query the summary view

This script is designed to integrate with your existing fee structure tables while providing the flexibility to use either a complex period-based or simplified late fee model. You can adapt the example code to your needs by replacing the placeholder GUIDs and values with your actual data.
The system maintains a detailed audit trail with CreatedBy/ModifiedBy fields and timestamps, ensuring you can track all changes. It also includes data validation to prevent invalid configurations, such as negative fine amounts or invalid charge types.
To use this script, simply run it in your SQL Server Management Studio (or equivalent tool) to create the necessary tables, procedures, and views. Then use the stored procedures to configure your late fee settings and calculate late fees when processing payments.
*/