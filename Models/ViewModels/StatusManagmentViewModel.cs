using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.ViewModels
{
    public class StatusManagmentViewModel
    {
        public Guid id { get; set; }
        public Guid accountId { get; set; }
        public Guid programId { get; set; }
        public Guid medicalAidId { get; set; }
        public Guid planId { get; set; }
        public bool Added { get; set; }

        [DisplayName("Name")]
        public string statusName { get; set; }

        [DisplayName("Status code")]
        public string statusCode { get; set; }


    }
}