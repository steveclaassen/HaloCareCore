using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class CoMorbidConditionVM
    {
        public Guid dependantID { get; set; }
        public int id { get; set; }
        public int CMTypeID { get; set; }
        public int CMConditionId { get; set; }
        public string Condition { get; set; }
        public string icd10Condition { get; set; }
        public string icd10 { get; set; }
        public string mappingCode { get; set; }
        public string programCode { get; set; }

        public DateTime? createdDate { get; set; }
        public DateTime? effectiveDate { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }


    }
}
