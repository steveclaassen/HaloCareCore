using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Patient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class pathologyAttachment
    {
        public Pathology pathology { get; set; }
        public Attachments attachment { get; set; }
    }
}