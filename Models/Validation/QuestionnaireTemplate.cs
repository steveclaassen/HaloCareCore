using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class QuestionnaireTemplate
    {
        [Key]
        [Required]
        public int QuestionnaireTemplateID { get; set; }

        [DisplayName("Template name")]
        public string TemplateName{ get; set; }

        [DisplayName("Question")]
        public string Question{ get; set; }

        [DisplayName("Question No")]
        public int QuestionNo { get; set; }

        [DisplayName("Created by")]
        public string CreatedBy { get; set; }

        [DisplayName("Modified by")]
        public string ModifiedBy { get; set; }
        
        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; }
        
        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ModifiedDate { get; set; }

        [DisplayName("Answer")]
        public bool Answer { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }
    }
}