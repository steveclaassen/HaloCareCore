using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class EnquirySearch
    {
        public Guid DependantID { get; set; }
        public Guid SchemeID { get; set; }
        public Guid? ProgramID { get; set; }
        public Guid? UserID { get; set; }

        [DisplayName("Enquiry ID")]
        public string EnquiryID { get; set; }

        [DisplayName("Scheme")]
        public string MedicalScheme { get; set; }

        [DisplayName("Program")]
        public string Program { get; set; }

        [DisplayName("Date from")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DateFrom { get; set; }

        [DisplayName("Date to")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DateTo { get; set; }

        [DisplayName("ID/Auth #")]
        public string MemberID { get; set; }

        [DisplayName("Member number")]
        public string MemberNumber { get; set; }

        [DisplayName("Dep code")]
        public string DependantCode { get; set; }

        [DisplayName("Member name")]
        public string MemberName { get; set; }

        [DisplayName("Management status")]
        public string ManagementStatus { get; set; }

        [DisplayName("Enquiry source")]
        public string EnquirySource { get; set; }

        [DisplayName("Enquiry by")]
        public string EnquiryBy { get; set; }

        [DisplayName("Enquiry type")]
        public string EnquiryType { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EnquiryEffectiveDate { get; set; }

        [DisplayName("Enquiry priority")]
        public string EnquiryPriority { get; set; }

        [DisplayName("Enquiry details")]
        public string EnquiryDetails { get; set; }

        [DisplayName("Enquiry status")]
        public string EnquiryStatus { get; set; }

        [DisplayName("Enquiry owner")]
        public string EnquiryOwner { get; set; }

        [DisplayName("Enquiry comment")]
        public string EnquiryComment { get; set; }

        [DisplayName("Enquiry reference")]
        public string EnquiryReference { get; set; }

        [DisplayName("Program code")]
        public string ProgramCode { get; set; }

        [DisplayName("Created by")]
        public string CreatedBy { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CreatedDate { get; set; }

        [DisplayName("Modified by")]
        public string ModifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ModifiedDate { get; set; }

        [DisplayName("Resolved by")]
        public string ResolvedBy { get; set; }

        [DisplayName("Resolved date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ResolvedDate { get; set; }

        [DisplayName("Follow up")]
        public bool FollowUp { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }

    }
}
