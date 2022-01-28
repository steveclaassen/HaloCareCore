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
    public class PathologySearchVM
    {
        public PathologySearch PathologySearch { get; set; }
        public List<PathologySearch> PathologySearches { get; set; }

        public PathologySortVM PathologySearchResult { get; set; }
        public List<PathologySortVM> PathologySearchResults { get; set; }

        public List<MedicalAid> MedicalAids { get; set; }
        public string[] SelectedMedicalAids { get; set; }
        public List<Programs> Programs { get; set; }

        public string[] SelectedPrograms { get; set; }

        //public PathologyFields PathologyField { get; set; }

        public List<PathologyFields> PathologyFields { get; set; }
        public List<PathologyFields> PathologyFieldsList { get; set; }
        public string[] SelectedPathologyFields { get; set; }

        //public List<string> pFields { get; set; }

        public Guid DependentID { get; set; }
        public Guid? pro { get; set; }

        public List<ClinicalRules> ClinicalRules { get; set; }

    }
}
