using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Validation;
using HaloCareCore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models
{
    public class AssignmentTypeViewModel
    {
        public AssignmentTypes assignmentType { get; set; }
        public List<AssignmentTypes> assignmentTypes { get; set; }

        public AssignmentItemTypes assignmentItemType { get; set; }
        public List<AssignmentItemTypes> assignmentItemTypes { get; set; }

        public AssignmentItemTaskTypes assignmentItemTaskType { get; set; }
        public List<AssignmentItemTaskTypes> assignmentItemTaskTypes { get; set; }

        public TaskTypeRequirements taskTypeRequirement { get; set; }
        public List<TaskTypeRequirements> taskTypeRequirements { get; set; }

        public TaskRequirementItemLinking taskRequirementItemLinking { get; set; }
        public List<TaskRequirementItemLinking> taskRequirementItemLinkings { get; set; }

        public List<HospitalizationICD10types> hospitalizationICD10types { get; set; }
        public List<PathologyAssignmentViewModel> PathologyAssignments { get; set; }
        public bool active { set; get; }

    }
}
