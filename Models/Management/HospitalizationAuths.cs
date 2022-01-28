using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Management
{
    public class HospitalizationAuths
    {
        [Key]
        public int id { get; set; }

        public Guid? questionnaireID { get; set; }

        [DisplayName("Member #")]
        public string membershipNo { get; set; }

        [DisplayName("Dependant code")]
        public string dependantCode { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? createdDate { get; set; }

        [DisplayName("Approval #")]
        public string approvalNo { get; set; }

        [DisplayName("Auth status")]
        public string authStatus { get; set; }

        [DisplayName("Req vendor")]
        public string requestingVendor { get; set; }

        [DisplayName("Req provider")]
        public string requestingProvider { get; set; }

        [DisplayName("Vendor")]
        public string vendor { get; set; }

        [DisplayName("Provider")]
        public string provider { get; set; }

        [DisplayName("Planned date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? plannedDate { get; set; }

        public int bedDays { get; set; }

        [DisplayName("Discharged date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? dischargeDate { get; set; }

        [DisplayName("Procedure date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? procedureDate { get; set; }

        [DisplayName("Auth amount")]
        public string authAmount { get; set; }

        [DisplayName("Auth pending")]
        public string authPendingDenyReason { get; set; }

        [DisplayName("Vendor reason")]
        public string vendorReference { get; set; }

        [DisplayName("Administered date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? actualAdminDate { get; set; }

        [DisplayName("Actual discharge")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? actualDischargeDate { get; set; }

        [DisplayName("Emergency")]
        public string emergency { get; set; }

        [DisplayName("Epi auth")]
        public string epiAuth { get; set; }

        [DisplayName("Case closed")]
        public string caseClosed { get; set; }

        [DisplayName("Closed date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? closedDate { get; set; }

        [DisplayName("Auth type")]
        public string authType { get; set; }

        [DisplayName("Closed reason")]
        public string closedReason { get; set; }

        [DisplayName("Client ID")]
        public string clientId { get; set; }

        [DisplayName("Scheme option")]
        public string benefitPlan { get; set; }

        [DisplayName("Medical scheme")]
        public string medicalAid { get; set; }

        [DisplayName("Hospitalised")]
        public bool hasBeenHospitalised { get; set; }

        [DisplayName("Comment")]
        public string generalComments { get; set; }

        [DisplayName("Program")]
        public string programType { get; set; }

        [DisplayName("Follow up")]
        public bool followUp { get; set; }

        [DisplayName("Questionnaire ID")]
        public int? questionnaireID_old { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }

    }
}