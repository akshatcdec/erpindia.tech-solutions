using ERPIndia.Models.Exam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ERPIndia.Repositories.Exam
{
    public interface IExamRepository
    {
        Task<IEnumerable<ExamMaster>> GetExamsBySessionAsync(Guid sessionId, Guid tenantId);
        Task<bool> InitializeExamsForSessionAsync(Guid sessionId, Guid tenantId, int tenantCode, Guid userId, int sessionYear);
        Task<bool> BulkUpdateExamsAsync(IEnumerable<ExamMaster> exams);
    }
}