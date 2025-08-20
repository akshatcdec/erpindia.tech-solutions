using FluentValidation;
using System;
using System.Collections.Generic;

namespace ERPIndia
{
    public class StudentValidator : AbstractValidator<StudentData>
    {
        public StudentValidator(StudentData studentData)
        {
            var Category = new List<string>() { "GENERAL", "OBC", "SC", "ST", "MINORITY" };
            var BloodGroup = new List<string>() { "Blood Group", "A+", "A-", "B+", "B-", "O+", "O-" };
            RuleFor(student => student.AdmsnNo).NotNull().NotEmpty().Must(CheckIfNumberString).WithMessage("AdmsnNo Must be Number");
            RuleFor(student => student.SchoolCode).NotNull().NotEmpty().Must(CheckIfNumberString).WithMessage("SchoolCode Must be Number");
            RuleFor(student => student.StudentNo).NotNull().NotEmpty().Must(CheckIfNumberString).WithMessage("SrNo Must be Number");
            RuleFor(student => student.RollNo).NotNull().NotEmpty().Must(CheckIfNumberString).WithMessage("Roll No Must be Number");
            RuleFor(student => student.Class).NotNull().NotEmpty();
            RuleFor(student => student.Section).NotNull().NotEmpty().Must(CheckIfString).WithMessage("Section Must be Alphabates"); ;
            RuleFor(student => student.FirstName).NotNull().NotEmpty();
            RuleFor(student => student.Gender).NotNull().NotEmpty().Must(CheckGender).WithMessage("Please only use male or female");
            RuleFor(student => student.Dob).NotNull().NotEmpty().Must(isValidDate).WithMessage("Dob Must be in dd/mm/YYYY format");
            RuleFor(student => student.Category).Must(student => Category.Contains(student)).WithMessage("Please only use: " + String.Join(",", Category));
            RuleFor(student => student.Religion).NotNull().NotEmpty();
            RuleFor(student => student.Mobile).Must(CheckValidMobileNo).WithMessage("Mobile No Invalid");
            RuleFor(student => student.WhatsAppNum).Must(CheckValidMobileNo).WithMessage("WhatsAppNum No Invalid");
            RuleFor(student => student.AdmsnDate).NotNull().NotEmpty().Must(isValidDate).WithMessage("AdmsnDate Must be in dd/mm/YYYY format");
            RuleFor(student => student.Photo).NotNull().NotEmpty();
            RuleFor(student => student.BloodGroup).NotNull().NotEmpty().Must(student => BloodGroup.Contains(student)).WithMessage("Please only use: " + String.Join(",", BloodGroup));
            RuleFor(student => student.AsOnDt).NotNull().NotEmpty().Must(isValidDate).WithMessage("AsOnDt Must be in dd/mm/YYYY format");
            RuleFor(student => student.OldBalance).Must(CheckIfNumberString).WithMessage("Amount Must be Number");
            RuleFor(student => student.House).NotNull().NotEmpty();
            RuleFor(student => student.FeeCategory).NotNull().NotEmpty();
            RuleFor(student => student.DiscountCategory).NotNull().NotEmpty();
            RuleFor(student => student.Active).NotNull().NotEmpty().Must(CheckTrueFalse).WithMessage("Please only use true or false");
            RuleFor(student => student.EnquiryData).NotNull().NotEmpty().Must(CheckTrueFalse).WithMessage("Please only use true or false");
            RuleFor(student => student.SendSms).NotNull().NotEmpty().Must(CheckTrueFalse).WithMessage("Please only use true or false");
            RuleFor(student => student.UserId).NotNull().NotEmpty().Must(CheckIfNumberString).WithMessage("User Id Must be Number");
            RuleFor(student => student.EntryDate).NotNull().NotEmpty().Must(isValidDate).WithMessage("Entry Date Must be in dd/mm/YYYY format");
        }
        private bool CheckIfNumberString(string input)
        {
            foreach (var chr in input)
            {
                if (!Char.IsDigit(chr))
                {
                    return false;
                }
            }

            return true;
        }
        private bool CheckGender(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            if (input.ToLower() == "male" || input.ToLower() == "female")
                return true;
            else
                return false;
        }
        private bool CheckTrueFalse(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            if (input.ToLower() == "true" || input.ToLower() == "false")
                return true;
            else
                return false;
        }
        private bool CheckValidMobileNo(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return true;
            }
            foreach (char c in input)
            {
                if (c < '0' || c > '9')
                    return false;
            }
            if (input.Length == 10)
                return true;
            else
                return false;
        }
        private bool CheckIfString(string input)
        {
            foreach (var chr in input)
            {
                if (Char.IsNumber(chr))
                {
                    return false;
                }
            }

            return true;
        }
        private bool isValidDate(string value)
        {
            DateTime dateTime;
            bool isDateTime = false;

            // Check for empty string.
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }

            isDateTime = DateTime.TryParse(value, out dateTime);
            return isDateTime;

        }
    }
}