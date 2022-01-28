using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class wfUserVM
    {
        public int Id { get; set; }

        public Guid? UserID { get; set; }
        public Guid DependantID { get; set; }
        public Guid QueueID { get; set; }
        public Guid MedicalAidID { get; set; }
        public Guid ProgramID { get; set; }

        [DisplayName("Scheme")]
        public string MedicalScheme { get; set; }

        [DisplayName("Program")]
        public string Program { get; set; }

        [DisplayName("Management status")]
        public string ManagementStatus { get; set; }

        [DisplayName("Management status code")]
        public string ManagementStatusCode { get; set; } //hcare-1315

        [DisplayName("Membership #")]
        public string MembershipNumber { get; set; }

        [DisplayName("Dependant code")]
        public string DependantCode { get; set; }

        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }

        [DisplayName("Case manager")]
        public string UserFullName
        {
            get
            {
                return UserFirstName.ToUpper().Substring(0, 1) + UserFirstName.ToLower().Substring(1) + " " + UserLastName.ToUpper().Substring(0, 1) + UserLastName.ToLower().Substring(1);
            }
        }

        [DisplayName("Title")]
        public string Title { get; set; }

        [DisplayName("Risk rating")]
        public string RiskRating { get; set; }

        [DisplayName("Assignment")]
        public string AssignmentItemType { get; set; }

        [DisplayName("Instruction")]
        public string Instruction { get; set; }

        [DisplayName("Instruction")]
        public string Status { get; set; }

        public bool Open { get; set; }
        public bool InProgress { get; set; }
        public bool Closed { get; set; }

        [DisplayName("Queue name")]
        public string QueueName { get; set; }

    }
}
