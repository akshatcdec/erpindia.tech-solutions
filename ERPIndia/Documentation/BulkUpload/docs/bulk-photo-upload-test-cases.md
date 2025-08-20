# Bulk Photo Upload Module - Test Cases

## Test Case Document Information
- **Module**: Bulk Photo Upload
- **Version**: 1.0
- **Created Date**: Current Date
- **Test Environment**: ERP India System

---

## 1. Functional Test Cases

### TC001: Load Students by Class
**Priority**: High  
**Preconditions**: User logged in with appropriate permissions

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Navigate to Bulk Photo Upload page | Page loads successfully with class and section dropdowns | |
| 2 | Select a class from dropdown | Class is selected | |
| 3 | Click "Load Students" button | Loading spinner appears | |
| 4 | Wait for response | Student table displays with all students from selected class | |

### TC002: Load Students by Class and Section
**Priority**: High  
**Preconditions**: User logged in with appropriate permissions

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Navigate to Bulk Photo Upload page | Page loads successfully | |
| 2 | Select a class from dropdown | Class is selected | |
| 3 | Select a section from dropdown | Section is selected | |
| 4 | Click "Load Students" button | Student table displays filtered by class and section | |

### TC003: Load Students Without Selecting Class
**Priority**: Medium  
**Preconditions**: User logged in

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Navigate to Bulk Photo Upload page | Page loads successfully | |
| 2 | Without selecting class, click "Load Students" | Error toast: "Please select a class" | |

---

## 2. Photo Upload Test Cases

### TC004: Upload Photo via File Selection
**Priority**: High  
**Preconditions**: Students loaded in table

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Click on empty photo circle | File dialog opens | |
| 2 | Select a JPG image < 5MB | Loading spinner appears on photo circle | |
| 3 | Wait for processing | Photo appears in circle with green border | |
| 4 | | Compression info shows (e.g., "95KB") | |
| 5 | | Row highlights in yellow (has-changes class) | |
| 6 | | Toast: "Photo added. Click Save to upload." | |

### TC005: Upload Large Image (>5MB)
**Priority**: Medium  
**Preconditions**: Students loaded

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Click on photo circle | File dialog opens | |
| 2 | Select image > 5MB | Error toast: "File size must be less than 5MB" | |
| 3 | | Photo circle remains empty | |

### TC006: Upload Invalid File Type
**Priority**: Medium  
**Preconditions**: Students loaded

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Click on photo circle | File dialog opens | |
| 2 | Select a GIF/BMP/PDF file | Error toast: "Only JPG and PNG files allowed" | |
| 3 | | Photo circle remains empty | |

### TC007: Replace Existing Photo
**Priority**: Medium  
**Preconditions**: Photo already uploaded for a student

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Click on photo circle with existing photo | File dialog opens | |
| 2 | Select new image | Old photo replaced with new one | |
| 3 | | Row highlights in yellow | |

### TC008: Remove Photo
**Priority**: Medium  
**Preconditions**: Photo uploaded

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Hover over uploaded photo | Remove button (X) appears | |
| 2 | Click remove button | Photo removed, user icon appears | |
| 3 | | Photo circle returns to empty state | |

---

## 3. Camera Capture Test Cases

### TC009: Capture Photo Using Camera
**Priority**: High  
**Preconditions**: Device with camera, students loaded

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Click camera icon on photo circle | Camera modal opens | |
| 2 | | Live camera preview displays | |
| 3 | Click "Capture" button | Photo captured, preview shows | |
| 4 | | Buttons change to "Recapture" and "Use Photo" | |
| 5 | Click "Use Photo" | Modal closes, photo appears in circle | |
| 6 | | Row highlights in yellow | |

### TC010: Switch Camera (Mobile)
**Priority**: Medium  
**Preconditions**: Mobile device with front/back camera

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Open camera modal | Camera selection buttons visible | |
| 2 | Click "Back Camera" | Camera switches to back | |
| 3 | Click "Front Camera" | Camera switches to front | |

### TC011: Recapture Photo
**Priority**: Medium  
**Preconditions**: Photo captured in camera modal

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | After capture, click "Recapture" | Returns to live camera view | |
| 2 | Capture new photo | New photo preview displays | |

### TC012: Cancel Camera Capture
**Priority**: Low  
**Preconditions**: Camera modal open

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Click "Cancel" or X button | Modal closes without saving | |
| 2 | | Photo circle remains unchanged | |

---

## 4. Save Functionality Test Cases

### TC013: Save Single Student Photos
**Priority**: High  
**Preconditions**: Photos uploaded for a student

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Upload 1-4 photos for a student | Photos display in circles | |
| 2 | Click "Save" button | Button changes to "Saving..." with spinner | |
| 3 | | Button turns yellow | |
| 4 | Wait for save | Button shows "Saved!" with checkmark | |
| 5 | | Success modal: "X images have been saved successfully" | |
| 6 | | Row removes yellow highlight | |
| 7 | After 3 seconds | Button returns to normal "Save" state | |

### TC014: Save Without Photos
**Priority**: Medium  
**Preconditions**: No photos uploaded

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Click Save without uploading photos | Error toast: "No new photos to save for this student" | |
| 2 | | Save button remains enabled | |

### TC015: Save Multiple Students
**Priority**: High  
**Preconditions**: Photos uploaded for multiple students

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Upload photos for Student A | Row A highlights yellow | |
| 2 | Upload photos for Student B | Row B highlights yellow | |
| 3 | Save Student A | Student A saved successfully | |
| 4 | Save Student B | Student B saved successfully | |
| 5 | | Each save shows individual success modal | |

### TC016: Network Error During Save
**Priority**: Medium  
**Preconditions**: Photos uploaded, network issues simulated

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Upload photos | Photos display | |
| 2 | Disconnect network | | |
| 3 | Click Save | Error toast with appropriate message | |
| 4 | | Save button returns to normal state | |

---

## 5. Mobile Responsiveness Test Cases

### TC017: Table Horizontal Scroll
**Priority**: High  
**Preconditions**: Mobile device, students loaded

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Load page on mobile | Scroll indicator appears | |
| 2 | Swipe left on table | Table scrolls horizontally | |
| 3 | | Save button column remains sticky | |
| 4 | Swipe right | Table scrolls back | |

### TC018: Touch Targets
**Priority**: Medium  
**Preconditions**: Mobile device

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Tap photo circle | File selection or camera opens | |
| 2 | Tap camera icon | Camera modal opens | |
| 3 | | All touch targets >= 44px | |

### TC019: Modal Full Screen (Mobile)
**Priority**: Low  
**Preconditions**: Mobile device

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Open camera modal | Modal displays full screen | |
| 2 | | All controls accessible | |

---

## 6. Visual Design Test Cases

### TC020: Alternating Row Colors
**Priority**: Low  
**Preconditions**: Students loaded

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Load students | Odd rows: white background | |
| 2 | | Even rows: gray (#e9ecef) background | |
| 3 | | Save button column follows same pattern | |

### TC021: Header Colors
**Priority**: Low  
**Preconditions**: Page loaded

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Check table headers | All headers have #5BC0DE background | |
| 2 | | White text color | |
| 3 | On mobile, check sticky header | Save header maintains blue color | |

### TC022: Hover Effects
**Priority**: Low  
**Preconditions**: Desktop, students loaded

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Hover over table row | Row background changes to #dee2e6 | |
| 2 | Hover over photo circle | Border color changes to blue | |
| 3 | Hover over save button | Slight elevation effect | |

---

## 7. File Management Test Cases

### TC023: File Naming Convention
**Priority**: High  
**Preconditions**: Backend access to verify files

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Save student photo (AdmsnNo: 1234) | File saved as "1234_stu.jpg" | |
| 2 | Save father photo | File saved as "1234_father.jpg" | |
| 3 | Save mother photo | File saved as "1234_mother.jpg" | |
| 4 | Save guardian photo | File saved as "1234_guard.jpg" | |

### TC024: File Path Structure
**Priority**: Medium  
**Preconditions**: Backend access

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Save photos for school code 101 | Files saved in /Documents/101/StudentProfile/ | |
| 2 | Verify directory structure | Correct path maintained | |

### TC025: Replace Existing Files
**Priority**: Medium  
**Preconditions**: Photos already saved

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Upload new photo for same type | Old file deleted | |
| 2 | | New file saved with same name | |
| 3 | | Database updated with new path | |

---

## 8. Database Test Cases

### TC026: Database Update - Basic Info
**Priority**: High  
**Preconditions**: Database access

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Save student photo | StudentInfoBasic.Photo updated | |
| 2 | | ModifiedDate updated | |
| 3 | | ModifiedBy set correctly | |

### TC027: Database Update - Family Info
**Priority**: High  
**Preconditions**: Database access

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Save father photo | StudentInfoFamily.FPhoto updated | |
| 2 | Save mother photo | StudentInfoFamily.MPhoto updated | |
| 3 | Save guardian photo | StudentInfoFamily.GPhoto updated | |
| 4 | | Family record created if not exists | |

### TC028: StudentId vs AdmsnNo Usage
**Priority**: High  
**Preconditions**: Debug mode

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Save photos | Database updated using StudentId | |
| 2 | Check file system | Files named using AdmsnNo | |

---

## 9. Error Handling Test Cases

### TC029: Corrupted Image Upload
**Priority**: Low  
**Preconditions**: Corrupted image file

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Select corrupted image | Error handled gracefully | |
| 2 | | Error toast displayed | |
| 3 | | System remains stable | |

### TC030: Concurrent Save Operations
**Priority**: Medium  
**Preconditions**: Multiple photos ready

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Click Save for Student A | Save begins | |
| 2 | Immediately click Save for Student B | Second save queues or processes | |
| 3 | | Both complete successfully | |

---

## 10. Performance Test Cases

### TC031: Image Compression Performance
**Priority**: Medium  
**Preconditions**: Various image sizes

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Upload 5MB image | Compression completes < 2 seconds | |
| 2 | Upload 1MB image | Compression completes < 1 second | |
| 3 | | Final size ~95KB | |

### TC032: Load 100 Students
**Priority**: Medium  
**Preconditions**: Class with 100+ students

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Select large class | Loading begins | |
| 2 | | Table renders < 3 seconds | |
| 3 | | Scrolling remains smooth | |

---

## 11. Security Test Cases

### TC033: Authentication Check
**Priority**: High  
**Preconditions**: Not logged in

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Access bulk photo URL directly | Redirected to login | |
| 2 | | No access to functionality | |

### TC034: Path Traversal Prevention
**Priority**: High  
**Preconditions**: Security testing tools

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Attempt to upload with "../" in filename | Attack prevented | |
| 2 | | File saved with safe name | |

### TC035: SQL Injection Prevention
**Priority**: High  
**Preconditions**: Testing with malicious input

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Enter SQL in class selection | Input sanitized | |
| 2 | | No database errors | |

---

## 12. Browser Compatibility Test Cases

### TC036: Chrome Testing
**Priority**: High  
**Preconditions**: Latest Chrome

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Test all functionality | All features work | |
| 2 | | No console errors | |

### TC037: Safari Testing
**Priority**: Medium  
**Preconditions**: Latest Safari

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Test camera capture | Camera access works | |
| 2 | Test file upload | File selection works | |
| 3 | | Styling renders correctly | |

### TC038: Mobile Browser Testing
**Priority**: High  
**Preconditions**: iOS Safari, Chrome Mobile

| Step | Action | Expected Result | Status |
|------|--------|-----------------|--------|
| 1 | Test on iOS Safari | All features functional | |
| 2 | Test on Chrome Mobile | Touch events work | |
| 3 | | Camera capture works | |

---

## Test Execution Summary

| Category | Total | Passed | Failed | Blocked | Not Tested |
|----------|-------|--------|--------|---------|------------|
| Functional | 3 | | | | |
| Photo Upload | 5 | | | | |
| Camera Capture | 4 | | | | |
| Save Functionality | 4 | | | | |
| Mobile Responsiveness | 3 | | | | |
| Visual Design | 3 | | | | |
| File Management | 3 | | | | |
| Database | 3 | | | | |
| Error Handling | 2 | | | | |
| Performance | 2 | | | | |
| Security | 3 | | | | |
| Browser Compatibility | 3 | | | | |
| **TOTAL** | **38** | | | | |

---

## Defect Tracking Template

| Defect ID | Test Case | Description | Severity | Status | Assigned To |
|-----------|-----------|-------------|----------|---------|-------------|
| | | | | | |

## Test Environment Details

- **Server**: [Server Name]
- **Database**: SQL Server [Version]
- **Browser Versions Tested**: 
  - Chrome: [Version]
  - Firefox: [Version]
  - Safari: [Version]
  - Edge: [Version]
- **Mobile Devices**:
  - iOS: [Device/Version]
  - Android: [Device/Version]
- **Test Data**: [Details of test data used]

## Sign-off

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Test Lead | | | |
| Developer | | | |
| Project Manager | | | |
| Client Representative | | | |