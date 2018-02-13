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
    class clsAPICourseEnrollment
    {
        private IWebDriver iWebdriver;
        private string _ecomTransactionKey = string.Empty;
        private string _ecomLoginKey = string.Empty;
        private string _orgID = string.Empty;

        public clsAPICourseEnrollment(IWebDriver iWeb)
        {
            this.iWebdriver = iWeb;
        }


        public bool UserEnrollment(object[] lStrValue)
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
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "POST", ApplicationSettings.APIURI() + clsAPI.CourseEnrollment.Replace("{orgid}", _orgID).Replace("{CourseID}", lCourseId[0].ToString().Trim()), oUserEnrollment, "H1", _ecomLoginKey, _ecomTransactionKey);

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
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.CourseEnrollment.Replace("{orgid}", _orgID).Replace("{CourseID}", lCourseId[0].ToString().Trim()), oUserEnrollment, "H1", _ecomLoginKey, _ecomTransactionKey);
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
                        if (oTranscriptData[icount].CourseName.ToString().ToUpper().Equals(htblTestData["CourseName"].ToString().ToUpper()))
                        {
                            return true;
                        }




                    }
                    return false;

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

                if (_Flag == true)
                {

                    iWebdriver.Navigate().GoToUrl(oPage.SetUrl(htblTestData["url"].ToString()));

                    clsGlobalVariable.strExceptionReport = string.Empty;
                    string tag = ApplicationSettings.ElementByPath();
                    object[] objparam = new object[] { ApplicationSettings.EleUserName(),
                                               ApplicationSettings.ElePassword(),
                                               ApplicationSettings.EleLogin() };


                    System.Threading.Thread.Sleep(clsGlobalVariable.iWaitMedium);
                    if (iWebdriver.GetType().Name.Contains("InternetExplorer") && clsGlobalVariable.IEFlag == true)
                    {
                        iWebdriver.Navigate().GoToUrl("javascript:document.getElementById('overridelink').click()");
                        clsGlobalVariable.IEFlag = false;
                    }
                    try
                    {
                        System.Threading.Thread.Sleep(clsGlobalVariable.iWaitHigh);
                        iWebdriver.FindElement((By)oGeneric.InvokeMethodwith_Param(tag, new object[] { objparam[0].ToString() })).SendKeys(htblTestData["UserEmail"].ToString());
                        iWebdriver.FindElement((By)oGeneric.InvokeMethodwith_Param(tag, new object[] { objparam[1].ToString() })).SendKeys(htblTestData["Password"].ToString());
                        iWebdriver.FindElement((By)oGeneric.InvokeMethodwith_Param(tag, new object[] { objparam[2].ToString() })).Click();

                        iWebdriver.SwitchTo().DefaultContent();
                        iWebdriver.SwitchTo().Frame(clsPageObject.frmLCLeftFrame);
                        iWebdriver.SwitchTo().Frame(clsPageObject.frmLCRightFrameFinal);


                        if (iWebdriver.FindElement(By.XPath(clsPageObject.scormCourseName.Replace("$", htblTestData["CourseName"].ToString()))).Displayed)
                        {
                            return true;
                        }
                        else
                        {

                            clsGlobalVariable.strExceptionReport = "User is not enrolled in " + htblTestData["CourseName"].ToString();
                            return false;
                        }

                    }
                    catch (Exception ex)
                    {
                        clsGlobalVariable.strExceptionReport = "User is not enrolled in " + htblTestData["CourseName"].ToString();
                        return false;
                    }


                }
                else { return false; }


            }
            catch (Exception ex) { }



            return _Flag;
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

                //  oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.user.Replace("{orgid}", _orgID), oUser, "H1", _ecomLoginKey, _ecomTransactionKey);

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
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "DELETE", ApplicationSettings.APIURI() + clsAPI.CourseEnrollment.Replace("{orgid}", _orgID).Replace("{CourseID}", lCourseId[0].ToString().Trim()), oUserEnrollment, "H1", _ecomLoginKey, _ecomTransactionKey);


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
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.CourseEnrollment.Replace("{orgid}", _orgID).Replace("{CourseID}", lCourseId[0].ToString().Trim()), oUserEnrollment, "H1", _ecomLoginKey, _ecomTransactionKey);
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
                        if (oTranscriptData[icount].CourseName.ToString().ToUpper().Equals(htblTestData["CourseName"].ToString().ToUpper()))
                        {
                            _Flag = false;
                            return false;
                        }




                    }
                    _Flag = true;
                    return true;

                }

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
                    iWebdriver.Navigate().GoToUrl(ApplicationSettings.Portal());

                    clsGlobalVariable.strExceptionReport = string.Empty;
                    string tag = ApplicationSettings.ElementByPath();
                    object[] objparam = new object[] { ApplicationSettings.EleUserName(),
                                               ApplicationSettings.ElePassword(),
                                               ApplicationSettings.EleLogin() };


                    System.Threading.Thread.Sleep(clsGlobalVariable.iWaitMedium);
                    if (iWebdriver.GetType().Name.Contains("InternetExplorer") && clsGlobalVariable.IEFlag == true)
                    {
                        iWebdriver.Navigate().GoToUrl("javascript:document.getElementById('overridelink').click()");
                        clsGlobalVariable.IEFlag = false;
                    }
                    try
                    {
                        System.Threading.Thread.Sleep(clsGlobalVariable.iWaitHigh);
                        iWebdriver.FindElement((By)oGeneric.InvokeMethodwith_Param(tag, new object[] { objparam[0].ToString() })).SendKeys(htblTestData["UserEmail"].ToString());
                        iWebdriver.FindElement((By)oGeneric.InvokeMethodwith_Param(tag, new object[] { objparam[1].ToString() })).SendKeys(htblTestData["Password"].ToString());
                        iWebdriver.FindElement((By)oGeneric.InvokeMethodwith_Param(tag, new object[] { objparam[2].ToString() })).Click();

                        iWebdriver.SwitchTo().DefaultContent();
                        iWebdriver.SwitchTo().Frame(clsPageObject.frmLCLeftFrame);
                        iWebdriver.SwitchTo().Frame(clsPageObject.frmLCRightFrameFinal);

                        try
                        {
                            if (iWebdriver.FindElement(By.XPath(clsPageObject.scormCourseName.Replace("$", htblTestData["CourseName"].ToString()))).Displayed)
                            {
                                return false;
                            }
                            else
                            {

                                clsGlobalVariable.strExceptionReport = "User is  unenrolled in " + htblTestData["CourseName"].ToString();
                                return true;
                            }
                        }
                        catch (Exception er)
                        {
                            clsGlobalVariable.strExceptionReport = "User is  unenrolled in " + htblTestData["CourseName"].ToString();
                            return true;
                        }


                    }


                    catch (Exception e)
                    {
                        clsException.ExceptionHandler(e, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

                        return false;
                    }
                }
                else { return false; }


            }
            catch (Exception ex) { }



            return _Flag;
        }

    }
}
