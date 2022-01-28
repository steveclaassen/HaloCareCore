using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Service
{
    public class ServiceResult
    {
        public ServiceResult()
        {
            this.Errors = new List<ServiceError>();
        }

        public bool Success { get; set; }

        public List<ServiceError> Errors { get; set; }

        public Object ReturnValue { get; set; }

        public Dictionary<String, Object> ReturnValues { get; set; }
    }
}