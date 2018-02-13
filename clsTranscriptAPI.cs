/************************************************************************************************************************************
************************************** Created by : Sandeep Dwivedi                                        **************************
************************************** Date: 05/07/2016                                                    **************************
************************************** Class contains all function related to                              **************************
************************************** transcript API. All get and put requests are covered in this.       **************************
*************************************************************************************************************************************
*************************************************************************************************************************************                             
*************************************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System.Collections;
using System.Threading;
using ILMS.ApplicationSettingsManager;

namespace ILMS
{
    class clsTranscriptAPI
    {
        private IWebDriver iWebdriver;
        private string _ecomTransactionKey = string.Empty;
        private string _ecomLoginKey = string.Empty;
        private string _orgID = string.Empty;
        private string _SessionID = string.Empty;

        public clsTranscriptAPI(IWebDriver iWeb)
        {
            this.iWebdriver = iWeb;
        }



        /// <summary>
        /// function to perform get and put request for transcript
        /// </summary>
        /// <param name="lTranscript"></param>
        /// <returns>it returns true or false for reporting function</returns>
        public bool Transcript(object[] lTranscript)
        {
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            Hashtable hTable = new Hashtable();
            hTable = oGeneric.GetTestData(lTranscript);

            switch (hTable["Function"].ToString().ToUpper().Trim())
            {
                case "GET":
                    return GetTranscript(hTable);
                //break;
                case "UPDATE":
                    return UpdateTranscript(hTable);
                    //break;
            }

            return true;
        }




        /// <summary>
        /// function to perform get request for transcript along with verification
        /// </summary>
        /// <param name="hTable">this is data used for this function to perform operation</param>
        /// <returns>true/false</returns>
        private bool GetTranscript(Hashtable hTable)
        {
            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            string courseID = string.Empty;

            bool _Flag = false;
            string _UserID = string.Empty;
            try
            {
                clsPage oPage = new clsPage(iWebdriver);

                _orgID = oPage.GetOrganizationID();
                clsAPI.orgID = _orgID;
                courseID = GetID(hTable["CourseName"].ToString(), hTable["TranscriptFor"].ToString());

                Users oUser = new Users();

                GetAPICredentials oGetAPICredentials = new GetAPICredentials();
                ////////////////////get api credentials for organization

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", _orgID), oUser, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);


                #region get user id by there type

                if (hTable["UserStatus"].ToString().ToUpper().Contains("INACTIVE"))
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.user.Replace("{orgid}", _orgID) + "?F015=" + hTable["UserEmail"].ToString() + "*&userstatus=inactive", oUser, "H1", _ecomLoginKey, _ecomTransactionKey);

                }
                else if (hTable["UserStatus"].ToString().ToUpper().Contains("DELETED"))
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.user.Replace("{orgid}", _orgID) + "?F015=" + hTable["UserEmail"].ToString() + "*&userstatus=Deleted", oUser, "H1", _ecomLoginKey, _ecomTransactionKey);

                }
                else
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.user.Replace("{orgid}", _orgID), oUser, "H1", _ecomLoginKey, _ecomTransactionKey);

                }
                var oUserData = JsonConvert.DeserializeObject<List<Users>>(result);




                for (int iUser = 0; iUser < oUserData.Count; iUser++)
                {
                    if (oUserData[iUser].Profile.F015.ToString() == hTable["UserMailID"].ToString().Trim())
                    {
                        _UserID = oUserData[iUser].ID;
                        break;
                    }
                }

                #endregion

                #region Activate deactivate course
                clsAPI_Courses oclsAPI_Courses = new clsAPI_Courses(iWebdriver);

                oclsAPI_Courses.ActivateDeactiveCourse(hTable["CourseName"].ToString(), hTable["CourseStatus"].ToString().ToUpper());

                #endregion

                //////////function to perform transcript function
                if (hTable["TranscriptFor"].ToString().ToUpper() == "CURRICULA")
                {
                    _Flag = CurriclaTranscript(_orgID, _UserID, hTable["TestCaseType"].ToString(), hTable["CourseName"].ToString());
                }
                else if (hTable["TranscriptFor"].ToString().ToUpper() == "SESSION")
                { }
                else
                {
                    _Flag = GetUserTranscript(_orgID, _UserID, hTable["TestCaseType"].ToString(), hTable["CourseName"].ToString());
                }
                if (_Flag == true)
                { return true; }
                else { return false; }
            }
            catch (Exception er)
            {
                clsGlobalVariable.strExceptionReport = "Something went wrong while fetching data from api";
                return false;
            }

        }


        /// <summary>
        /// update transcript function for put api request
        /// </summary>
        /// <param name="hTable">input from transcript function</param>
        /// <returns>true/false</returns>
        private bool UpdateTranscript(Hashtable hTable)
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

            try
            {

                Transcript oTranscript = new ILMS.Transcript();
                clsAPITranscript oclsAPITranscript = new clsAPITranscript();


                clsPage oPage = new clsPage(iWebdriver);

                _orgID = oPage.GetOrganizationID();
                clsAPI.orgID = _orgID;


                Users oUser = new Users();

                GetAPICredentials oGetAPICredentials = new GetAPICredentials();
                ////////////////////get api credentials for organization

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", _orgID), oUser, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);


                #region get user id by there type

                if (hTable["UserStatus"].ToString().ToUpper().Contains("INACTIVE"))
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.user.Replace("{orgid}", _orgID) + "?F015=" + hTable["UserEmail"].ToString() + "*&userstatus=inactive", oUser, "H1", _ecomLoginKey, _ecomTransactionKey);

                }
                else if (hTable["UserStatus"].ToString().ToUpper().Contains("DELETED"))
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.user.Replace("{orgid}", _orgID) + "?F015=" + hTable["UserEmail"].ToString() + "*&userstatus=Deleted", oUser, "H1", _ecomLoginKey, _ecomTransactionKey);

                }
                else
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.user.Replace("{orgid}", _orgID), oUser, "H1", _ecomLoginKey, _ecomTransactionKey);

                }
                var oUserData = JsonConvert.DeserializeObject<List<Users>>(result);




                for (int iUser = 0; iUser < oUserData.Count; iUser++)
                {
                    if (oUserData[iUser].Profile.F015.ToString() == hTable["UserMailID"].ToString().Trim())
                    {
                        _UserID = oUserData[iUser].ID;
                        break;
                    }
                }

                #endregion

                #region course
                if (hTable["TranscriptFor"].ToString().ToUpper() == "COURSE")
                {
                    try
                    {
                        courseID = GetID(hTable["CourseName"].ToString(), hTable["TranscriptFor"].ToString());
                        ///old transcript
                        oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetSpecificCourseTranscript.Replace("{orgID}", clsAPI.orgID).Replace("{userID}", _UserID).Replace("{courseID}", courseID), oclsAPITranscript, "H1", _ecomLoginKey, _ecomTransactionKey);


                        oTranscript.CompletionDate = hTable["CompletionDate"].ToString();
                        oTranscript.EnrollmentDate = hTable["EnrollmentDate"].ToString();
                        oTranscript.Score = hTable["Score"].ToString();
                        oTranscript.StartDate = hTable["StartDate"].ToString();
                        oTranscript.Status = hTable["Status"].ToString();
                        oTranscript.LicenseExpirationDate = hTable["LicenseExpirationDate"].ToString();
                        oTranscript.RequirementType = hTable["RequirementType"].ToString();
                        oTranscript.DueDate = hTable["DueDate"].ToString();
                        oTranscript.CertificationExpirationDate = hTable["CertificationExpirationDate"].ToString();

                        /////update transcript
                        string resultupdate = string.Empty;
                        string resultnew = string.Empty;
                        oGeneric.GetApiResponseCodeData(out statusCodeUpdate, out resultupdate, "PUT", ApplicationSettings.APIURI() + clsAPI.GetSpecificCourseTranscript.Replace("{orgID}", clsAPI.orgID).Replace("{userID}", _UserID).Replace("{courseID}", courseID), oclsAPITranscript, "H1", _ecomLoginKey, _ecomTransactionKey);

                        Thread.Sleep(clsGlobalVariable.iWaitHigh);
                        oGeneric.GetApiResponseCodeData(out statusCode, out resultnew, "GET", ApplicationSettings.APIURI() + clsAPI.GetSpecificCourseTranscript.Replace("{orgID}", clsAPI.orgID).Replace("{userID}", _UserID).Replace("{courseID}", courseID), oclsAPITranscript, "H1", _ecomLoginKey, _ecomTransactionKey);
                        var status = JsonConvert.DeserializeObject<List<clsAPITranscript>>(resultnew);
                        var statusold = JsonConvert.DeserializeObject<List<clsAPITranscript>>(result);

                        _flag = VerifyTranscriptUpdate(result, resultnew, statusCodeUpdate, hTable);

                    }
                    catch (Exception e)
                    {
                        clsException.ExceptionHandler(e, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

                        return false;
                    }

                }
                #endregion
                #region session
                else if (hTable["TranscriptFor"].ToString().ToUpper() == "SESSION")
                {
                    List<string> lCourseId = new List<string>();
                    string resultnew = string.Empty;
                    lCourseId = oPage.GetCourseId(hTable["CourseName"].ToString());
                    clsGenericAPI oclsGenericAPI = new clsGenericAPI();
                    oclsGenericAPI.GetORGCredentials(out _ecomTransactionKey, out _ecomLoginKey, _orgID);


                    if (lCourseId.Count > 0)
                    {
                        oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetSessionID.Replace("{orgid}", _orgID).Replace("{CourseID}", lCourseId[0].ToString()), oUser, "H1", _ecomLoginKey, _ecomTransactionKey);


                        var oSessionData = JsonConvert.DeserializeObject<List<clsAPISession>>(result);

                        if (result == "") { return false; }

                        _SessionID = oSessionData[0].SessionID;

                        oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetSpecificSessionTranscript.Replace("{orgID}", _orgID).Replace("{CourseID}", lCourseId[0].ToString()).Replace("{sessionID}", _SessionID).Replace("{userID}", _UserID), oUser, "H1", _ecomLoginKey, _ecomTransactionKey);

                    
                        _flag = VerifyTranscriptUpdate(result, resultnew, statusCodeUpdate, hTable);

                    }
                    else { return false; }

                }
                #endregion
                #region Curricula
                else
                {

                    string resultnew = string.Empty;
                    courseID = oPage.GetCurriculumId(hTable["CourseName"].ToString());


                    try
                    {//////////////////////getting transcript

                        oTranscript.CompletionDate = hTable["CompletionDate"].ToString();
                        oTranscript.EnrollmentDate = hTable["EnrollmentDate"].ToString();
                        oTranscript.Score = hTable["Score"].ToString();
                        oTranscript.StartDate = hTable["StartDate"].ToString();
                        oTranscript.Status = hTable["Status"].ToString();
                        oTranscript.LicenseExpirationDate = hTable["LicenseExpirationDate"].ToString();
                        oTranscript.RequirementType = hTable["RequirementType"].ToString();
                        oTranscript.DueDate = hTable["DueDate"].ToString();
                        oTranscript.CertificationExpirationDate = hTable["CertificationExpirationDate"].ToString();

                        oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetSpecificCurriculumTranscript.Replace("{orgID}", _orgID).Replace("{userID}", _UserID).Replace("{courseID}", courseID), oclsAPITranscript, "H1", _ecomLoginKey, _ecomTransactionKey);

                        var status = JsonConvert.DeserializeObject<List<clsAPITranscript>>(result);


                        oGeneric.GetApiResponseCodeData(out statusCodeUpdate, out resultput, "PUT", ApplicationSettings.APIURI() + clsAPI.GetSpecificCurriculumTranscript.Replace("{orgID}", _orgID).Replace("{userID}", _UserID).Replace("{courseID}", courseID), oclsAPITranscript, "H1", _ecomLoginKey, _ecomTransactionKey);
                        Thread.Sleep(clsGlobalVariable.iWaitHigh);

                        oGeneric.GetApiResponseCodeData(out statusCode, out resultnew, "GET", ApplicationSettings.APIURI() + clsAPI.GetSpecificCurriculumTranscript.Replace("{orgID}", _orgID).Replace("{userID}", _UserID).Replace("{courseID}", courseID), oclsAPITranscript, "H1", _ecomLoginKey, _ecomTransactionKey);
                        var statusold = JsonConvert.DeserializeObject<List<clsAPITranscript>>(result);


                        _flag = VerifyTranscriptUpdate(result, resultnew, statusCodeUpdate, hTable);

                    }
                    catch { clsGlobalVariable.strExceptionReport = "Get transcript scenario failed"; return false; }
                    return _flag;


                }
                #endregion


            }
            catch (Exception e)
            {
                clsException.ExceptionHandler(e, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

                return false;
            }




            return _flag;
        }

        /// <summary>
        /// /function to get course id depending on course name and search flag like session or curricula
        /// </summary>
        /// <param name="CourseName">first parameter</param>
        /// <param name="SearchFlag">curricula,session,course</param>
        /// <returns>string value</returns>
        public string GetID(string CourseName, string SearchFlag)
        {
            List<string> courseID = new List<string>();
            clsPage oPage = new clsPage(iWebdriver);
            string ID = string.Empty;
            if (SearchFlag.ToUpper() == "CURRICULA")
            {
                ID = oPage.GetCurriculumId(CourseName);
            }
            else if (SearchFlag.ToUpper() == "SESSION")
            {
                courseID = oPage.GetCourseId(CourseName);
                if (courseID.Count > 0)
                {
                    ID = courseID[0].ToString();
                }
            }
            else if (SearchFlag.ToUpper() == "COURSE")
            {
                courseID = oPage.GetCourseId(CourseName);
                if (courseID.Count > 0)
                {
                    ID = courseID[0].ToString();
                }
            }

            return ID;
        }


        /// <summary>
        /// function to get user transcript
        /// </summary>
        /// <param name="orgID">organization id</param>
        /// <param name="userID">user id</param>
        /// <param name="testCaseType">positive/negative</param>
        /// <param name="certificationCourseName">course name</param>
        /// <returns>true/false</returns>
        public bool GetUserTranscript(string orgID, string userID, string testCaseType, string certificationCourseName)
        {
            bool _flag = false;
            string statusCode = string.Empty;
            string result = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            clsAPITranscript oclsAPITranscript = new clsAPITranscript();

            try
            {
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetUserTranscript.Replace("{orgID}", orgID).Replace("{userid}", userID), oclsAPITranscript, "H1", _ecomLoginKey, _ecomTransactionKey);

                if (testCaseType.ToUpper() == "NEGATIVE")
                {

                    if (statusCode == "OK/200") { _flag = true; }
                    else { _flag = false; }
                }
                else if (testCaseType.ToUpper() == "POSITIVE")
                {
                    if (statusCode == "OK/200") { _flag = true; }
                    else { _flag = false; }
                }
                else if (testCaseType == "CERTIFICATION")
                {
                    try
                    {

                        var certCourseID = JsonConvert.DeserializeObject<List<clsAPITranscript>>(result);

                        var courseid = certCourseID.Where(cus => cus.CourseName == certificationCourseName);

                        List<ILMS.clsAPITranscript> t = new List<clsAPITranscript>();
                        t = courseid.ToList();
                        UserEnrollment ouser = new UserEnrollment();
                        ouser.userid = userID;

                        oGeneric.GetApiResponseCodeData(out statusCode, out result, "POST", ApplicationSettings.APIURI() + clsAPI.EnrollRecertification.Replace("{orgid}", orgID).Replace("{CourseID}", t[0].CourseID.ToString()), ouser, "H1", _ecomLoginKey, _ecomTransactionKey);


                        oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetUserTranscript.Replace("{orgID}", orgID).Replace("{courseID}", t[0].CourseID.ToString()).Replace("{userid}", userID), ouser, "H1", _ecomLoginKey, _ecomTransactionKey);


                        var status = JsonConvert.DeserializeObject<List<clsAPITranscript>>(result);

                        if (status.Count > 0)
                        {
                            courseid = status.Where(cus => cus.CourseName == certificationCourseName);
                            t = courseid.ToList();

                            if (t[0].Status.ToUpper() == "NOT STARTED")
                            {
                                _flag = true;
                            }
                            else { _flag = false; }

                        }
                        else
                        {
                            _flag = false;
                        }


                    }
                    catch (Exception er) { clsGlobalVariable.strExceptionReport = "Recertification scenario failed"; return false; }
                }
            }
            catch { clsGlobalVariable.strExceptionReport = "Get transcript scenario failed"; return false; }
            return _flag;

        }


        /// <summary>
        /// function to get user transcript related to curricula
        /// </summary>
        /// <param name="orgID">organization id</param>
        /// <param name="userID">user id</param>
        /// <param name="testCaseType">positive/negative</param>
        /// <param name="certificationCourseName">course name</param>
        /// <returns>true/false</returns>
        public bool CurriclaTranscript(string orgID, string userID, string testcaseType, string certificationCourseName)
        {
            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            string courseID = string.Empty;
            bool _flag = false;
            string lCourseId = string.Empty;
            clsPage oPage = new clsPage(iWebdriver);

            clsAPITranscript oclsAPITranscript = new clsAPITranscript();
            lCourseId = oPage.GetCurriculumId(certificationCourseName);


            try
            {//////////////////////getting transcript
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetSpecificCurriculumTranscript.Replace("{orgID}", orgID).Replace("{userID}", userID).Replace("{courseID}", lCourseId), oclsAPITranscript, "H1", _ecomLoginKey, _ecomTransactionKey);

                var status = JsonConvert.DeserializeObject<List<clsAPITranscript>>(result);

                if (testcaseType.ToUpper() == "NEGATIVE")
                {

                    if (statusCode == "OK/200") { _flag = true; }
                    else { _flag = false; }
                }
                else if (testcaseType.ToUpper() == "POSITIVE")
                {
                    if (statusCode == "OK/200") { _flag = true; }
                    else { _flag = false; }
                }

            }
            catch (Exception e)
            {
                clsException.ExceptionHandler(e, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

                return false;
            }
            return _flag;



        }

        /// <summary>
        /// function to get user transcript related to session
        /// </summary>
        /// <param name="orgID">organization id</param>
        /// <param name="userID">user id</param>
        /// <param name="testCaseType">positive/negative</param>
        /// <param name="certificationCourseName">course name</param>
        /// <returns>true/false</returns>
        public bool SessionTranscript(string orgID, string userID, string testcaseType, string certificationCourseName)
        {
            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            string courseID = string.Empty;
            bool _flag = false;
            string lCourseId = string.Empty;
            clsPage oPage = new clsPage(iWebdriver);

            clsAPITranscript oclsAPITranscript = new clsAPITranscript();
            lCourseId = GetID(certificationCourseName, "SESSION");


            try
            {
                //////////////////////getting session
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetSessionID.Replace("{orgid}", _orgID).Replace("{CourseID}", lCourseId.ToString().Trim()), oclsAPITranscript, "H1", _ecomLoginKey, _ecomTransactionKey);

                var oSessionData = JsonConvert.DeserializeObject<List<clsAPISession>>(result);

                _SessionID = oSessionData[0].SessionID;

                //////////////////////getting transcript
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetSpecificSessionTranscript.Replace("{orgID}", orgID).Replace("{userID}", userID).Replace("{courseID}", lCourseId).Replace("{sessionID}", _SessionID), oclsAPITranscript, "H1", _ecomLoginKey, _ecomTransactionKey);

                var status = JsonConvert.DeserializeObject<List<clsAPITranscript>>(result);

                if (testcaseType.ToUpper() == "NEGATIVE")
                {

                    if (statusCode == "OK/200") { _flag = true; }
                    else { _flag = false; }
                }
                else if (testcaseType.ToUpper() == "POSITIVE")
                {
                    if (statusCode == "OK/200") { _flag = true; }
                    else { _flag = false; }
                }

            }
            catch (Exception e)
            {
                clsException.ExceptionHandler(e, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

                return false;
            }
            return _flag;



        }



        public bool VerifyTranscriptUpdate(string resultold, string resultNew, string statusCodeUpdate, Hashtable hTable)
        {

            bool _flag = true;

            try
            {
                var statusold = JsonConvert.DeserializeObject<List<clsAPITranscript>>(resultold);
                var status = JsonConvert.DeserializeObject<List<clsAPITranscript>>(resultNew);
                if (hTable["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                {
                    if (statusCodeUpdate.Contains("204"))
                    {
                        if (hTable["verification"].ToString() == "")
                        {
                            _flag = false;
                            for (int sCounter = 0; sCounter < status.Count; sCounter++)
                            {
                                for (int jCounter = 0; jCounter < statusold.Count; jCounter++)
                                {

                                    if (status[sCounter].CompletionDate != statusold[jCounter].CompletionDate)
                                    {
                                        _flag = true;
                                    }
                                    else if (status[sCounter].Status != statusold[jCounter].Status)
                                    {
                                        _flag = true;
                                    }
                                    else if (status[sCounter].Score != statusold[jCounter].Score)
                                    {
                                        _flag = true;
                                    }

                                }

                            }
                        }
                        else
                        {

                            string[] Verification = hTable["verification"].ToString().Split(',');
                            foreach (string strverify in Verification)
                            {
                                for (int sCounter = 0; sCounter < status.Count; sCounter++)
                                {
                                    for (int jCounter = 0; jCounter < statusold.Count; jCounter++)
                                    {
                                        if (strverify.ToUpper() == "SCORE")
                                        {
                                            if (status[sCounter].Score == statusold[jCounter].Score)
                                            {
                                                _flag = false;

                                            }
                                        }
                                        else if (strverify.ToUpper() == "STATUS")
                                        {
                                            if (status[sCounter].Status == statusold[jCounter].Status)
                                            {
                                                _flag = false;

                                            }
                                        }
                                        else if (strverify.ToUpper() == "REQUIREMENTTYPE")
                                        {
                                            if (status[sCounter].RequirementType == statusold[jCounter].RequirementType)
                                            {
                                                _flag = false;

                                            }
                                        }
                                        else if (strverify.ToUpper() == "CERTIFICATIONEXPIRATIONDATE")
                                        {
                                            if (status[sCounter].CertificationExpirationDate == statusold[jCounter].CertificationExpirationDate)
                                            {
                                                _flag = false;

                                            }
                                        }
                                        else if (strverify.ToUpper() == "COMPLETIONDATE")
                                        {
                                            if (status[sCounter].CompletionDate == statusold[jCounter].CompletionDate)
                                            {
                                                _flag = false;

                                            }
                                        }
                                        else if (strverify.ToUpper() == "DUEDATE")
                                        {
                                            if (status[sCounter].DueDate == statusold[jCounter].DueDate)
                                            {
                                                _flag = false;

                                            }
                                        }
                                        else if (strverify.ToUpper() == "STARTDATE")
                                        {
                                            if (status[sCounter].StartDate == statusold[jCounter].StartDate)
                                            {
                                                _flag = false;

                                            }
                                        }
                                        else if (strverify.ToUpper() == "LICENSEEXPIRATIONDATE")
                                        {
                                            if (status[sCounter].LicenseExpirationDate == statusold[jCounter].LicenseExpirationDate)
                                            {
                                                _flag = false;

                                            }
                                        }
                                        else if (strverify.ToUpper() == "ENROLLMENTDATE")
                                        {
                                            if (status[sCounter].EnrollmentDate == statusold[jCounter].EnrollmentDate)
                                            {
                                                _flag = false;

                                            }
                                        }

                                    }
                                }
                            }

                        }
                    }
                    else
                    {
                        _flag = false;
                    }
                }
                else
                {

                    if (statusCodeUpdate.Contains("204"))
                    { _flag = false; }
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
