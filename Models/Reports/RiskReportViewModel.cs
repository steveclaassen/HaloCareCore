using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Reports
{
    public class RiskReportViewModel
    {
        public string zeroto50 {get;set;}
        public string fifty1to200 { get; set; }
        public string two01to350 { get; set; }
        public string three51to500 { get; set; }
        public string five01plus { get; set; }
        public string total { get; set; }
    }
    public class ACTPatients
    {
        public string reportMonth { get; set; }
        public string reportYear { get; set; }
        public string statusCountACT { get; set; }
    }

    public class ACTGENPatients
    {
        public ACTPatients actpatients { get; set; }
        public string statusCountACTGender { get; set; }
    }

    public class AVGPathCount
    {
        public ACTPatients actpatients { get; set; }
        public string avgPathCount { get; set; }
    }
}