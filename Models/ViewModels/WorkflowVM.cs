using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class WorkflowVM
    {
        public int Id { get; set; }
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
        [DisplayName("Scheme")]
        public string MedicalScheme { get; set; }

        [DisplayName("Program")]
        public string Program { get; set; }

        [DisplayName("Management status")]
        public string ManagementStatus { get; set; }

        [DisplayName("From date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? FromDate { get; set; }

        [DisplayName("To date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ToDate { get; set; }

        [DisplayName("Risk rating")]
        public string RiskRating { get; set; }

        [DisplayName("Assignment")]
        public string AssignmentItemType { get; set; }

        [DisplayName("Instruction")]
        public string Instruction { get; set; }

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
        public bool Active { get; set; }
    }
}
