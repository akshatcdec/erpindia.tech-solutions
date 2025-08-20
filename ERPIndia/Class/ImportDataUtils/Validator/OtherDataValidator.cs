using FluentValidation;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ERPIndia
{
    public class OtherDataValidator : AbstractValidator<OtherData>
    {
        public OtherDataValidator(OtherData studentData)
        {
            RuleFor(student => student.AdmsnNo).NotNull().NotEmpty().Must(CheckIfNumberString).WithMessage("AdmsnNo Must be Number");
            RuleFor(student => student.SchoolCode).NotNull().NotEmpty().Must(CheckIfNumberString).WithMessage("SchoolCode Must be Number");
            RuleFor(other => other.UploadTitle1).NotNull().NotEmpty();
            RuleFor(other => other.UpldPath1).NotNull().NotEmpty();
            RuleFor(other => other.UpldPath2).NotNull().NotEmpty();
            RuleFor(other => other.UploadTitle3).NotNull().NotEmpty();
            RuleFor(other => other.UpldPath3).NotNull().NotEmpty();
            RuleFor(other => other.UploadTitle4).NotNull().NotEmpty();
            RuleFor(other => other.UpldPath4).NotNull().NotEmpty();
            RuleFor(other => other.UploadTitle5).NotNull().NotEmpty();
            RuleFor(other => other.UpldPath5).NotNull().NotEmpty();
            RuleFor(other => other.UploadTitle6).NotNull().NotEmpty();
            RuleFor(other => other.UpldPath6).NotNull().NotEmpty();

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
            if (string.IsNullOrWhiteSpace(value))
                return true;
            DateTime dt;
            Regex regex = new Regex(@"(((0|1)[0-9]|2[0-9]|3[0-1])\/(0[1-9]|1[0-2])\/((19|20)\d\d))$");
            bool isValid = regex.IsMatch(value.Trim());
            isValid = DateTime.TryParseExact(value, "dd/MM/yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out dt);
            if (!isValid) return false;
            else
                return true;
        }
    }
}