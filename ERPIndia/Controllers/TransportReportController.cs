using DocumentFormat.OpenXml.Office2010.PowerPoint;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TransportDTO;
namespace TransportDTO
{
    public class DefaulterTrpReportCriteria
    {
        [Required(ErrorMessage = "Please select at least one month")]
        public List<string> SelectedMonths { get; set; }
        /// <summary>
        /// Selected Bus ID
        /// </summary>
        public string SelectedBus { get; set; }

        /// <summary>
        /// Selected Route ID
        /// </summary>
        public string SelectedRoute { get; set; }

        /// <summary>
        /// Session dropdown items
        /// </summary>
        public List<SelectListItem> BusList { get; set; }

        /// <summary>
        /// Class dropdown items
        /// </summary>
        public List<SelectListItem> RouteList { get; set; }

        // Add constructor to initialize the collection
        public DefaulterTrpReportCriteria()
        {
            SelectedMonths = new List<string>();
            BusList = new List<SelectListItem>();
            RouteList = new List<SelectListItem>();
           
        }
    }
    
    public class StudentTransportDTO
    {
        public string SrNo { get; set; }
        public string Class { get; set; }
        public string Name { get; set; }
        public string Father { get; set; }
        public string Mobile { get; set; }
        public string StCurrentAddress { get; set; }
        public decimal ReqTransp { get; set; }
        public decimal RecvdTransp { get; set; }
        public decimal RemainTran { get; set; }
        public string RouteName { get; set; }

        // Helper property to determine if student is a defaulter
        public bool IsDefaulter => RemainTran > 0;

        // Helper for formatting amounts in the view
        public string FormattedRemainTran => RemainTran.ToString("0.00");
        public string FormattedReqTransp => ReqTransp.ToString("0.00");
        public string FormattedRecvdTransp => RecvdTransp.ToString("0.00");
    }
    public class TransportDefaulterViewModel
    {
        public DefaulterTrpReportCriteria Criteria { get; set; }
        public List<StudentTransportDTO> Results { get; set; }
        public Dictionary<string, List<StudentTransportDTO>> RouteGroupedResults { get; set; }

        public TransportDefaulterViewModel()
        {
            Criteria = new DefaulterTrpReportCriteria();
            Results = new List<StudentTransportDTO>();
            RouteGroupedResults = new Dictionary<string, List<StudentTransportDTO>>();
        }
    }
}
namespace ERPIndia.Controllers
{
    public class TransportReportController : BaseController
    {
        public ActionResult DefaulterTrp()
        {
            TransportDefaulterViewModel transportDefaulterViewModel = new TransportDefaulterViewModel();
            return View(transportDefaulterViewModel);
        }
        public ActionResult DefaulterBuswise()
        {
            TransportDefaulterViewModel transportDefaulterViewModel = new TransportDefaulterViewModel();
            return View(transportDefaulterViewModel);
        }
        public ActionResult DefaulterRoutewise()
        {
            TransportDefaulterViewModel transportDefaulterViewModel = new TransportDefaulterViewModel();
            return View(transportDefaulterViewModel);
        }
        public ActionResult DefaulterFeeWithTrp()
        {
            return View();
        }
        
    }
}