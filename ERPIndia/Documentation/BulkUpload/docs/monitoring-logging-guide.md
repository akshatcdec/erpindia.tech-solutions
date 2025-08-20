# Monitoring and Logging Guide - Bulk Photo Upload Module

## 1. Logging Architecture

### Log Levels and Categories

```csharp
public enum LogLevel
{
    Debug = 0,    // Detailed information for debugging
    Info = 1,     // General informational messages
    Warning = 2,  // Warning messages
    Error = 3,    // Error messages
    Critical = 4  // Critical failures
}

public enum LogCategory
{
    Authentication,
    FileUpload,
    ImageProcessing,
    DatabaseOperation,
    Security,
    Performance,
    UserAction
}
```

### Structured Logging Implementation

```csharp
public interface IPhotoUploadLogger
{
    void Log(LogLevel level, LogCategory category, string message, object data = null);
    void LogUpload(UploadLogEntry entry);
    void LogError(Exception ex, string context, object additionalData = null);
    void LogPerformance(string operation, TimeSpan duration, object metadata = null);
}

public class PhotoUploadLogger : IPhotoUploadLogger
{
    private readonly ILogger _logger;
    
    public void Log(LogLevel level, LogCategory category, string message, object data = null)
    {
        var logEntry = new
        {
            Timestamp = DateTime.UtcNow,
            Level = level.ToString(),
            Category = category.ToString(),
            Message = message,
            Data = data,
            SessionId = HttpContext.Current?.Session?.SessionID,
            UserId = HttpContext.Current?.User?.Identity?.Name,
            RequestId = HttpContext.Current?.Items["RequestId"],
            MachineName = Environment.MachineName
        };
        
        _logger.Log(logEntry);
    }
    
    public void LogUpload(UploadLogEntry entry)
    {
        var logData = new
        {
            entry.Timestamp,
            entry.UserId,
            entry.StudentId,
            entry.AdmissionNo,
            entry.PhotoType,
            entry.FileName,
            entry.FileSize,
            entry.CompressedSize,
            entry.ProcessingTime,
            entry.Success,
            entry.ErrorMessage,
            entry.IpAddress,
            entry.UserAgent
        };
        
        Log(LogLevel.Info, LogCategory.FileUpload, "Photo uploaded", logData);
    }
}
```

---

## 2. What to Log

### User Actions

```csharp
// Log search operations
logger.Log(LogLevel.Info, LogCategory.UserAction, "Students loaded", new
{
    Class = className,
    Section = section,
    StudentCount = students.Count,
    LoadTime = stopwatch.ElapsedMilliseconds
});

// Log photo uploads
logger.LogUpload(new UploadLogEntry
{
    Timestamp = DateTime.UtcNow,
    UserId = CurrentUserId,
    StudentId = studentId,
    AdmissionNo = admissionNo,
    PhotoType = photoType,
    FileName = fileName,
    FileSize = originalSize,
    CompressedSize = compressedSize,
    ProcessingTime = processingTime,
    Success = true,
    IpAddress = Request.UserHostAddress,
    UserAgent = Request.UserAgent
});
```

### Security Events

```csharp
// Log authentication failures
logger.Log(LogLevel.Warning, LogCategory.Security, "Authentication failed", new
{
    Username = username,
    IpAddress = Request.UserHostAddress,
    Reason = "Invalid credentials",
    AttemptNumber = failedAttempts
});

// Log unauthorized access attempts
logger.Log(LogLevel.Warning, LogCategory.Security, "Unauthorized access attempt", new
{
    Resource = "BulkPhotoUpload",
    UserId = User?.Identity?.Name,
    IpAddress = Request.UserHostAddress,
    RequestedUrl = Request.Url.ToString()
});
```

### Performance Metrics

```csharp
public class PerformanceTracker : IDisposable
{
    private readonly IPhotoUploadLogger _logger;
    private readonly string _operation;
    private readonly Stopwatch _stopwatch;
    private readonly Dictionary<string, object> _metadata;
    
    public PerformanceTracker(IPhotoUploadLogger logger, string operation)
    {
        _logger = logger;
        _operation = operation;
        _stopwatch = Stopwatch.StartNew();
        _metadata = new Dictionary<string, object>();
    }
    
    public void AddMetadata(string key, object value)
    {
        _metadata[key] = value;
    }
    
    public void Dispose()
    {
        _stopwatch.Stop();
        _logger.LogPerformance(_operation, _stopwatch.Elapsed, _metadata);
    }
}

// Usage
using (var tracker = new PerformanceTracker(logger, "ImageCompression"))
{
    tracker.AddMetadata("OriginalSize", originalSize);
    tracker.AddMetadata("TargetSize", targetSize);
    
    // Perform compression
    var result = CompressImage(image);
    
    tracker.AddMetadata("CompressedSize", result.Size);
    tracker.AddMetadata("Quality", result.Quality);
}
```

### Error Logging

```csharp
try
{
    // Operation
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to process image", new
    {
        StudentId = studentId,
        PhotoType = photoType,
        FileName = fileName,
        FileSize = fileSize,
        Operation = "ImageCompression"
    });
    
    throw;
}
```

---

## 3. Log Storage and Rotation

### File-Based Logging Configuration

```xml
<log4net>
  <appender name="RollingFileAppender" type="log4net.Appender.RollingFileAppender">
    <file value="Logs\BulkPhotoUpload.log" />
    <appendToFile value="true" />
    <rollingStyle value="Composite" />
    <datePattern value="yyyyMMdd" />
    <maxSizeRollBackups value="30" />
    <maximumFileSize value="100MB" />
    <staticLogFileName value="true" />
    <layout type="log4net.Layout.JsonLayout" />
  </appender>
  
  <appender name="ErrorFileAppender" type="log4net.Appender.RollingFileAppender">
    <file value="Logs\BulkPhotoUpload.Errors.log" />
    <appendToFile value="true" />
    <rollingStyle value="Date" />
    <datePattern value="yyyyMMdd" />
    <filter type="log4net.Filter.LevelRangeFilter">
      <levelMin value="ERROR" />
      <levelMax value="FATAL" />
    </filter>
    <layout type="log4net.Layout.JsonLayout" />
  </appender>
</log4net>
```

### Database Logging

```sql
CREATE TABLE [dbo].[PhotoUploadLogs] (
    [LogId] UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    [Timestamp] DATETIME2 NOT NULL,
    [Level] NVARCHAR(20) NOT NULL,
    [Category] NVARCHAR(50) NOT NULL,
    [Message] NVARCHAR(500) NOT NULL,
    [UserId] NVARCHAR(100),
    [SessionId] NVARCHAR(100),
    [RequestId] NVARCHAR(50),
    [MachineName] NVARCHAR(100),
    [Data] NVARCHAR(MAX),
    [Exception] NVARCHAR(MAX),
    INDEX IX_Timestamp (Timestamp DESC),
    INDEX IX_Level_Category (Level, Category),
    INDEX IX_UserId (UserId)
);

CREATE TABLE [dbo].[PhotoUploadAudit] (
    [AuditId] UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    [Timestamp] DATETIME2 NOT NULL,
    [UserId] NVARCHAR(100) NOT NULL,
    [StudentId] UNIQUEIDENTIFIER NOT NULL,
    [AdmissionNo] INT NOT NULL,
    [PhotoType] NVARCHAR(20) NOT NULL,
    [FileName] NVARCHAR(255),
    [FileSize] BIGINT,
    [CompressedSize] BIGINT,
    [ProcessingTime] INT,
    [Success] BIT NOT NULL,
    [ErrorMessage] NVARCHAR(500),
    [IpAddress] NVARCHAR(50),
    [UserAgent] NVARCHAR(500),
    INDEX IX_Timestamp (Timestamp DESC),
    INDEX IX_StudentId (StudentId),
    INDEX IX_UserId (UserId)
);
```

---

## 4. Real-time Monitoring

### Application Insights Integration

```csharp
public class ApplicationInsightsLogger : IPhotoUploadLogger
{
    private readonly TelemetryClient _telemetryClient;
    
    public void TrackPhotoUpload(PhotoUploadMetrics metrics)
    {
        var telemetry = new EventTelemetry("PhotoUpload");
        
        // Properties
        telemetry.Properties["StudentId"] = metrics.StudentId;
        telemetry.Properties["PhotoType"] = metrics.PhotoType;
        telemetry.Properties["Success"] = metrics.Success.ToString();
        
        // Metrics
        telemetry.Metrics["FileSize"] = metrics.FileSize;
        telemetry.Metrics["CompressedSize"] = metrics.CompressedSize;
        telemetry.Metrics["ProcessingTime"] = metrics.ProcessingTime;
        telemetry.Metrics["CompressionRatio"] = metrics.CompressionRatio;
        
        _telemetryClient.TrackEvent(telemetry);
    }
    
    public void TrackException(Exception ex, Dictionary<string, string> properties = null)
    {
        _telemetryClient.TrackException(ex, properties);
    }
}
```

### Custom Performance Counters

```csharp
public class PhotoUploadPerformanceCounters
{
    private readonly PerformanceCounter _uploadsPerSecond;
    private readonly PerformanceCounter _activeUploads;
    private readonly PerformanceCounter _failedUploads;
    private readonly PerformanceCounter _averageProcessingTime;
    private readonly PerformanceCounter _averageFileSize;
    
    public PhotoUploadPerformanceCounters()
    {
        const string categoryName = "Bulk Photo Upload";
        
        if (!PerformanceCounterCategory.Exists(categoryName))
        {
            var counters = new CounterCreationDataCollection
            {
                new CounterCreationData("Uploads/sec", "", PerformanceCounterType.RateOfCountsPerSecond32),
                new CounterCreationData("Active Uploads", "", PerformanceCounterType.NumberOfItems32),
                new CounterCreationData("Failed Uploads", "", PerformanceCounterType.NumberOfItems32),
                new CounterCreationData("Avg Processing Time", "", PerformanceCounterType.AverageTimer32),
                new CounterCreationData("Avg File Size", "", PerformanceCounterType.AverageCount64)
            };
            
            PerformanceCounterCategory.Create(categoryName, "Bulk Photo Upload Metrics", 
                PerformanceCounterCategoryType.SingleInstance, counters);
        }
        
        _uploadsPerSecond = new PerformanceCounter(categoryName, "Uploads/sec", false);
        _activeUploads = new PerformanceCounter(categoryName, "Active Uploads", false);
        _failedUploads = new PerformanceCounter(categoryName, "Failed Uploads", false);
        _averageProcessingTime = new PerformanceCounter(categoryName, "Avg Processing Time", false);
        _averageFileSize = new PerformanceCounter(categoryName, "Avg File Size", false);
    }
    
    public void IncrementUpload() => _uploadsPerSecond.Increment();
    public void IncrementActiveUploads() => _activeUploads.Increment();
    public void DecrementActiveUploads() => _activeUploads.Decrement();
    public void IncrementFailedUploads() => _failedUploads.Increment();
    public void RecordProcessingTime(long milliseconds) => _averageProcessingTime.RawValue = milliseconds;
    public void RecordFileSize(long bytes) => _averageFileSize.RawValue = bytes;
}
```

---

## 5. Monitoring Dashboard

### Key Metrics to Track

```sql
-- Upload Statistics View
CREATE VIEW vw_PhotoUploadStatistics AS
SELECT 
    CAST(Timestamp AS DATE) as Date,
    COUNT(*) as TotalUploads,
    SUM(CASE WHEN Success = 1 THEN 1 ELSE 0 END) as SuccessfulUploads,
    SUM(CASE WHEN Success = 0 THEN 1 ELSE 0 END) as FailedUploads,
    AVG(ProcessingTime) as AvgProcessingTime,
    AVG(FileSize) as AvgFileSize,
    AVG(CompressedSize) as AvgCompressedSize,
    COUNT(DISTINCT UserId) as UniqueUsers,
    COUNT(DISTINCT StudentId) as UniqueStudents
FROM PhotoUploadAudit
GROUP BY CAST(Timestamp AS DATE);

-- Error Analysis View
CREATE VIEW vw_PhotoUploadErrors AS
SELECT 
    CAST(Timestamp AS DATE) as Date,
    ErrorMessage,
    COUNT(*) as ErrorCount,
    COUNT(DISTINCT UserId) as AffectedUsers
FROM PhotoUploadAudit
WHERE Success = 0
GROUP BY CAST(Timestamp AS DATE), ErrorMessage;

-- Performance Metrics View
CREATE VIEW vw_PhotoUploadPerformance AS
SELECT 
    DATEPART(HOUR, Timestamp) as Hour,
    AVG(ProcessingTime) as AvgProcessingTime,
    MAX(ProcessingTime) as MaxProcessingTime,
    MIN(ProcessingTime) as MinProcessingTime,
    PERCENTILE_CONT(0.95) WITHIN GROUP (ORDER BY ProcessingTime) 
        OVER (PARTITION BY DATEPART(HOUR, Timestamp)) as P95ProcessingTime
FROM PhotoUploadAudit
WHERE Timestamp >= DATEADD(DAY, -7, GETDATE())
GROUP BY DATEPART(HOUR, Timestamp);
```

### Real-time Dashboard HTML

```html
<!DOCTYPE html>
<html>
<head>
    <title>Bulk Photo Upload Monitoring Dashboard</title>
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <style>
        .dashboard { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 20px; }
        .metric-card { background: #f8f9fa; padding: 20px; border-radius: 8px; }
        .metric-value { font-size: 2em; font-weight: bold; }
        .metric-label { color: #6c757d; }
    </style>
</head>
<body>
    <h1>Bulk Photo Upload Monitoring</h1>
    
    <div class="dashboard">
        <div class="metric-card">
            <div class="metric-label">Total Uploads Today</div>
            <div class="metric-value" id="totalUploads">0</div>
        </div>
        
        <div class="metric-card">
            <div class="metric-label">Success Rate</div>
            <div class="metric-value" id="successRate">0%</div>
        </div>
        
        <div class="metric-card">
            <div class="metric-label">Active Users</div>
            <div class="metric-value" id="activeUsers">0</div>
        </div>
        
        <div class="metric-card">
            <div class="metric-label">Avg Processing Time</div>
            <div class="metric-value" id="avgProcessingTime">0ms</div>
        </div>
    </div>
    
    <div style="margin-top: 30px;">
        <canvas id="uploadsChart"></canvas>
    </div>
    
    <script>
        // Real-time updates using SignalR or periodic AJAX
        setInterval(updateDashboard, 5000);
        
        function updateDashboard() {
            fetch('/api/monitoring/photoUploadStats')
                .then(response => response.json())
                .then(data => {
                    document.getElementById('totalUploads').textContent = data.totalUploads;
                    document.getElementById('successRate').textContent = data.successRate + '%';
                    document.getElementById('activeUsers').textContent = data.activeUsers;
                    document.getElementById('avgProcessingTime').textContent = data.avgProcessingTime + 'ms';
                    
                    updateChart(data.hourlyStats);
                });
        }
        
        function updateChart(hourlyData) {
            // Update Chart.js chart with hourly upload statistics
        }
    </script>
</body>
</html>
```

---

## 6. Alert Configuration

### Alert Rules

```csharp
public class AlertConfiguration
{
    public List<AlertRule> Rules { get; set; } = new List<AlertRule>
    {
        new AlertRule
        {
            Name = "High Error Rate",
            Condition = "ErrorRate > 10",
            TimeWindow = TimeSpan.FromMinutes(5),
            Severity = AlertSeverity.Critical,
            Actions = new[] { "Email", "SMS" }
        },
        new AlertRule
        {
            Name = "Slow Processing",
            Condition = "AvgProcessingTime > 3000",
            TimeWindow = TimeSpan.FromMinutes(10),
            Severity = AlertSeverity.Warning,
            Actions = new[] { "Email" }
        },
        new AlertRule
        {
            Name = "Disk Space Low",
            Condition = "DiskFreeSpace < 1073741824", // 1GB
            TimeWindow = TimeSpan.FromMinutes(1),
            Severity = AlertSeverity.Critical,
            Actions = new[] { "Email", "SMS", "Slack" }
        }
    };
}

public class AlertManager
{
    private readonly IAlertNotificationService _notificationService;
    private readonly IMetricsService _metricsService;
    
    public async Task CheckAlerts()
    {
        foreach (var rule in _configuration.Rules)
        {
            var metrics = await _metricsService.GetMetrics(rule.TimeWindow);
            
            if (EvaluateCondition(rule.Condition, metrics))
            {
                await _notificationService.SendAlert(new Alert
                {
                    Rule = rule,
                    Timestamp = DateTime.UtcNow,
                    Metrics = metrics,
                    Message = $"Alert: {rule.Name} - {rule.Condition}"
                });
            }
        }
    }
}
```

### Email Alert Template

```html
<html>
<body>
    <h2>Bulk Photo Upload Alert</h2>
    <p><strong>Alert:</strong> {{AlertName}}</p>
    <p><strong>Severity:</strong> {{Severity}}</p>
    <p><strong>Time:</strong> {{Timestamp}}</p>
    <p><strong>Condition:</strong> {{Condition}}</p>
    
    <h3>Current Metrics:</h3>
    <ul>
        <li>Error Rate: {{ErrorRate}}%</li>
        <li>Processing Time: {{AvgProcessingTime}}ms</li>
        <li>Active Users: {{ActiveUsers}}</li>
        <li>Failed Uploads (last hour): {{FailedUploads}}</li>
    </ul>
    
    <p><a href="{{DashboardUrl}}">View Dashboard</a></p>
</body>
</html>
```

---

## 7. Log Analysis Queries

### Common Analysis Queries

```sql
-- Top errors in last 24 hours
SELECT TOP 10
    ErrorMessage,
    COUNT(*) as Count,
    MAX(Timestamp) as LastOccurrence
FROM PhotoUploadAudit
WHERE Success = 0
    AND Timestamp >= DATEADD(HOUR, -24, GETDATE())
GROUP BY ErrorMessage
ORDER BY COUNT(*) DESC;

-- User activity analysis
SELECT 
    UserId,
    COUNT(*) as TotalUploads,
    SUM(CASE WHEN Success = 1 THEN 1 ELSE 0 END) as SuccessfulUploads,
    AVG(ProcessingTime) as AvgProcessingTime,
    SUM(FileSize) / 1024.0 / 1024.0 as TotalMB
FROM PhotoUploadAudit
WHERE Timestamp >= DATEADD(DAY, -7, GETDATE())
GROUP BY UserId
ORDER BY TotalUploads DESC;

-- Performance degradation detection
WITH HourlyPerformance AS (
    SELECT 
        DATEPART(HOUR, Timestamp) as Hour,
        AVG(ProcessingTime) as AvgTime
    FROM PhotoUploadAudit
    WHERE Timestamp >= DATEADD(DAY, -1, GETDATE())
    GROUP BY DATEPART(HOUR, Timestamp)
)
SELECT 
    h1.Hour,
    h1.AvgTime as CurrentAvg,
    h2.AvgTime as PreviousAvg,
    ((h1.AvgTime - h2.AvgTime) / h2.AvgTime * 100) as PercentChange
FROM HourlyPerformance h1
JOIN HourlyPerformance h2 ON h1.Hour = h2.Hour + 1
WHERE ((h1.AvgTime - h2.AvgTime) / h2.AvgTime * 100) > 20;
```

---

## 8. Retention and Archival

### Log Retention Policy

```csharp
public class LogRetentionService
{
    private readonly IConfiguration _configuration;
    
    public async Task ExecuteRetentionPolicy()
    {
        var policies = new[]
        {
            new RetentionPolicy { Level = "Debug", RetentionDays = 7 },
            new RetentionPolicy { Level = "Info", RetentionDays = 30 },
            new RetentionPolicy { Level = "Warning", RetentionDays = 90 },
            new RetentionPolicy { Level = "Error", RetentionDays = 180 },
            new RetentionPolicy { Level = "Critical", RetentionDays = 365 }
        };
        
        foreach (var policy in policies)
        {
            await ArchiveLogs(policy);
            await DeleteOldLogs(policy);
        }
    }
    
    private async Task ArchiveLogs(RetentionPolicy policy)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-policy.RetentionDays);
        
        var sql = @"
            INSERT INTO PhotoUploadLogsArchive
            SELECT * FROM PhotoUploadLogs
            WHERE Level = @Level AND Timestamp < @CutoffDate";
        
        await _database.ExecuteAsync(sql, new { policy.Level, CutoffDate = cutoffDate });
    }
}
```

### Automated Cleanup Script

```sql
-- Stored procedure for log cleanup
CREATE PROCEDURE sp_CleanupPhotoUploadLogs
AS
BEGIN
    DECLARE @DebugRetention DATE = DATEADD(DAY, -7, GETDATE());
    DECLARE @InfoRetention DATE = DATEADD(DAY, -30, GETDATE());
    DECLARE @WarningRetention DATE = DATEADD(DAY, -90, GETDATE());
    DECLARE @ErrorRetention DATE = DATEADD(DAY, -180, GETDATE());
    
    -- Archive before deletion
    INSERT INTO PhotoUploadLogsArchive
    SELECT * FROM PhotoUploadLogs
    WHERE (Level = 'Debug' AND Timestamp < @DebugRetention)
        OR (Level = 'Info' AND Timestamp < @InfoRetention)
        OR (Level = 'Warning' AND Timestamp < @WarningRetention)
        OR (Level = 'Error' AND Timestamp < @ErrorRetention);
    
    -- Delete old logs
    DELETE FROM PhotoUploadLogs
    WHERE (Level = 'Debug' AND Timestamp < @DebugRetention)
        OR (Level = 'Info' AND Timestamp < @InfoRetention)
        OR (Level = 'Warning' AND Timestamp < @WarningRetention)
        OR (Level = 'Error' AND Timestamp < @ErrorRetention);
    
    -- Update statistics
    UPDATE STATISTICS PhotoUploadLogs;
    UPDATE STATISTICS PhotoUploadLogsArchive;
END;
```

---

## 9. Integration with External Systems

### Splunk Integration

```xml
<!-- Splunk forwarder configuration -->
<configuration>
  <input>
    <monitor>
      <path>C:\Logs\BulkPhotoUpload*.log</path>
      <sourcetype>bulk_photo_upload</sourcetype>
      <index>erpindia</index>
    </monitor>
  </input>
  
  <props>
    <sourcetype name="bulk_photo_upload">
      <SHOULD_LINEMERGE>false</SHOULD_LINEMERGE>
      <LINE_BREAKER>([\r\n]+)</LINE_BREAKER>
      <TRUNCATE>10000</TRUNCATE>
      <KV_MODE>json</KV_MODE>
    </sourcetype>
  </props>
</configuration>
```

### ELK Stack Configuration

```yaml
# Logstash configuration
input {
  file {
    path => "/logs/BulkPhotoUpload*.log"
    start_position => "beginning"
    codec => "json"
  }
}

filter {
  date {
    match => [ "Timestamp", "ISO8601" ]
  }
  
  if [Level] == "Error" or [Level] == "Critical" {
    mutate {
      add_tag => [ "alert" ]
    }
  }
}

output {
  elasticsearch {
    hosts => ["localhost:9200"]
    index => "bulkphotoupload-%{+YYYY.MM.dd}"
  }
}
```

---

## 10. Monitoring Checklist

### Daily Monitoring Tasks
- [ ] Check error rate (< 5%)
- [ ] Verify average processing time (< 2 seconds)
- [ ] Review failed uploads
- [ ] Check disk space availability
- [ ] Monitor active user count

### Weekly Monitoring Tasks
- [ ] Analyze performance trends
- [ ] Review top errors
- [ ] Check log file sizes
- [ ] Verify backup completion
- [ ] Review security alerts

### Monthly Monitoring Tasks
- [ ] Generate usage reports
- [ ] Analyze user patterns
- [ ] Review system capacity
- [ ] Update alert thresholds
- [ ] Performance optimization review