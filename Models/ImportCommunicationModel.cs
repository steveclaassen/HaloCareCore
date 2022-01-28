using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class ImportCommunicationModel
    {
        public Guid DepedentID { set; get; }
        public string Scheme { set; get; }
        public string MemberNumber { set; get; }
        public string Depcode { set; get; }
        public string ProfileNumber { set; get; }
        public string Type { set; get; }
        public string Description { set; get; }
        public string Details { set; get; }
        public DateTime DateSent { set; get; }
        public string Createdby { set; get; }
        public string Program { set; get; }
        public Guid ProgramId { set; get; }
        public string messageTo {get;set;}
        public string NoteType { set; get; }

    }
}