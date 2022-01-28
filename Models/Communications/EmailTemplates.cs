using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Communications
{
    public class EmailTemplates
    {
        [Key]
        [Required]
        public int templateID { get; set; }

        [DisplayName("Category")]
        public string category { get; set; }

 
        [DisplayName("Title")]
        public string title { get; set; }
        
        [DisplayName("Template")]
        public string template { get; set; }

        [DisplayName("Heading")]
        public string templateHeading { get; set; }

        [DisplayName("Subject")]
        public string templateSubject { get; set; }

  
        [DisplayName("Body")]
        public string templateBody { get; set; }

        [DisplayName("Image")]
        public string image { get; set; }

        [Required]
        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [Required]
        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [DisplayName("Modified by")]
        public string modifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }
        //HCare-1043
        [DisplayName("Program")]
        public string program { get; set; }

        public List<Programs> Programs { get; set; }
        public List<Language> Languages { get; set; }
   
    }
}