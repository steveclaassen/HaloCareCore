using HaloCareCore.Models.Admin;
using HaloCareCore.Models.Management;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class wfSettingsVM
    {
        public WorkflowVM WorkflowVM { get; set; }
        public List<WorkflowVM> WorkflowVMs { get; set; }
        
        public wfSettings wfSetting { get; set; }
        public List<wfSettings> wfSettings { get; set; }

        public wfQueueVM wfQueueVM { get; set; }
        public List<wfQueueVM> wfQueueVMs { get; set; }

        public wfUserQueue wfUserQueue { get; set; }
        public List<wfUserQueue> wfUserQueues { get; set; }

        public wfViewModel wfViewModel { get; set; }
        public List<wfViewModel> wfViewModels { get; set; }

        public List<wfSettings> Queues { get; set; }
        public string[] SelectedQueues { get; set; }

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
