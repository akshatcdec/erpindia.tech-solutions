using Dapper;
using ERPIndia.Class.Helper;
using ERPK12Models.DTO.Acc;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    
    public class NoticeController : BaseController
    {
        private readonly string _connectionString;

        public NoticeController()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        }

        // GET: Notice
        public ActionResult Index(string searchTerm, string filterType, int? page)
        {
            int pageSize = 10;
            int pageNumber = page ?? 1;

            using (var connection = new SqlConnection(_connectionString))
            {
                // Build the base query with search and filter conditions
                var whereConditions = new List<string> { "1=1" };
                var parameters = new DynamicParameters();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    whereConditions.Add("(Title LIKE @SearchTerm OR Description LIKE @SearchTerm)");
                    parameters.Add("SearchTerm", $"%{searchTerm}%");
                }

                if (!string.IsNullOrEmpty(filterType) && filterType != "All")
                {
                    whereConditions.Add("NoticeType = @FilterType");
                    parameters.Add("FilterType", filterType);
                }

                var whereClause = string.Join(" AND ", whereConditions);

                // Get total count
                var countQuery = $"SELECT COUNT(*) FROM Notices WHERE {whereClause}";
                var totalCount = connection.QuerySingle<int>(countQuery, parameters);

                // Get paginated results
                var query = $@"
                SELECT NoticeId, Title, Description, NoticeType, IconClass, 
                       PublishedBy, PublishedDate, ExpiryDate, IsActive
                FROM Notices
                WHERE {whereClause}
                ORDER BY PublishedDate DESC
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

                parameters.Add("Offset", (pageNumber - 1) * pageSize);
                parameters.Add("PageSize", pageSize);

                var notices = connection.Query<NoticeViewModel>(query, parameters).ToList();

                var viewModel = new NoticeListViewModel
                {
                    Notices = notices,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    FilterType = filterType
                };

                if (Request.IsAjaxRequest())
                {
                    return PartialView("_NoticeList", viewModel);
                }

                return View(viewModel);
            }
        }

        // GET: Notice/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                SELECT NoticeId, Title, Description, NoticeType, IconClass, 
                       PublishedBy, PublishedDate, ExpiryDate, IsActive
                FROM Notices
                WHERE NoticeId = @NoticeId";

                var notice = connection.QueryFirstOrDefault<NoticeViewModel>(query, new { NoticeId = id });

                if (notice == null)
                {
                    return HttpNotFound();
                }

                return View(notice);
            }
        }

        // GET: Notice/Create
        public ActionResult Create()
        {
            var model = new NoticeViewModel
            {
                PublishedDate = DateTime.Now,
                IsActive = true
            };

            ViewBag.NoticeTypes = GetNoticeTypes();
            return View(model);
        }

        // POST: Notice/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(NoticeViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var query = @"
                    INSERT INTO Notices (Title, Description, NoticeType, IconClass, 
                                       PublishedBy, PublishedDate, ExpiryDate, IsActive)
                    VALUES (@Title, @Description, @NoticeType, @IconClass, 
                            @PublishedBy, @PublishedDate, @ExpiryDate, @IsActive)";

                    var parameters = new
                    {
                        Title = model.Title,
                        Description = model.Description,
                        NoticeType = model.NoticeType,
                        IconClass = GetIconClassForType(model.NoticeType),
                        PublishedBy = CommonLogic.GetSessionValue(StringConstants.UserId),
                        PublishedDate = DateTime.Now,
                        ExpiryDate = model.ExpiryDate,
                        IsActive = model.IsActive
                    };

                    connection.Execute(query, parameters);

                    TempData["Success"] = "Notice created successfully!";
                    return RedirectToAction("Index");
                }
            }

            ViewBag.NoticeTypes = GetNoticeTypes();
            return View(model);
        }

        // GET: Notice/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                SELECT NoticeId, Title, Description, NoticeType, IconClass, 
                       PublishedBy, PublishedDate, ExpiryDate, IsActive
                FROM Notices
                WHERE NoticeId = @NoticeId";

                var notice = connection.QueryFirstOrDefault<NoticeViewModel>(query, new { NoticeId = id });

                if (notice == null)
                {
                    return HttpNotFound();
                }

                ViewBag.NoticeTypes = GetNoticeTypes();
                return View(notice);
            }
        }

        // POST: Notice/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(NoticeViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var query = @"
                    UPDATE Notices
                    SET Title = @Title,
                        Description = @Description,
                        NoticeType = @NoticeType,
                        IconClass = @IconClass,
                        ExpiryDate = @ExpiryDate,
                        IsActive = @IsActive
                    WHERE NoticeId = @NoticeId";

                    var parameters = new
                    {
                        NoticeId = model.NoticeId,
                        Title = model.Title,
                        Description = model.Description,
                        NoticeType = model.NoticeType,
                        IconClass = GetIconClassForType(model.NoticeType),
                        ExpiryDate = model.ExpiryDate,
                        IsActive = model.IsActive
                    };

                    var rowsAffected = connection.Execute(query, parameters);

                    if (rowsAffected == 0)
                    {
                        return HttpNotFound();
                    }

                    TempData["Success"] = "Notice updated successfully!";
                    return RedirectToAction("Index");
                }
            }

            ViewBag.NoticeTypes = GetNoticeTypes();
            return View(model);
        }

        // GET: Notice/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                SELECT NoticeId, Title, Description, NoticeType, 
                       PublishedDate, PublishedBy
                FROM Notices
                WHERE NoticeId = @NoticeId";

                var notice = connection.QueryFirstOrDefault<NoticeViewModel>(query, new { NoticeId = id });

                if (notice == null)
                {
                    return HttpNotFound();
                }

                return View(notice);
            }
        }

        // POST: Notice/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "DELETE FROM Notices WHERE NoticeId = @NoticeId";
                var rowsAffected = connection.Execute(query, new { NoticeId = id });

                if (rowsAffected > 0)
                {
                    TempData["Success"] = "Notice deleted successfully!";
                }
            }

            return RedirectToAction("Index");
        }

        // POST: Notice/ToggleStatus/5
        [HttpPost]
        public ActionResult ToggleStatus(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                // First get the current status
                var currentStatusQuery = "SELECT IsActive FROM Notices WHERE NoticeId = @NoticeId";
                var currentStatus = connection.QueryFirstOrDefault<bool?>(currentStatusQuery, new { NoticeId = id });

                if (currentStatus.HasValue)
                {
                    // Toggle the status
                    var newStatus = !currentStatus.Value;
                    var updateQuery = "UPDATE Notices SET IsActive = @IsActive WHERE NoticeId = @NoticeId";
                    connection.Execute(updateQuery, new { NoticeId = id, IsActive = newStatus });

                    return Json(new { success = true, isActive = newStatus });
                }
            }

            return Json(new { success = false });
        }

        // Helper methods
        private SelectList GetNoticeTypes()
        {
            var types = new List<SelectListItem>
        {
            new SelectListItem { Value = "Instruction", Text = "Instruction" },
            new SelectListItem { Value = "Event", Text = "Event" },
            new SelectListItem { Value = "Notification", Text = "Notification" },
            new SelectListItem { Value = "Preparation", Text = "Preparation" },
            new SelectListItem { Value = "Schedule", Text = "Schedule" }
        };

            return new SelectList(types, "Value", "Text");
        }

        private string GetIconClassForType(string noticeType)
        {
            switch (noticeType)
            {
                case "Instruction":
                    return "fa-file-alt";
                case "Event":
                    return "fa-calendar-event";
                case "Notification":
                    return "fa-bell";
                case "Preparation":
                    return "fa-laptop";
                case "Schedule":
                    return "fa-calendar";
                default:
                    return "fa-info-circle";
            }
        }
    }

    
}