using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Management
{
    public class TariffCode
    {
        [Key]
        public int Id { set; get; }

        [Required]
        [DisplayName("Tariff billing code")]
        public string TariffBillingCode { set; get; }
        [Required]
        [DisplayName("Tariff description")]
        public string TariffDescription { set; get; }
        [Required]
        [DisplayName("Assignment")]
        public string Assignment { set; get; }
        [Required]
        [DisplayName("Pathology")]
        public string Pathology { set; get; }
        [Required]
        [DisplayName("Program")]
        public string programType { get; set; }
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
        public DateTime? modifieddate { get; set; }


        [DisplayName("Active")]
        public bool active { get; set; }
    }
}