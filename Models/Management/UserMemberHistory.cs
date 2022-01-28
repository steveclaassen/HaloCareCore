using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Management
{
    public class UserMemberHistory
    {
        [Key]
        public int Id { get; set; }

        public Guid DependantID { get; set; }
        public Guid UserID { get; set; }

        [DisplayName("Assignment ID")]
        public int? AssignmentID { get; set; }

        [DisplayName("Task ID")]
        public int? TaskID { get; set; }

        [DisplayName("Case manager")]
        public string UserFullName { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EffectiveDate { get; set; }

        [DisplayName("Created by")]
        public string CreatedBy { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Modified by")]
        public string ModifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ModifiedDate { get; set; }

        [DisplayName("Program")]
        public string Program { get; set; }

        [DisplayName("Comment")]
        public string Comment { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }

        public List<Users> Users { get; set; }
    }
}
