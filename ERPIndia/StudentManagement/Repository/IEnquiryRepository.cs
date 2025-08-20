using ERPK12Models.DTO;
using ERPK12Models.ViewModel.Enquiry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERPIndia.StudentManagement.Repository
{
    public interface IEnquiryRepository
    {
        // Receipt methods
        Task<ReceiptInfo> GetReceiptInfoAsync(Guid enquiryId, Guid sessionId, int tenantCode);
        Task<string> GenerateReceiptNumberAsync(Guid sessionId, int tenantCode);
        Task<ConversionResult> ConvertEnquiryToAdmissionAsync(Guid enquiryId,int schoolCode,DateTime admissionDate,Guid sessionId,int tenantCode,Guid createdBy);
        // Enquiry methods - All require SessionID and TenantCode for security
        Task<IEnumerable<StudentEnquiry>> GetAllEnquiriesAsync(Guid sessionId, int tenantCode,
            EnquiryFilterViewModel filters = null, int page = 1, int pageSize = 10);
        Task<StudentEnquiry> GetEnquiryByIdAsync(Guid id, Guid sessionId, int tenantCode);
        Task<Guid> CreateEnquiryAsync(StudentEnquiry enquiry, Guid sessionId, int tenantCode, Guid createdBy);
        Task<bool> UpdateEnquiryAsync(StudentEnquiry enquiry, Guid sessionId, int tenantCode, Guid modifiedBy);
        Task<bool> DeleteEnquiryAsync(Guid id, Guid sessionId, int tenantCode, Guid deletedBy);
        Task<int> GetTotalEnquiriesCountAsync(Guid sessionId, int tenantCode, EnquiryFilterViewModel filters = null);

        // Follow-up methods - All require SessionID and TenantCode for security
        Task<IEnumerable<EnquiryFollowUp>> GetFollowUpsByEnquiryIdAsync(Guid enquiryId, Guid sessionId, int tenantCode);
        Task<Guid> CreateFollowUpAsync(EnquiryFollowUp followUp, Guid sessionId, int tenantCode, Guid createdBy);
        Task<bool> UpdateFollowUpAsync(EnquiryFollowUp followUp, Guid sessionId, int tenantCode, Guid modifiedBy);
        Task<bool> DeleteFollowUpAsync(Guid id, Guid sessionId, int tenantCode, Guid deletedBy);
    }
}