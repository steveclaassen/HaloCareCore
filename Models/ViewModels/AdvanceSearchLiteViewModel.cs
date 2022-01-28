using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using HaloCareCore.Models.ViewModels.AdvancedSearchLiteViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class AdvanceSearchLiteViewModel
    {
        public string medicalAidCode { set; get; }
        public string Name { set; get; }

        public string code { set; get; }

        public string ProgramName { set; get; }
        public List<MedicalAid> medicalAids { set; get; }
        public List<Programs> programs { set; get; }
        public List<HIVAdvancedSearchResults> HIVAdvancedSearchResults { set; get; }
        public HIVAdvancedSearchResults HIVAdvanced { set; get; }
        public MHAdvancedSearchResults MHAdvanced { set; get; }
        public DiabetesAdvancedSearchResults DiabetesAdvanced { set; get; }

        public List<MHAdvancedSearchResults> MHAdvancedSearchResults { set; get; }
        public List<DiabetesAdvancedSearchResults> DiabetesAdvancedSearchResults { set; get; }

    }
}