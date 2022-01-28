using HaloCareCore.Models.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class RulesVM
    {
        public List<ClinicalRules> ClinicalRules { get; set; }
        public List<HIVRiskRatingRules> HIVRiskRatingRules { get; set; }
        public List<HIVStagingRiskRules> HIVStagingRiskRules { get; set; }
        public List<MHRiskRatingRules> MHRiskRatingRules { get; set; }
        public List<HypoRiskRules> RiskRules { get; set; }
        public List<LifeExpectancyRules> LifeExpectancyRules { get; set; }
        public List<HivcomorbidRules> HivcomorbidRules { set; get; }    //HCare-1000
        public List<RiskRatingMonitor> RiskRatingMonitors { set; get; } //hcare-1395
        public List<TariffCode> TariffCodes { set; get; } //HCare-1017
        public List<ICD10OnDischargeSetup> ICD10OnDischargeSetups { set; get; } //HCare-1481
    } 
}
