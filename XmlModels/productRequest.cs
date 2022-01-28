using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HaloCareCore.XmlModels
{
    public class productRequest
    {
        [JsonProperty("productRequest")]
        public productDetails productDetails { get; set; }
    }

    public class productDetails
    {
        [JsonProperty("product")]
        public string productNo { get; set; }//Nappiecode
       
    }
}