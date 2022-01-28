using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class CaseManagerHistoryView
    {
        [DisplayName("Case manager #")]
        public string caseManagerNo { get; set; }

        [DisplayName("Name")]
        public string caseManagerName { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime effectiveDate { get; set; }

        [DisplayName("Created by")]
        public string createdBy { get; set; }
    }
}