using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Admin
{
    public class AccountAttachmentTemplates
    {
        [Key]
        public Guid Id { get; set; }
        public Guid AccountID { get; set; }
        public Guid SchemeID { get; set; }
        public Guid ProgramID { get; set; }
        public Guid TemplateID { get; set; }

        [DisplayName("Created by")]
        public string CreatedBy { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [DisplayName("Created date")]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Modified by")]
        public string ModifiedBy { get; set; }

        [DisplayName("Modified date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ModifiedDate { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }
    }
}
