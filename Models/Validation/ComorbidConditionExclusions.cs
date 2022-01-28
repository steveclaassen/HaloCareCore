using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class ComorbidConditionExclusions
    {
        [Key]
        public int id { get; set; }

        [DisplayName("Condition mapping code")]
        public string mappingCode { get; set; }

        [DisplayName("Condition mapping description")]
        public string mappingDescription { get; set; }

        [DisplayName("Formulary mapping code")]
        public string formularyCode { get; set; }

        [DisplayName("ICD-10 code")]
        public string ICD10Code { get; set; }

        [DisplayName("Created by")]
        public string CreatedBy { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Modified by")]
        public string ModifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ModifiedDate { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }

        public string ICD10Condition
        {
            get
            {
                return mappingDescription + " - " + ICD10Code;
            }
        }




    }
}