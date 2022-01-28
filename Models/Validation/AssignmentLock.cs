using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models.Validation
{
    public class AssignmentLock
    {
        public int Id { set; get; }
        public int AssignmentID { set; get; }
        public Guid UserID { set; get; }
        public DateTime createdDate { set; get; }
    }
}