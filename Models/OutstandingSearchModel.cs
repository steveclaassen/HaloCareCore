using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class OutstandingSearchModel
    {
        [DisplayName("Medical scheme")]
        public string medicalAidName { get; set; }

        [DisplayName("Status code")]
        public string statusCode { get; set; }

        [DisplayName("Expiry date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? expiryDate { get; set; }

        [DisplayName("Months")]
        public int nrMonths { get; set; }
    }
}