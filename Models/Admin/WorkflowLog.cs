using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Admin
{
    public class WorkflowLog
    {
        public int Id { get; set; }
        public Guid UserID { get; set; }
        public string CaseManager { get; set; }

        [DisplayName("Scheme")]
        public string MedicalScheme { get; set; }

        [DisplayName("Program")]
        public string Program { get; set; }

        [DisplayName("Member #")]
        public string MembershipNumber { get; set; }

        [DisplayName("Dependant ID")]
        public Guid DependantID { get; set; }

        [DisplayName("Dependant code")]
        public string DependantCode { get; set; }

        [DisplayName("Title")]
        public string Title { get; set; }

        [DisplayName("First name")]
        public string FirstName { get; set; }

        [DisplayName("Last name")]
        public string LastName { get; set; }

        [DisplayName("Management status")]
        public string ManagementStatus { get; set; }

        [DisplayName("Risk rating")]
        public string RiskRating { get; set; }

        [DisplayName("Assignment")]
        public string Assignment { get; set; }

        [DisplayName("Instruction")]
        public string Instruction { get; set; }

        [DisplayName("In progress")]
        public bool InProgress { get; set; }

        [DisplayName("In progress date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? InProgressDate { get; set; }

        [DisplayName("In progress by")]
        public string InProgressBy { get; set; }

        [DisplayName("Completed")]
        public bool Closed { get; set; }

        [DisplayName("Closed date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ClosedDate { get; set; }

        [DisplayName("Closed by")]
        public string ClosedBy { get; set; }

        [DisplayName("Comment")]
        public string Comment { get; set; }

        public bool Active { get; set; }

    }
}
