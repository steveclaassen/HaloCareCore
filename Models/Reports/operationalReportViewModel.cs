using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Reports
{
    public class operationalReportViewModel
    {
        [DisplayName("New enrollment")]
        public int newEnrolmentCount { get; set; }

        [DisplayName("Name")]
        public string strName { get; set; }

        [DisplayName("Count")]
        public int strCount { get; set; }
    }
}