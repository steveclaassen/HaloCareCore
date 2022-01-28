using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class MedicalAidPrograms
    {
        [Key]
        public int id { get; set; }

        [DisplayName("Program name")]
        public string codeType { get; set; }

        public string medicalAidId { get; set; }

        [DisplayName("Program code")]
        public string program { get; set; }

        [DisplayName("Created by")]
        public string createdBy { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }

        [DisplayName("Referral from")]
        public string referralFrom { get; set; }
    }
}