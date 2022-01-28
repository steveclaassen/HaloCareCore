
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Admin
{
    public class WorkflowUser
    {
        public int Id { get; set; }
        public Guid UserID { get; set; }
        
        [DisplayName("Scheme")]
        public string MedicalScheme { get; set; }
        
        [DisplayName("Program")]
        public string Program { get; set; }
        
        [DisplayName("Member #")]
        public string MembershipNumber { get; set; }

        [DisplayName("Member #")]
        public Guid DependantID { get; set; }

        [DisplayName("Dependant code")]
        public string DependantCode { get; set; }

        [DisplayName("Title")]
        public string Title { get; set; }

        [DisplayName("First name")]
        public string FirstName { get; set; }
        
        [DisplayName("Last name")]
        public string LastName { get; set; }

        [DisplayName("Member name")]
        public string MemberName
        {
            get
            {
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(FirstName.ToUpper().Substring(0, 1) + FirstName.ToLower().Substring(1)) + " " + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(LastName.ToUpper().Substring(0, 1) + LastName.ToLower().Substring(1));
            }

        }

        [DisplayName("Management status")]
        public string ManagementStatus { get; set; }
        
        [DisplayName("Risk rating")]
        public string RiskRating { get; set; }

        [DisplayName("Instruction")]
        public string Instruction { get; set; }

        [DisplayName("Open")]
        public bool Open { get; set; }

        [DisplayName("In progress")]
        public bool InProgress { get; set; }
        
        [DisplayName("Closed")]
        public bool Closed { get; set; }

    }
}
