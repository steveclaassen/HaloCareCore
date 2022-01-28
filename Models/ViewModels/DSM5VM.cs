using HaloCareCore.Models.Questionnaire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class DSM5VM
    {
        public MH_DSM5Response MH_DSM5Response { get; set; }
        public List<MH_DSM5Response> MH_DSM5Responses { get; set; }
    }
}
