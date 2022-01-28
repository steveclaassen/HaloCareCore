using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class CommunicationLogResultsVM
    {
        public CommunicationLogVM CommunicationLogVM { get; set; }
        public List<CommunicationLogVM> communicationLogVMs { set; get; }
    }
}