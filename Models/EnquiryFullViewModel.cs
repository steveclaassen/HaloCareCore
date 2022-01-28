using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class EnquiryFullViewModel
    {
        public Guid dependantID { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime effectiveDate { get; set; }

        [DisplayName("Reference #")]
        public string enquiryRefNumber { get; set; }

        [DisplayName("Text information")]
        public string TextInformation { get; set; }

        [DisplayName("Enquiry by")]
        public string enquiryBy { get; set; }

        [DisplayName("Enquiry type")]
        public string enquiryTypeName { get; set; }

        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [DisplayName("Status")]
        public string status { get; set; }

        [DisplayName("Enquiry ID")]
        public int enquiryId { get; set; }

        [DisplayName("Owner")]
        public string owner { get; set; }

    }
}