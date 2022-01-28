using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class EmailLayoutVM
    {
        public Guid Id { get; set; }
        public Guid AccountID { get; set; }
        public Guid ProgramID { get; set; }
        public Guid MedicalAidID { get; set; }
        public Guid PlanID { get; set; }
        public Guid LayoutID { get; set; }

        public bool Added { get; set; }

        [DisplayName("Layout description")]
        public string LayoutDescription { get; set; }
        public string LayoutType { get; set; }
        public string LayoutTemplate { get; set; }
        public string AttachmentFile { get; set; }
    }
}
