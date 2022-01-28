using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class CoMorbidTypes
    {
        [Key]
        [Required]
        public int id { get; set; }

        [DisplayName("Condition")]
        public string condition { get; set; }

        [DisplayName("Category")]
        public string category { get; set; }

        [DisplayName("Sub category")]
        public string subCategory { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }

        [DisplayName("ICD-10 code")] //HCare-958
        public string icd10 { get; set; }

        [DisplayName("CoMorbid")]
        public string icd10Condition
        {
            get
            {
                return condition + " - " + icd10;
            }
        }
    }
}