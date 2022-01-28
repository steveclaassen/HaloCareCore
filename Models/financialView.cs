using HaloCareCore.Models.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class financialView
    {
        public List<financialReport> finList { get; set; }
        public List<operationalReportViewModel> statList { get; set; }
    }
}