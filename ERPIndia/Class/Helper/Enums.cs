namespace ERPIndia.Class.Helper
{
    /// <summary>
    /// Enumeration constant for multiple operation type.
    /// </summary>
    public enum MultiOperationType : int
    {
        Active = 1,
        Inactive = 2,
        Delete = 3
    }

    /// <summary>
    /// Enumeration constant for role type.
    /// </summary>
    public enum RoleType : int
    {
        SuperAdmin = 1,
        Admin = 2,
        Doctor = 3,
        Nurse = 4,
        Pharmacist = 5,
        Laboratorist = 6,
        Accountant = 7,
        Patient = 8,
        Client = 11,
        Agents = 12
    }

    /// <summary>
    /// Enumeration constant for login history action.
    /// </summary>
    public enum SystemLoginHistoryAction : int
    {
        LogIn = 1,
        LogOut = 2
    }

    /// <summary>
    /// Enumeration constant for paged list display mode.
    /// </summary>
    public enum PagedListDisplayMode
    {
        Always,
        Never,
        IfNeeded
    }

    /// <summary>
    /// Enumeration constant for multiple operation type.
    /// </summary>
    public enum PaymentStatusType : int
    {
        UnPaid = 0,
        Paid = 1,
    }

    public enum PatientTestStatusType : int
    {
        Pending = 0,
        Delivered = 1,
    }

    public enum ReportType : int
    {
        Birth = 1,
        Death = 2,
    }
}
