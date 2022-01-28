using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class BulkEmailDetailsVM
    {
        public Guid Id { get; set; }
        public Guid SchemeID { get; set; }
        public Guid ProgramID { get; set; }

        [DisplayName("Scheme")]
        public string SchemeName { get; set; }

        [DisplayName("Scheme option")]
        public string SchemeOption { get; set; }

        [DisplayName("Program")]
        public string Program { get; set; }

        [DisplayName("Patient status")]
        public string PatientStatus { get; set; }

        [DisplayName("Management status")]
        public string ManagementStatus { get; set; }

        [DisplayName("Layout header")]
        public string LayoutHeader { get; set; }

        [DisplayName("Layout footer")]
        public string LayoutFooter { get; set; }

        [DisplayName("Email template")]
        public string EmailTemplate { get; set; }

        [DisplayName("Email subject")]
        public string EmailSubject { get; set; }

        [DisplayName("Email body")]
        public string EmailBody { get; set; }

        [DisplayName("Email attachment")]
        public string EmailAttachment { get; set; }

        [DisplayName("Effective date")]
        public DateTime? EffectiveDate { get; set; }

        public string Excel { get; set; }
        public string Service { get; set; }

        [DisplayName("File")]
        public bool FileUploaded { get; set; }

        [DisplayName("File root")]
        public string FileRoot { get; set; }

        [DisplayName("File name")]
        public string FileName { get; set; }

        [DisplayName("File uploaded date")]
        public DateTime? FileUploadedDate { get; set; }

        [DisplayName("File uploaded by")]
        public string FileUploadedBy { get; set; }

        [DisplayName("Created date")]
        public DateTime CreatedDate { get; set; }
        
        [DisplayName("Created by")]
        public string CreatedBy { get; set; }

        [DisplayName("Modified date")]
        public DateTime? ModifiedDate { get; set; }

        [DisplayName("Modified by")]
        public string ModifiedBy { get; set; }

        [DisplayName("Status")]
        public string Status { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }


        public List<ManagementStatus> ManagementStatuses { get; set; }
        public List<AttachmentTemplate> Attachments { get; set; }
        public List<PatientStatus> PatientStatuses { get; set; }
        public List<MedicalAidPlans> Options { get; set; }

    }
}
