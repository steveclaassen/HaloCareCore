using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class ProgramSubStatus
    {
        public Guid Id { get; set; }
        public Guid ProgramId { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
    }
}