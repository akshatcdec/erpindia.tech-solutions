using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERPIndia.Models.Exam
{
    public class ExamBulkUpdateViewModel
    {
        public Guid SessionID { get; set; }
        public int SessionYear { get; set; }
        public string SessionName { get; set; }
        public List<ExamUpdateItem> Exams { get; set; }

        public ExamBulkUpdateViewModel()
        {
            Exams = new List<ExamUpdateItem>();
        }
    }
}