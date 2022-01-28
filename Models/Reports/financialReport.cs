using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Reports
{
    public class financialReport
    {
        [DisplayName("Member #")]
        public string membershipNo { get; set; }

        [DisplayName("Dep code")]
        public string dependantCode { get; set; }

        [DisplayName("Medical scheme code")]
        public string medicalAidCode { get; set; }

        [DisplayName("Status")]
        public string status { get; set; }
    }
}