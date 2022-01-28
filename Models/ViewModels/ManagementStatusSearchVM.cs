using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class ManagementStatusSearchVM
    {
        public ManagementStatusSearch ManagementStatusSearch { get; set; }
        public List<ManagementStatusSearch> ManagementStatusSearches { get; set; }
        public List<MedicalAid> MedicalAids { get; set; }
        public string[] SelectedMedicalAids { get; set; }
        public List<Programs> Programs { get; set; }
        public string[] SelectedPrograms { get; set; }
        public List<ManagementStatus> ManagementStatuses { get; set; }
        public string[] ManagementStatus { get; set; }

        public bool filter { get; set; }

    }
}
