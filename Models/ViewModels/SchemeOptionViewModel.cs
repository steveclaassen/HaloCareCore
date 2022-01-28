using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class SchemeOptionViewModel
    {
        public MedicalAid medicalAid { get; set; }
        public List<MedicalAid> medicalAids { get; set; }

        public MedicalAidPlans medicalAidPlan { get; set; }
        public List<MedicalAidPlans> medicalAidPlans { get; set; }

        public MedicalAidVM medicalAidVM { get; set; }
        public List<MedicalAidVM> medicalAidVMs { get; set; }

        public List<MedicalAid> MedicalAids { get; set; }

        public List<MedicalAidVM> MedicalAidList { get; set; } //hcare-1346
        public List<MedicalAidVM> MedicalAidOptionList { get; set; } //hcare-1346


    }
}
