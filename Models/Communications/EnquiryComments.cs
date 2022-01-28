using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Communications
{
    public class EnquiryComments
    {
        [Key]
        [Required]
        [DisplayName("Comment ID")]
        public int Id { get; set; }

        [Required]
        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime effectiveDate { get; set; }

        [Required]
        [DisplayName("Enquiry #")]
        public int queryId { get; set; }

        //[Required] //HCare-648
        [DisplayName("Comment")]
        public string comment { get; set; }

        [Required]
        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [Required]
        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [DisplayName("Reference #")]
        public string ReferenceNumber { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; }
    }
}