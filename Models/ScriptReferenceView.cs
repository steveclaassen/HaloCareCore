using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class ScriptReferenceView
    {
        public ScriptReferences script { get; set; }
        public List<Attachments> attachments { get; set; }
    }
}