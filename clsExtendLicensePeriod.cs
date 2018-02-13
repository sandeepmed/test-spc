using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenQA.Selenium;
using ILMS.ApplicationSettingsManager;

namespace ILMS
{

    class clsExtendLicensePeriod
    {
        private IWebDriver iWebdriver;
        private string _ecomTransactionKey = string.Empty;
        private string _ecomLoginKey = string.Empty;


        public clsExtendLicensePeriod(IWebDriver iWeb)
        {
            this.iWebdriver = iWeb;
        }

        public bool APIExtendLicense(object[] lStrvalue)
        {
            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            Hashtable htblTestData = new Hashtable();
            htblTestData = oGeneric.GetTestData(lStrvalue);
            bool _Flag = false;

            string[] strUserId = htblTestData["UserID"].ToString().Split(',');
            string[] strCourseId = htblTestData["OrgCourseID"].ToString().Split(',');

            RootAPIExtendLicense oRootAPIExtendLicense = new RootAPIExtendLicense();
            try
            {


                List<APIExtendLicense> ol = new List<ILMS.APIExtendLicense>();
                foreach (string Course in strCourseId)
                {
                    APIExtendLicense oListAPIExtendLicense = new ILMS.APIExtendLicense();
                    List<string> olist = new List<string>();
                    foreach (string userid in strUserId)
                    {
                        olist.Add(userid);

                    }
                    oListAPIExtendLicense.LearnerIDs = olist;
                    oListAPIExtendLicense.OrgCourseID = Course;
                    ol.Add(oListAPIExtendLicense);

                }
                oRootAPIExtendLicense.CourseUserRecords = ol;

                GetAPICredentials oGetAPICredentials = new GetAPICredentials();


                Thread.Sleep(5000);
                do
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", htblTestData["OrgID"].ToString()), oRootAPIExtendLicense, "H2", "", "");

                    if (result.Contains("Login")) { break; }
                } while (result.Contains("Login"));
                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);
                clsAPI.orgID = htblTestData["OrgID"].ToString();
                oGeneric.GetApiResponseCodeData(out statusCode, out result, htblTestData["MethodType"].ToString(), ApplicationSettings.APIURI() + clsAPI.apiExtendLicense.Replace("$", htblTestData["OrgID"].ToString()).Replace("#", htblTestData["EcomID"].ToString()), oRootAPIExtendLicense, htblTestData["HeaderType"].ToString(), _ecomLoginKey, _ecomTransactionKey);

                if (htblTestData["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                {
                    if (result.Contains("") && !result.Contains("Error"))
                    {
                        _Flag = true;
                    }
                }
                else
                {
                    if (!result.Contains("Error"))
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
