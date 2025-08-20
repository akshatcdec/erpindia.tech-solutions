using ERPIndia.Class.BLL;
using ERPIndia.Models;
using System.Collections.Generic;

namespace ERPIndia.ViewModel
{
    public class InvoiceViewModel
    {
        public InvoiceViewModel()
            : this(0, 0)
        {

        }
        public InvoiceViewModel(long? invoiceId, long? parentId)
        {
            this.Invoice = new InvoiceModel();
            this.Clients = ClientBLL.GetAllMyClientActive(parentId.HasValue ? parentId.Value : 0);
            this.Taxes = TaxBLL.GetAllActive();
            //this.InvoiceDetailModels = InvoiceBLL.GetInvoiceDetailByInvoiceId(invoiceId.HasValue ? invoiceId.Value : 0);
            this.PatientTestDetails = InvoiceBLL.GetInvoiceDetailByInvoiceId(invoiceId.HasValue ? invoiceId.Value : 0);

        }
        public List<InvoiceDetailModel> PatientTestDetails { get; set; }
        public InvoiceModel Invoice { get; set; }
        public List<InvoiceDetailModel> InvoiceDetailModels { get; set; }
        public List<ClientModel> Clients { get; set; }
        public List<TaxModel> Taxes { get; set; }
        public ClientModel Client { get; set; }


    }
}