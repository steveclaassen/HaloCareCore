using HaloCareCore.Models.Management;
using HaloCareCore.Models.Validation;
using HaloCareCore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models
{
    public class coMorbidViewModel
    {
        public CoMormidConditions coMormidCondition { get; set; }
        public List<CoMormidConditions> coMormidConditions { get; set; }

        public CoMorbidTypes coMorbidType { get; set; }
        public List<CoMorbidTypes> coMorbidTypes { get; set; }

        public ComorbidView comorbidView { get; set; }
        public List<ComorbidView> comorbidViews { get; set; }

        public CoMorbidConditionVM coMorbidConditionVM { get; set; }
        public List<CoMorbidConditionVM> coMorbidConditionVMs { get; set; }

        public ComorbidConditionExclusions ComorbidConditionExclusion { get; set; } //HCare-859
        public List<ComorbidConditionExclusions> ComorbidConditionExclusions { get; set; } //HCare-859
    }
}
