using FluentValidation;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ERPIndia
{
    public class FamilyDataValidator : AbstractValidator<FamilyData>
    {
        public FamilyDataValidator(FamilyData studentData)
        {
            RuleFor(student => student.AdmsnNo).NotNull().NotEmpty().Must(CheckIfNumberString).WithMessage("AdmsnNo Must be Number");
            RuleFor(student => student.SchoolCode).NotNull().NotEmpty().Must(CheckIfNumberString).WithMessage("SchoolCode Must be Number");
            // RuleFor(family => family.FPhone).Must(IsValidMobileNO).WithMessage("Invalid Number"); ;
            //RuleFor(family => family.MPhone).Must(IsValidMobileNO).WithMessage("Invalid Number"); ;
            //RuleFor(family => family.MPhoto).NotNull().NotEmpty();
            // RuleFor(family => family.FPhoto).NotNull().NotEmpty();
            // RuleFor(family => family.GPhoto).NotNull().NotEmpty();
            //RuleFor(family => family.RouteName).NotNull().NotEmpty();
            // RuleFor(family => family.HostelNo).NotNull().NotEmpty();

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
        private bool IsValidMobileNO(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;
            Regex regex = new Regex(@"^[0-9]{10}$");
            bool isValid = regex.IsMatch(value.Trim());
            if (!isValid) return false;
            else
                return true;
        }
    }
}