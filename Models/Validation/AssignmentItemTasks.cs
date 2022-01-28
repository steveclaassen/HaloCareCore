using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class AssignmentItemTasks
    {
        [Key]
        [Required]
        public int id { get; set; }

        [Required]
        [DisplayName("Assignment item ID")]
        public int assignmentItemID { get; set; }

        [Required]
        [DisplayName("Task type ID")]
        public string taskTypeId { get; set; }

        [Required]
        [DisplayName("Requirement ID")]
        public int requirementId { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime effectiveDate { get; set; }

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

        [DisplayName("Closed by")]
        public string closedBy { get; set; }

        [DisplayName("Closed date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? closedDate { get; set; }

        [DisplayName("Comment")]
        public string comment { get; set; }

        [DisplayName("Status")]
        public string status { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }

        [DisplayName("Closed")]
        public bool closed { get; set; }
        
    }
}