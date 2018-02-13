using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System.Collections;
using ILMS.ApplicationSettingsManager;

namespace ILMS
{
    class clsAPIGetOrganization
    {
        private IWebDriver iWebdriver;
        private string _ecomTransactionKey = string.Empty;
        private string _ecomLoginKey = string.Empty;

        public clsAPIGetOrganization(IWebDriver iWeb)
        {
            this.iWebdriver = iWeb;
        }

        public bool GetOrganization(object[] lStrValue)
        {
            try
            {
                clsGlobalVariable.strExceptionReport = string.Empty;
                string statusCode = string.Empty;
                string result = string.Empty;
                clsGeneric oGeneric = new clsGeneric();
                string URI = string.Empty;
                Hashtable htblTestData = new Hashtable();
                htblTestData = oGeneric.GetTestData(lStrValue);
                bool _Flag = false;

                clsAPIOrganization clOrgData = new clsAPIOrganization();

                GetAPICredentials oGetAPICredentials = new GetAPICredentials();
                if (htblTestData["OrgID"].ToString() != "")
                {
                    clsAPI.orgID = htblTestData["OrgID"].ToString();
                }

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", clsAPI.orgID), clOrgData, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.apiOrg.Replace("$", clsAPI.orgID), clOrgData, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);

                if (htblTestData["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                {
                    if (result.Contains("ID"))
                    {
                        var oGetOrgID = JsonConvert.DeserializeObject<clsAPIOrganization>(result);
                        if (oGetOrgID.ID.Equals(clsAPI.orgID))
                        {
                            _Flag = true;
                        }
                    }
                }
                else
                {
                    if (!result.Contains("ID"))
                    {
                        _Flag = true;
                    }
                }

                return _Flag;
            }
            catch (Exception er)
            {
                clsException.ExceptionHandler(er, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
                return false;
            }
        }
    }
}
