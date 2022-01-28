using HaloCareCore.Models.Admin;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class AccountSetupViewModel
    {
        public Guid accountID { get; set; }

        [Required]
        [DisplayName("System Account Name")]
        public string name { get; set; }

        [Required]
        [DisplayName("System Account Description")]
        public string description { get; set; }

        [DisplayName("Scheme Access")]
        public List<MedicalAid> medicalAids { get; set; }
        public string[] selectedSchemes { get; set; }

        [DisplayName("Scheme Programs")]
        public List<Programs> schemePrograms { get; set; }
        public string[] selectedPrograms { get; set; }

        [DisplayName("Scheme Options")]
        public List<MedicalAidPlans> schemePlans { get; set; }
        public string[] selectedSchemeOptions { get; set; }

        public List<ClinicalRulesSetupViewModal> clinicalRules { get; set; }

        public List<StatusManagmentViewModel> statuses { get; set; }

    }
}