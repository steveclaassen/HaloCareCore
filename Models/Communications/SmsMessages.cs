using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Communications
{
    public class SmsMessages
    {
        [Key]
        [Required]
        public int messageID { get; set; }

        [Required]
        public Guid dependantID { get; set; }

        [DisplayName("Mobile #")]
        [DataType(DataType.PhoneNumber)]
        public string cell_no { get; set; }

        [Required]
        [DisplayName("Template ID")]
        public int templateID { get; set; }
        public string importSMSSubject { set; get; }

        [Required]
        [DisplayName("Message")]
        public string message { get; set; }

        [Required]
        [DisplayName("Status")]
        public string status { get; set; }

        [Required]
        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime effectiveDate { get; set; }

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

        public string AttuneID { get; set; }

        public string responseString { get; set; }

        [DisplayName("Reference #")]
        public string ReferenceNumber { get; set; }

        public string AttuneMessageStatus { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? AttuneDeliveryDate { get; set; }
        public Guid programId { set; get; }


    }
}