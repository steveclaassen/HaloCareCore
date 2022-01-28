using System;
using System.ComponentModel.DataAnnotations;

namespace HaloCareCore.Models.Communications
{
    public class BulkEmail
    {
        [Key]
        public Guid Id { get; set; }
        public Guid SchemeID { get; set; }
        public string SchemeOptionID { get; set; }
        public Guid ProgramID { get; set; }

        public string PatientStatus { get; set; }
        public string ManagementStatus { get; set; }

        public string EmailLayoutHeader { get; set; }
        public string EmailLayoutFooter { get; set; }
        public string EmailTemplate { get; set; }
        public string EmailSubject { get; set; }

        //[AllowHtml]
        public string EmailBody { get; set; }
        public string EmailAttachment { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string Status { get; set; }

        public string Excel { get; set; }
        public string Service { get; set; }
        public bool FileUploaded { get; set; }
        public string FileRoot { get; set; }
        public string FileName { get; set; }
        public DateTime? FileUploadedDate { get; set; }
        public string FileUploadedBy { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public bool Active { get; set; }

    }
}
