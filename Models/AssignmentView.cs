using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class AssignmentView
    {
        public int assignmentID { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime effectiveDate { get; set; }

        [DisplayName("Assignment type")]
        public string assignmentType { get; set; }

        [DisplayName("Postponement reason")]
        public string AssignmentItemType { get; set; }

        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [DisplayName("Comment")]
        public string comment { get; set; }

        [DisplayName("Instruction")]
        public string Instruction { get; set; }

        [DisplayName("Closed by")]
        public string closedBy { get; set; }

        [DisplayName("Closed date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? closedDate { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? createdDate { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }

        public string itemType { get; set; }

        public Guid? program { get; set; } //hcare-1112

    }
}