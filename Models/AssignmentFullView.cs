using HaloCareCore.Models.Admin;
using HaloCareCore.Models.Communications;
using HaloCareCore.Models.Patient;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class AssignmentFullView
    {
        public Assignments assignment { get; set; }

        public List<AssignmentItemView> items { get; set; }
        public Attachments attachment { get; set; }

        public List<TaskViewModel> tasks { get; set; }

        public DisclaimerQuestions AcknowledgementQuestion { get; set; }
        public List<DisclaimerQuestions> AcknowledgementQuestions { get; set; }
        public Programs programs { set; get; }

        public byte[]  attachmentString { get; set; }

    }
}