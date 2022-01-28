using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.Models
{
    public class OutstandingsMultiView
    {
        public List<OutstandingsView> outstandings { get; set;}
        public OutstandingSearchModel search { get; set; }
    }
}