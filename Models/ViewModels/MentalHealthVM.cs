using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Questionnaire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class MentalHealthVM
    {
        public MH_DSM5Response MH_DSM5Response { get; set; }
        public List<MH_DSM5Response> MH_DSM5Responses { get; set; }

        public MH_SchizophreniaResponse MH_SchizophreniaResponse { get; set; }
        public List<MH_SchizophreniaResponse> MH_SchizophreniaResponses { get; set; }

        public MH_BipolarResponse MH_BipolarResponse { get; set; }
        public List<MH_BipolarResponse> MH_BipolarResponses { get; set; }
        
        public MH_DepressionResponse MH_DepressionResponse { get; set; }
        public List<MH_DepressionResponse> MH_DepressionResponses { get; set; }

        public DoctorReferral DoctorReferral { get; set; }
        public List<DoctorReferral> DoctorReferrals { get; set; }


    }
}
