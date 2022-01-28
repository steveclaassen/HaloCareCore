using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Questionnaire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class PregnantVM
    {
        public QuestionnaireOther Pregnant { get; set; }
        public List<QuestionnaireOther> Pregnancy { get; set; }

        public Dependant Dependant { get; set; }
        public List<Dependant> Dependants { get; set; }
    }
}
