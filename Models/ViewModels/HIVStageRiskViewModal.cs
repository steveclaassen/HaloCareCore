using HaloCareCore.Models.Management;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class HIVStageRiskViewModal
    {
        public HIVStagingRiskRules rules { get; set; }
        public List<RiskRatingTypes> ratingTypes { get; set; }

    }
}