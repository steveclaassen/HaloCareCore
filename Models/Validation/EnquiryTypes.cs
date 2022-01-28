using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class EnquiryTypes
    {
        public int id { get; set; }

        [DisplayName("Enquiry type")]
        public string enquiryType { get; set; }

        [DisplayName("Enquiry")]
        public string enquiryName { get; set; }

        [DisplayName("Source")]
        public string source { get; set; }

        [DisplayName("Active")]
        public bool active { get; set; } 
    }
}