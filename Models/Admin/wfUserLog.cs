using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Admin
{
    public class wfUserLog
    {
        [Key]
        public int Id { get; set; }
        public Guid QueueID { get; set; }
        public Guid UserID { get; set; }
        public Guid SchemeID { get; set; }
        public Guid ProgramID { get; set; }
        public Guid DependantID { get; set; }

        [DisplayName("Member #")]
        public string MembershipNumber { get; set; }

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

        [DisplayName("Status")]
        public string Status { get; set; }

        [DisplayName("Closed date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ModifiedDate { get; set; }

        [DisplayName("Closed by")]
        public string ModifiedBy { get; set; }

        [DisplayName("Comment")]
        public string Comment { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }
    }
}
