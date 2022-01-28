using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Admin
{
    public class UserRoleWorkflowRights
    {
        [Key]
        public int Id {get;set;}
        public Guid roleId { get; set; }
        public string assignmentTypeId { get; set; }
        public bool active { get; set; }
        public string createdBy { get; set; }
        public DateTime createdDate { get; set; }
        public string modifiedBy { get; set; }
        public DateTime? modifiedDate { get; set; }
    }
}