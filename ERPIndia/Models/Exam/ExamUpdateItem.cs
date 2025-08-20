using System;
using System.ComponentModel.DataAnnotations;

namespace ERPIndia.Models.Exam
{
    public class ExamUpdateItem
    {
        public Guid ExamID { get; set; }
        public int SerialNumber { get; set; }

        [StringLength(20)]
        public string ExamMonth { get; set; }

        [StringLength(100)]
        public string ExamName { get; set; }

        [StringLength(20)]
        public string ExamType { get; set; }

        public int MaxMarks { get; set; }

        [StringLength(200)]
        public string Remarks { get; set; }

        public bool IsActive { get; set; }
        public bool Num { get; set; }              // Checkbox
        public bool MS { get; set; }               // Checkbox
        public string AdmitCard { get; set; }       // Text field
        public bool AC { get; set; }
    }
}