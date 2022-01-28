using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class MedicalAidPatientVM
    {
        public Guid DependantID { get; set; }
        public Guid MedicalAidID { get; set; }

        public string MedicalAidCode { get; set; }
        public string MedicalAidName { get; set; }
    }
}
