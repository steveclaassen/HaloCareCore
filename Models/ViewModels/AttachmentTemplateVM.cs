using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class AttachmentTemplateVM
    {
        public Guid Id { get; set; }
        public Guid AccountID { get; set; }
        public Guid ProgramID { get; set; }
        public Guid MedicalAidID { get; set; }
        public Guid PlanID { get; set; }
        public Guid TemplateID { get; set; }
        
        public bool Added { get; set; }

        [DisplayName("Attachment name")]
        public string AttachmentName { get; set; }

        [DisplayName("Attachment file")]
        public string AttachmentFile { get; set; }
    }
}
