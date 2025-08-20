using System;
using System.Collections.Generic;
using System.Linq;

namespace ERPIndia.Fee
{
    public class FeeCollectRepository
    {
        // Static sample data
        private static List<StudentInfo> _students = new List<StudentInfo>
        {
            new StudentInfo
            {
                Id = 1,
                Name = "John Smith",
                Class = "10",
                Section = "A",
                AdmissionNumber = "ADM-2022-001",
                Balance = 0
            },
            new StudentInfo
            {
                Id = 2,
                Name = "Emily Johnson",
                Class = "9",
                Section = "B",
                AdmissionNumber = "ADM-2022-045",
                Balance = 1200
            },
            new StudentInfo
            {
                Id = 3,
                Name = "David Williams",
                Class = "11",
                Section = "C",
                AdmissionNumber = "ADM-2021-089",
                Balance = 500
            }
        };

        private static List<FeeData> _commonFeeData = new List<FeeData>
        {
            new FeeData
            {
                Id = 1,
                Name = "Tuition Fee",
                Amounts = new List<decimal> { 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                Fines = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                Discounts = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            },
            new FeeData
            {
                Id = 2,
                Name = "Computer Lab",
                Amounts = new List<decimal> { 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000 },
                Fines = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                Discounts = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            },
            new FeeData
            {
                Id = 3,
                Name = "Library Fee",
                Amounts = new List<decimal> { 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500 },
                Fines = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                Discounts = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            },
            new FeeData
            {
                Id = 4,
                Name = "Sports Fee",
                Amounts = new List<decimal> { 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800 },
                Fines = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                Discounts = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            },
            new FeeData
            {
                Id = 5,
                Name = "Transport",
                Amounts = new List<decimal> { 1500, 1500, 1500, 1500, 1500, 1500, 1500, 1500, 1500, 1500, 1500, 1500 },
                Fines = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                Discounts = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            }
        };

        // Sample fine data for student 2
        private static List<FeeData> _studentTwoFeeData = new List<FeeData>
        {
            new FeeData
            {
                Id = 1,
                Name = "Tuition Fee",
                Amounts = new List<decimal> { 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                Fines = new List<decimal> { 0, 0, 500, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                Discounts = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            },
            new FeeData
            {
                Id = 2,
                Name = "Computer Lab",
                Amounts = new List<decimal> { 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000 },
                Fines = new List<decimal> { 0, 0, 100, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                Discounts = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            },
            new FeeData
            {
                Id = 3,
                Name = "Library Fee",
                Amounts = new List<decimal> { 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500 },
                Fines = new List<decimal> { 0, 0, 50, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                Discounts = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            },
            new FeeData
            {
                Id = 4,
                Name = "Sports Fee",
                Amounts = new List<decimal> { 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800 },
                Fines = new List<decimal> { 0, 0, 80, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                Discounts = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            },
            new FeeData
            {
                Id = 5,
                Name = "Transport",
                Amounts = new List<decimal> { 1500, 1500, 1500, 1500, 1500, 1500, 1500, 1500, 1500, 1500, 1500, 1500 },
                Fines = new List<decimal> { 0, 0, 150, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                Discounts = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            }
        };

        // Sample discount data for student 3
        private static List<FeeData> _studentThreeFeeData = new List<FeeData>
        {
            new FeeData
            {
                Id = 1,
                Name = "Tuition Fee",
                Amounts = new List<decimal> { 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000 },
                Fines = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                Discounts = new List<decimal> { 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500 }
            },
            new FeeData
            {
                Id = 2,
                Name = "Computer Lab",
                Amounts = new List<decimal> { 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000, 1000 },
                Fines = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                Discounts = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            },
            new FeeData
            {
                Id = 3,
                Name = "Library Fee",
                Amounts = new List<decimal> { 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500, 500 },
                Fines = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                Discounts = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            },
            new FeeData
            {
                Id = 4,
                Name = "Sports Fee",
                Amounts = new List<decimal> { 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800 },
                Fines = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                Discounts = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            },
            new FeeData
            {
                Id = 5,
                Name = "Transport",
                Amounts = new List<decimal> { 1500, 1500, 1500, 1500, 1500, 1500, 1500, 1500, 1500, 1500, 1500, 1500 },
                Fines = new List<decimal> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                Discounts = new List<decimal> { 750, 750, 750, 750, 750, 750, 750, 750, 750, 750, 750, 750 }
            }
        };

        private static List<LedgerEntry> _studentLedgerEntries = new List<LedgerEntry>
        {
            new LedgerEntry
            {
                Date = "01-04-2023",
                ReceiptNumber = "Receipt No 20230401001",
                Description = "Fee Payment - April 2023",
                Debit = 8800,
                Credit = 8800,
                Balance = 0
            },
            new LedgerEntry
            {
                Date = "01-05-2023",
                ReceiptNumber = "Receipt No 20230501002",
                Description = "Fee Payment - May 2023",
                Debit = 8800,
                Credit = 8800,
                Balance = 0
            }
        };

        private static List<ReceiptDetailsResponse> _receipts = new List<ReceiptDetailsResponse>
        {
            new ReceiptDetailsResponse
            {
                ReceiptNumber = "Receipt No 20230401001",
                Date = new DateTime(2023, 4, 1),
                StudentName = "John Smith",
                Class = "10",
                Section = "A",
                AdmissionNumber = "ADM-2022-001",
                Fees = new List<FeePaymentItem>
                {
                    new FeePaymentItem
                    {
                        Id = 1,
                        Month = "April",
                        Name = "Tuition Fee",
                        Amount = 5000,
                        Fine = 0,
                        Discount = 0,
                        NetAmount = 5000
                    },
                    new FeePaymentItem
                    {
                        Id = 2,
                        Month = "April",
                        Name = "Computer Lab",
                        Amount = 1000,
                        Fine = 0,
                        Discount = 0,
                        NetAmount = 1000
                    },
                    new FeePaymentItem
                    {
                        Id = 3,
                        Month = "April",
                        Name = "Library Fee",
                        Amount = 500,
                        Fine = 0,
                        Discount = 0,
                        NetAmount = 500
                    },
                    new FeePaymentItem
                    {
                        Id = 4,
                        Month = "April",
                        Name = "Sports Fee",
                        Amount = 800,
                        Fine = 0,
                        Discount = 0,
                        NetAmount = 800
                    },
                    new FeePaymentItem
                    {
                        Id = 5,
                        Month = "April",
                        Name = "Transport",
                        Amount = 1500,
                        Fine = 0,
                        Discount = 0,
                        NetAmount = 1500
                    }
                },
                Total = 8800,
                Concession = 0,
                LateFee = 0,
                FinalAmount = 8800,
                PaymentMethod = "cash",
                Note = "Fee payment for April 2023"
            }
        };

        // Get student information with fee data
        public FeeCollectData GetStudentFeeData(int studentId)
        {
            var student = _students.FirstOrDefault(s => s.Id == studentId);
            if (student == null)
            {
                return null;
            }

            // Different fee data for each student to show various scenarios
            List<FeeData> feeData;
            if (studentId == 2)
            {
                feeData = _studentTwoFeeData;
            }
            else if (studentId == 3)
            {
                feeData = _studentThreeFeeData;
            }
            else
            {
                feeData = _commonFeeData;
            }

            return new FeeCollectData
            {
                StudentInfo = student,
                FeeData = feeData
            };
        }

        // Process payment
        public FeePaymentResponse ProcessPayment(FeePaymentRequest request)
        {
            var student = _students.FirstOrDefault(s => s.Id == request.StudentId);
            if (student == null)
            {
                return new FeePaymentResponse
                {
                    Success = false,
                    Message = "Student not found"
                };
            }

            // In a real implementation, you would save the payment to a database
            // For this example, we'll just return a success response with the receipt number

            // Update student balance
            student.Balance = request.Remaining;

            return new FeePaymentResponse
            {
                Success = true,
                ReceiptNumber = request.ReceiptNumber,
                Message = "Payment processed successfully",
                NewBalance = student.Balance
            };
        }

        // Get receipt details
        public ReceiptDetailsResponse GetReceiptDetails(string receiptNumber)
        {
            return _receipts.FirstOrDefault(r => r.ReceiptNumber == receiptNumber);
        }

        // Get student ledger
        public LedgerResponse GetStudentLedger(int studentId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var student = _students.FirstOrDefault(s => s.Id == studentId);
            if (student == null)
            {
                return null;
            }

            var entries = _studentLedgerEntries;

            // Filter by date if specified
            if (fromDate.HasValue)
            {
                entries = entries.Where(e => DateTime.Parse(e.Date) >= fromDate.Value).ToList();
            }

            if (toDate.HasValue)
            {
                entries = entries.Where(e => DateTime.Parse(e.Date) <= toDate.Value).ToList();
            }

            return new LedgerResponse
            {
                Student = student,
                Entries = entries,
                OpeningBalance = entries.Any() ? entries.First().Balance : 0,
                ClosingBalance = entries.Any() ? entries.Last().Balance : 0
            };
        }

        // Update student balance
        public bool UpdateStudentBalance(int studentId, decimal balance)
        {
            var student = _students.FirstOrDefault(s => s.Id == studentId);
            if (student == null)
            {
                return false;
            }

            student.Balance = balance;
            return true;
        }

        // Get all students (for demo purposes)
        public List<StudentInfo> GetAllStudents()
        {
            return _students;
        }
    }
}
public class FeeData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<decimal> Amounts { get; set; }
    public List<decimal> Fines { get; set; }
    public List<decimal> Discounts { get; set; }
}

public class StudentInfo
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Class { get; set; }
    public string Section { get; set; }
    public string AdmissionNumber { get; set; }
    public decimal Balance { get; set; }
}

public class FeeCollectData
{
    public List<FeeData> FeeData { get; set; }
    public StudentInfo StudentInfo { get; set; }
}

// Request/Response models for API

public class FeePaymentItem
{
    public int Id { get; set; }
    public string Month { get; set; }
    public string Name { get; set; }
    public decimal Amount { get; set; }
    public decimal Fine { get; set; }
    public decimal Discount { get; set; }
    public decimal NetAmount { get; set; }
}

public class FeePaymentRequest
{
    public List<FeePaymentItem> Fees { get; set; }
    public decimal Balance { get; set; }
    public decimal Total { get; set; }
    public decimal FinalTotal { get; set; }
    public decimal Concession { get; set; }
    public decimal LateFee { get; set; }
    public decimal Received { get; set; }
    public decimal Remaining { get; set; }
    public string Note { get; set; }
    public string PaymentMethod { get; set; }
    public string ReceiptNumber { get; set; }
    public string Date { get; set; }
    public int StudentId { get; set; }
}

public class FeePaymentResponse
{
    public bool Success { get; set; }
    public string ReceiptNumber { get; set; }
    public string Message { get; set; }
    public decimal NewBalance { get; set; }
}

public class StudentFeeDataRequest
{
    public int StudentId { get; set; }
}

public class ReceiptDetailsRequest
{
    public string ReceiptNumber { get; set; }
}

public class ReceiptDetailsResponse
{
    public string ReceiptNumber { get; set; }
    public DateTime Date { get; set; }
    public string StudentName { get; set; }
    public string Class { get; set; }
    public string Section { get; set; }
    public string AdmissionNumber { get; set; }
    public List<FeePaymentItem> Fees { get; set; }
    public decimal Total { get; set; }
    public decimal Concession { get; set; }
    public decimal LateFee { get; set; }
    public decimal FinalAmount { get; set; }
    public string PaymentMethod { get; set; }
    public string Note { get; set; }
}

public class LedgerRequest
{
    public int StudentId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class LedgerEntry
{
    public string Date { get; set; }
    public string ReceiptNumber { get; set; }
    public string Description { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance { get; set; }
}

public class LedgerResponse
{
    public StudentInfo Student { get; set; }
    public List<LedgerEntry> Entries { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
}
