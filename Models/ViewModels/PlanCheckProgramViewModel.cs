using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class PlanCheckProgramViewModel
    {
        public Guid optionId { get; set; }
        public Guid planProgramId { get; set; }
        public Guid programID { get; set; }
        public Guid accountID { get; set; }
        public Guid medicalAidID { get; set; }

        [DisplayName("Plan")]
        public string planName { get; set; }

        [DisplayName("Program")]
        public string programName { get; set; }

        [DisplayName("Include")]
        public bool include { get; set; }

        public Guid medId { get; set; }
        public bool Active { set; get; }

    }
}