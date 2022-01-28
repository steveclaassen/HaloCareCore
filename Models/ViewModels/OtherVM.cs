using HaloCareCore.Models.Admin;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Script;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class OtherVM
    {
        public List<AttachmentTypes> AttachmentTypes { get; set; }
        public List<Laboratory> Laboratories { get; set; }
        public List<Log> Logs { get; set; }
        public List<NonCLDFlags> NonCLDFlags { get; set; }
        public List<PathologyTypes> PathologyTypes { get; set; }
        public List<PreferredMethodOfContact> PreferredMethodOfContacts { get; set; }
        public List<Icd10Codes> Icd10Codes { get; set; }
        public List<InsulinDependance> InsulinDependances { get; set; }
    }
}
