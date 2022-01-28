using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Admin
{
    public class WorkflowSettings
    {
        [Key]
        public int Id { get; set; }
        public Guid MedicalAidID { get; set; }
        public Guid ProgramID { get; set; }
        public Guid UserID { get; set; }

        [DisplayName("Management status")]
        public string ManagementStatus { get; set; }
        
        [DisplayName("Instruction")]
        public string Instruction { get; set; }

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
        public string Assignment { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CreatedDate { get; set; }

        [DisplayName("Created by")]
        public string CreatedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ModifiedDate { get; set; }

        [DisplayName("Modified by")]
        public string ModifiedBy { get; set; }
       
        public bool Active { get; set; }
    }
}
