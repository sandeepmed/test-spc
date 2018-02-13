using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenQA.Selenium;
using ILMS.ApplicationSettingsManager;

namespace ILMS
{
    class ClsResetPasswordUrl
    {
        private IWebDriver iWebdriver;
        private string _ecomTransactionKey = string.Empty;
        private string _ecomLoginKey = string.Empty;



        public ClsResetPasswordUrl(IWebDriver iWeb)
        {
            this.iWebdriver = iWeb;
        }
        public bool GetResetPasswordUrl(object[] lStrvalue)
        {
            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            Hashtable htblTestData = new Hashtable();
            htblTestData = oGeneric.GetTestData(lStrvalue);
            bool _Flag = false;

            clsCoursePushDown clData = new clsCoursePushDown();

            GetAPICredentials oGetAPICredentials = new GetAPICredentials();
            try
            {
                if (htblTestData["OrgID"].ToString() != "")
                {
                    clsAPI.orgID = htblTestData["OrgID"].ToString();
                    clsAPI.userid = htblTestData["UserID"].ToString();
                }


                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", clsAPI.orgID), clData, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);
                // clsAPI.orgID = htblTestData["OrgID"].ToString();
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.apiResetPasswordURL.Replace("$", clsAPI.orgID).Replace("#", clsAPI.userid), clData, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);

                if (htblTestData["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                {
                    if (result.Contains("Url"))
                    {

                        ResetPasswordURL oResetPasswordURL = new ResetPasswordURL();
                        var oResetPassword = JsonConvert.DeserializeObject<ResetPasswordURL>(result);
                        iWebdriver.Navigate().GoToUrl(oResetPassword.URL);
                        _Flag = true;
                    }
                }
                else
                {
                    if (!result.Contains("Url"))
                    {
                        _Flag = true;
                    }
                }

                return _Flag;
            }
            catch (Exception e)
            {
                clsException.ExceptionHandler(e, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

                return false;
            }
        }
    }
}
