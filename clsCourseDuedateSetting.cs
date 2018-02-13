using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ILMS.ApplicationSettingsManager;

namespace ILMS
{
    class clsAPICourseDuedate
    {
        private IWebDriver iWebdriver;
        private string _ecomTransactionKey = string.Empty;
        private string _ecomLoginKey = string.Empty;
        private string _orgID = string.Empty;

        public clsAPICourseDuedate(IWebDriver iWeb)
        {
            this.iWebdriver = iWeb;
        }


        public bool FetchUpdateCourseDuedateSetting(object[] oValue)
        {

            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            Hashtable hTable = new Hashtable();
            hTable = oGeneric.GetTestData(oValue);

            switch (hTable["Method"].ToString().ToUpper().Trim())
            {
                case "FETCH":
                    return FetchCourseDuedateSetting(hTable);

                case "UPDATE":
                    return UpdateCourseDuedateSetting(hTable);
            }

            return true;

        }


        public bool FetchCourseDuedateSetting(Hashtable hTable)
        {
            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            List<string> lCourseId = new List<string>();
            bool _Flag = false;
            string _UserID = string.Empty;
            try
            {
                clsPage oPage = new clsPage(iWebdriver);

                _orgID = oPage.GetOrganizationID();
                clsAPI.orgID = _orgID;

                //Getting org API credentials
                GetAPICredentials oGetAPICredentials = new GetAPICredentials();
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", _orgID), oGetAPICredentials, "H2", "", "");
                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);

                DefaultDueDateSetting oDefaultDueDateSetting = new DefaultDueDateSetting();

                //Getting couse id of my course
                lCourseId = oPage.GetCourseId(hTable["CourseName"].ToString());


                //Hitting API URI
                if (hTable["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                {
                    //organizations/{orgID}/courses/{courseID}/duedate
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.CourseDuedateSetting.Replace("{orgid}", _orgID).Replace("{courseID}", lCourseId[0].ToString().Trim()), oDefaultDueDateSetting, "H1", _ecomLoginKey, _ecomTransactionKey);

                    //var oCoursesettingdata = JsonConvert.DeserializeObject<DefaultDueDateSetting>(result);                  


                    if (statusCode == "OK/200")
                    {
                        _Flag = true;
                    }
                    else { _Flag = false; }
                }

                else if
                    (hTable["TestCaseType"].ToString().ToUpper() == "NEGATIVE")
                {

                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.CourseDuedateSetting.Replace("{orgid}", _orgID).Replace("{courseID}", lCourseId[0].ToString().Trim()), oDefaultDueDateSetting, "H1", _ecomLoginKey, _ecomTransactionKey);
                    if (result.Contains("ErrorCode"))
                    {
                        _Flag = true;
                    }
                    else { _Flag = false; }
                }

            }
            catch (Exception e)
            {
                clsException.ExceptionHandler(e, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

                return false;
            }

            return _Flag;

        }



        public bool UpdateCourseDuedateSetting(Hashtable hTable)
        {
            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            List<string> lCourseId = new List<string>();
            bool _Flag = false;
            try
            {
                clsPage oPage = new clsPage(iWebdriver);

                _orgID = oPage.GetOrganizationID();
                clsAPI.orgID = _orgID;

                //Getting org API credentials
                GetAPICredentials oGetAPICredentials = new GetAPICredentials();
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", _orgID), oGetAPICredentials, "H2", "", "");
                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);

                //Getting the couse ID
                lCourseId = oPage.GetCourseId(hTable["CourseName"].ToString());

                if (hTable["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                {
                    DefaultDueDateSetting oDefaultDueDateSetting = new DefaultDueDateSetting();
                    oDefaultDueDateSetting.DefaultDueDate = hTable["NewDuedateDefault"].ToString();
                    oDefaultDueDateSetting.DaysAfterEnrollment = hTable["NewDuedateAfterEnrollment"].ToString();

                    //Modifying old duedate
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "PUT", ApplicationSettings.APIURI() + clsAPI.CourseDuedateSetting.Replace("{orgid}", _orgID).Replace("{courseID}", lCourseId[0].ToString().Trim()), oDefaultDueDateSetting, "H1", _ecomLoginKey, _ecomTransactionKey);

                    //Getting new duedate after updating
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.CourseDuedateSetting.Replace("{orgid}", _orgID).Replace("{courseID}", lCourseId[0].ToString().Trim()), oDefaultDueDateSetting, "H1", _ecomLoginKey, _ecomTransactionKey);
                    var oCoursesettingdata = JsonConvert.DeserializeObject<DefaultDueDateSetting>(result);


                    string newDuedateAfterEnrollment = oCoursesettingdata.DaysAfterEnrollment;
                    string newDefaultDuedate = oCoursesettingdata.DefaultDueDate;


                    if (!string.IsNullOrEmpty(newDuedateAfterEnrollment))
                    {
                        if (newDuedateAfterEnrollment.Contains(hTable["NewDuedateAfterEnrollment"].ToString()))
                        {
                            if (string.IsNullOrEmpty(newDefaultDuedate) && hTable["NewDuedateDefault"].ToString() == "null")
                            {
                                _Flag = true;
                            }
                            else
                            {
                                _Flag = false;
                            }
                        }

                        else
                        {
                            _Flag = false;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(newDefaultDuedate))
                        {

                            if (string.IsNullOrEmpty(newDuedateAfterEnrollment) && hTable["NewDuedateAfterEnrollment"].ToString() == "null")
                            {
                                if (newDefaultDuedate.Contains(hTable["NewDuedateDefault"].ToString()))
                                {
                                    _Flag = true;

                                }

                                else
                                {
                                    _Flag = false;
                                }
                            }
                            else
                            {

                                if (string.IsNullOrEmpty(newDuedateAfterEnrollment) && hTable["NewDuedateAfterEnrollment"].ToString() == "null")
                                {
                                    if (string.IsNullOrEmpty(newDefaultDuedate) && hTable["NewDuedateDefault"].ToString() == "null")
                                    {
                                        _Flag = true;

                                    }

                                    else
                                    {
                                        _Flag = false;
                                    }
                                }
                            }
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
