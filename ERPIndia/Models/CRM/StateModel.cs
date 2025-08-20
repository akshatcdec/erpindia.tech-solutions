namespace ERPIndia.Models
{
    public class StateModel
    {
        public StateModel()
        {
            this.IsActive = true;
        }
        public long StateId { get; set; }
        public string StateName { get; set; }
        public string AlphaCode { get; set; }
        public string GSTStateCode { get; set; }
        public bool IsActive { get; set; }
        public long CompanyId { get; set; }

        public long CreatedBy { get; set; }
        public int TotalRecordCount { get; set; }

        public string DuplicateColumn { get; set; }
    }

}