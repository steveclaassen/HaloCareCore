using HaloCareCore.Models.Patient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class MedicalAidProgramViewModel
    {
        public int id { get; set; }

        [DisplayName("Program name")]
        public string codeType { get; set; }

        public string medicalAidId { get; set; }

        [DisplayName("Program code")]
        public string program { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }

        [DisplayName("Referral from ")]
        public string referralFrom  { get; set; }

        public List<ReferralFrom> referralFroms { set; get; }
    } 
}