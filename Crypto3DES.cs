//====This class added for encrypt QUERY STRING parameter to pass with API 3rd Party Application Launch
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace ILMS
{
    public class Crypto3DES
    {
        private System.Text.Encoding _encoding;
        private string _key;
        public Crypto3DES(string key )
        {
            _key = key;
        }

        //public string Key
        //{
        //    get
        //    {
        //        return _key;
        //    }
        //}


        public System.Text.Encoding Encoding
        {
            get
            {
                if (_encoding == null)
                {
                    _encoding = System.Text.Encoding.UTF8;
                }
                return _encoding;
            }

            set
            {
                _encoding = value;
            }
        }

        public string GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }

        public string Encrypt3DES(string strString)
        {
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();

            DES.Key = Encoding.GetBytes(_key);
            DES.Mode = CipherMode.ECB;
            DES.Padding = PaddingMode.Zeros;

            ICryptoTransform DESEncrypt = DES.CreateEncryptor();

            byte[] Buffer = Encoding.GetBytes(strString);

            return Convert.ToBase64String(DESEncrypt.TransformFinalBlock(Buffer, 0, Buffer.Length));
        }


        public string Decrypt3DES(string strString)
        {
            try
            {
                DESCryptoServiceProvider DES = new DESCryptoServiceProvider();

                DES.Key = Encoding.UTF8.GetBytes(_key);
                DES.Mode = CipherMode.ECB;
                DES.Padding = PaddingMode.Zeros;
                ICryptoTransform DESDecrypt = DES.CreateDecryptor();

                byte[] Buffer = Convert.FromBase64String(strString);
                return UTF8Encoding.UTF8.GetString(DESDecrypt.TransformFinalBlock(Buffer, 0, Buffer.Length));
            }
            catch (ArgumentException oAE)
            {
                return "";
            }
            
            }

    }
}
