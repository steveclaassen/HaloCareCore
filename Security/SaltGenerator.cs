using System.Security.Cryptography;
using System.Text;

namespace HaloCareCore.Security
{
    public class SaltGenerator
    {
        private static RNGCryptoServiceProvider m_cryptoServiceProvider = null;
        private const int SALT_SIZE = 12;

        static SaltGenerator()
        {
            m_cryptoServiceProvider = new RNGCryptoServiceProvider();
        }

        public static string GetSaltString()
        {
            // Lets create a byte array to store the salt bytes
            byte[] saltBytes = new byte[SALT_SIZE];

            // lets generate the salt in the byte array
            m_cryptoServiceProvider.GetNonZeroBytes(saltBytes);

            // Let us get some string representation for this salt
            string saltString = Encoding.UTF8.GetString(saltBytes);

            // Now we have our salt string ready lets return it to the caller
            return saltString;
        }
    }
}