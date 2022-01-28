using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class AttachmentTypeViewModel
    {
        public IEnumerable<AssignmentItemTypes> itemTypes { set; get; }
        public AttachmentTypes attachmentTypes { set; get; }
    }
}