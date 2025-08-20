# Database Schema Documentation - Bulk Photo Upload Module

## Table Structures

### 1. StudentInfoBasic
Primary table for student information including student photo path.

```sql
CREATE TABLE [dbo].[StudentInfoBasic] (
    [StudentId] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    [AdmsnNo] INT NOT NULL,
    [SchoolCode] INT NOT NULL,
    [StudentNo] NVARCHAR(50),
    [SrNo] NVARCHAR(50),
    [RollNo] NVARCHAR(50),
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100),
    [Gender] NVARCHAR(10),
    [DOB] DATE,
    [Mobile] NVARCHAR(20),
    [Email] NVARCHAR(100),
    [Photo] NVARCHAR(500), -- Student photo path
    [ClassId] UNIQUEIDENTIFIER,
    [SectionId] UNIQUEIDENTIFIER,
    [Class] NVARCHAR(50),
    [Section] NVARCHAR(50),
    [ClassName] NVARCHAR(100),
    [SectionName] NVARCHAR(100),
    [FatherName] NVARCHAR(100), -- Denormalized from StudentInfoFamily
    [MotherName] NVARCHAR(100), -- Denormalized from StudentInfoFamily
    [IsActive] BIT DEFAULT 1,
    [IsDeleted] BIT DEFAULT 0,
    [CreatedBy] UNIQUEIDENTIFIER,
    [CreatedDate] DATETIME DEFAULT GETDATE(),
    [ModifiedBy] UNIQUEIDENTIFIER,
    [ModifiedDate] DATETIME,
    [TenantID] UNIQUEIDENTIFIER,
    [TenantCode] INT,
    [SessionID] UNIQUEIDENTIFIER,
    CONSTRAINT [UK_StudentInfoBasic_AdmsnNo_SchoolCode] UNIQUE ([AdmsnNo], [SchoolCode])
);

-- Indexes
CREATE INDEX [IX_StudentInfoBasic_Class_Section] ON [StudentInfoBasic]([Class], [Section]);
CREATE INDEX [IX_StudentInfoBasic_SchoolCode] ON [StudentInfoBasic]([SchoolCode]);
CREATE INDEX [IX_StudentInfoBasic_IsActive] ON [StudentInfoBasic]([IsActive], [IsDeleted]);
```

### 2. StudentInfoFamily
Stores family information including family member photos.

```sql
CREATE TABLE [dbo].[StudentInfoFamily] (
    [FamilyId] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    [StudentId] UNIQUEIDENTIFIER NOT NULL,
    [AdmsnNo] INT NOT NULL,
    [SchoolCode] INT NOT NULL,
    [FName] NVARCHAR(100), -- Father Name
    [FPhone] NVARCHAR(20), -- Father Phone
    [FOccupation] NVARCHAR(100),
    [FAadhar] NVARCHAR(20),
    [FPhoto] NVARCHAR(500), -- Father photo path
    [MName] NVARCHAR(100), -- Mother Name
    [MPhone] NVARCHAR(20), -- Mother Phone
    [MOccupation] NVARCHAR(100),
    [MAadhar] NVARCHAR(20),
    [MPhoto] NVARCHAR(500), -- Mother photo path
    [GName] NVARCHAR(100), -- Guardian Name
    [GPhone] NVARCHAR(20), -- Guardian Phone
    [GRelation] NVARCHAR(50),
    [GPhoto] NVARCHAR(500), -- Guardian photo path
    [IsActive] BIT DEFAULT 1,
    [IsDeleted] BIT DEFAULT 0,
    [CreatedBy] UNIQUEIDENTIFIER,
    [CreatedDate] DATETIME DEFAULT GETDATE(),
    [ModifiedBy] UNIQUEIDENTIFIER,
    [ModifiedDate] DATETIME,
    [TenantID] UNIQUEIDENTIFIER,
    [TenantCode] INT,
    [SessionID] UNIQUEIDENTIFIER,
    CONSTRAINT [FK_StudentInfoFamily_StudentId] FOREIGN KEY ([StudentId]) 
        REFERENCES [StudentInfoBasic]([StudentId])
);

-- Indexes
CREATE INDEX [IX_StudentInfoFamily_StudentId] ON [StudentInfoFamily]([StudentId]);
CREATE INDEX [IX_StudentInfoFamily_AdmsnNo_SchoolCode] ON [StudentInfoFamily]([AdmsnNo], [SchoolCode]);
```

### 3. vwStudentInfo (View)
Combined view for student information used in queries.

```sql
CREATE VIEW [dbo].[vwStudentInfo]
AS
SELECT 
    b.StudentId,
    b.AdmsnNo,
    b.SchoolCode,
    b.StudentNo,
    b.SrNo,
    b.RollNo,
    b.FirstName,
    b.LastName,
    b.Gender,
    b.DOB,
    b.Mobile,
    b.Email,
    b.Photo,
    b.Class,
    b.Section,
    b.ClassName,
    b.SectionName,
    b.FatherName,
    b.MotherName,
    b.IsActive,
    b.IsDeleted,
    b.TenantID,
    b.SessionID,
    f.FPhoto AS FatherPhoto,
    f.MPhoto AS MotherPhoto,
    f.GPhoto AS GuardianPhoto,
    f.FAadhar AS FatherAadhar,
    f.MAadhar AS MotherAadhar,
    f.FPhone AS FatherMobile,
    f.MPhone AS MotherMobile
FROM 
    dbo.StudentInfoBasic b
LEFT JOIN 
    dbo.StudentInfoFamily f ON b.StudentId = f.StudentId
WHERE 
    b.IsDeleted = 0;
```

## Stored Procedures

### 1. UpdateStudentPhotosByStudentId
Updates photo paths for a student using StudentId.

```sql
CREATE PROCEDURE [dbo].[UpdateStudentPhotosByStudentId]
    @StudentId UNIQUEIDENTIFIER,
    @SchoolCode INT,
    @StudentPhoto NVARCHAR(500) = NULL,
    @FatherPhoto NVARCHAR(500) = NULL,
    @MotherPhoto NVARCHAR(500) = NULL,
    @GuardianPhoto NVARCHAR(500) = NULL,
    @ModifiedBy UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- Update student photo if provided
        IF @StudentPhoto IS NOT NULL
        BEGIN
            UPDATE StudentInfoBasic 
            SET Photo = @StudentPhoto,
                ModifiedDate = GETDATE(),
                ModifiedBy = @ModifiedBy
            WHERE StudentId = @StudentId 
            AND SchoolCode = @SchoolCode;
        END
        
        -- Check if family photos need updating
        IF @FatherPhoto IS NOT NULL OR @MotherPhoto IS NOT NULL OR @GuardianPhoto IS NOT NULL
        BEGIN
            -- Ensure family record exists
            IF NOT EXISTS (SELECT 1 FROM StudentInfoFamily WHERE StudentId = @StudentId)
            BEGIN
                INSERT INTO StudentInfoFamily (
                    StudentId, AdmsnNo, SchoolCode, 
                    CreatedBy, CreatedDate, IsActive
                )
                SELECT 
                    StudentId, AdmsnNo, SchoolCode, 
                    @ModifiedBy, GETDATE(), 1
                FROM StudentInfoBasic
                WHERE StudentId = @StudentId AND SchoolCode = @SchoolCode;
            END
            
            -- Update family photos
            UPDATE StudentInfoFamily 
            SET FPhoto = ISNULL(@FatherPhoto, FPhoto),
                MPhoto = ISNULL(@MotherPhoto, MPhoto),
                GPhoto = ISNULL(@GuardianPhoto, GPhoto),
                ModifiedDate = GETDATE(),
                ModifiedBy = @ModifiedBy
            WHERE StudentId = @StudentId 
            AND SchoolCode = @SchoolCode;
        END
        
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
```

### 2. sp_UpdateStudentDenormalizedFields
Updates denormalized fields in StudentInfoBasic from StudentInfoFamily.

```sql
CREATE PROCEDURE [dbo].[sp_UpdateStudentDenormalizedFields]
    @StudentId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE b
    SET b.FatherName = f.FName,
        b.MotherName = f.MName,
        b.ModifiedDate = GETDATE()
    FROM StudentInfoBasic b
    INNER JOIN StudentInfoFamily f ON b.StudentId = f.StudentId
    WHERE b.StudentId = @StudentId;
END;
```

### 3. sp_GetStudentPhotoStatus
Returns photo upload status for a class.

```sql
CREATE PROCEDURE [dbo].[sp_GetStudentPhotoStatus]
    @Class NVARCHAR(50),
    @Section NVARCHAR(50) = NULL,
    @SchoolCode INT
AS
BEGIN
    SELECT 
        COUNT(*) AS TotalStudents,
        SUM(CASE WHEN b.Photo IS NOT NULL THEN 1 ELSE 0 END) AS StudentsWithPhoto,
        SUM(CASE WHEN f.FPhoto IS NOT NULL THEN 1 ELSE 0 END) AS FatherPhotos,
        SUM(CASE WHEN f.MPhoto IS NOT NULL THEN 1 ELSE 0 END) AS MotherPhotos,
        SUM(CASE WHEN f.GPhoto IS NOT NULL THEN 1 ELSE 0 END) AS GuardianPhotos
    FROM StudentInfoBasic b
    LEFT JOIN StudentInfoFamily f ON b.StudentId = f.StudentId
    WHERE b.Class = @Class 
        AND (@Section IS NULL OR b.Section = @Section)
        AND b.SchoolCode = @SchoolCode
        AND b.IsActive = 1 
        AND b.IsDeleted = 0;
END;
```

## Database Maintenance Scripts

### 1. Cleanup Orphaned Photo Records
```sql
-- Find students with photo paths but missing files
CREATE PROCEDURE [dbo].[sp_CleanupOrphanedPhotos]
AS
BEGIN
    -- This returns records for manual verification
    -- Actual file deletion should be done by application
    SELECT 
        'Student' AS PhotoType,
        b.StudentId,
        b.AdmsnNo,
        b.Photo AS PhotoPath
    FROM StudentInfoBasic b
    WHERE b.Photo IS NOT NULL
        AND b.IsDeleted = 0
    
    UNION ALL
    
    SELECT 
        'Father' AS PhotoType,
        f.StudentId,
        f.AdmsnNo,
        f.FPhoto AS PhotoPath
    FROM StudentInfoFamily f
    WHERE f.FPhoto IS NOT NULL
    
    UNION ALL
    
    SELECT 
        'Mother' AS PhotoType,
        f.StudentId,
        f.AdmsnNo,
        f.MPhoto AS PhotoPath
    FROM StudentInfoFamily f
    WHERE f.MPhoto IS NOT NULL
    
    UNION ALL
    
    SELECT 
        'Guardian' AS PhotoType,
        f.StudentId,
        f.AdmsnNo,
        f.GPhoto AS PhotoPath
    FROM StudentInfoFamily f
    WHERE f.GPhoto IS NOT NULL;
END;
```

### 2. Archive Old Student Photos
```sql
CREATE TABLE [dbo].[StudentPhotoArchive] (
    [ArchiveId] UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    [StudentId] UNIQUEIDENTIFIER,
    [AdmsnNo] INT,
    [PhotoType] NVARCHAR(20),
    [PhotoPath] NVARCHAR(500),
    [ArchivedDate] DATETIME DEFAULT GETDATE(),
    [ArchivedBy] UNIQUEIDENTIFIER
);

CREATE PROCEDURE [dbo].[sp_ArchiveStudentPhotos]
    @SessionId UNIQUEIDENTIFIER,
    @ArchiveBy UNIQUEIDENTIFIER
AS
BEGIN
    -- Archive photos from previous sessions
    INSERT INTO StudentPhotoArchive (StudentId, AdmsnNo, PhotoType, PhotoPath, ArchivedBy)
    SELECT 
        b.StudentId,
        b.AdmsnNo,
        'Student',
        b.Photo,
        @ArchiveBy
    FROM StudentInfoBasic b
    WHERE b.SessionID != @SessionId
        AND b.Photo IS NOT NULL
    
    UNION ALL
    
    SELECT 
        f.StudentId,
        f.AdmsnNo,
        'Father',
        f.FPhoto,
        @ArchiveBy
    FROM StudentInfoFamily f
    WHERE f.SessionID != @SessionId
        AND f.FPhoto IS NOT NULL
    
    -- Continue for other photo types...
END;
```

## Performance Optimization

### Recommended Indexes
```sql
-- For photo retrieval
CREATE INDEX [IX_Photo_Lookup] ON [StudentInfoBasic]([AdmsnNo], [SchoolCode]) INCLUDE ([Photo]);
CREATE INDEX [IX_Family_Photo_Lookup] ON [StudentInfoFamily]([StudentId]) INCLUDE ([FPhoto], [MPhoto], [GPhoto]);

-- For bulk operations
CREATE INDEX [IX_Bulk_Update] ON [StudentInfoBasic]([Class], [Section], [SchoolCode], [IsActive], [IsDeleted]);

-- For session-based queries
CREATE INDEX [IX_Session_Filter] ON [StudentInfoBasic]([SessionID], [TenantID]) INCLUDE ([StudentId]);
```

### Statistics Update
```sql
-- Run weekly
UPDATE STATISTICS [dbo].[StudentInfoBasic] WITH FULLSCAN;
UPDATE STATISTICS [dbo].[StudentInfoFamily] WITH FULLSCAN;
```

## Data Dictionary

| Column | Data Type | Description | Constraints |
|--------|-----------|-------------|-------------|
| StudentId | UNIQUEIDENTIFIER | Unique student identifier | PK, Not Null |
| AdmsnNo | INT | Admission number | Not Null |
| SchoolCode | INT | School identifier | Not Null |
| Photo | NVARCHAR(500) | Student photo file path | Nullable |
| FPhoto | NVARCHAR(500) | Father photo file path | Nullable |
| MPhoto | NVARCHAR(500) | Mother photo file path | Nullable |
| GPhoto | NVARCHAR(500) | Guardian photo file path | Nullable |

## Migration Scripts

### Add Photo Columns (if not exists)
```sql
-- Add photo columns to existing tables
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('StudentInfoBasic') AND name = 'Photo')
BEGIN
    ALTER TABLE StudentInfoBasic ADD Photo NVARCHAR(500);
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('StudentInfoFamily') AND name = 'FPhoto')
BEGIN
    ALTER TABLE StudentInfoFamily 
    ADD FPhoto NVARCHAR(500),
        MPhoto NVARCHAR(500),
        GPhoto NVARCHAR(500);
END
```