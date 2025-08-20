namespace ERPIndia
{
    public class FamilyData
    {
        public string AdmsnNo { get; set; }
        public string SchoolCode { get; set; }
        public string FName { get; set; }
        public string FPhone { get; set; }
        public string FOccupation { get; set; }
        public string FAadhar { get; set; }
        public string FNote { get; set; }
        public string FPhoto { get; set; }
        public string MName { get; set; }
        public string MPhone { get; set; }
        public string MOccupation { get; set; }
        public string MAadhar { get; set; }
        public string MNote { get; set; }
        public string MPhoto { get; set; }
        public string GName { get; set; }
        public string GRelation { get; set; }
        public string GEmail { get; set; }
        public string GPhoto { get; set; }
        public string GPhone { get; set; }
        public string GOccupation { get; set; }
        public string GAddress { get; set; }
        public string GRemark { get; set; }
        public string StCurrentAddress { get; set; }
        public string StPermanentAddress { get; set; }
        public string RouteName { get; set; }
        public string HostelDetail { get; set; }
        public string HostelNo { get; set; }
        public void SetDefaultValue()
        {
            IsValidData = "N";
        }
        public string IsValidData { get; set; }

    }
}