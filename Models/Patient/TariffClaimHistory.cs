using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Patient
{
    public class TariffClaimHistory
    {
        [Key]
        public int Id { set; get; }

        [Required]
        public Guid DependentId  { set; get; } 

        [Required]
        [DisplayName("Tariff")]
        public string Tariff { set; get; }
        [Required]
        [DisplayName("Tariff description")]
        public string TariffDescription { set; get; }
        [Required]
        [DisplayName("Provider pr#")]
        public string Provider { set; get; }
        [Required]
        [DisplayName("Provider description")]
        public string ProviderDescription { set; get; }
        [Required]
        [DisplayName("Date of claim")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DateOfClaim { get; set; }

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

        [Required]
        public Guid programID { get; set; }

    }
}