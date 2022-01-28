using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class AccountMedicalAidViewModel
    {
        public Guid accountId { get; set; }
        public Guid medicalAidId { get; set; }
        [DisplayName("Medical scheme name")]
        public string MedicalAidName { get; set; }

        public Guid optionId { get; set; }
        [DisplayName("Option")]
        public string OptionName { get; set; }
        [DisplayName("Active")]
        public bool active { get; set; }
    }
}