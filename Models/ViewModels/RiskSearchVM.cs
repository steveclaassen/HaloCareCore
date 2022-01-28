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
    public class RiskSearchVM
    {
        public RiskSearch RiskSearch { get; set; }
        public List<RiskSearch> RiskSearches { get; set; }

        public List<MedicalAid> MedicalAids { get; set; }
        public string[] SelectedMedicalAids { get; set; }
        public List<Programs> Programs { get; set; }
        public string[] SelectedPrograms { get; set; }
        public List<RiskRatingTypes> RiskRating { get; set; }
        public string[] SelectedRiskRating { get; set; }

        public bool filter { get; set; }
    }
}
