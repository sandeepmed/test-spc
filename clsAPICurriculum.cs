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
    class clsAPICurriculum
    {
        private IWebDriver iWebdriver;
        private string _ecomTransactionKey = string.Empty;
        private string _ecomLoginKey = string.Empty;
        private string _orgID = string.Empty;

        public clsAPICurriculum(IWebDriver iWeb)
        {
            this.iWebdriver = iWeb;
        }

        public bool UserCurriculaEnrollment(object[] lStrValue)
        {

            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            Hashtable htblTestData = new Hashtable();
            htblTestData = oGeneric.GetTestData(lStrValue);

            switch (htblTestData["UserEnrollment"].ToString().ToUpper().Trim())
            {
                case "ENROL":

                    return Enrolluser(lStrValue);
                //break;
                case "UNENROL":
                    return UnEnrolluser(lStrValue);
                    //break;
            }

            return true;

        }



        public bool Enrolluser(object[] lStrValue)
        {
            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            Hashtable htblTestData = new Hashtable();
            htblTestData = oGeneric.GetTestData(lStrValue);
            string lCourseId = string.Empty;
            bool _Flag = false;
            string _UserID = string.Empty;
            try
            {
                clsPage oPage = new clsPage(iWebdriver);

                _orgID = oPage.GetOrganizationID();
                clsAPI.orgID = _orgID;
                lCourseId = oPage.GetCurriculumId(htblTestData["CurriculumName"].ToString());

                Users oUser = new Users();

                GetAPICredentials oGetAPICredentials = new GetAPICredentials();


                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", _orgID), oUser, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);



                if (htblTestData["Status"].ToString().ToUpper().Contains("INACTIVE"))
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.user.Replace("{orgid}", _orgID) + "?F015=" + htblTestData["UserEmail"].ToString() + "*&userstatus=inactive", oUser, "H1", _ecomLoginKey, _ecomTransactionKey);

                }
                else if (htblTestData["Status"].ToString().ToUpper().Contains("DELETED"))
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.user.Replace("{orgid}", _orgID) + "?F015=" + htblTestData["UserEmail"].ToString() + "*&userstatus=Deleted", oUser, "H1", _ecomLoginKey, _ecomTransactionKey);

                }
                else
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.user.Replace("{orgid}", _orgID), oUser, "H1", _ecomLoginKey, _ecomTransactionKey);

                }




                var oUserData = JsonConvert.DeserializeObject<List<Users>>(result);

                for (int iUser = 0; iUser < oUserData.Count; iUser++)
                {
                    if (oUserData[iUser].Profile.F015.ToString() == htblTestData["UserEmail"].ToString().Trim())
                    {
                        _UserID = oUserData[iUser].ID;
                        break;
                    }
                }



                UserEnrollment oUserEnrollment = new ILMS.UserEnrollment();
                oUserEnrollment.userid = _UserID;
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "POST", ApplicationSettings.APIURI() + clsAPI.CurriculumEnrollment.Replace("{orgid}", _orgID).Replace("{CourseID}", lCourseId.ToString()), oUserEnrollment, "H1", _ecomLoginKey, _ecomTransactionKey);

                Thread.Sleep(clsGlobalVariable.iWaitHigh);

                if (htblTestData["TestCaseType"].ToString().ToUpper() == "NEGATIVE")
                {
                    if (result.ToUpper().Contains("!DOCTYPE"))
                    {
                        clsGlobalVariable.strExceptionReport = "Resource not found";
                        return true;
                    }


                    var oError = JsonConvert.DeserializeObject<List<SuperAdminCoursePushDownError>>(result);
                    SuperAdminCoursePushDownErrorCode oErrorCode = new SuperAdminCoursePushDownErrorCode();
                    foreach (string strErrorCode in oErrorCode.coursePushDownErrorCode)
                    {
                        if (strErrorCode == oError[0].ErrorCode)
                        {
                            _Flag = true;
                        }
                    }
                    if (_Flag == true)
                    {
                        return true;
                    }
                    else
                    {
                        clsGlobalVariable.strExceptionReport = "Negative Case Failed!";
                        return false;
                    }

                }

                try
                {
                    if (htblTestData["Status"].ToString().ToUpper() == "Deleted".ToUpper())
                    {
                        if (_Flag == true)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                catch { }
                do
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.CurriculumEnrollment.Replace("{orgid}", _orgID).Replace("{CourseID}", lCourseId.ToString().Trim()), oUserEnrollment, "H1", _ecomLoginKey, _ecomTransactionKey);
                } while (result == "");

                if (htblTestData["Status"].ToString().ToUpper() == "INACTIVE")
                {
                    do
                    {
                        oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetTranscript.Replace("{orgid}", _orgID).Replace("{UserID}", _UserID.ToString().Trim()), oUserEnrollment, "H1", _ecomLoginKey, _ecomTransactionKey);
                    } while (result == "");

                    var oTranscriptData = JsonConvert.DeserializeObject<List<clsAPITranscript>>(result);

                    for (int icount = 0; icount < oTranscriptData.Count; icount++)
                    {
                        if (oTranscriptData[icount].CourseName.ToString().ToUpper().Equals(htblTestData["CurriculumName"].ToString().ToUpper()))
                        {
                            return true;
                        }
                        else
                        {

                            return false;
                        }
                    }

                }




                var oCourseData = JsonConvert.DeserializeObject<List<CourseData>>(result);

                for (int iUser = 0; iUser < oCourseData.Count; iUser++)
                {
                    if (oCourseData[iUser].User.Profile_Basic.F015.ToString() == htblTestData["UserEmail"].ToString().Trim())
                    {
                        _UserID = oUserData[iUser].ID;
                        _Flag = true;
                        break;
                    }
                    else
                    {
                        _Flag = false;
                    }
                }



                if (_Flag == true)
                {
                    return true;


                }
                else { return false; }


            }
            catch (Exception ex)
            {
                clsGlobalVariable.strExceptionReport = "User not enrolled in curriculum.";
                return false;
            }




        }


        public bool UnEnrolluser(object[] lStrValue)
        {
            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            Hashtable htblTestData = new Hashtable();
            htblTestData = oGeneric.GetTestData(lStrValue);
            string lCourseId = string.Empty;
            bool _Flag = false;
            string _UserID = string.Empty;
            try
            {
                clsPage oPage = new clsPage(iWebdriver);

                _orgID = oPage.GetOrganizationID();
                clsAPI.orgID = _orgID;
                lCourseId = oPage.GetCurriculumId(htblTestData["CourseName"].ToString());

                Users oUser = new Users();

                GetAPICredentials oGetAPICredentials = new GetAPICredentials();


                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", _orgID), oUser, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.user.Replace("{orgid}", _orgID), oUser, "H1", _ecomLoginKey, _ecomTransactionKey);


                var oUserData = JsonConvert.DeserializeObject<List<Users>>(result);

                for (int iUser = 0; iUser < oUserData.Count; iUser++)
                {
                    if (oUserData[iUser].Profile.F015.ToString() == htblTestData["UserEmail"].ToString().Trim())
                    {

                        _UserID = oUserData[iUser].ID;
                        break;
                    }
                }



                UserEnrollment oUserEnrollment = new ILMS.UserEnrollment();
                oUserEnrollment.userid = _UserID;
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "DELETE", ApplicationSettings.APIURI() + clsAPI.CurriculumEnrollment.Replace("{orgid}", _orgID).Replace("{CourseID}", lCourseId.ToString().Trim()), oUserEnrollment, "H1", _ecomLoginKey, _ecomTransactionKey);


                if (htblTestData["TestCaseType"].ToString().ToUpper() == "NEGATIVE")
                {
                    if (result.ToUpper().Contains("!DOCTYPE"))
                    {
                        clsGlobalVariable.strExceptionReport = "Resource not found";
                        return true;
                    }


                    var oError = JsonConvert.DeserializeObject<List<SuperAdminCoursePushDownError>>(result);
                    SuperAdminCoursePushDownErrorCode oErrorCode = new SuperAdminCoursePushDownErrorCode();
                    foreach (string strErrorCode in oErrorCode.coursePushDownErrorCode)
                    {
                        if (strErrorCode == oError[0].ErrorCode)
                        {
                            _Flag = true;
                        }
                    }
                    if (_Flag == true)
                    {
                        return true;
                    }
                    else
                    {
                        clsGlobalVariable.strExceptionReport = "Negative Case Failed!";
                        return false;
                    }

                }



                Thread.Sleep(clsGlobalVariable.iWaitHigh);
                do
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.CurriculumEnrollment.Replace("{orgid}", _orgID).Replace("{CourseID}", lCourseId.ToString().Trim()), oUserEnrollment, "H1", _ecomLoginKey, _ecomTransactionKey);
                } while (result == "");

                var oCourseData = JsonConvert.DeserializeObject<List<CourseData>>(result);

                for (int iUser = 0; iUser < oCourseData.Count; iUser++)
                {
                    if (oCourseData[iUser].User.Profile_Basic.F015.ToString() == htblTestData["UserEmail"].ToString().Trim())
                    {
                        if (htblTestData["TestCaseType"].ToString().Trim().ToUpper() == "NEGATIVE")
                        {
                            clsGlobalVariable.strExceptionReport = "User is  enrolled in " + htblTestData["CourseName"].ToString();
                            _Flag = false;
                        }
                        _UserID = oUserData[iUser].ID;
                        _Flag = false;
                        break;
                    }
                    else
                    {
                        _Flag = true;

                    }
                }



                if (_Flag == true || oCourseData.Count == 0)
                {
                    return true;
                }
                else { return false; }


            }
            catch (Exception ex) { }



            return _Flag;
        }


        public bool GetCurricula(object[] oObject)
        {

            string _UserID = string.Empty;
            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string statusCodeUpdate = string.Empty;
            string result = string.Empty;
            string resultput = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            string courseID = string.Empty;
            bool _flag = false;
            string lCourseId = string.Empty;
            string resultnew = string.Empty;
            string sessionId = string.Empty;

            Hashtable hTable = new Hashtable();
            hTable = oGeneric.GetTestData(oObject);

            clsPage oPage = new clsPage(iWebdriver);
            lCourseId = oPage.GetCurriculumId(hTable["CourseName"].ToString());
            clsGenericAPI oclsGenericAPI = new clsGenericAPI();


            _orgID = oPage.GetOrganizationID();
            clsAPI.orgID = _orgID;
            oclsGenericAPI.GetORGCredentials(out _ecomTransactionKey, out _ecomLoginKey, _orgID);
            User oUser = new User();
            try
            {
                if (hTable["FunctionFor"].ToString().ToUpper() == "ALL")
                {

                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetCuriculla.Replace("{orgid}", _orgID).Replace("{CourseID}", lCourseId), oUser, "H1", _ecomLoginKey, _ecomTransactionKey);


                    var oCurriculaData = JsonConvert.DeserializeObject<List<curiculla>>(result);

                    if (result == "") { return false; }
                    if (oCurriculaData.Count > 0)
                    {
                        _flag = true;
                    }
                }
                else if (hTable["FunctionFor"].ToString().ToUpper() == "SPECIFIC")
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetCuriculla.Replace("{orgid}", _orgID)+"/"+lCourseId, oUser, "H1", _ecomLoginKey, _ecomTransactionKey);


                    var oSessionData = JsonConvert.DeserializeObject<List<curiculla>>(result);
                                     

                                    if (oSessionData.Count == 1)
                    { return true; }
                    else { return false; }

                }
                else
                {
                    DefaultDueDateSettings oDefaultDueDateSettings = new DefaultDueDateSettings();

                    oDefaultDueDateSettings.DefaultDueDate = hTable["DefaultDueDate"].ToString();
                        oDefaultDueDateSettings.DaysAfterEnrollment= hTable["DaysAfterEnrollment"].ToString();
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "PUT", ApplicationSettings.APIURI() + clsAPI.GetCuriculla.Replace("{orgid}", _orgID) + "/" + lCourseId+"/DueDate", oDefaultDueDateSettings, "H1", _ecomLoginKey, _ecomTransactionKey);

                    if (hTable["TestCaseType"].ToString().ToUpper() == "NEGATIVE")
                    {
                        if (statusCode.Contains("204") || statusCode.Contains("200"))
                        {
                            return false;
                        }
                        else { return true; }
                    }

                    if (statusCode.Contains("204") || statusCode.Contains("200"))
                    {
                        return true;
                    }
                    else { return false; }


                }

            }
            catch (Exception er) { }


            return _flag;
        }
    }
}
