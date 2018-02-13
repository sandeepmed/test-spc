using ILMS.ApplicationSettingsManager;
using System;
using System.Reflection;
using System.Web;
using System.Web.UI;



namespace ILMS
{
    public partial class DESEnc 
    {

        #region Fields and Properties

        private string _ecomKey = string.Empty;
        private string _ecomLoginKey = string.Empty;
        private Crypto3DES _des;

        #endregion

        #region Events


        //protected void DecryptQueryStringData_Clicked(object sender, EventArgs e)
        //{
        //    if (!string.IsNullOrEmpty(txtQueryString.Text))
        //    {
        //        string encryptedValue = txtQueryString.Text.Trim();

        //        lblDecryptedQueryStringData.Text = Decrypt(encryptedValue);
        //    }
        //}

        //protected void EncryptStringData_Clicked(object sender, EventArgs e)
        //{
        //    if (!string.IsNullOrEmpty(txtEncryptString.Text))
        //    {
        //        string decryptedValue = txtEncryptString.Text.Trim();

        //        lblEncryptString.Text = Encrypt(decryptedValue);
        //    }
        //}

        //protected void Page_Load(object sender, EventArgs e)
        //{
        //    _ecomKey = ApplicationSettings.EComModuleEncKey().ToString();
        //    _ecomLoginKey = ApplicationSettings.EComModuleLoginKey().ToString();

        //    _des = new Crypto3DES(_ecomKey);

        //    lblEcomModKeyValue.Text = GetEcomModKey();
        //}

        protected void Page_PreInit(object sender, EventArgs e)
        {
           // Page.Theme = Themes.AdminPageTheme();
        }

        #endregion Events
        
        #region Private Events

        /// <summary>
        ///     Decrypt Data
        /// </summary>
        /// <param name="plainText">Plain Text</param>
        /// <returns>string</returns>
        private string Encrypt(string plainText)
        {
            return _des.Encrypt3DES(plainText);
        }

        /// <summary>
        ///     Decrypt Data
        /// </summary>
        /// <param name="cipherText">Cipher Text</param>
        /// <returns>string</returns>
        public string Decrypt(string cipherText)
        {
            return _des.Decrypt3DES(cipherText);
        }


        private string GetEcomModKey()
        {
            string keyToEncrypt = "timestamp="+DateTime.UtcNow.Ticks.ToString()+"|key="+ApplicationSettings.EComModuleLoginKey();

            return keyToEncrypt + " >>>> " + Encrypt(keyToEncrypt);
        }

        #endregion
    }
}