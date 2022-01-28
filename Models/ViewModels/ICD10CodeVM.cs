using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class ICD10CodeVM
    {
        public Icd10Codes icd10Code { get; set; }
        public List<Icd10Codes> icd10Codes { get; set; }

        public Programs program { get; set; }
        public List<Programs> programs { get; set; }
    }
}
