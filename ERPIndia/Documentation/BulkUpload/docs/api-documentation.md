# API Documentation - Bulk Photo Upload Module

## Base URL
```
https://[your-domain]/BulkPhotoUpload
```

## Authentication
All endpoints require authentication via session cookie or authorization header.

---

## 1. Get Students for Photo Upload

### Endpoint
```
POST /BulkPhotoUpload/GetStudentsForPhotoUpload
```

### Description
Retrieves a list of students based on class and optional section filter.

### Request
```http
POST /BulkPhotoUpload/GetStudentsForPhotoUpload HTTP/1.1
Content-Type: application/x-www-form-urlencoded

className=10&section=A
```

### Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| className | string | Yes | Class identifier |
| section | string | No | Section identifier (optional) |

### Response
```json
{
  "success": true,
  "data": [
    {
      "StudentId": "550e8400-e29b-41d4-a716-446655440000",
      "AdmsnNo": 1234,
      "SchoolCode": 101,
      "RollNo": "16",
      "Class": "10 - A",
      "StudentName": "John Doe",
      "FatherName": "James Doe",
      "MotherName": "Jane Doe",
      "Gender": "Male",
      "Mobile": "9044542084",
      "StudentPhoto": "/Documents/101/StudentProfile/1234_stu.jpg",
      "FatherPhoto": "/Documents/101/StudentProfile/1234_father.jpg",
      "MotherPhoto": null,
      "GuardianPhoto": null
    }
  ]
}
```

### Error Responses
```json
{
  "success": false,
  "message": "Error message describing the issue"
}
```

### Status Codes
- `200 OK` - Successful retrieval
- `400 Bad Request` - Invalid parameters
- `401 Unauthorized` - Authentication required
- `500 Internal Server Error` - Server error

---

## 2. Save Student Photos

### Endpoint
```
POST /BulkPhotoUpload/SaveStudentPhotos
```

### Description
Uploads and saves photos for students and their family members.

### Request
```http
POST /BulkPhotoUpload/SaveStudentPhotos HTTP/1.1
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW

------WebKitFormBoundary7MA4YWxkTrZu0gW
Content-Disposition: form-data; name="photos"; filename="student_photo.jpg"
Content-Type: image/jpeg

[Binary image data]
------WebKitFormBoundary7MA4YWxkTrZu0gW
Content-Disposition: form-data; name="photoInfo"

{"studentId":"550e8400-e29b-41d4-a716-446655440000","admsnNo":"1234","photoType":"student","fileName":"1234_student.jpg"}
------WebKitFormBoundary7MA4YWxkTrZu0gW--
```

### Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| photos | file[] | Yes | Array of image files |
| photoInfo | JSON[] | Yes | Array of photo metadata |

### PhotoInfo Object
```json
{
  "studentId": "550e8400-e29b-41d4-a716-446655440000",
  "admsnNo": "1234",
  "photoType": "student|father|mother|guardian",
  "fileName": "1234_stu.jpg"
}
```

### Response
```json
{
  "success": true,
  "message": "3 images have been saved successfully.",
  "errors": []
}
```

### Error Response
```json
{
  "success": false,
  "message": "No photos were uploaded successfully",
  "errors": [
    "Invalid file type for student 1234",
    "Error processing photo 2: File size exceeds limit"
  ]
}
```

### File Validation
- **Allowed formats**: JPG, JPEG, PNG
- **Max file size**: 5MB (before compression)
- **Compression target**: ~95KB
- **Min dimensions**: 200x200 pixels

---

## 3. Get Student Photo

### Endpoint
```
GET /BulkPhotoUpload/GetStudentPhoto
```

### Description
Retrieves a specific photo for a student.

### Request
```http
GET /BulkPhotoUpload/GetStudentPhoto?admsnNo=1234&photoType=student HTTP/1.1
```

### Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| admsnNo | string | Yes | Admission number |
| photoType | string | Yes | Type of photo (student/father/mother/guardian) |

### Response
Returns the image file with appropriate headers:
```
Content-Type: image/jpeg
Content-Length: [file size]
```

### Status Codes
- `200 OK` - Image returned successfully
- `404 Not Found` - Image not found
- `500 Internal Server Error` - Server error

---

## Error Code Reference

| Code | Message | Description |
|------|---------|-------------|
| ERR001 | Invalid file type | File format not supported |
| ERR002 | File size exceeded | File larger than 5MB |
| ERR003 | No file uploaded | Request contains no files |
| ERR004 | Invalid student ID | Student ID format invalid |
| ERR005 | Database update failed | Failed to update database |
| ERR006 | File system error | Failed to save file |
| ERR007 | Compression failed | Image compression error |
| ERR008 | Invalid photo type | Photo type not recognized |

---

## Rate Limiting
- **Requests per minute**: 60
- **Max concurrent uploads**: 10
- **Max file size per request**: 20MB total

---

## Examples

### cURL Example - Get Students
```bash
curl -X POST https://your-domain/BulkPhotoUpload/GetStudentsForPhotoUpload \
  -H "Cookie: ASP.NET_SessionId=your-session-id" \
  -d "className=10&section=A"
```

### JavaScript Example - Save Photos
```javascript
const formData = new FormData();
formData.append('photos', photoFile, '1234_student.jpg');
formData.append('photoInfo', JSON.stringify({
  studentId: '550e8400-e29b-41d4-a716-446655440000',
  admsnNo: '1234',
  photoType: 'student',
  fileName: '1234_student.jpg'
}));

fetch('/BulkPhotoUpload/SaveStudentPhotos', {
  method: 'POST',
  body: formData,
  credentials: 'include'
})
.then(response => response.json())
.then(data => console.log(data));
```

### C# Example - Using HttpClient
```csharp
using (var client = new HttpClient())
{
    var content = new MultipartFormDataContent();
    content.Add(new ByteArrayContent(imageBytes), "photos", "1234_student.jpg");
    content.Add(new StringContent(photoInfoJson), "photoInfo");
    
    var response = await client.PostAsync(
        "https://your-domain/BulkPhotoUpload/SaveStudentPhotos", 
        content
    );
    
    var result = await response.Content.ReadAsStringAsync();
}
```