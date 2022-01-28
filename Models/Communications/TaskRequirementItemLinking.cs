using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Communications
{
    public class TaskRequirementItemLinking
    {
        [Key]
        [Required]
        public int id { get; set; }

        [Required]
        [DisplayName("Task item")]
        public string itemId { get; set; }

        [DisplayName("Task type")]
        public string taskType { get; set; }

        [DisplayName("Requirement")]
        public int requirementId { get; set; }

        [DisplayName("Template ID")]
        public string templateID { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [DisplayName("Modified by")]
        public string modifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }

        [DisplayName("Sequence")]
        public int taskSequence { get; set; }
    }
}