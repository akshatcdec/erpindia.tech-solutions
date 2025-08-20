# Bulk Photo Upload Module - Requirements Document

## 1. Executive Summary

The Bulk Photo Upload module is a web-based feature designed for educational institutions using ERP India system. It enables administrators to efficiently upload and manage photos of students and their family members (father, mother, guardian) in bulk for an entire class or section.

## 2. Business Objectives

- **Efficiency**: Reduce time spent on individual photo uploads by allowing bulk operations
- **Data Integrity**: Ensure photos are correctly linked to students using unique identifiers
- **User Experience**: Provide an intuitive interface that works seamlessly on both desktop and mobile devices
- **Storage Optimization**: Compress images to maintain quality while minimizing storage space

## 3. Functional Requirements

### 3.1 User Interface Requirements

#### 3.1.1 Search and Filter
- Dropdown selection for Class (Required field)
- Dropdown selection for Section (Optional - defaults to "All Sections")
- Load Students button to fetch student records

#### 3.1.2 Student Data Display
- Tabular format showing:
  - Save button for each student
  - Photo upload circles for: Student, Father, Mother, Guardian
  - Student information: Name, Father's Name, Class, Admission No., Roll No., Gender, Mobile
- Alternating row colors (white and gray #e9ecef) for better readability
- Visual indicators for uploaded photos

#### 3.1.3 Photo Upload Methods
- **File Upload**: Click on photo circle to select image from device
- **Camera Capture**: Click camera icon to capture photo using device camera
- **Drag and Drop**: Support for dragging images onto photo circles (desktop)

#### 3.1.4 Mobile Responsiveness
- Horizontal scrolling for table on mobile devices
- Sticky first column (Save button) for easy access while scrolling
- Touch-optimized controls with minimum 44px touch targets
- Full-screen camera modal on mobile devices
- Swipe indicator showing table is scrollable

### 3.2 Photo Management Requirements

#### 3.2.1 Supported File Types
- JPEG/JPG
- PNG
- Maximum file size: 5MB before compression

#### 3.2.2 Image Processing
- Automatic compression to 95KB while maintaining quality
- Resize images if necessary (minimum 200x200 pixels)
- Convert all images to JPEG format for consistency
- Quality setting: 90%

#### 3.2.3 File Naming Convention
- Pattern: `{AdmissionNumber}_{PhotoType}.jpg`
- Photo type suffixes:
  - Student: "stu"
  - Father: "father"
  - Mother: "mother"
  - Guardian: "guard"
- Example: `1234_stu.jpg`, `1234_father.jpg`

#### 3.2.4 Storage Structure
- Base path: `/Documents/{SchoolCode}/StudentProfile/`
- Example: `/Documents/101/StudentProfile/1234_stu.jpg`

### 3.3 Camera Capture Requirements

#### 3.3.1 Camera Features
- Support for front and back camera on mobile devices
- Live preview before capture
- Capture, Recapture, and Use Photo options
- Automatic compression after capture
- Size display after compression

### 3.4 Save Functionality

#### 3.4.1 Save Process
- Individual save button per student
- Button states:
  - Normal: "Save" (Green background)
  - Processing: "Saving..." with spinner (Yellow background)
  - Success: "Saved!" with checkmark (Green background)
- Batch upload of all selected photos for a student
- Visual feedback with highlighted rows for unsaved changes

#### 3.4.2 Success Feedback
- Modal popup showing "X images have been saved successfully"
- Proper grammar (1 image has / 2 images have)
- Auto-close after 3 seconds
- Optional success sound effect
- Toast notifications for errors

### 3.5 Database Requirements

#### 3.5.1 Update Strategy
- Use StudentId (GUID) for database updates
- Use AdmissionNumber for file naming
- Update both StudentInfoBasic and StudentInfoFamily tables
- Maintain photo paths in respective columns:
  - Student photo: StudentInfoBasic.Photo
  - Father photo: StudentInfoFamily.FPhoto
  - Mother photo: StudentInfoFamily.MPhoto
  - Guardian photo: StudentInfoFamily.GPhoto

## 4. Technical Requirements

### 4.1 Architecture
- **Frontend**: ASP.NET MVC with Razor views
- **Backend**: C# controllers with repository pattern
- **Database**: SQL Server with stored procedures
- **File Storage**: Server file system

### 4.2 Browser Compatibility
- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)
- Mobile browsers (iOS Safari, Chrome Mobile)

### 4.3 Performance Requirements
- Image compression must complete within 2 seconds
- Page load time < 3 seconds
- Smooth scrolling on mobile devices
- Handle up to 100 students per class

### 4.4 Security Requirements
- Authentication required (BaseController)
- Authorization based on user role
- Secure file upload validation
- Prevention of path traversal attacks
- Input sanitization for all user inputs

## 5. User Experience Requirements

### 5.1 Visual Design
- Consistent with ERP India branding
- Clear visual hierarchy
- Color scheme:
  - Primary: #5BC0DE (Light Blue)
  - Success: #28a745 (Green)
  - Warning: #ffc107 (Yellow)
  - Error: #dc3545 (Red)
  - Class badge: #7B3FF2 (Purple)

### 5.2 Accessibility
- Sufficient color contrast for text
- Alternative text for images
- Keyboard navigation support
- Clear error messages
- Loading indicators for all async operations

### 5.3 Error Handling
- Clear error messages for:
  - Invalid file types
  - File size exceeded
  - Network errors
  - Camera access denied
  - Database update failures
- Graceful degradation when features unavailable

## 6. Integration Requirements

### 6.1 Existing System Integration
- Integrate with existing StudentRepository
- Use existing authentication/authorization
- Compatible with current database schema
- Maintain existing file path conventions

### 6.2 Dependencies
- jQuery for DOM manipulation
- Bootstrap 5 for UI components
- Font Awesome for icons
- .NET Framework image processing libraries

## 7. Constraints and Limitations

- Maximum file size: 5MB per image
- Supported formats: JPEG, PNG only
- One photo per type per student
- Requires modern browser with camera API support
- Internet connection required for saving

## 8. Future Enhancements

- Bulk delete functionality
- Photo cropping/editing tools
- Face detection for automatic cropping
- Progress bar for batch uploads
- Export functionality for photo reports
- Integration with cloud storage services
- AI-powered photo quality checks

## 9. Success Metrics

- Reduction in time spent on photo management by 70%
- User satisfaction score > 4.5/5
- Zero data loss during uploads
- 99.9% successful upload rate
- Mobile usage adoption > 40%

## 10. Acceptance Criteria

- [ ] All photos upload successfully with correct naming
- [ ] Mobile scrolling works smoothly
- [ ] Save button shows proper state transitions
- [ ] Success modal displays after saving
- [ ] Alternating row colors display correctly
- [ ] Camera capture works on mobile devices
- [ ] Image compression maintains acceptable quality
- [ ] All error scenarios handled gracefully
- [ ] Database updates complete successfully
- [ ] File system storage organized correctly