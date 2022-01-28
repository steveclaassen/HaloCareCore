using HaloCareCore.Models.Communications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class EnquiryFull
    {
        public List<EnquiryFullViewModel> enquiryList { get; set; }
        public EnquiryComments enquiryComment { get; set; }

        public Guid DependantID { get; set; }

    }
}