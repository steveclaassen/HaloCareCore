using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 
namespace HaloCareCore.Models.Communications
{
    public class Emails
    {
        [Key]
        [Required]
        public int emailId { get; set; }

        [Required]
        public Guid dependantID { get; set; }

        [Required]
        [DisplayName("Email to")]
        public string emailTo { get; set; }

        [DisplayName("CC")]
        public string cc { get; set; }

        [Required]
        [DisplayName("Message")]
        public string emailMassage { get; set; }

        [Required]
        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime effectivedate { get; set; }

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

        [DisplayName("Status")]
        public string status { get; set; }

        [DisplayName("Subject")]
        public string subject { get; set; }

        [DisplayName("Reference #")]
        public string ReferenceNumber { get; set; }

        [DisplayName("Attachment")]
        [NotMapped]
        public string[] attachment { get; set; } /*HCare-860*/
        public Guid programId { set; get; }

        

    }
}