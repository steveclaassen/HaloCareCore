using HaloCareCore.Models.Admin;
using HaloCareCore.Models.Communications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class EmailVM
    {
        public int Id { get; set; }
        public Guid DependantID { get; set; }
        public Guid ProgramID { get; set; }
        public string Subject { get; set; }
        public string EmailAddressTo { get; set; }
        public string EmailAddressCc { get; set; }
        public string EmailMessage { get; set; }
        public string AttachmentName { get; set; }
        public string FileName { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string Status { get; set; }
        public string Program { get; set; }
        public List<AttachmentTemplate> Attachments { get; set; }
    }
}
