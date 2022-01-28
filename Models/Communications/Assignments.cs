using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace HaloCareCore.Models.Communications
{
    public class Assignments
    {
        [Key]
        [Required]
        public int assignmentID { get; set; }

        [Required]
        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime effectiveDate { get; set; }

        [Required]
        [DisplayName("Assignment type")]
        public string assignmentType { get; set; }

        [DisplayName("Postponement reason")]
        public string postponementReason { get; set; }

        [Required]
        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [DisplayName("Created date")]
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

        [Required]
        [DisplayName("Dependant ID")]
        public Guid dependentID { get; set; }

        [DisplayName("Comment")]
        public string comment { get; set; }

        [DisplayName("Postpone")]
        public bool postpone { get; set; }

        [DisplayName("Postpone until")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? postponeToDate { get; set; }

        [DisplayName("Closed by")]
        public string closedBy { get; set; }

        [DisplayName("Closed date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? closedDate { get; set; }

        [DisplayName("Assignment status")]
        public string status { get; set; }

        [DisplayName("Pathology ID")]
        public int pathologyID { get; set; }

        [DisplayName("Instruction")]
        public string Instruction { get; set; }

        public Guid? programId { get; set; }

        //HCare-131
        public int attachmentId { get; set; }

    }
}