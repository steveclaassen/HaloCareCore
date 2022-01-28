using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class UnAuthsScripts
    {
        public Guid dependantID { get; set; }

        [DisplayName("Script ID")]
        public int scriptId { get; set; }

        [DisplayName("Member #")]
        public string membershipNo { get; set; }

        [DisplayName("Dep")]
        public string dependantCode { get; set; }

        [DisplayName("Scheme")]
        public string schemeName { get; set; }

        [DisplayName("Prescription date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime effectiveDate { get; set; }

        [DisplayName("Status")]
        public string status { get; set; }

        [DisplayName("Period unauthorised")]
        public string overdue { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [DisplayName("Member status")]
        public string memStatus { get; set; }
    }
}