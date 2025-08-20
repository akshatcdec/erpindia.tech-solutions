/**
 * Enhanced Webcam Photo Upload System with Image Compression
 * Supports both webcam capture and file upload with automatic compression to <100KB
 * Uses a single modal for all photo types (student, father, mother, guardian)
 * Includes mobile device front/back camera selection
 * Shows validation warnings for files over 100KB
 * Restricts uploads to JPG and PNG file types only
 */
document.addEventListener('DOMContentLoaded', function () {
    // Create custom styles for camera selection and validation messages
    const style = document.createElement('style');
    style.textContent = `
    .camera-selection .btn-group {
        display: flex;
        justify-content: center;
        margin-bottom: 15px;
    }
    .camera-selection .btn {
        padding: 8px 16px;
    }
    .camera-selection .btn.active {
        background-color: #0d6efd;
        color: white;
    }
    .compression-info {
        text-align: center;
        font-size: 13px;
        color: #6c757d;
        margin-top: 10px;
    }
    .compression-info.warning {
        color: #dc3545;
    }
    .file-size-warning {
        color: #dc3545;
        font-size: 12px;
        margin-top: 5px;
        display: none;
    }
    `;
    document.head.appendChild(style);

    // Get common elements
    const webcamModal = document.getElementById('webcamModal');
    const modalTitle = webcamModal.querySelector('.modal-title');
    const webcamEl = document.getElementById('webcam');
    const canvasEl = document.getElementById('webcamCanvas');
    const capturedImageEl = document.getElementById('capturedImage');
    const captureBtnEl = document.getElementById('captureBtn');
    const recaptureBtnEl = document.getElementById('recaptureBtn');
    const saveBtnEl = document.getElementById('saveImageBtn');
    const frontCameraBtnEl = document.getElementById('frontCameraBtn');
    const backCameraBtnEl = document.getElementById('backCameraBtn');
    const cameraSelectionDiv = webcamModal.querySelector('.camera-selection');

    // Create compression info element
    const compressionInfoEl = document.createElement('div');
    compressionInfoEl.className = 'compression-info';
    compressionInfoEl.textContent = 'Image will be automatically compressed to under 100KB';
    webcamModal.querySelector('.modal-body').appendChild(compressionInfoEl);

    // Initialize Bootstrap modal
    const modal = new bootstrap.Modal(webcamModal);

    // Current state
    let currentStream = null;
    let currentFacingMode = 'user'; // Default to front camera
    let currentPhotoType = null;
    let currentFileInput = null;
    let currentContainer = null;
    let capturedImageBlob = null;
    let capturedImageSize = 0;

    // Function to detect if device is mobile
    function isMobileDevice() {
        return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);
    }

    // Update file input restrictions to only accept JPG and PNG
    document.querySelectorAll('.image-sign').forEach(input => {
        input.setAttribute('accept', 'image/jpeg, image/png');
    });

    // Add size warning message to all photo containers
    document.querySelectorAll('.profile-upload').forEach(container => {
        const warningEl = document.createElement('div');
        warningEl.className = 'file-size-warning';
        warningEl.textContent = 'File size must be less than 100KB. Your file will be compressed automatically.';
        container.appendChild(warningEl);
    });

    // Setup click handlers for all "Take Photo" buttons
    document.querySelectorAll('.take-photo-btn').forEach(btn => {
        btn.addEventListener('click', function () {
            // Get the photo type and title from data attributes
            const photoType = this.dataset.photoType;
            const title = this.dataset.title;

            // Update modal title
            modalTitle.textContent = title;

            // Store current photo type and related elements
            currentPhotoType = photoType;
            currentFileInput = document.getElementById(`${photoType}PhotoFile`);
            currentContainer = document.querySelector(`#${photoType}PhotoContainer .frames`);

            // Reset compression info
            compressionInfoEl.textContent = 'Image will be automatically compressed to under 100KB';
            compressionInfoEl.classList.remove('warning');

            // Show camera selection buttons only on mobile devices
            if (isMobileDevice()) {
                cameraSelectionDiv.style.display = 'block';
                // Set active button based on current facing mode
                if (currentFacingMode === 'user') {
                    frontCameraBtnEl.classList.add('active');
                    backCameraBtnEl.classList.remove('active');
                } else {
                    backCameraBtnEl.classList.add('active');
                    frontCameraBtnEl.classList.remove('active');
                }
            } else {
                cameraSelectionDiv.style.display = 'none';
            }

            // Show modal and start webcam
            modal.show();
            startWebcam(currentFacingMode);
        });
    });

    // Front camera button click handler
    frontCameraBtnEl.addEventListener('click', function () {
        if (currentStream) {
            stopWebcam();
        }
        currentFacingMode = 'user';
        frontCameraBtnEl.classList.add('active');
        backCameraBtnEl.classList.remove('active');
        startWebcam(currentFacingMode);
    });

    // Back camera button click handler
    backCameraBtnEl.addEventListener('click', function () {
        if (currentStream) {
            stopWebcam();
        }
        currentFacingMode = 'environment';
        backCameraBtnEl.classList.add('active');
        frontCameraBtnEl.classList.remove('active');
        startWebcam(currentFacingMode);
    });

    // Start webcam with specified facing mode
    function startWebcam(facingMode) {
        // Check if browser supports getUserMedia
        if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
            // Request camera access with specified facing mode
            navigator.mediaDevices.getUserMedia({
                video: {
                    facingMode: facingMode,
                    width: { ideal: 1280 },
                    height: { ideal: 720 }
                }
            })
                .then(function (mediaStream) {
                    // Store the stream for later cleanup
                    currentStream = mediaStream;

                    // Set video source to the stream
                    webcamEl.srcObject = mediaStream;
                    webcamEl.play();

                    // Reset UI state
                    showCaptureUI();
                })
                .catch(function (error) {
                    console.error('Camera access error:', error);

                    // More detailed error message based on the error
                    let errorMessage = 'Could not access camera. Please ensure you have granted camera permissions or use the file upload option instead.';

                    if (error.name === 'NotAllowedError' || error.name === 'PermissionDeniedError') {
                        errorMessage = 'Camera access denied. Please allow camera permissions in your browser settings or use the file upload option instead.';
                    } else if (error.name === 'NotFoundError' || error.name === 'DevicesNotFoundError') {
                        errorMessage = 'No camera found on your device. Please use the file upload option instead.';
                    } else if (error.name === 'NotReadableError' || error.name === 'TrackStartError') {
                        errorMessage = 'Your camera is being used by another application. Please close other applications using your camera or use the file upload option instead.';
                    }

                    alert(errorMessage);
                    modal.hide();
                });
        } else {
            alert('Your browser does not support webcam access. Please use the file upload option instead.');
            modal.hide();
        }
    }

    // Function to stop the webcam
    function stopWebcam() {
        if (currentStream) {
            currentStream.getTracks().forEach(track => {
                track.stop();
            });
            currentStream = null;
        }
    }

    // Show UI for capturing a photo
    function showCaptureUI() {
        webcamEl.style.display = 'block';
        capturedImageEl.style.display = 'none';
        captureBtnEl.style.display = 'block';
        recaptureBtnEl.style.display = 'none';
        saveBtnEl.style.display = 'none';
    }

    // Show UI for reviewing captured photo
    function showReviewUI() {
        webcamEl.style.display = 'none';
        capturedImageEl.style.display = 'block';
        captureBtnEl.style.display = 'none';
        recaptureBtnEl.style.display = 'block';
        saveBtnEl.style.display = 'block';
    }

    // Function to check if file type is allowed
    function isFileTypeAllowed(file) {
        const allowedTypes = ['image/jpeg', 'image/png'];
        return allowedTypes.includes(file.type);
    }

    // Helper function to compress an image to a target size (< 100KB)
    function compressImage(sourceImageDataUrl, maxSizeKB = 100) {
        return new Promise((resolve, reject) => {
            const img = new Image();
            img.onload = function () {
                // Create a canvas for the compression
                const canvas = document.createElement('canvas');
                let ctx = canvas.getContext('2d');

                // Start with these values and adjust as needed
                let quality = 0.9;    // Initial JPEG quality (0-1)
                let width = img.width;
                let height = img.height;
                let dataUrl;
                let blob;

                // If the image is very large, start by resizing it
                if (width > 1600 || height > 1600) {
                    const scaleFactor = 1600 / Math.max(width, height);
                    width = Math.floor(width * scaleFactor);
                    height = Math.floor(height * scaleFactor);
                }

                canvas.width = width;
                canvas.height = height;

                // Function to check file size
                const checkSize = async (dataUrl) => {
                    return new Promise((resolve) => {
                        const binaryString = atob(dataUrl.split(',')[1]);
                        const bytes = new Uint8Array(binaryString.length);
                        for (let i = 0; i < binaryString.length; i++) {
                            bytes[i] = binaryString.charCodeAt(i);
                        }
                        const blob = new Blob([bytes], { type: 'image/jpeg' });
                        resolve({
                            blob: blob,
                            size: blob.size,
                            sizeKB: blob.size / 1024
                        });
                    });
                };

                // Recursive function to compress until target size is reached
                const compressUntilTargetSize = async () => {
                    // Clear canvas and draw image
                    ctx.clearRect(0, 0, canvas.width, canvas.height);
                    ctx.drawImage(img, 0, 0, width, height);

                    // Get data URL with current quality
                    dataUrl = canvas.toDataURL('image/jpeg', quality);

                    // Check current size
                    const result = await checkSize(dataUrl);
                    blob = result.blob;

                    // If size is too large, reduce quality or dimensions
                    if (result.sizeKB > maxSizeKB && (quality > 0.3 || width > 400)) {
                        // Decide whether to reduce quality or dimensions
                        if (quality > 0.3) {
                            // Reduce quality first
                            quality -= 0.1;
                            if (quality < 0.3) quality = 0.3; // Don't go below 0.3
                        } else {
                            // If quality is already at minimum, resize dimensions
                            width = Math.floor(width * 0.9);
                            height = Math.floor(height * 0.9);
                            canvas.width = width;
                            canvas.height = height;
                        }

                        // Try again with new settings
                        return compressUntilTargetSize();
                    } else {
                        // Target size reached or minimum quality/size reached
                        return {
                            blob: blob,
                            dataUrl: dataUrl,
                            size: result.size,
                            sizeKB: result.sizeKB.toFixed(2),
                            width: width,
                            height: height,
                            quality: quality.toFixed(2)
                        };
                    }
                };

                // Start compression process
                compressUntilTargetSize()
                    .then(result => resolve(result))
                    .catch(err => reject(err));
            };

            img.onerror = function () {
                reject(new Error('Failed to load image'));
            };

            img.src = sourceImageDataUrl;
        });
    }

    // Capture photo from webcam
    captureBtnEl.addEventListener('click', function () {
        // Set canvas dimensions to match video
        canvasEl.width = webcamEl.videoWidth;
        canvasEl.height = webcamEl.videoHeight;

        // Draw video frame to canvas
        const context = canvasEl.getContext('2d');
        context.drawImage(webcamEl, 0, 0, canvasEl.width, canvasEl.height);

        // Get uncompressed image data
        const rawImageDataUrl = canvasEl.toDataURL('image/jpeg', 1.0);

        // Compress the image
        compressImage(rawImageDataUrl, 95) // Target 95KB to leave some margin
            .then(result => {
                // Update the image preview with compressed image
                capturedImageEl.src = result.dataUrl;
                capturedImageBlob = result.blob;
                capturedImageSize = result.sizeKB;

                // Update the compression info text
                compressionInfoEl.textContent = `Image compressed to ${result.sizeKB}KB (${result.width}x${result.height}, quality: ${result.quality})`;
                if (result.sizeKB > 100) {
                    compressionInfoEl.classList.add('warning');
                    compressionInfoEl.textContent += ' - Warning: Could not compress below 100KB while maintaining acceptable quality';
                } else {
                    compressionInfoEl.classList.remove('warning');
                }

                // Update UI
                showReviewUI();
            })
            .catch(error => {
                console.error('Error compressing image:', error);
                alert('Failed to process the photo. Please try again.');
            });
    });

    // Recapture photo
    recaptureBtnEl.addEventListener('click', function () {
        showCaptureUI();
    });

    // Save captured photo
    saveBtnEl.addEventListener('click', function () {
        if (!capturedImageBlob) {
            alert('No image captured. Please try again.');
            return;
        }

        // Create a file from the blob
        const file = new File([capturedImageBlob], `${currentPhotoType}-photo.jpg`, { type: 'image/jpeg' });

        // Create a DataTransfer object to simulate file selection
        const dataTransfer = new DataTransfer();
        dataTransfer.items.add(file);

        // Assign to file input
        currentFileInput.files = dataTransfer.files;

        // Trigger change event to update preview
        const event = new Event('change', { bubbles: true });
        currentFileInput.dispatchEvent(event);

        // Close modal and stop webcam
        modal.hide();
        stopWebcam();
    });

    // Setup file input change handlers to update preview and compress uploaded files
    document.querySelectorAll('.image-sign').forEach(input => {
        input.addEventListener('change', function (e) {
            if (this.files && this.files[0]) {
                const file = this.files[0];
                const photoType = this.id.replace('PhotoFile', '');
                const container = document.querySelector(`#${photoType}PhotoContainer .frames`);
                const warningElement = this.closest('.profile-upload').querySelector('.file-size-warning');

                // Check file type
                if (!isFileTypeAllowed(file)) {
                    warningElement.style.display = 'block';
                    warningElement.textContent = 'Only JPG and PNG files are allowed.';
                    // Clear the invalid file
                    this.value = '';
                    return;
                }

                // Check if file size exceeds 100KB
                if (file.size > 100 * 1024) {
                    // Show the size warning message
                    warningElement.style.display = 'block';
                    warningElement.textContent = `File size (${(file.size / 1024).toFixed(2)}KB) exceeds 100KB limit. Your file will be compressed automatically.`;

                    // File needs compression
                    const reader = new FileReader();
                    reader.onload = function (e) {
                        // Compress the uploaded image
                        compressImage(e.target.result, 95) // Target 95KB to leave some margin
                            .then(result => {
                                // Create a new file from the compressed blob
                                const compressedFile = new File([result.blob], file.name, {
                                    type: 'image/jpeg',
                                    lastModified: new Date().getTime()
                                });

                                // Replace the file in the input
                                const dataTransfer = new DataTransfer();
                                dataTransfer.items.add(compressedFile);
                                input.files = dataTransfer.files;

                                // Update preview
                                updatePreview(container, result.dataUrl);

                                // Update the warning message with compression results
                                warningElement.textContent = `File compressed from ${(file.size / 1024).toFixed(2)}KB to ${result.sizeKB}KB`;

                                // Hide the warning after 5 seconds
                                setTimeout(() => {
                                    warningElement.style.display = 'none';
                                }, 5000);
                            })
                            .catch(error => {
                                console.error('Error compressing uploaded image:', error);
                                // Still update preview with original file
                                updatePreview(container, e.target.result);
                                warningElement.textContent = 'Could not compress the image below 100KB. The original image will be used, but it may cause upload issues.';
                            });
                    };
                    reader.readAsDataURL(file);
                } else {
                    // Hide warning if it was previously shown
                    warningElement.style.display = 'none';

                    // File is already small enough, just update preview
                    const reader = new FileReader();
                    reader.onload = function (e) {
                        updatePreview(container, e.target.result);
                    };
                    reader.readAsDataURL(file);
                }
            }
        });
    });

    // Function to update image preview
    function updatePreview(container, imageDataUrl) {
        container.innerHTML = '';
        const imgPreview = document.createElement('img');
        imgPreview.src = imageDataUrl;
        imgPreview.className = 'img-thumbnail';
        imgPreview.style.maxHeight = '100%';
        imgPreview.style.maxWidth = '100%';
        imgPreview.style.objectFit = 'cover';
        container.appendChild(imgPreview);
    }

    // Clean up when modal is closed
    webcamModal.addEventListener('hidden.bs.modal', function () {
        stopWebcam();
    });

    // Handle mobile orientation changes
    window.addEventListener('orientationchange', function () {
        if (currentStream) {
            // Stop and restart webcam on orientation change to adjust the camera
            setTimeout(function () {
                stopWebcam();
                startWebcam(currentFacingMode);
            }, 200);
        }
    });
});
/**
 * PDF Document Upload Handler
 * Features:
 * - Restricts uploads to PDF files only
 * - Maximum file size validation (4MB)
 * - PDF preview functionality 
 * - Document removal tracking
 * - Auto-display of existing documents
 */
document.addEventListener('DOMContentLoaded', function () {
    // 1. Setup document upload fields
    setupDocumentUploadFields();

    // 2. Create hidden fields for document removal tracking
    createRemovalTrackingFields();

    // 3. Initialize existing document previews (for edit forms)
    initializeExistingDocuments();

    // 4. Show document section if there are existing documents
    checkAndShowDocumentSection();

    // 5. Setup document section toggle
    setupDocumentSectionToggle();
});

/**
 * Setup all document upload fields with the proper configurations
 */
function setupDocumentUploadFields() {
    const documentInputs = document.querySelectorAll('input[name="documentFiles"]');

    documentInputs.forEach((input, index) => {
        // Set accept attribute to PDF only
        input.setAttribute('accept', 'application/pdf');

        // Create a preview container for each upload
        const uploadContainer = input.closest('.d-flex');
        if (uploadContainer) {
            const previewContainer = document.createElement('div');
            previewContainer.className = 'pdf-preview-container mt-2 w-100';
            previewContainer.style.display = 'none';
            previewContainer.dataset.index = index;

            // Add it after the upload container
            uploadContainer.parentNode.insertBefore(previewContainer, uploadContainer.nextSibling);

            // Add change event handler
            input.addEventListener('change', function () {
                handleDocumentUpload(input, previewContainer, index);
            });
        }
    });
}

/**
 * Create hidden fields to track document removals
 */
function createRemovalTrackingFields() {
    const form = document.getElementById('studentForm') || document.querySelector('form');

    if (!form) {
        console.error('Form not found for adding document removal fields');
        return;
    }

    // Create fields for tracking up to 4 document removals
    for (let i = 1; i <= 4; i++) {
        const fieldId = `removeDocument${i}`;

        // Check if field already exists
        if (!document.getElementById(fieldId)) {
            const hiddenField = document.createElement('input');
            hiddenField.type = 'hidden';
            hiddenField.id = fieldId;
            hiddenField.name = fieldId;
            hiddenField.value = 'false';
            form.appendChild(hiddenField);
            console.log(`Created document removal tracking field: ${fieldId}`);
        }
    }
}

/**
 * Initialize previews for existing documents (used when editing)
 */
function initializeExistingDocuments() {
    const documentPaths = [
        {
            path: document.getElementById('Other_UpldPath1')?.value,
            title: document.getElementById('Other_UploadTitle1')?.value,
            index: 0
        },
        {
            path: document.getElementById('Other_UpldPath2')?.value,
            title: document.getElementById('Other_UploadTitle2')?.value,
            index: 1
        },
        {
            path: document.getElementById('Other_UpldPath3')?.value,
            title: document.getElementById('Other_UploadTitle3')?.value,
            index: 2
        },
        {
            path: document.getElementById('Other_UpldPath4')?.value,
            title: document.getElementById('Other_UploadTitle4')?.value,
            index: 3
        }
    ];

    // Display previews for documents that have paths
    documentPaths.forEach(doc => {
        if (doc.path) {
            const documentInputs = document.querySelectorAll('input[name="documentFiles"]');
            if (doc.index < documentInputs.length) {
                // Find the preview container
                const uploadContainer = documentInputs[doc.index].closest('.d-flex');
                if (uploadContainer) {
                    let previewContainer = uploadContainer.nextElementSibling;

                    if (previewContainer && previewContainer.classList.contains('pdf-preview-container')) {
                        // Extract filename from path
                        const pathParts = doc.path.split('/');
                        const fileName = pathParts[pathParts.length - 1];

                        // Create preview
                        createExistingDocumentPreview(fileName, doc.path, doc.title, previewContainer, doc.index);
                    }
                }
            }
        }
    });
}

/**
 * Handle document upload event
 */
function handleDocumentUpload(input, previewContainer, index) {
    if (input.files && input.files[0]) {
        const file = input.files[0];

        // Validate file type
        if (file.type !== 'application/pdf') {
            showError('Only PDF files are allowed for document uploads');
            clearUpload(input, previewContainer);
            return;
        }

        // Validate file size (4MB limit)
        const maxSize = 4 * 1024 * 1024; // 4MB in bytes
        if (file.size > maxSize) {
            showError('PDF file size must not exceed 4MB');
            clearUpload(input, previewContainer);
            return;
        }

        // Create preview for the valid file
        createDocumentPreview(file, previewContainer, index, input);

        // Auto-populate title field if empty
        populateDocumentTitle(file, index);
    }
}

/**
 * Create preview for a newly uploaded document
 */
function createDocumentPreview(file, container, index, fileInput) {
    container.innerHTML = '';
    container.style.display = 'block';

    // Create preview card
    const card = document.createElement('div');
    card.className = 'border rounded p-2 d-flex align-items-center';

    // PDF icon
    const icon = document.createElement('div');
    icon.innerHTML = '<i class="ti ti-file-text text-danger fs-24 me-2"></i>';

    // File info
    const info = document.createElement('div');
    info.className = 'flex-grow-1';

    const fileName = document.createElement('div');
    fileName.className = 'fw-medium';
    fileName.textContent = file.name;

    const fileSize = document.createElement('div');
    fileSize.className = 'fs-12 text-muted';
    fileSize.textContent = formatFileSize(file.size);

    info.appendChild(fileName);
    info.appendChild(fileSize);

    // Preview button
    const previewBtn = document.createElement('button');
    previewBtn.type = 'button';
    previewBtn.className = 'btn btn-sm btn-outline-primary ms-2';
    previewBtn.innerHTML = '<i class="ti ti-eye me-1"></i>Preview';
    previewBtn.addEventListener('click', function () {
        openDocumentPreview(file);
    });

    // Remove button
    const removeBtn = document.createElement('button');
    removeBtn.type = 'button';
    removeBtn.className = 'btn btn-sm btn-outline-danger ms-2';
    removeBtn.innerHTML = '<i class="ti ti-trash me-1"></i>Remove';
    removeBtn.addEventListener('click', function () {
        removeNewDocument(fileInput, container, index);
    });

    // Assemble the preview card
    card.appendChild(icon);
    card.appendChild(info);
    card.appendChild(previewBtn);
    card.appendChild(removeBtn);
    container.appendChild(card);
}

/**
 * Create preview for an existing document
 */
function createExistingDocumentPreview(fileName, filePath, fileTitle, container, index) {
    container.innerHTML = '';
    container.style.display = 'block';

    // Create preview card
    const card = document.createElement('div');
    card.className = 'border rounded p-2 d-flex align-items-center';

    // PDF icon
    const icon = document.createElement('div');
    icon.innerHTML = '<i class="ti ti-file-text text-danger fs-24 me-2"></i>';

    // File info
    const info = document.createElement('div');
    info.className = 'flex-grow-1';

    const fileNameEl = document.createElement('div');
    fileNameEl.className = 'fw-medium';
    fileNameEl.textContent = fileTitle || fileName;

    const fileTypeEl = document.createElement('div');
    fileTypeEl.className = 'fs-12 text-muted';
    fileTypeEl.textContent = 'PDF Document';

    info.appendChild(fileNameEl);
    info.appendChild(fileTypeEl);

    // Preview button
    const previewBtn = document.createElement('button');
    previewBtn.type = 'button';
    previewBtn.className = 'btn btn-sm btn-outline-primary ms-2';
    previewBtn.innerHTML = '<i class="ti ti-eye me-1"></i>Preview';
    previewBtn.addEventListener('click', function () {
        openExistingDocumentPreview(filePath);
    });

    // Remove button
    const removeBtn = document.createElement('button');
    removeBtn.type = 'button';
    removeBtn.className = 'btn btn-sm btn-outline-danger ms-2';
    removeBtn.innerHTML = '<i class="ti ti-trash me-1"></i>Remove';
    removeBtn.addEventListener('click', function () {
        removeExistingDocument(container, index);
    });

    // Assemble the preview card
    card.appendChild(icon);
    card.appendChild(info);
    card.appendChild(previewBtn);
    card.appendChild(removeBtn);
    container.appendChild(card);
}

/**
 * Remove a newly uploaded document
 */
function removeNewDocument(input, container, index) {
    // Clear the file input
    if (input) {
        input.value = '';
    }

    // Clear the title field
    const titleInputId = `Other_UploadTitle${index + 1}`;
    const titleInput = document.getElementById(titleInputId);
    if (titleInput) {
        titleInput.value = '';
    }

    // Hide and clear the container
    container.style.display = 'none';
    container.innerHTML = '';
}

/**
 * Remove an existing document
 */
function removeExistingDocument(container, index) {
    console.log(`Marking document ${index + 1} for removal`);

    // Set the removal marker field to true
    const removeFieldId = `removeDocument${index + 1}`;
    const removeField = document.getElementById(removeFieldId);

    if (removeField) {
        removeField.value = 'true';
        console.log(`Set ${removeFieldId} = true`);
    } else {
        console.error(`Could not find removal field: ${removeFieldId}`);
        // Try to create it if it doesn't exist
        createRemovalField(index + 1);
    }

    // Clear the path field
    const pathFieldId = `Other_UpldPath${index + 1}`;
    const pathField = document.getElementById(pathFieldId);
    if (pathField) {
        pathField.value = '';
        console.log(`Cleared ${pathFieldId}`);
    }

    // Clear the title field
    const titleFieldId = `Other_UploadTitle${index + 1}`;
    const titleField = document.getElementById(titleFieldId);
    if (titleField) {
        titleField.value = '';
        console.log(`Cleared ${titleFieldId}`);
    }

    // Hide and clear the preview
    container.style.display = 'none';
    container.innerHTML = '';

    // Show confirmation
    showSuccess('Document marked for deletion. It will be removed when you save the form.');
}

/**
 * Create a single removal tracking field
 */
function createRemovalField(index) {
    const form = document.getElementById('studentForm') || document.querySelector('form');

    if (form) {
        const fieldId = `removeDocument${index}`;
        const removeField = document.createElement('input');
        removeField.type = 'hidden';
        removeField.id = fieldId;
        removeField.name = fieldId;
        removeField.value = 'true'; // Set to true as we're creating this specifically for removal
        form.appendChild(removeField);
        console.log(`Created and set ${fieldId} = true`);
        return true;
    }

    return false;
}

/**
 * Open preview for a newly uploaded document
 */
function openDocumentPreview(file) {
    // Create a URL for the PDF file
    const fileUrl = URL.createObjectURL(file);

    // Get or create preview modal
    const modal = getOrCreatePreviewModal();

    // Set the preview iframe source
    const iframe = modal.querySelector('#pdfFrame');
    iframe.src = fileUrl;

    // Show the modal
    const bsModal = new bootstrap.Modal(modal);
    bsModal.show();

    // Clean up the URL when modal is closed
    modal.addEventListener('hidden.bs.modal', function () {
        URL.revokeObjectURL(fileUrl);
    }, { once: true }); // Use once to prevent memory leaks
}

/**
 * Open preview for an existing document
 */
function openExistingDocumentPreview(filePath) {
    // Get or create preview modal
    const modal = getOrCreatePreviewModal();

    // Set the preview iframe source
    const iframe = modal.querySelector('#pdfFrame');
    iframe.src = filePath;

    // Show the modal
    const bsModal = new bootstrap.Modal(modal);
    bsModal.show();
}

/**
 * Get or create the PDF preview modal
 */
function getOrCreatePreviewModal() {
    const modalId = 'pdfPreviewModal';
    let modal = document.getElementById(modalId);

    if (!modal) {
        modal = document.createElement('div');
        modal.className = 'modal fade';
        modal.id = modalId;
        modal.setAttribute('tabindex', '-1');
        modal.setAttribute('aria-hidden', 'true');

        const modalHTML = `
            <div class="modal-dialog modal-lg modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">PDF Preview</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body p-0">
                        <iframe id="pdfFrame" style="width: 100%; height: 500px; border: none;"></iframe>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                    </div>
                </div>
            </div>
        `;

        modal.innerHTML = modalHTML;
        document.body.appendChild(modal);
    }

    return modal;
}

/**
 * Populate the document title input based on the uploaded file
 */
function populateDocumentTitle(file, index) {
    const titleInputId = `Other_UploadTitle${index + 1}`;
    const titleInput = document.getElementById(titleInputId);

    if (titleInput && !titleInput.value) {
        // Extract name without extension
        let fileName = file.name.replace(/\.[^/.]+$/, "");

        // Clean up the filename and format it
        fileName = fileName.replace(/[_-]/g, " ")
            .replace(/\w\S*/g, function (txt) {
                return txt.charAt(0).toUpperCase() + txt.substr(1).toLowerCase();
            });

        titleInput.value = fileName;
    }
}

/**
 * Check if there are existing documents and show the section if needed
 */
function checkAndShowDocumentSection() {
    const hasExistingDocuments =
        document.getElementById('Other_UpldPath1')?.value ||
        document.getElementById('Other_UpldPath2')?.value ||
        document.getElementById('Other_UpldPath3')?.value ||
        document.getElementById('Other_UpldPath4')?.value;

    if (hasExistingDocuments) {
        const documentsSwitch = document.getElementById('documentsSwitch');
        const documentsFields = document.getElementById('documentsFields');

        if (documentsSwitch && documentsFields) {
            documentsSwitch.checked = true;
            documentsFields.style.display = 'block';
        }
    }
}

/**
 * Setup the document section toggle switch
 */
function setupDocumentSectionToggle() {
    const documentsSwitch = document.getElementById('documentsSwitch');
    const documentsFields = document.getElementById('documentsFields');

    if (documentsSwitch && documentsFields) {
        documentsSwitch.addEventListener('change', function () {
            documentsFields.style.display = this.checked ? 'block' : 'none';
        });
    }
}

/**
 * Clear an upload field and its preview
 */
function clearUpload(input, container) {
    input.value = '';
    container.style.display = 'none';
    container.innerHTML = '';
}

/**
 * Format file size for display
 */
function formatFileSize(bytes) {
    if (bytes < 1024) {
        return bytes + ' bytes';
    } else if (bytes < 1024 * 1024) {
        return (bytes / 1024).toFixed(2) + ' KB';
    } else {
        return (bytes / (1024 * 1024)).toFixed(2) + ' MB';
    }
}

/**
 * Show an error message
 */
function showError(message) {
    alert(message);
}

/**
 * Show a success message
 */
function showSuccess(message) {
    alert(message);
}