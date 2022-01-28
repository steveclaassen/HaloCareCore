using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Management
{
    public class EmployerGroupMaster
    {

        [Key]
        public Guid employerID { get; set; }

        [DisplayName("Carrier")]
        public string carrier { get; set; }

        [DisplayName("Employer Code")]
        public string employerCode { get; set; }

        [DisplayName("Employer Description")]
        public string employerDescription { get; set; }
    }
}