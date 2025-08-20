// Configuration settings for the Fee Collection System
const CONFIG = {
    // API endpoints
    API: {
        BASE_URL: 'https://api.example.com/v1', // Replace with your actual API base URL
        STUDENT_INFO: '/student/info',
        FEE_DETAILS: '/fee/details',
        FEE_SUMMARY: '/fee/summary',
        PAYMENT_METHODS: '/payment/methods',
        RECEIPT_TEMPLATES: '/receipt/templates',
        SUBMIT_PAYMENT: '/payment/submit'
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
    MAX_VISIBLE_ROWS: 4
};