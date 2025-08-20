using ERPIndia.Models.Exam;
using ERPIndia.Repositories.Exam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class ExamController : BaseController
    {
        private readonly IExamRepository _examRepository;

        public ExamController()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            _examRepository = new ExamRepository(connectionString);
        }

        // GET: Exam
        public async Task<ActionResult> Index()
        {
            var sessionId = CurrentSessionID;
            var tenantId = CurrentTenantID;
            var exams = await _examRepository.GetExamsBySessionAsync(sessionId, tenantId);

            if (!exams.Any())
            {
                // Initialize default exams if none exist
                var tenantCode = Utils.ParseInt(CurrentTenantCode);
                var userId = CurrentTenantUserID;
                var sessionYear = DateTime.Now.Year;
                await _examRepository.InitializeExamsForSessionAsync(sessionId, tenantId, tenantCode, userId, sessionYear);
                exams = await _examRepository.GetExamsBySessionAsync(sessionId, tenantId);
            }

            var viewModel = new ExamBulkUpdateViewModel
            {
                SessionID = sessionId,
                SessionYear = exams.FirstOrDefault()?.SessionYear ?? DateTime.Now.Year,
                SessionName = GetSessionName(sessionId),
                Exams = exams.Select(e => new ExamUpdateItem
                {
                    ExamID = e.ExamID,
                    SerialNumber = e.SerialNumber,
                    ExamMonth = e.ExamMonth,
                    ExamName = e.ExamName,
                    ExamType = e.ExamType,
                    Remarks = e.Remarks,
                    IsActive = e.IsActive,
                    // Handle nullable booleans
                    Num = e.Num ?? false,
                    MS = e.MS ?? false,
                    AdmitCard = e.AdmitCard ?? string.Empty,
                    AC = e.AC ?? false
                }).OrderBy(e => e.SerialNumber).ToList()
            };

            return View(viewModel);
        }

        // POST: Exam/BulkUpdate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(ExamBulkUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var currentUserId = CurrentTenantUserID;
                var examsToUpdate = model.Exams.Select(e => new ExamMaster
                {
                    ExamID = e.ExamID,
                    ExamMonth = e.ExamMonth,
                    ExamName = e.ExamName,
                    ExamType = e.ExamType,
                    Remarks = e.Remarks,
                    IsActive = e.IsActive,
                    // Set new fields
                    Num = e.Num,
                    MS = e.MS,
                    AdmitCard = e.AdmitCard,
                    AC = e.AC,
                    ModifiedBy = currentUserId,
                    ModifiedDate = DateTime.Now
                }).ToList();

                var result = await _examRepository.BulkUpdateExamsAsync(examsToUpdate);

                if (result)
                {
                    TempData["Success"] = "All exams have been updated successfully.";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred while updating the exams.");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred: " + ex.Message);
            }

            return View(model);
        }

        #region Helper Methods
        private string GetSessionName(Guid sessionId)
        {
            // TODO: Implement to get session name from database
            return "Academic Session " + DateTime.Now.Year;
        }
        #endregion
    }
}