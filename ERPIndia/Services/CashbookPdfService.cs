using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ERPK12Models;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.IO;
using System.Text;
namespace ERPIndia.Services
{
    
        public class CashbookPdfService
        {
            private readonly PdfFont _boldFont;
            private readonly PdfFont _regularFont;
            private readonly Color _headerColor = new DeviceRgb(230, 230, 230);
            private readonly Color _borderColor = new DeviceRgb(0, 0, 0);

            public CashbookPdfService()
            {
                _boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                _regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            }

            public byte[] GenerateCashbookPdf(CashbookViewModel model)
            {
                using (var stream = new MemoryStream())
                {
                    var writer = new PdfWriter(stream);
                    var pdf = new PdfDocument(writer);
                    var document = new Document(pdf);

                    // Set margins
                    document.SetMargins(20, 20, 20, 20);

                    // Add header
                    AddHeader(document, model);

                    // Add cashbook details
                    AddCashbookDetails(document, model);

                    // Add entries table
                    AddEntriesTable(document, model);

                    // Add summaries
                    AddSummaries(document, model);

                    document.Close();
                    return stream.ToArray();
                }
            }

            private void AddHeader(Document document, CashbookViewModel model)
            {
                // School name and details
                var headerTable = new Table(2).UseAllAvailableWidth();

                // Left side - School details
                var leftCell = new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.TOP);

                leftCell.Add(new Paragraph(model.SchoolName)
                    .SetFont(_boldFont)
                    .SetFontSize(16)
                    .SetTextAlignment(TextAlignment.LEFT));

                leftCell.Add(new Paragraph(model.SchoolAddress)
                    .SetFont(_regularFont)
                    .SetFontSize(10));

                leftCell.Add(new Paragraph(model.SchoolEmail)
                    .SetFont(_regularFont)
                    .SetFontSize(10));

                leftCell.Add(new Paragraph(model.SchoolPhone)
                    .SetFont(_regularFont)
                    .SetFontSize(10));

                // Right side - Page info
                var rightCell = new Cell()
                    .SetBorder(Border.NO_BORDER)
                    .SetVerticalAlignment(VerticalAlignment.TOP)
                    .SetTextAlignment(TextAlignment.RIGHT);

                rightCell.Add(new Paragraph($"Page {model.PageNumber} of {model.TotalPages}")
                    .SetFont(_regularFont)
                    .SetFontSize(10)
                    .SetBorder(new SolidBorder(1)));

                rightCell.Add(new Paragraph(model.GeneratedOn.ToString("dd-MMM-yyyy"))
                    .SetFont(_regularFont)
                    .SetFontSize(10)
                    .SetBorder(new SolidBorder(1)));

                rightCell.Add(new Paragraph(model.GeneratedOn.ToString("hh:mm:sstt"))
                    .SetFont(_regularFont)
                    .SetFontSize(10)
                    .SetBorder(new SolidBorder(1)));

                headerTable.AddCell(leftCell);
                headerTable.AddCell(rightCell);

                document.Add(headerTable);
                document.Add(new Paragraph("\n"));
            }

            private void AddCashbookDetails(Document document, CashbookViewModel model)
            {
                // CASHBOOK title
                document.Add(new Paragraph("CASHBOOK")
                    .SetFont(_boldFont)
                    .SetFontSize(14)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBackgroundColor(_headerColor)
                    .SetBorder(new SolidBorder(1))
                    .SetPadding(5));

                // Details table
                var detailsTable = new Table(4).UseAllAvailableWidth();

                // Session
                detailsTable.AddCell(CreateDetailCell("Session:", true));
                detailsTable.AddCell(CreateDetailCell($"( {model.Session} )", false));

                // Payment Mode
                detailsTable.AddCell(CreateDetailCell("Payment Mode:", true));
                detailsTable.AddCell(CreateDetailCell(model.PaymentMode, false));

                // From Date
                detailsTable.AddCell(CreateDetailCell("From:", true));
                detailsTable.AddCell(CreateDetailCell(model.FromDate.ToString("dd/MM/yyyy"), false));

                // Opening Balance
                detailsTable.AddCell(CreateDetailCell("Opening Bal:", true));
                detailsTable.AddCell(CreateDetailCell(model.OpeningBalance.ToString("N0"), false));

                // To Date
                detailsTable.AddCell(CreateDetailCell("To:", true));
                detailsTable.AddCell(CreateDetailCell(model.ToDate.ToString("dd/MM/yyyy"), false));

                // Empty cells for alignment
                detailsTable.AddCell(CreateDetailCell("", false));
                detailsTable.AddCell(CreateDetailCell("", false));

                document.Add(detailsTable);
                document.Add(new Paragraph("\n"));
            }

            private void AddEntriesTable(Document document, CashbookViewModel model)
            {
                // Create table with appropriate column widths
                float[] columnWidths = { 1f, 3f, 3f, 1f, 1f, 1.5f, 4f, 1.5f };
                var table = new Table(columnWidths).UseAllAvailableWidth();

                // Add headers
                string[] headers = { "Recp No.", "Student Name", "Father Name", "Class", "Section", "PayMode", "Date/Note/User", "Recv. Amt." };

                foreach (var header in headers)
                {
                    table.AddHeaderCell(new Cell()
                        .Add(new Paragraph(header).SetFont(_boldFont).SetFontSize(8))
                        .SetBackgroundColor(_headerColor)
                        .SetBorder(new SolidBorder(_borderColor, 1))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetPadding(3));
                }

                // Add data rows
                foreach (var entry in model.Entries)
                {
                    // Receipt No
                    table.AddCell(CreateDataCell(entry.ReceiptNo.ToString()));

                    // Student Name
                    table.AddCell(CreateDataCell(entry.StudentName));

                    // Father Name
                    table.AddCell(CreateDataCell(entry.FatherName));

                    // Class
                    table.AddCell(CreateDataCell(entry.Class));

                    // Section
                    table.AddCell(CreateDataCell(entry.Section));

                    // Payment Mode
                    table.AddCell(CreateDataCell(entry.PaymentMode));

                    // Date/Note/User
                    var dateNoteText = $"{entry.Date:dd/MM/yyyy}";
                    if (!string.IsNullOrEmpty(entry.Notes))
                    {
                        dateNoteText += $" - {entry.Notes}";
                    }
                    dateNoteText += $" - {entry.UserId}";
                    table.AddCell(CreateDataCell(dateNoteText));

                    // Received Amount
                    table.AddCell(CreateDataCell(entry.ReceivedAmount.ToString("N0"), TextAlignment.RIGHT));
                }

                document.Add(table);
            }

            private void AddSummaries(Document document, CashbookViewModel model)
            {
                document.Add(new Paragraph("\n"));

                // Payment Mode Summary
                var summaryTable = new Table(2).UseAllAvailableWidth();

                summaryTable.AddCell(new Cell()
                    .Add(new Paragraph("Payment Mode Summary With User-wise").SetFont(_boldFont))
                    .SetBackgroundColor(_headerColor)
                    .SetBorder(new SolidBorder(1))
                    .SetPadding(5));

                summaryTable.AddCell(new Cell()
                    .Add(new Paragraph("Class Wise Summary").SetFont(_boldFont))
                    .SetBackgroundColor(_headerColor)
                    .SetBorder(new SolidBorder(1))
                    .SetPadding(5));

                // Payment summary details
                var paymentDetails = new StringBuilder();
                paymentDetails.AppendLine($"Cash: {model.PaymentSummary.Cash:N0}");
                paymentDetails.AppendLine($"UPI: {model.PaymentSummary.UPI:N0}");
                paymentDetails.AppendLine($"Paytm: {model.PaymentSummary.Paytm:N0}");
                paymentDetails.AppendLine($"Bank: {model.PaymentSummary.Bank:N0}");
                paymentDetails.AppendLine($"Cheque: {model.PaymentSummary.Cheque:N0}");
                paymentDetails.AppendLine($"Other: {model.PaymentSummary.Other:N0}");
                paymentDetails.AppendLine($"Total Amount: {model.PaymentSummary.TotalAmount:N0}");

                summaryTable.AddCell(new Cell()
                    .Add(new Paragraph(paymentDetails.ToString()).SetFont(_regularFont).SetFontSize(8))
                    .SetBorder(new SolidBorder(1))
                    .SetPadding(5));

                // Class wise summary
                var classDetails = new StringBuilder();
                foreach (var classSummary in model.ClassSummaries.OrderBy(c => c.Class))
                {
                    classDetails.AppendLine($"{classSummary.Class}: {classSummary.Amount:N0}");
                }

                summaryTable.AddCell(new Cell()
                    .Add(new Paragraph(classDetails.ToString()).SetFont(_regularFont).SetFontSize(8))
                    .SetBorder(new SolidBorder(1))
                    .SetPadding(5));

                document.Add(summaryTable);

                // Total Amount
                document.Add(new Paragraph($"\nTotal Amount: {model.TotalAmount:N0}")
                    .SetFont(_boldFont)
                    .SetFontSize(12)
                    .SetTextAlignment(TextAlignment.RIGHT));
            }

            private Cell CreateDetailCell(string text, bool isBold)
            {
                return new Cell()
                    .Add(new Paragraph(text).SetFont(isBold ? _boldFont : _regularFont).SetFontSize(9))
                    .SetBorder(new SolidBorder(1))
                    .SetPadding(3);
            }

            private Cell CreateDataCell(string text, TextAlignment alignment = TextAlignment.LEFT)
            {
                return new Cell()
                    .Add(new Paragraph(text).SetFont(_regularFont).SetFontSize(7))
                    .SetBorder(new SolidBorder(_borderColor, 0.5f))
                    .SetTextAlignment(alignment)
                    .SetPadding(2);
            }
        }
    }
