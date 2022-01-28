using HaloCareCore.Models.Management;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class HIVCormodisViewModel
    {
        public List<HIVStages> HIVStages = new List<HIVStages>();
        public HivcomorbidRules HivcomorbidRules { set; get; }
        public List<ComorbidConditionExclusions> ComorbidConditionExclusions = new List<ComorbidConditionExclusions>(); 

    }
}