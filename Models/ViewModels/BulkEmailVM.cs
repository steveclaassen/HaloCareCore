using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class BulkEmailVM
    {
        public List<BulkEmailDetailsVM> Details { get; set; }
        public BulkEmailDetailsVM Detail { get; set; }
        public BulkEmail BulkEmail { get; set; }

        [DisplayName("Scheme")]
        public List<MedicalAid> Schemes { get; set; }
        public string[] SelectedSchemes { get; set; }

        [DisplayName("Scheme option")]
        public List<MedicalAidPlans> Options { get; set; }
        public string SelectedOptions { get; set; }

        [DisplayName("Program")]
        public List<Programs> Programs { get; set; }
        public string[] SelectedPrograms { get; set; }

        [DisplayName("Patient status")]
        public List<PatientStatus> PatientStatus { get; set; }
        public string[] SelectedPatientStatus { get; set; }

        [DisplayName("M/status")]
        public List<ManagementStatus> ManagementStatus { get; set; }
        public string[] SelectedManagementStatus { get; set; }

        [DisplayName("Email template")]
        public List<EmailTemplates> EmailTemplates { get; set; }
        public string[] SelectedEmailTemplates { get; set; }

        [DisplayName("Email attachments")]
        public List<AttachmentTemplate> AttachmentTemplates { get; set; }
        public string[] SelectedAttachmentTemplate { get; set; }

        [DisplayName("Effective date")]
        public DateTime? EffectiveDate { get; set; }

        public string EmailBody { get; set; }

        public Guid UserID { get; set; }


    }
}
