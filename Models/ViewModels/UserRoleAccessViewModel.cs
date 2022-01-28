using HaloCareCore.Models.Admin;
using HaloCareCore.Models.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class UserRoleAccessViewModel
    {
        public Guid roleId { get; set; }
        [DisplayName("Role")]
        public string role { get; set; }
        [DisplayName("Administrator")]
        public bool admin { get; set; }
        [DisplayName("Workflow Access")]
        public List<WorkflowViewModel> workflows { get; set; }
        public string[] selectedWorkFlows { get; set; }
        [DisplayName("Settings Access")]
        public bool settingsRights { get; set; }

        public List<AccessFunctions> roleViewss { set; get; }
        public List<Functions> functions { set; get; }
    }
}