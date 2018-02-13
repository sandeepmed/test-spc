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
    class clsCoursePushDownEnrollment
    {
        private IWebDriver iWebdriver;
        private string _ecomTransactionKey = string.Empty;
        private string _ecomLoginKey = string.Empty;
        public clsCoursePushDownEnrollment(IWebDriver iWeb)
        {
            this.iWebdriver = iWeb;
        }

        public bool APIEnrollments(object[] lStrvalue)
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

            try {

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", htblTestData["OrgID"].ToString()), clData, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);




                clsAPI.orgID = htblTestData["OrgID"].ToString();
                oGeneric.GetApiResponseCodeData(out statusCode, out result, htblTestData["MethodType"].ToString(), ApplicationSettings.APIURI() + clsAPI.apiEnrollment.Replace("&", htblTestData["OrgID"].ToString()).Replace("$", htblTestData["EcomID"].ToString()).Replace("#", htblTestData["CourseID"].ToString()), clData, htblTestData["HeaderType"].ToString(), _ecomLoginKey, _ecomTransactionKey);



                if (htblTestData["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                {
                    var oEnrolledUserCourseUserData = JsonConvert.DeserializeObject<List<EnrolledUserCourseUserData>>(result);
                    if (oEnrolledUserCourseUserData.Count > 0)
                    {
                        for (int iEnrolUser = 0; iEnrolUser < oEnrolledUserCourseUserData.Count; iEnrolUser++)
                        {
                            if (oEnrolledUserCourseUserData[iEnrolUser].CourseID.ToString().Contains(htblTestData["CourseID"].ToString()))
                            {
                                _Flag = true;

                            }
                        }
                    }
                }
                else if (htblTestData["TestCaseType"].ToString().ToUpper() == "NEGATIVE")
                {
                    if (_ecomTransactionKey == "" && _ecomLoginKey == "")
                    {

                        return true;
                    }
                    if (result.ToUpper().Contains("!DOCTYPE"))
                    {

                        return true;
                    }

                    if (statusCode.Contains("40") || statusCode.Contains("50"))
                    {
                        if (statusCode.Contains("404") || statusCode.Contains("500") || statusCode.Contains("405"))
                        {

                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                clsException.ExceptionHandler(e, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

                return false;
            }
            return _Flag;
        }
    }
}
