using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class ClinicalRulesVM
    {
        public Guid dependantID { get; set; }

        public int ClinicalRulesID { get; set; }

        public int PathologyID { get; set; }

        public string RuleName { get; set; }

        public decimal Greater { get; set; }

        public string GtMessage { get; set; }

        public decimal Less { get; set; }

        public string LtMessage { get; set; }

        public string PathologyField { get; set; }

        public string ProgramCode { get; set; }

        public string Gender { get; set; }

        public string Message { get; set; } 

        public string pathFieldName { get; set; }
        public string pathFieldValue { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; }
    
        public string Instruction { get; set; } //hcare-1344
        public string RuleBroken { get; set; } //hcare-1344

    }
}
