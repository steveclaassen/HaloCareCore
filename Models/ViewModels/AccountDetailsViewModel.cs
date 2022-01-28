using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class AccountDetailsViewModel
    {
        public Guid accountID { get; set; }
        [DisplayName("System account")]
        public string name { get; set; }

        [DisplayName("Description")]
        public string description { get; set; }

        [DisplayName("Scheme Information")]
        public List<AccountMedicalAidViewModel> medicalAids { get; set; }

        public List<ClinicalRulesSetupViewModal> clinicalRules { get; set; }

        public List<StatusManagmentViewModel> statuses { get; set; }
        public bool AddAccess { set; get; }
        public bool EditAccess { set; get; }
    }
}