using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.Admin
{
    public class EmailAttachmentHistory
    {
        [Key]
        public Guid Id { get; set; }

        public Guid TemplateID { get; set; }
        public Guid DependantID { get; set; }
        public Guid ProgramID { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool Active { get; set; }

        public int EmailID { get; set; }
        public bool Sent { get; set; }
    }
}
