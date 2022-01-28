using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Management
{
    public class HospitalizationClaims
    {
        [Key]
        public int id { get; set; }

        [DisplayName("Member #")]
        public string membershipNo { get; set; }

        [DisplayName("Dep code")]
        public string dependantCode { get; set; }

        [Required]
        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime createdDate { get; set; }

        [DisplayName("Claim #")]
        public string claimNo { get; set; }

        [DisplayName("Vendor #")]
        public string vendorReference { get; set; }

        [DisplayName("Auth #")]
        public string authNo { get; set; }

        [DisplayName("Claim status")]
        public string claimStatus { get; set; }

        [DisplayName("Req vendor")]
        public string requestingVendor { get; set; }

        [DisplayName("Req provider")]
        public string requestingProvider { get; set; }

        [DisplayName("Vendor")]
        public string vendor { get; set; }

        [DisplayName("Provider")]
        public string provider { get; set; }

        [DisplayName("Claim created")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? claimCreatedDate { get; set; }

        [DisplayName("Claim updated")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? claimUpdatedDate { get; set; }

        [DisplayName("Claim total")]
        public string claimTotal { get; set; }

        [DisplayName("Payment total")]
        public string payTotal { get; set; }

        [DisplayName("Admin")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? adminDate { get; set; }

        [DisplayName("Discharged")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? dischargeDate { get; set; }

        [DisplayName("Co-Payment")]
        public string coPayment { get; set; }

        [DisplayName("Min benefit code")]
        public string minBenefitCode { get; set; }

        [DisplayName("Batch #")]
        public string batchNo { get; set; }

        [DisplayName("Received")]
        public string receiveDate { get; set; }

        [DisplayName("Data source")]
        public string datasource { get; set; }
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? calculatedAdminDate { get; set; }

        [DisplayName("Calc discharge date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? calculatedDischargeDate { get; set; }

        [DisplayName("Medical scheme")]
        public string medicalAid { get; set; }

    }
}