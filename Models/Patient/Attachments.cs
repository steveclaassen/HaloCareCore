using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HaloCareCore.Models.Patient
{
    public class Attachments
    {
        [Key]
        [Required]
        public int attachmentID { get; set; }

        [Required]
        public Guid dependentID { get; set; }

        [Required]
        [DisplayName("Attachment type")]
        public string attachmentType { get; set; }

        [Required]
        //[AllowHtml]
        [DisplayName("Attachment name")]
        public string attachmentName { get; set; }
        public string ImportAttachmentSubject { set; get; }

        [Required]
        //[AllowHtml]
        public string Link { get; set; }

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

        [DisplayName("Modfied date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? modifiedDate { get; set; }

        [Required]
        [DisplayName("Active")]
        public bool Active { get; set; }

        public Guid? programId { get; set; }

    }
}