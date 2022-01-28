using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class BulkEmailResultsVM
    {
        public Guid DependantID { get; set; }
        public Guid SchemeID { get; set; }
        public Guid SchemeOptionID { get; set; }
        public Guid ProgramID { get; set; }

        [DisplayName("Scheme")]
        public string SchemeName { get; set; }

        [DisplayName("Scheme option")]
        public string SchemeOption { get; set; }

        [DisplayName("Program")]
        public string Program { get; set; }

        [DisplayName("Member number")]
        public string MembershipNumber { get; set; }

        [DisplayName("Dep code")]
        public string DependantCode { get; set; }

        [DisplayName("Patient status code")]
        public string PatientStatusCode { get; set; }

        [DisplayName("Patient status")]
        public string PatientStatus { get; set; }

        [DisplayName("ID/Auth #")]
        public string MemberID { get; set; }

        [DisplayName("Member name")]
        public string MemberName { get; set; }

        [DisplayName("Management status code")]
        public string ManagementStatusCode { get; set; }

        [DisplayName("Management status")]
        public string ManagementStatus { get; set; }

        [DisplayName("MS start date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ManagementStatusStartDate { get; set; }

        [DisplayName("MS end date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ManagementStatusEndDate { get; set; }

        public string EmailAddress1 { get; set; }
        public string EmailAddress2 { get; set; }

        public bool EmailOptOut { get; set; }

    }
}
