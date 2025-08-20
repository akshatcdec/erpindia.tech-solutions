// API service for the Fee Collection System
const API = {
    callAPI: async function (endpoint, method = 'GET', data = null) {
        const url = CONFIG.API.BASE_URL + endpoint;
        const options = {
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            credentials: 'include' // Include cookies for authentication
        };

        if (data && (method === 'POST' || method === 'PUT')) {
            options.body = JSON.stringify(data);
        }

        try {
            const response = await fetch(url, options);

            if (!response.ok) {
                throw new Error(`API error: ${response.status} ${response.statusText}`);
            }

            const result = await response.json();

            if (!result.success) {
                throw new Error(result.message || 'Unknown API error');
            }

            return result.data;
        } catch (error) {
            console.error('API request failed:', error);
            throw error;
        }
    },

    // For demo purposes, we'll simulate API calls with Promise-based functions
    // In a production environment, these would make actual fetch/axios calls to your backend

    /**
     * Fetch student information
     * @param {string} studentId - The ID of the student
     * @returns {Promise<Object>} - Student information
     */
    getStudentInfo: async function (studentId) {
        // Endpoint to fetch student details from database
        const endpoint = `${CONFIG.API.STUDENT_INFO}?studentId=${studentId}`;

        try {
            const data = await this.callAPI(endpoint);

            // Format the response to match the expected structure
            return {
                id: data.AdmissionNo || studentId,
                name: data.Name || '',
                father: data.FatherName || '-',
                class: data.ClassName || '',
                section: data.SectionName || '',
                contact: data.ContactNumber || '-',
                discountCategory: data.DiscountCategory || ''
            };
        } catch (error) {
            console.error('Error fetching student info:', error);
            throw error;
        }
    },
    /**
     * Fetch fee details for a student
     * @param {string} studentId - The ID of the student
     * @returns {Promise<Array>} - Fee details for all months
     */
    getFeeDetails: async function (studentId) {
        // Real API endpoint for fee details
        const endpoint = `${CONFIG.API.FEE_DETAILS}?studentId=${studentId}`;

        try {
            // Call the actual API instead of using the setTimeout mock
            const data = await this.callAPI(endpoint);
            console.log("fee" + data);
            // The API should return data in this format already, but we can transform it if needed
            return data;

        } catch (error) {
            console.error('Error fetching fee details:', error);

            // Fallback to mock data in case of API failure (for development/testing)
            //return this.getMockFeeDetails();
        }
    },
    getFeeSummary: async function (studentId) {
        // Real API endpoint for fee summary
        const endpoint = `${CONFIG.API.FEE_SUMMARY}?studentId=${studentId}`;
        try {
            // Call the actual API
            const data = await this.callAPI(endpoint);
            console.log("feeSummary: ", data);
            // Return the data from API
            return data;
        } catch (error) {
            console.error('Error fetching fee summary:', error);
            // Fallback to mock data in case of API failure
            return {
                discount: 0,
                lateFee: 0,
                oldBalance: 0
            };
        }
    },
    /**
     * Get mock fee details (for development/testing)
     * @returns {Array} - Mock fee details
     */
    getMockFeeDetails: function () {
        return [
            {
                id: '1719',
                name: 'Exam fee',
                // Regular amounts before discount
                regularAmounts: {
                    Apr: 600, May: 0, Jun: 0, Jul: 0, Aug: 600,
                    Sep: 0, Oct: 0, Nov: 0, Dec: 600, Jan: 0, Feb: 0, Mar: 0
                },
                // Final amounts after discount
                months: {
                    Apr: 500, May: 0, Jun: 0, Jul: 0, Aug: 500,
                    Sep: 0, Oct: 0, Nov: 0, Dec: 500, Jan: 0, Feb: 0, Mar: 0
                },
                // Fixed discount amounts for each month
                discounts: {
                    Apr: 100, May: 0, Jun: 0, Jul: 0, Aug: 100,
                    Sep: 0, Oct: 0, Nov: 0, Dec: 100, Jan: 0, Feb: 0, Mar: 0
                }
            },
            {
                id: '1721',
                name: 'Admission Fee',
                // Regular amounts before discount
                regularAmounts: {
                    Apr: 1200, May: 0, Jun: 0, Jul: 0, Aug: 0,
                    Sep: 0, Oct: 0, Nov: 0, Dec: 0, Jan: 0, Feb: 0, Mar: 0
                },
                // Final amounts after discount
                months: {
                    Apr: 1000, May: 0, Jun: 0, Jul: 0, Aug: 0,
                    Sep: 0, Oct: 0, Nov: 0, Dec: 0, Jan: 0, Feb: 0, Mar: 0
                },
                // Fixed discount amounts for each month
                discounts: {
                    Apr: 200, May: 0, Jun: 0, Jul: 0, Aug: 0,
                    Sep: 0, Oct: 0, Nov: 0, Dec: 0, Jan: 0, Feb: 0, Mar: 0
                }
            },
            {
                id: '1722',
                name: 'Monthly Fee',
                // Regular amounts before discount
                regularAmounts: {
                    Apr: 900, May: 900, Jun: 900, Jul: 900, Aug: 900,
                    Sep: 900, Oct: 900, Nov: 900, Dec: 900, Jan: 900, Feb: 900, Mar: 900
                },
                // Final amounts after discount
                months: {
                    Apr: 750, May: 750, Jun: 750, Jul: 750, Aug: 750,
                    Sep: 750, Oct: 750, Nov: 750, Dec: 750, Jan: 750, Feb: 750, Mar: 750
                },
                // Fixed discount amounts for each month
                discounts: {
                    Apr: 150, May: 150, Jun: 150, Jul: 150, Aug: 150,
                    Sep: 150, Oct: 150, Nov: 150, Dec: 150, Jan: 150, Feb: 150, Mar: 150
                }
            },
            {
                id: '1723',
                name: 'Transport Fee',
                // Regular amounts before discount
                regularAmounts: {
                    Apr: 500, May: 500, Jun: 500, Jul: 500, Aug: 500,
                    Sep: 500, Oct: 500, Nov: 500, Dec: 500, Jan: 500, Feb: 500, Mar: 500
                },
                // Final amounts after discount
                months: {
                    Apr: 400, May: 400, Jun: 400, Jul: 400, Aug: 400,
                    Sep: 400, Oct: 400, Nov: 400, Dec: 400, Jan: 400, Feb: 400, Mar: 400
                },
                // Fixed discount amounts for each month
                discounts: {
                    Apr: 100, May: 100, Jun: 100, Jul: 100, Aug: 100,
                    Sep: 100, Oct: 100, Nov: 100, Dec: 100, Jan: 100, Feb: 100, Mar: 100
                }
            }
        ];
    },


    /**
     * Fetch available payment methods
     * @returns {Promise<Array>} - Available payment methods
     */
    getPaymentMethods: function() {
        // Simulate API call with a Promise
        return new Promise((resolve) => {
            setTimeout(() => {
                resolve([
                    { id: 'cash', name: 'Cash', default: true },
                    { id: 'bank', name: 'Bank', default: false },
                    { id: 'cheque', name: 'CHEQUE', default: false },
                    { id: 'upi', name: 'UPI', default: false },
                    { id: 'paytm', name: 'PAYTM', default: false }
                ]);
            }, 400);
        });
    },

    /**
     * Fetch receipt templates
     * @returns {Promise<Array>} - Available receipt templates
     */
    getReceiptTemplates: function() {
        // Simulate API call with a Promise
        return new Promise((resolve) => {
            setTimeout(() => {
                resolve([
                    { 
                        id: 'template1', 
                        content: 'Dear Mr.XYZ, Your fee amount Rs. A /- has been received on Dt: dd-mm-yyyy. Subtotal: Rs. B /-, Discount: Rs. C /-, Total after discount: Rs. D /-, Remaining: Rs. E /-',
                        default: true
                    },
                    { 
                        id: 'template2', 
                        content: 'प्रिय Mr. XYZ, आपकी मासिक शुल्क रुपए A /- दिनांक: dd-mm-yyyy को जमा हो गयी है। सबटोटल: रुपए B /-, छूट: रुपए C /-, कुल भुगतान: रुपए D /-, शेष राशि: रुपए E /-',
                        default: false
                    },
                    { 
                        id: 'template3', 
                        content: 'Dear Mr. XYZ Your dues INR A /- for the month of B has been submitted. Discount applied: INR C /-',
                        default: false
                    }
                ]);
            }, 300);
        });
    },

    /**
     * Submit payment
     * @param {Object} paymentData - Payment information
     * @returns {Promise<Object>} - Payment receipt information
     */
    submitPayment: function(paymentData) {
        // Simulate API call with a Promise
        return new Promise((resolve) => {
            setTimeout(() => {
                resolve({
                    success: true,
                    receiptNumber: 'RCPT-' + Date.now(),
                    date: new Date().toISOString(),
                    subtotalAmount: paymentData.subtotalAmount,
                    discountAmount: paymentData.discountAmount,
                    totalAmount: paymentData.totalAmount,
                    receivedAmount: paymentData.receivedAmount,
                    method: paymentData.paymentMethod,
                    remainingBalance: paymentData.remainAmount
                });
            }, 1000);
        });
    }
};