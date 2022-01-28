using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Admin
{
    public class wfQueue
    {
        [Key]
        public int Id { get; set; }
        public Guid QueueID { get; set; }
        public Guid? UserID { get; set; }
        public Guid SchemeID { get; set; }
        public Guid ProgramID { get; set; }
        public Guid DependantID { get; set; }

        [DisplayName("Created by")]
        public string CreatedBy { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CreatedDate { get; set; }

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

        [DisplayName("Status")]
        public string Status { get; set; }


    }
}
