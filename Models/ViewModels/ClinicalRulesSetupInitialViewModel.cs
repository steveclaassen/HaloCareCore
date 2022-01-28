using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class ClinicalRulesSetupInitialViewModel
    {
        public Guid accountID { get; set; }
        public Guid programID { get; set; }
        [DisplayName("Program name")]
        public string programName { get; set; }
        [DisplayName("Completed")]
        public bool done { get; set; }
    }
}