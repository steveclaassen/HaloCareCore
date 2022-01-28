using HaloCareCore.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaloCareCore.Models.ViewModels
{
    public class WorkflowUserVM
    {
        public WorkflowUser WorkflowUser { get; set; }
        public List<WorkflowUser> WorkflowUsers { get; set; }

        public WorkflowLog WorkflowLog { get; set; }
        public List<WorkflowLog> WorkflowLogs { get; set; }

        public wfUserVM wfUser { get; set; }
        public List<wfUserVM> wfUsers { get; set; }
        
        public wfUserVM wfQueue { get; set; }
        public List<wfUserVM> wfQueues { get; set; }


    }
}
