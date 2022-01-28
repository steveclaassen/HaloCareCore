using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class EmployerMasterViewModel
    {
        public string EmployerDescription { get; set; }
        public DateTime effectiveDate { get; set; }
        public bool active { set; get; }
    }
}