using Newtonsoft.Json;
using HaloCareCore.XmlModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace HaloCareCore.Management
{
    public class ProductValidationService
    {
        public static productResponse POSTREQ(string url, string jsonContent)
        {
            System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";

            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            Byte[] byteArray = encoding.GetBytes(jsonContent);

            request.ContentLength = byteArray.Length;
            request.Accept = @"*/*";
            request.ContentType = @"application/json";
            request.Headers.Add("apikey", "l7360ccd017d27435ea0f8c1d2fd88d5e1");
            

            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                        var ress = reader.ReadToEnd();
                        productResponse jsonResponse = (productResponse)JsonConvert.DeserializeObject(ress, typeof(productResponse));

                        return jsonResponse;
                    }
                }
            }
            catch (WebException ex)
            {
                var except = ex.Message;
                return null;
            }
        }

    }
}