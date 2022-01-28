using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class ClinicalStatusSetup
    {
        public List<ClinicalRulesSetupViewModal> clinicalRules { get; set; }
        public List<StatusManagmentViewModel> statuses { get; set; }

        public string ProgramName { get; set; } //hcare-1338
        public Guid AccountID { get; set; } //hcare-1338
        public Guid ProgramID { get; set; } //hcare-1338
        public Guid MedicalAidID { get; set; } //hcare-1338
    }
}