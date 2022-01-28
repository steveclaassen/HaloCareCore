using HaloCareCore.Models.Admin;
using HaloCareCore.Models.Management;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using System.Collections.Generic;

namespace HaloCareCore.Models.ViewModels
{
    public class ProductionSearchVM
    {
        public ProductionSearch ProductionSearch { get; set; }
        public List<ProductionResultsVM> ProductionSearches { get; set; }

        public List<MedicalAid> MedicalAids { get; set; }
        public string[] SelectedMedicalAids { get; set; }

        public List<Programs> Programs { get; set; }
        public string[] SelectedPrograms { get; set; }

        public List<Users> Users { get; set; }
        public string[] SelectedUsers { get; set; }

        public List<AssignmentItemTypes> AssignmentItemTypes { get; set; }
        public string[] SelectedAssignmentTypes { get; set; }
        
        public List<AssignmentItemTaskTypes> AssignmentItemTaskTypes { get; set; }
        public string[] SelectedAssignmentItemTaskTypes { get; set; }

        public List<ProductionWorkItems> WorkItems { get; set; }
        public string[] SelectedWorkItems { get; set; }

        public bool filter { get; set; }
    }
}
