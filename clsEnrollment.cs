using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using OpenQA.Selenium;
using ILMS.ApplicationSettingsManager;
using System;

namespace ILMS
{
    internal class clsEnrollment
    {
        private IWebDriver iWebdriver;
        private string _ecomTransactionKey = string.Empty;
        private string _ecomLoginKey = string.Empty;
        public clsEnrollment(IWebDriver iWeb)
        {
            this.iWebdriver = iWeb;
        }


        public bool APIEcomEnrollmentDynamic(object[] lStrvalue)
        {
            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string uriApiCredential = string.Empty;
            string result = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            Hashtable htblTestData = new Hashtable();
            htblTestData = oGeneric.GetTestData(lStrvalue);
            bool _Flag = false;
            string _orgID = string.Empty;
            clsPage oPage = new clsPage(iWebdriver);


            try
            {

                EcomEnrolUser oEcomEnrolUser = new EcomEnrolUser();

                oEcomEnrolUser.OrderID = htblTestData["OrderID"].ToString();
                oEcomEnrolUser.UserID = clsAPI.userid;
                // clsAPI.orgID = htblTestData["OrgID"].ToString();


                GetAPICredentials oGetAPICredentials = new GetAPICredentials();

                URI = ApplicationSettings.APIURI() + clsAPI.EcomCourseSeats.Replace("$", clsAPI.orgID) + clsAPI.EcommMgrID.Replace("#", clsAPI.userid);
                uriApiCredential = ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", clsAPI.orgID);

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET",
                 uriApiCredential, oEcomEnrolUser, "H2", _ecomLoginKey, _ecomTransactionKey);

                Thread.Sleep(clsGlobalVariable.iWaitHigh);

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);


                URI = ApplicationSettings.APIURI() + clsAPI.EcomCourseSeats.Replace("$", clsAPI.orgID) + clsAPI.EcommMgrID.Replace("#", clsAPI.userid);

                do
                {
                    Thread.Sleep(60000);
                    for (int i = 0; i < 5; i++)
                    {
                        Thread.Sleep(clsGlobalVariable.iWaitHigh);
                        oGeneric.GetApiResponseCodeData(out statusCode, out result, "Get", URI, oEcomEnrolUser, htblTestData["HeaderType"].ToString(), _ecomLoginKey, _ecomTransactionKey);
                    }
                    if (result.Contains("Error") || result.Contains("Invalid") || result.Contains("DOCTYPE"))
                    {
                        break;
                    }
                } while (!result.ToString().Contains("OrgCourseID"));
                Thread.Sleep(clsGlobalVariable.iWaitHigh);

                // APIGetCourseSeats courseSeats = new APIGetCourseSeats();

                var courseSeats = JsonConvert.DeserializeObject<List<APICourseSeats>>(result);
                string CourseID = string.Empty;
                for (int i = 0; i < courseSeats.Count; i++)
                {
                    if (courseSeats[i].CourseName == (htblTestData["CourseName"].ToString()))
                    {
                        CourseID = courseSeats[i].OrgCourseID;

                    }
                }




                URI = string.Empty;
                URI = ApplicationSettings.APIURI() + clsAPI.EcommEnrolUser.Replace("{orgid}", clsAPI.orgID).Replace("{userid}", clsAPI.userid).Replace("{courseid}", CourseID);
                oGeneric.GetApiResponseCodeData(out statusCode, out result, htblTestData["MethodType"].ToString(), URI, oEcomEnrolUser, htblTestData["HeaderType"].ToString(), _ecomLoginKey, _ecomTransactionKey);

                if (htblTestData["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                {
                    if (statusCode.Contains("204"))
                    {
                        _Flag = true;
                    }
                }
                else
                {
                    if (!statusCode.Contains("204"))
                    {
                        _Flag = false;
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


        public bool APIEcomEnrollment(object[] lStrvalue)
        {
            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string uriApiCredential = string.Empty;
            string result = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            Hashtable htblTestData = new Hashtable();
            htblTestData = oGeneric.GetTestData(lStrvalue);
            bool _Flag = false;


            try
            {

                EcomEnrolUser oEcomEnrolUser = new EcomEnrolUser();

                oEcomEnrolUser.OrderID = htblTestData["OrderID"].ToString();
                oEcomEnrolUser.UserID = htblTestData["UserID"].ToString();
                clsAPI.orgID = htblTestData["OrgID"].ToString();


                GetAPICredentials oGetAPICredentials = new GetAPICredentials();

                URI = ApplicationSettings.APIURI() + clsAPI.EcomCourseSeats.Replace("$", clsAPI.orgID) + clsAPI.EcommMgrID.Replace("#", clsAPI.userid);
                uriApiCredential = ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", clsAPI.orgID);

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET",
                 uriApiCredential, oEcomEnrolUser, "H2", _ecomLoginKey, _ecomTransactionKey);

                Thread.Sleep(clsGlobalVariable.iWaitHigh);

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);


                URI = string.Empty;
                URI = ApplicationSettings.APIURI() + clsAPI.EcommEnrolUser.Replace("{orgid}", htblTestData["OrgID"].ToString()).Replace("{userid}", htblTestData["EcomUserId"].ToString()).Replace("{courseid}", htblTestData["CourseID"].ToString());
                oGeneric.GetApiResponseCodeData(out statusCode, out result, htblTestData["MethodType"].ToString(), URI, oEcomEnrolUser, htblTestData["HeaderType"].ToString(), _ecomLoginKey, _ecomTransactionKey);

                if (htblTestData["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                {
                    if (statusCode.Contains("204"))
                    {
                        _Flag = true;
                    }
                }
                else
                {
                    if (!statusCode.Contains("204"))
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