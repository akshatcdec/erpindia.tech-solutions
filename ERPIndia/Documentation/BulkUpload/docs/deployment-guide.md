# Deployment Guide - Bulk Photo Upload Module

## Prerequisites

### System Requirements
- **Operating System**: Windows Server 2016 or later
- **Web Server**: IIS 8.0 or later
- **.NET Framework**: 4.5 or later
- **SQL Server**: 2014 or later
- **RAM**: Minimum 4GB (8GB recommended)
- **Storage**: Minimum 50GB for photo storage

### Software Dependencies
- ASP.NET MVC 5
- jQuery 3.x
- Bootstrap 5.x
- Font Awesome 5.x
- Newtonsoft.Json
- Dapper (for database operations)

## Installation Steps

### 1. IIS Configuration

#### Enable Required Features
```powershell
# Run as Administrator
Enable-WindowsFeature -Name IIS-WebServerRole, IIS-WebServer, IIS-CommonHttpFeatures, IIS-HttpErrors, IIS-HttpRedirect, IIS-ApplicationDevelopment, IIS-NetFxExtensibility45, IIS-HealthAndDiagnostics, IIS-HttpLogging, IIS-Security, IIS-RequestFiltering, IIS-Performance, IIS-WebServerManagementTools, IIS-ManagementConsole, IIS-IIS6ManagementCompatibility, IIS-Metabase, IIS-ASPNET45 -IncludeAllSubFeature
```

#### Configure Application Pool
```xml
<applicationPools>
    <add name="ERPIndiaAppPool">
        <processModel identityType="ApplicationPoolIdentity" />
        <recycling>
            <periodicRestart time="03:00:00" />
        </recycling>
        <enable32BitAppOnWin64>false</enable32BitAppOnWin64>
        <managedRuntimeVersion>v4.0</managedRuntimeVersion>
    </add>
</applicationPools>
```

### 2. File System Configuration

#### Create Directory Structure
```powershell
# Create base directories
New-Item -ItemType Directory -Path "C:\inetpub\wwwroot\ERPIndia"
New-Item -ItemType Directory -Path "C:\inetpub\wwwroot\ERPIndia\Documents"

# Set permissions
$acl = Get-Acl "C:\inetpub\wwwroot\ERPIndia\Documents"
$permission = "IIS_IUSRS","FullControl","ContainerInherit,ObjectInherit","None","Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule $permission
$acl.SetAccessRule($accessRule)
Set-Acl "C:\inetpub\wwwroot\ERPIndia\Documents" $acl
```

### 3. Web.config Settings

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <connectionStrings>
    <add name="ConnectionString" 
         connectionString="Data Source=YOUR_SERVER;Initial Catalog=ERPIndia;User ID=YOUR_USER;Password=YOUR_PASSWORD;MultipleActiveResultSets=true" 
         providerName="System.Data.SqlClient" />
  </connectionStrings>
  
  <appSettings>
    <!-- Photo Upload Settings -->
    <add key="MaxPhotoSizeMB" value="5" />
    <add key="PhotoCompressionQuality" value="90" />
    <add key="PhotoTargetSizeKB" value="95" />
    <add key="AllowedPhotoFormats" value=".jpg,.jpeg,.png" />
    <add key="PhotoStoragePath" value="~/Documents" />
    
    <!-- Session Settings -->
    <add key="SessionTimeout" value="30" />
  </appSettings>
  
  <system.web>
    <compilation debug="false" targetFramework="4.5" />
    <httpRuntime targetFramework="4.5" maxRequestLength="20480" executionTimeout="300" />
    
    <!-- Session State Configuration -->
    <sessionState mode="InProc" timeout="30" />
    
    <!-- Authentication -->
    <authentication mode="Forms">
      <forms loginUrl="~/Account/Login" timeout="30" />
    </authentication>
    
    <!-- Custom Errors -->
    <customErrors mode="RemoteOnly" defaultRedirect="~/Error">
      <error statusCode="404" redirect="~/Error/NotFound" />
      <error statusCode="500" redirect="~/Error/ServerError" />
    </customErrors>
  </system.web>
  
  <system.webServer>
    <!-- MIME Types for Images -->
    <staticContent>
      <mimeMap fileExtension=".jpg" mimeType="image/jpeg" />
      <mimeMap fileExtension=".jpeg" mimeType="image/jpeg" />
      <mimeMap fileExtension=".png" mimeType="image/png" />
    </staticContent>
    
    <!-- Security Headers -->
    <httpProtocol>
      <customHeaders>
        <add name="X-Content-Type-Options" value="nosniff" />
        <add name="X-Frame-Options" value="SAMEORIGIN" />
        <add name="X-XSS-Protection" value="1; mode=block" />
      </customHeaders>
    </httpProtocol>
    
    <!-- URL Rewrite Rules -->
    <rewrite>
      <rules>
        <rule name="Block Direct Document Access">
          <match url="^Documents/.*" />
          <conditions>
            <add input="{HTTP_REFERER}" pattern="^$" />
          </conditions>
          <action type="CustomResponse" statusCode="403" />
        </rule>
      </rules>
    </rewrite>
    
    <!-- Request Filtering -->
    <security>
      <requestFiltering>
        <requestLimits maxAllowedContentLength="20971520" />
        <fileExtensions>
          <add fileExtension=".jpg" allowed="true" />
          <add fileExtension=".jpeg" allowed="true" />
          <add fileExtension=".png" allowed="true" />
        </fileExtensions>
      </requestFiltering>
    </security>
  </system.webServer>
</configuration>
```

### 4. Database Deployment

#### Run Database Scripts
```sql
-- 1. Create tables (if not exists)
-- Run scripts from Database Schema Documentation

-- 2. Create stored procedures
-- Run all stored procedures from Database Schema Documentation

-- 3. Create indexes
-- Run index creation scripts

-- 4. Grant permissions
GRANT EXECUTE ON dbo.UpdateStudentPhotosByStudentId TO [ERPIndiaUser];
GRANT EXECUTE ON dbo.sp_UpdateStudentDenormalizedFields TO [ERPIndiaUser];
GRANT SELECT, INSERT, UPDATE ON dbo.StudentInfoBasic TO [ERPIndiaUser];
GRANT SELECT, INSERT, UPDATE ON dbo.StudentInfoFamily TO [ERPIndiaUser];
```

### 5. Application Deployment

#### Build and Publish
```powershell
# Using MSBuild
msbuild ERPIndia.csproj /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=Production

# Or using Visual Studio
# Right-click project > Publish > Select Production Profile
```

#### Copy Files
```powershell
# Copy published files to IIS directory
Copy-Item -Path ".\bin\Release\Publish\*" -Destination "C:\inetpub\wwwroot\ERPIndia" -Recurse -Force
```

### 6. SSL Certificate Configuration

```powershell
# Import SSL certificate
Import-PfxCertificate -FilePath "C:\certs\erpindia.pfx" -CertStoreLocation Cert:\LocalMachine\My -Password (ConvertTo-SecureString -String "YourPassword" -AsPlainText -Force)

# Bind to IIS
New-IISSiteBinding -Name "ERPIndia" -BindingInformation "*:443:" -Protocol https -CertificateThumbPrint "YOUR_CERT_THUMBPRINT"
```

## Environment-Specific Configuration

### Development
```xml
<appSettings>
  <add key="Environment" value="Development" />
  <add key="EnableDebugLogging" value="true" />
  <add key="PhotoStoragePath" value="~/App_Data/Documents" />
</appSettings>
```

### Staging
```xml
<appSettings>
  <add key="Environment" value="Staging" />
  <add key="EnableDebugLogging" value="true" />
  <add key="PhotoStoragePath" value="D:\ERPIndia\Documents" />
</appSettings>
```

### Production
```xml
<appSettings>
  <add key="Environment" value="Production" />
  <add key="EnableDebugLogging" value="false" />
  <add key="PhotoStoragePath" value="E:\ERPIndia\Documents" />
</appSettings>
```

## Post-Deployment Verification

### 1. Health Check Script
```csharp
public class HealthCheckController : Controller
{
    public ActionResult Index()
    {
        var checks = new Dictionary<string, bool>();
        
        // Database connection
        try
        {
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
            {
                conn.Open();
                checks["Database"] = true;
            }
        }
        catch { checks["Database"] = false; }
        
        // File system access
        try
        {
            var path = Server.MapPath("~/Documents");
            Directory.Exists(path);
            checks["FileSystem"] = true;
        }
        catch { checks["FileSystem"] = false; }
        
        // Session state
        checks["Session"] = Session != null;
        
        return Json(checks, JsonRequestBehavior.AllowGet);
    }
}
```

### 2. Smoke Tests
1. Navigate to `/BulkPhotoUpload`
2. Verify dropdowns load
3. Select a class and load students
4. Upload a test photo
5. Verify photo saves and displays

### 3. Performance Baseline
```powershell
# Test load time
Measure-Command { Invoke-WebRequest -Uri "https://your-domain/BulkPhotoUpload" }

# Test photo upload
Measure-Command { 
    $form = @{
        photos = Get-Item "test.jpg"
        photoInfo = '{"studentId":"test","admsnNo":"1234","photoType":"student"}'
    }
    Invoke-RestMethod -Uri "https://your-domain/BulkPhotoUpload/SaveStudentPhotos" -Method Post -Form $form
}
```

## Monitoring Setup

### 1. IIS Logging
```xml
<system.applicationHost>
  <sites>
    <site name="ERPIndia">
      <logFile logFormat="W3C" directory="%SystemDrive%\inetpub\logs\LogFiles" period="Daily" />
    </site>
  </sites>
</system.applicationHost>
```

### 2. Application Logging
```csharp
// In Global.asax.cs
protected void Application_Error(object sender, EventArgs e)
{
    Exception exception = Server.GetLastError();
    
    // Log to file
    string logPath = Server.MapPath("~/App_Data/Logs/");
    string logFile = Path.Combine(logPath, $"error_{DateTime.Now:yyyyMMdd}.log");
    
    File.AppendAllText(logFile, $"{DateTime.Now}: {exception.Message}\n{exception.StackTrace}\n\n");
}
```

### 3. Performance Counters
Monitor these Windows Performance Counters:
- ASP.NET Applications\Requests/Sec
- ASP.NET Applications\Errors Total/Sec
- Process\Private Bytes (w3wp.exe)
- PhysicalDisk\Avg. Disk Queue Length

## Rollback Procedure

### 1. Database Rollback
```sql
-- Keep backup of photo paths before deployment
CREATE TABLE StudentPhotoBackup_[DATE] AS 
SELECT StudentId, Photo FROM StudentInfoBasic;

-- Rollback script
UPDATE b
SET b.Photo = bk.Photo
FROM StudentInfoBasic b
JOIN StudentPhotoBackup_[DATE] bk ON b.StudentId = bk.StudentId;
```

### 2. Application Rollback
```powershell
# Backup current version
Copy-Item -Path "C:\inetpub\wwwroot\ERPIndia" -Destination "C:\Backups\ERPIndia_$(Get-Date -Format 'yyyyMMdd')" -Recurse

# Restore previous version
Stop-WebSite -Name "ERPIndia"
Remove-Item -Path "C:\inetpub\wwwroot\ERPIndia\*" -Recurse -Force
Copy-Item -Path "C:\Backups\ERPIndia_Previous\*" -Destination "C:\inetpub\wwwroot\ERPIndia" -Recurse
Start-WebSite -Name "ERPIndia"
```

## Troubleshooting

### Common Issues

1. **Photos not uploading**
   - Check IIS_IUSRS permissions on Documents folder
   - Verify maxRequestLength in web.config
   - Check application pool identity permissions

2. **Database connection errors**
   - Verify connection string
   - Check SQL Server authentication
   - Ensure firewall allows SQL port

3. **Session timeout issues**
   - Increase session timeout in web.config
   - Check application pool recycling settings
   - Verify session state mode

### Debug Mode
```xml
<!-- Enable for troubleshooting -->
<system.web>
  <compilation debug="true" />
  <customErrors mode="Off" />
  <trace enabled="true" pageOutput="false" requestLimit="100" localOnly="false" />
</system.web>
```

## Security Checklist

- [ ] SSL certificate installed and configured
- [ ] Directory browsing disabled
- [ ] Custom error pages configured
- [ ] Request filtering enabled
- [ ] Security headers configured
- [ ] SQL injection prevention verified
- [ ] File upload validation implemented
- [ ] Authentication required for all endpoints
- [ ] Sensitive data encrypted in web.config
- [ ] Regular security updates scheduled