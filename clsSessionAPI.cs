using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenQA.Selenium;
using ILMS.ApplicationSettingsManager;

namespace ILMS
{


    class clsSessionAPI
    {
        private IWebDriver iWebdriver;
        private string _ecomTransactionKey = string.Empty;
        private string _ecomLoginKey = string.Empty;
        private string _orgID = string.Empty;
        private string _SessionID = string.Empty;


        public clsSessionAPI(IWebDriver iWeb)
        {
            this.iWebdriver = iWeb;
        }

        public bool SessionEnrollment(object[] lStrvalue)
        {
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            Hashtable htblTestData = new Hashtable();
            htblTestData = oGeneric.GetTestData(lStrvalue);

            switch (htblTestData["UserEnrollment"].ToString().ToUpper().Trim())
            {
                case "ENROL":
                    return Enrolluser(htblTestData);
                //break;
                case "UNENROL":
                    return UnEnrolluser(htblTestData);

            }

            return true;
        }


        public bool Enrolluser(Hashtable htblTestData)
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
                lCourseId = oPage.GetCourseId(htblTestData["CourseName"].ToString());

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


                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetSessionID.Replace("{orgid}", _orgID).Replace("{CourseID}", lCourseId[0].ToString().Trim()), oUser, "H1", _ecomLoginKey, _ecomTransactionKey);


                var oSessionData = JsonConvert.DeserializeObject<List<clsAPISession>>(result);

                if (result == "") { return false; }

                _SessionID = oSessionData[0].SessionID;

                UserEnrollment oUserEnrollment = new ILMS.UserEnrollment();
                oUserEnrollment.userid = _UserID;
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "POST", ApplicationSettings.APIURI() + clsAPI.SessionEnrollment.Replace("{orgid}", _orgID).Replace("{CourseID}", lCourseId[0].ToString().Trim()).Replace("{SessionID}", _SessionID), oUserEnrollment, "H1", _ecomLoginKey, _ecomTransactionKey);

                Thread.Sleep(clsGlobalVariable.iWaitHigh);

                if (htblTestData["TestCaseType"].ToString().ToUpper() == "NEGATIVE")
                {
                    if (result.ToUpper().Contains("!DOCTYPE"))
                    {
                        clsGlobalVariable.strExceptionReport = "Resource not found";
                        return true;
                    }

                    if (result.Contains("Invalid URI"))
                    {
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


                do
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.SessionEnrollment.Replace("{orgid}", _orgID).Replace("{CourseID}", lCourseId[0].ToString().Trim()).Replace("{SessionID}", _SessionID), oUserEnrollment, "H1", _ecomLoginKey, _ecomTransactionKey);
                } while (result == "");




                var oCourseData = JsonConvert.DeserializeObject<List<SessionEnrollment>>(result);

                for (int iUser = 0; iUser < oCourseData.Count; iUser++)
                {
                    if (oCourseData[iUser].User.Profile_Basic.F015.ToString() == htblTestData["UserEmail"].ToString().Trim())
                    {
                        _UserID = oCourseData[iUser].User.ID;
                        _Flag = true;
                        break;
                    }
                    else
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


        public bool UnEnrolluser(Hashtable htblTestData)
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
                lCourseId = oPage.GetCourseId(htblTestData["CourseName"].ToString());

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


                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetSessionID.Replace("{orgid}", _orgID).Replace("{CourseID}", lCourseId[0].ToString().Trim()), oUser, "H1", _ecomLoginKey, _ecomTransactionKey);


                var oSessionData = JsonConvert.DeserializeObject<List<clsAPISession>>(result);

                if (result == "") { return false; }

                _SessionID = oSessionData[0].SessionID;

                UserEnrollment oUserEnrollment = new ILMS.UserEnrollment();
                oUserEnrollment.userid = _UserID;
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "DELETE", ApplicationSettings.APIURI() + clsAPI.SessionEnrollment.Replace("{orgid}", _orgID).Replace("{CourseID}", lCourseId[0].ToString().Trim()).Replace("{SessionID}", _SessionID), oUserEnrollment, "H1", _ecomLoginKey, _ecomTransactionKey);

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


                do
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.SessionEnrollment.Replace("{orgid}", _orgID).Replace("{CourseID}", lCourseId[0].ToString().Trim()).Replace("{SessionID}", _SessionID), oUserEnrollment, "H1", _ecomLoginKey, _ecomTransactionKey);
                } while (result == "");




                var oCourseData = JsonConvert.DeserializeObject<List<SessionEnrollment>>(result);

                for (int iUser = 0; iUser < oCourseData.Count; iUser++)
                {
                    if (oCourseData[iUser].User.Profile_Basic.F015.ToString() != htblTestData["UserEmail"].ToString().Trim())
                    {
                        _UserID = oCourseData[iUser].User.ID;
                        _Flag = true;
                        break;
                    }
                    else
                    {
                        _Flag = false;
                    }
                }

                if (oCourseData.Count == 0)
                {
                    _Flag = true;
                }


            }
            catch (Exception e)
            {
                clsException.ExceptionHandler(e, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

                return false;
            }



            return _Flag;
        }


        public bool EnrolluserInCanceledSession(object[] lStrvalue)
        {
            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            clsGeneric oGeneric = new clsGeneric();


            Hashtable htblTestData = new Hashtable();
            htblTestData = oGeneric.GetTestData(lStrvalue);
            string URI = string.Empty;

            List<string> lCourseId = new List<string>();
            bool _Flag = false;
            string _UserID = string.Empty;
            try
            {
                clsPage oPage = new clsPage(iWebdriver);

                _orgID = oPage.GetOrganizationID();
                clsAPI.orgID = _orgID;
                lCourseId = oPage.GetCourseId(htblTestData["CourseName"].ToString());

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


                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetSessionID.Replace("{orgid}", _orgID).Replace("{CourseID}", lCourseId[0].ToString()), oUser, "H1", _ecomLoginKey, _ecomTransactionKey);


                var oSessionData = JsonConvert.DeserializeObject<List<clsAPISession>>(result);

                if (result == "") { return false; }

                _SessionID = oSessionData[0].SessionID;


                clsSession oSession = new clsSession(iWebdriver);

                if (htblTestData["Function"].ToString().ToUpper() == "CANCEL")
                {
                    oSession.CancelSession(htblTestData["CourseName"].ToString(), "CANCEL");
                }
                else
                {
                    oSession.CancelSession(htblTestData["CourseName"].ToString(), "Delete");
                }

                UserEnrollment oUserEnrollment = new ILMS.UserEnrollment();
                oUserEnrollment.userid = _UserID;
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "POST", ApplicationSettings.APIURI() + clsAPI.SessionEnrollment.Replace("{orgid}", _orgID).Replace("{CourseID}", lCourseId[0].ToString().Trim()).Replace("{SessionID}", _SessionID), oUserEnrollment, "H1", _ecomLoginKey, _ecomTransactionKey);

                Thread.Sleep(clsGlobalVariable.iWaitHigh);

                if (htblTestData["TestCaseType"].ToString().ToUpper() == "NEGATIVE")
                {
                    if (result.ToUpper().Contains("!DOCTYPE"))
                    {
                        clsGlobalVariable.strExceptionReport = "Resource not found";
                        return true;
                    }


                    if (result.Contains("Invalid URI"))
                    {
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


                do
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.SessionEnrollment.Replace("{orgid}", _orgID).Replace("{CourseID}", lCourseId[0].ToString().Trim()).Replace("{SessionID}", _SessionID), oUserEnrollment, "H1", _ecomLoginKey, _ecomTransactionKey);
                } while (result == "");




                var oCourseData = JsonConvert.DeserializeObject<List<SessionEnrollment>>(result);

                for (int iUser = 0; iUser < oCourseData.Count; iUser++)
                {
                    if (oCourseData[iUser].User.Profile_Basic.F015.ToString() == htblTestData["UserEmail"].ToString().Trim())
                    {
                        _UserID = oCourseData[iUser].User.ID;
                        _Flag = true;
                        break;
                    }
                    else
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



        public bool GetSession(object[] oObject)
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
            List<string> lCourseId = new List<string>();
            string resultnew = string.Empty;
            string sessionId = string.Empty;

            Hashtable hTable = new Hashtable();
            hTable = oGeneric.GetTestData(oObject);

            clsPage oPage = new clsPage(iWebdriver);
            lCourseId = oPage.GetCourseId(hTable["CourseName"].ToString());
            clsGenericAPI oclsGenericAPI = new clsGenericAPI();


            _orgID = oPage.GetOrganizationID();
            clsAPI.orgID = _orgID;
            oclsGenericAPI.GetORGCredentials(out _ecomTransactionKey, out _ecomLoginKey, _orgID);
            User oUser = new User();
            try
            {
                if (hTable["FunctionFor"].ToString().ToUpper() == "ALL")
                {

                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetSessionID.Replace("{orgid}", _orgID).Replace("{CourseID}", lCourseId[0].ToString().Trim()), oUser, "H1", _ecomLoginKey, _ecomTransactionKey);


                    var oSessionData = JsonConvert.DeserializeObject<List<clsAPISession>>(result);

                    if (result == "") { return false; }
                    if (oSessionData.Count > 0)
                    {
                        _flag = true;
                    }
                }
                else if (hTable["FunctionFor"].ToString().ToUpper() == "ROOMNAME")
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetSessionID.Replace("{orgid}", _orgID).Replace("{CourseID}", lCourseId[0].ToString().Trim()), oUser, "H1", _ecomLoginKey, _ecomTransactionKey);


                    var oSessionData = JsonConvert.DeserializeObject<List<clsAPISession>>(result);

                    for (int iSessionCounter = 0; iSessionCounter < oSessionData.Count; iSessionCounter++)
                    {
                        if (oSessionData[iSessionCounter].RoomName == hTable["RoomName"].ToString())
                        {
                            sessionId = oSessionData[iSessionCounter].SessionID; break;
                        }
                    }

                    if (sessionId == "") return false;

                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetSessionID.Replace("{orgid}", _orgID).Replace("{CourseID}", lCourseId[0].ToString().Trim()) + "/" + sessionId, oUser, "H1", _ecomLoginKey, _ecomTransactionKey);

                    var oSessionDatanew = JsonConvert.DeserializeObject<List<clsAPISession>>(result);
                    if (oSessionDatanew.Count == 1)
                    { return true; }
                    else { return false; }

                }
                else
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetSessionID.Replace("{orgid}", _orgID).Replace("{CourseID}", lCourseId[0].ToString().Trim()) + "?sessionstartdate=" + hTable["StartDate"] + "&sessionenddate=" + hTable["EndDate"], oUser, "H1", _ecomLoginKey, _ecomTransactionKey);

                    if (hTable["TestCaseType"].ToString().ToUpper() == "NEGATIVE")
                    {
                        if (statusCode.Contains("204") || statusCode.Contains("200"))
                        {
                            return false;
                        }
                        else { return true; }
                    }
                    var oSessionData = JsonConvert.DeserializeObject<List<clsAPISession>>(result);

                    if (result == "") { return false; }
                    if (oSessionData.Count > 0)
                    {
                        _flag = true;
                    }

                  

                }

            }
            catch (Exception e)
            {
                clsException.ExceptionHandler(e, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

                return false;
            }


            return _flag;
        }
    }
}
