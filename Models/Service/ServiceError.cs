using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Service
{
    public class ServiceError
    {
        [DisplayName("Message")]
        public string Message { get; set; }
    }
}