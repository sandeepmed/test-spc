using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using OpenQA.Selenium;
using ILMS.ApplicationSettingsManager;

namespace ILMS
{
    class clsIlmsConnectSharedKey
    {
        private IWebDriver iWebdriver;
        private string _ecomTransactionKey = string.Empty;
        private string _ecomLoginKey = string.Empty;
        private string _decriptedKey = string.Empty;
        private string _Encriptedstring = string.Empty;

        public clsIlmsConnectSharedKey(IWebDriver iWeb)
        {
            this.iWebdriver = iWeb;
        }
        public bool GetIlmsConnectSharedKey(object[] lStrvalue)
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

                }

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", clsAPI.orgID), clData, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);
                //  clsAPI.orgID = htblTestData["OrgID"].ToString();
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.apiILMSConnect.Replace("$", clsAPI.orgID), clData, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);
                if (htblTestData["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                {
                    if (result.Contains("SharedKey") && result.Contains("BaseURL"))
                    {
                        var oSharedKey = JsonConvert.DeserializeObject<IlmsConnectSharedKey>(result);
                        string URL = oSharedKey.BaseURL;
                        _decriptedKey = _des.Decrypt3DES(oSharedKey.SharedKey);
                        clsAPI.ilmsconnectFormat = clsAPI.ilmsconnectFormat.Replace("{OrgID}", clsAPI.orgID).Replace("{Key}", _decriptedKey).Replace("{UserID}", clsAPI.userid);

                        _Encriptedstring = _des.Encrypt3DES(clsAPI.ilmsconnectFormat);
                        iWebdriver.Navigate().GoToUrl(URL + "?enc=" + _Encriptedstring);
                        try
                        {
                            iWebdriver.SwitchTo().Frame(clsPageObject.strHeaderFrameID);
                            string strActualResult = iWebdriver.FindElement(clsPageObject.lvLearnerLogout).Text;
                            StringAssert.AreEqualIgnoringCase("Logout", strActualResult);
                            _Flag = true;
                        }
                        catch (Exception exc)
                        {
                            clsGlobalVariable.strExceptionReport = exc.Message.ToString();
                            _Flag = false;
                        }


                        _Flag = true;
                    }
                }
                else
                {
                    if (!result.Contains("SharedKey") && !result.Contains("BaseURL"))
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
