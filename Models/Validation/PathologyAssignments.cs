using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class PathologyAssignments
    {
        [Key]
        [Required]
        public int PathologyAssignmentId { get; set; }
        [Required]
        [DisplayName("Program")]
        public string ProgramCode { get; set; }

        [Required]
        [DisplayName("Pathology")]
        public string PathologyField  { get; set; }
        [Required]
        [DisplayName("Assignment Item type")]
        public string AssignmentItemType { get; set; }
        [Required]
        [DisplayName("Due date (Days)")]
        public int PathologyDueDate { get; set; }

        [DisplayName("Created by")]
        public string createdBy { get; set; }

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

        [Required]
        [DisplayName("Active")]
        public bool Active { get; set; }
    }
}