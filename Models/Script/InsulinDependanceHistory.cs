using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Script
{
    public class InsulinDependanceHistory
    {
        [Key]
        public int Id { get; set; }
        public Guid DependantID { get; set; }
        public Guid ProgramID { get; set; }

        [DisplayName("Code")]
        public string Code { get; set; }

        [DisplayName("Descrption")]
        public string Description { get; set; }

        [DisplayName("T-Class")]
        public string TClass { get; set; }

        [DisplayName("Effective date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EffectiveDate { get; set; }

        [DisplayName("End date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? EndDate { get; set; }

        [DisplayName("Created date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Created by")]
        public string CreatedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ModifiedDate { get; set; }

        [DisplayName("Modified by")]
        public string ModifiedBy { get; set; }

        [DisplayName("Script ID")]
        public int? ScriptID { get; set; }

        [DisplayName("Script item ID")]
        public int? ScriptItemID { get; set; }

        [DisplayName("Script item status")]
        public string ScriptItemStatus { get; set; }

        [DisplayName("Comment")]
        public string Comment { get; set; }

        [DisplayName("Reason")]
        public string RecordInformation { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }


    }
}
