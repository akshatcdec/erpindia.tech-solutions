// Configuration settings for the Database-Driven Fee Collection System
const CONFIG = {
    // API endpoints
    API: {
        BASE_URL: window.location.origin, // Base URL for all API calls
        STUDENT_INFO: '/fee/GetStudentInfo',
        FEE_DETAILS: '/fee/GetFeeDetailsSP',
        FEE_SUMMARY: '/fee/GetFeeSummary',
        PAYMENT_METHODS: '/fee/GetPaymentMethods',
        RECEIPT_TEMPLATES: '/fee/GetReceiptTemplates',
        SUBMIT_PAYMENT: '/fee/SubmitPayment'
    },
    
    // Default values
    DEFAULTS: {
        DISCOUNT: 0,
        LATE_FEE: 0,
        OLD_BALANCE: 0
    },
    
    // Months configuration
    MONTHS: [
        { short: 'Apr', full: 'April' },
        { short: 'May', full: 'May' },
        { short: 'Jun', full: 'June' },
        { short: 'Jul', full: 'July' },
        { short: 'Aug', full: 'August' },
        { short: 'Sep', full: 'September' },
        { short: 'Oct', full: 'October' },
        { short: 'Nov', full: 'November' },
        { short: 'Dec', full: 'December' },
        { short: 'Jan', full: 'January' },
        { short: 'Feb', full: 'February' },
        { short: 'Mar', full: 'March' }
    ],
    
    // Maximum number of rows in the added fees table before scrolling
    MAX_VISIBLE_ROWS: 4,
    
    // Database schema mapping
    DB: {
        // Fee head types
        FEE_TYPES: {
            EXAM: 'Exam fee',
            ADMISSION: 'Admission Fee',
            MONTHLY: 'Monthly Fee',
            TRANSPORT: 'Transport Fee'
        },
        
        // Month column names in database
        MONTH_COLUMNS: {
            'Apr': 'April',
            'May': 'May',
            'Jun': 'June',
            'Jul': 'July',
            'Aug': 'August',
            'Sep': 'September',
            'Oct': 'October',
            'Nov': 'November',
            'Dec': 'December',
            'Jan': 'January',
            'Feb': 'February',
            'Mar': 'March'
        }
    }
};