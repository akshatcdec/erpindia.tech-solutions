# Error Code Reference Guide - Bulk Photo Upload Module

## Error Code Structure

Format: `BPU-XXXX`
- BPU = Bulk Photo Upload
- First digit: Category (1=Client, 2=Server, 3=Database, 4=File System, 5=Network)
- Last 3 digits: Specific error

---

## 1. Client-Side Errors (1000-1999)

### File Validation Errors

| Code | Message | Description | User Action |
|------|---------|-------------|-------------|
| BPU-1001 | Invalid file type | File is not JPG, JPEG, or PNG | Select a valid image file |
| BPU-1002 | File too large | File exceeds 5MB limit | Reduce file size or select smaller image |
| BPU-1003 | No file selected | No file chosen for upload | Click photo circle and select a file |
| BPU-1004 | Invalid image dimensions | Image smaller than 200x200 pixels | Use higher resolution image |
| BPU-1005 | Corrupted file | File appears to be corrupted | Try a different image file |

### Input Validation Errors

| Code | Message | Description | User Action |
|------|---------|-------------|-------------|
| BPU-1101 | Class not selected | No class chosen before loading | Select a class from dropdown |
| BPU-1102 | Invalid student data | Student information corrupted | Refresh page and try again |
| BPU-1103 | No photos to save | Save clicked without uploading photos | Upload at least one photo |
| BPU-1104 | Invalid photo type | Photo type not recognized | Contact support |

### Browser/Device Errors

| Code | Message | Description | User Action |
|------|---------|-------------|-------------|
| BPU-1201 | Camera access denied | Browser blocked camera access | Allow camera in browser settings |
| BPU-1202 | Camera not available | No camera detected on device | Use file upload instead |
| BPU-1203 | Browser not supported | Old browser version | Update browser or use different one |
| BPU-1204 | Storage quota exceeded | Browser storage full | Clear browser cache |
| BPU-1205 | JavaScript disabled | Browser JavaScript turned off | Enable JavaScript in settings |

---

## 2. Server-Side Errors (2000-2999)

### Authentication/Authorization Errors

| Code | Message | Description | User Action |
|------|---------|-------------|-------------|
| BPU-2001 | Session expired | User session timed out | Log in again |
| BPU-2002 | Unauthorized access | User lacks permission | Contact administrator for access |
| BPU-2003 | Invalid authentication token | Security token invalid | Refresh page and try again |
| BPU-2004 | Account locked | Too many failed attempts | Wait 30 minutes or contact admin |

### Processing Errors

| Code | Message | Description | User Action |
|------|---------|-------------|-------------|
| BPU-2101 | Image processing failed | Server couldn't process image | Try different image or format |
| BPU-2102 | Compression error | Failed to compress image | Upload smaller image |
| BPU-2103 | Invalid request format | Malformed request data | Refresh page and retry |
| BPU-2104 | Server timeout | Request took too long | Try uploading fewer photos |
| BPU-2105 | Memory limit exceeded | Server out of memory | Try again later |

### Validation Errors

| Code | Message | Description | User Action |
|------|---------|-------------|-------------|
| BPU-2201 | Invalid student ID | Student ID format incorrect | Verify student information |
| BPU-2202 | Invalid school code | School code not recognized | Contact support |
| BPU-2203 | Duplicate file upload | Same file uploaded twice | Remove duplicate and retry |
| BPU-2204 | Invalid photo metadata | Photo information corrupted | Re-upload the photo |

---

## 3. Database Errors (3000-3999)

### Connection Errors

| Code | Message | Description | User Action |
|------|---------|-------------|-------------|
| BPU-3001 | Database connection failed | Cannot connect to database | Try again in few minutes |
| BPU-3002 | Connection timeout | Database response too slow | Retry operation |
| BPU-3003 | Connection pool exhausted | Too many connections | Wait and try again |

### Query Errors

| Code | Message | Description | User Action |
|------|---------|-------------|-------------|
| BPU-3101 | Student not found | Student record doesn't exist | Verify student details |
| BPU-3102 | Update failed | Database update unsuccessful | Retry save operation |
| BPU-3103 | Transaction rollback | Operation cancelled due to error | Retry entire operation |
| BPU-3104 | Constraint violation | Data integrity issue | Contact support |
| BPU-3105 | Deadlock detected | Database conflict | Retry after few seconds |

### Data Integrity Errors

| Code | Message | Description | User Action |
|------|---------|-------------|-------------|
| BPU-3201 | Duplicate entry | Record already exists | Check existing data |
| BPU-3202 | Foreign key violation | Related record missing | Verify student enrollment |
| BPU-3203 | Data truncation | Data too long for field | Contact support |
| BPU-3204 | Null value error | Required field missing | Fill all required fields |

---

## 4. File System Errors (4000-4999)

### Storage Errors

| Code | Message | Description | User Action |
|------|---------|-------------|-------------|
| BPU-4001 | Disk space full | Server storage exhausted | Contact administrator |
| BPU-4002 | Write permission denied | Cannot save to directory | Contact IT support |
| BPU-4003 | Directory not found | Upload folder missing | Contact IT support |
| BPU-4004 | File already exists | Duplicate filename | Will be overwritten |

### File Operation Errors

| Code | Message | Description | User Action |
|------|---------|-------------|-------------|
| BPU-4101 | File save failed | Cannot write file to disk | Retry upload |
| BPU-4102 | File delete failed | Cannot remove old file | Contact support |
| BPU-4103 | File move failed | Cannot relocate file | Retry operation |
| BPU-4104 | File read error | Cannot access uploaded file | Re-upload file |

### Path Errors

| Code | Message | Description | User Action |
|------|---------|-------------|-------------|
| BPU-4201 | Invalid file path | Path contains illegal characters | Contact support |
| BPU-4202 | Path too long | File path exceeds limit | Contact support |
| BPU-4203 | Access denied | No permission for path | Contact administrator |

---

## 5. Network Errors (5000-5999)

### Connection Errors

| Code | Message | Description | User Action |
|------|---------|-------------|-------------|
| BPU-5001 | Network unavailable | No internet connection | Check connection |
| BPU-5002 | Request timeout | Network too slow | Check internet speed |
| BPU-5003 | Connection reset | Connection interrupted | Retry upload |
| BPU-5004 | DNS resolution failed | Cannot reach server | Check network settings |

### Transfer Errors

| Code | Message | Description | User Action |
|------|---------|-------------|-------------|
| BPU-5101 | Upload interrupted | Transfer incomplete | Retry upload |
| BPU-5102 | Download failed | Cannot retrieve image | Refresh page |
| BPU-5103 | Bandwidth exceeded | Transfer limit reached | Try again later |
| BPU-5104 | Proxy error | Proxy server issue | Check proxy settings |

---

## Error Response Format

### JSON Response Structure
```json
{
    "success": false,
    "error": {
        "code": "BPU-2101",
        "message": "Image processing failed",
        "details": "Unable to read image header",
        "timestamp": "2024-01-15T10:30:45Z",
        "requestId": "req_123456"
    },
    "data": null
}
```

### User-Friendly Display
```javascript
function displayError(error) {
    const userMessage = `
        <div class="alert alert-danger">
            <strong>Error ${error.code}</strong>
            <p>${error.message}</p>
            <small>Please try the suggested action or contact support with error code.</small>
        </div>
    `;
    showToast(userMessage, 'error');
}
```

---

## Error Handling Best Practices

### For Developers

1. **Always Include Error Code**
   ```csharp
   return Json(new {
       success = false,
       error = new {
           code = "BPU-2101",
           message = "Image processing failed",
           details = ex.Message
       }
   });
   ```

2. **Log Detailed Information**
   ```csharp
   Logger.Error($"Error {errorCode}: {ex.Message}", ex);
   ```

3. **Provide Actionable Messages**
   - Tell users what went wrong
   - Suggest what they can do
   - When to contact support

### For Support Staff

1. **Error Code Lookup Process**
   - Identify error category from first digit
   - Look up specific error in this guide
   - Check logs for additional details
   - Follow escalation procedure if needed

2. **Common Resolution Steps**
   - Client errors (1xxx): User action required
   - Server errors (2xxx): May need app restart
   - Database errors (3xxx): Check DB connectivity
   - File errors (4xxx): Verify permissions
   - Network errors (5xxx): Check connectivity

---

## Troubleshooting Flowchart

```
Error Occurs
    |
    v
Is it 1xxx? --> Yes --> Guide user through correct action
    |
    No
    |
    v
Is it 2xxx? --> Yes --> Check server logs, restart if needed
    |
    No
    |
    v
Is it 3xxx? --> Yes --> Verify database connectivity
    |
    No
    |
    v
Is it 4xxx? --> Yes --> Check file permissions and disk space
    |
    No
    |
    v
Is it 5xxx? --> Yes --> Verify network connectivity
```

---

## Support Escalation Matrix

| Error Range | Level 1 Support | Level 2 Support | Level 3 Support |
|-------------|-----------------|-----------------|-----------------|
| 1000-1999 | Can resolve | Escalate if persistent | Development team |
| 2000-2999 | Basic checks | Application team | Development team |
| 3000-3999 | Cannot resolve | Database team | DBA team |
| 4000-4999 | Check permissions | System admin | Infrastructure |
| 5000-5999 | Basic network check | Network team | ISP/Network admin |

---

## Quick Reference Card

### Most Common Errors

1. **BPU-1001**: Wrong file type → Use JPG/PNG only
2. **BPU-1002**: File too large → Reduce to under 5MB
3. **BPU-2001**: Session expired → Log in again
4. **BPU-3001**: Database down → Wait and retry
5. **BPU-5001**: No internet → Check connection

### Support Contact Info

- **Email**: support@erpindia.com
- **Phone**: 1800-XXX-XXXX
- **Priority Support**: priority@erpindia.com
- **Include**: Error code, timestamp, user ID, steps to reproduce