using HaloCareCore.Models.Admin;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Questionnaire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class DisclaimerVM
    {
        public DisclaimerQuestions DisclaimerQuestion { get; set; }
        public List<DisclaimerQuestions> DisclaimerQuestions { get; set; }
        public DisclaimerQuestions AcknowledgementQuestion { get; set; }
        public List<DisclaimerQuestions> AcknowledgementQuestions { get; set; }

        public DisclaimerResponse DisclaimerResponse { get; set; }
        public List<DisclaimerResponse> DisclaimerResponses { get; set; }

        public Dependant Dependant { get; set; }
        public List<Dependant> Dependants { get; set; }

        public ComsViewModel ContactVM { get; set; }
        public List<ComsViewModel> ContactVMs { get; set; }

        public NextOFKin NextOFKin { get; set; }
        public List<NextOFKin> NextOFKins { get; set; }
        public List<PreferredMethodOfContact> PMOC { get; set; }



    }
}
