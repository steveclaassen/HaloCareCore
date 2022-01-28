using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace HaloCareCore.Helpers
{
    public static class ValidationHelpers
    {

        public static bool HasFile(this IFormFile file)
        {
            return (file != null && file.Length > 0) ? true : false;
        }

        // This method assumes that the 13-digit id number has 
        // valid digits in position 0 through 12.  
        // Stored in a property 'ParseIdString'.  
        // Returns: the valid digit between 0 and 9, or  
        // -1 if the method fails.
        public static int GetControlDigit(string ParsedIdString)
        {
            int d = -1;
            try
            {
                int a = 0;
                for (int i = 0; i < 6; i++)
                {
                    a += int.Parse(ParsedIdString[2 * i].ToString());
                }
                int b = 0;
                for (int i = 0; i < 6; i++)
                {
                    b = b * 10 + int.Parse(ParsedIdString[2 * i + 1].ToString());
                }
                b *= 2;
                int c = 0;
                do
                {
                    c += b % 10;
                    b = b / 10;
                }
                while (b > 0);
                c += a;
                d = 10 - (c % 10);
                if (d == 10) d = 0;
            }
            catch
            {
                /*Do Nothing**/
            }
            return d;
        }

        public static string GenerateUniqueCode(int script_no)
        {
            string uniqueCode = null;

            return uniqueCode;
        }

        public static string GetUniqueKey(int maxSize)
        {
            //char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            byte[] data = new byte[1];
            {
                RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }
            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        public static string ValidateXml(this string xml, string nameSpace, string xsdUri)
        {
            try
            {
                var xmlReaderSettings = new XmlReaderSettings();
                xmlReaderSettings.Schemas.Add(nameSpace, xsdUri);
                xmlReaderSettings.ValidationType = ValidationType.Schema;
                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xml);
                var reader = new XmlNodeReader(xmlDocument.FirstChild);
                var reader2 = XmlReader.Create(reader, xmlReaderSettings);
                var xmlDocument2 = new XmlDocument();
                xmlDocument2.Load(reader2);
                var validationEventHandler = new ValidationEventHandler(ValidationEventHandler);
                xmlDocument2.Validate(validationEventHandler);
                xmlDocument2.Validate(validationEventHandler);
            }
            catch (System.Exception ex)
            {
                return ex.Message;
            }
            return "The XML is valid according to the specified schema XSD file";
        }
        private static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    System.Console.WriteLine("Error: {0}", e.Message);
                    return;
                case XmlSeverityType.Warning:
                    System.Console.WriteLine("Warning {0}", e.Message);
                    return;
                default:
                    return;
            }
        }

    }

}
