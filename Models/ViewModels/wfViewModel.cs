using HaloCareCore.Models.Admin;
using HaloCareCore.Models.Management;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class wfViewModel
    {
        public Guid QueueID { get; set; }
        public Guid MedicalAidID { get; set; }
        public Guid ProgramID { get; set; }

        [DisplayName("Queue")]
        public string QueueName { get; set; }

        [DisplayName("Scheme")]
        public string MedicalScheme { get; set; }

        [DisplayName("Program")]
        public string Program { get; set; }

        [DisplayName("Management status")]
        public string ManagementStatus { get; set; }

        [DisplayName("From date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? FromDate { get; set; }

        [DisplayName("To date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ToDate { get; set; }

        [DisplayName("Risk rating")]
        public string RiskRating { get; set; }

        [DisplayName("Assignment")]
        public string AssignmentItemType { get; set; }

        [DisplayName("Instruction")]
        public string Instruction { get; set; }

        [DisplayName("Created by")]
        public string CreatedBy { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CreatedDate { get; set; }

        [DisplayName("Modified by")]
        public string ModifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ModifiedDate { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }

        [DisplayName("Pathology field")]
        public string PathologyField { get; set; }

        [DisplayName("Less than")]
        public decimal Less { get; set; }

        [DisplayName("Greater than")]
        public decimal Greater { get; set; }

        public List<wfSettings> Queues { get; set; }
        public string[] SelectedQueues { get; set; }
        public List<wfViewModel> PathologyFields { get; set; }
        public string[] SelectedPathology { get; set; }

        public List<Users> Users { get; set; }
        public string[] SelectedUser { get; set; }

        public List<MedicalAid> MedicalAids { get; set; }
        public string[] SelectedMedicalAids { get; set; }

        public List<Programs> Programs { get; set; }
        public string[] SelectedPrograms { get; set; }

        public List<ManagementStatus> ManagementStatuses { get; set; }
        public string[] SelectedManagementStatus { get; set; }

        public List<RiskRatingTypes> RiskRatingTypes { get; set; }
        public string[] SelectedRiskRating { get; set; }

        public List<AssignmentItemTypes> AssignmentItemTypes { get; set; }
        public string[] SelectedAssignments { get; set; }

    }
}