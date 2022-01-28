using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class AttachmentTypes
    {
        [Key]
        [Required]
        [DisplayName("Code")]
        [StringLength(3, ErrorMessage = "Code restricted to 3 characters eg. APP")]
        public string attachmentType { get; set; }

        [Required]
        [DisplayName("Attachment type")]
        public string typeDescription { get; set; }


        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [DisplayName("Created")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? createdDate { get; set; }

        [DisplayName("Modified by")]
        public string modifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }
        //Hcare-1339
        [DisplayName("Assignment")]
        public string assignmentItemType { get; set; }
    }
}