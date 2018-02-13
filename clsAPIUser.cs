using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System.Collections;
using System.Xml.Linq;
using ILMS.ApplicationSettingsManager;

namespace ILMS
{
    class clsAPIUser
    {
        private IWebDriver iWebdriver;
        private string _ecomTransactionKey = string.Empty;
        private string _ecomLoginKey = string.Empty;
        private string _orgID = string.Empty;

        public clsAPIUser(IWebDriver iWeb)
        {
            this.iWebdriver = iWeb;
        }

        //Created by Niranjan
        //Modified by Sandeep
        //Modified by Niranjan - 20/7/2016
        public bool APICreateUser(object[] lStrvalue)
        {

            //UsersData odata = new UsersData();
            try
            {
                clsGlobalVariable.strExceptionReport = string.Empty;
                string statusCode = string.Empty;
                string result = string.Empty;
                //string allCourseResultresult = string.Empty;
                //string singleCourseresult = string.Empty;
                clsGeneric oGeneric = new clsGeneric();
                string URI = string.Empty;
                Hashtable htblTestData = new Hashtable();
                htblTestData = oGeneric.GetTestData(lStrvalue);
                bool _Flag = false;

                User userID = new User();

                GetAPICredentials oGetAPICredentials = new GetAPICredentials();

                //Get Org ID at runtime
                clsPage oPage = new clsPage(iWebdriver);
                _orgID = oPage.GetOrganizationID();
                clsAPI.orgID = _orgID;


                Thread.Sleep(clsGlobalVariable.iWaitHigh);

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", clsAPI.apiURI + clsAPI.orgCredential.Replace("$", clsAPI.orgID), oGetAPICredentials, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);

                UpdateUser ouser = new UpdateUser();
                CreateUser cuser = new CreateUser();

                XDocument doc = XDocument.Load(clsGlobalVariable.ProjectDirectory+ htblTestData["FieldsToBeUpdated"].ToString());

                List<UpdateUser> oList = doc.Root.Elements()
                                        .Select(x => new UpdateUser()
                                        {
                                            Key = x.Attribute("Key").Value,
                                            Value = x.Attribute("Value").Value
                                        }).ToList();

                for (int i = 0; i <= oList.Count; i++)
                {
                    cuser.userProfileData = oList;
                }
                cuser.sendRegistrationMail = "true";
                cuser.changePasswordAtNextLogin = "false";

                //Update Fields Value
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "POST", clsAPI.apiURI + clsAPI.user.Replace("{orgid}", clsAPI.orgID), cuser, "H1", _ecomLoginKey, _ecomTransactionKey);

                //1st code checks whether the test cases is Negative or Positive
                if (htblTestData["TestCaseType"].ToString().ToUpper() == "NEGATIVE")
                {
                    //If Negative then we expect an error code
                    if (result.Contains("ErrorCode") || result.Contains("Invalid URI."))
                    {
                        if (result.Contains("Invalid URI."))
                        {
                            return true;
                        }
                        //The error code can be from API of Course Pushdown
                        SuperAdminCoursePushDownErrorCode oError = new SuperAdminCoursePushDownErrorCode();
                        var error = JsonConvert.DeserializeObject<List<SuperAdminCoursePushDownError>>(result);

                        foreach (string errorcode in oError.coursePushDownErrorCode)
                        {
                            for (int i = 0; i < error.Count; i++)
                            {
                                if (error[i].ErrorCode == errorcode)
                                {
                                    _Flag = true;
                                }
                                else
                                {
                                    _Flag = false;
                                }
                            }
                            if (_Flag == true) break;
                        }

                        if (_Flag == false)
                        {
                            //If Not from Course Push Down then The error code can be from General APIs
                            GeneralAPIErrCodes oError1 = new GeneralAPIErrCodes();
                            var error1 = JsonConvert.DeserializeObject<List<GeneralAPIErrCodes>>(result);
                            foreach (string errorcode in oError1.allAPIErrorCodes)
                            {
                                for (int i = 0; i < error.Count; i++)
                                {
                                    if (error[i].ErrorCode == errorcode)
                                    {
                                        _Flag = true;
                                        if (_Flag == true) break;
                                    }
                                    else
                                    {
                                        _Flag = false;
                                    }
                                }
                                if (_Flag == true) break;
                            }
                        }
                    }
                    if (_Flag == true)
                    { return _Flag; }
                }
                else
                {
                    //If the test case is positive control will come here and then user should be created without any error code
                    if (statusCode == "Created/201")
                    {
                        _Flag = true;
                    }
                    else
                    {
                        _Flag = false;
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

        public bool APIGetUser(object[] lStrvalue)
        {

            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            string allCourseResultresult = string.Empty;
            string singleCourseresult = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            Hashtable htblTestData = new Hashtable();
            htblTestData = oGeneric.GetTestData(lStrvalue);
            bool _Flag = false;

            UserEnrollment userID = new UserEnrollment();

            GetAPICredentials oGetAPICredentials = new GetAPICredentials();

            //Get Org ID at runtime
            clsPage oPage = new clsPage(iWebdriver);
            _orgID = oPage.GetOrganizationID();
            clsAPI.orgID = _orgID;
            try
            {
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", clsAPI.orgID), oGetAPICredentials, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);

                if (htblTestData["RequestType"].ToString() == "All")
                {

                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.user.Replace("{orgid}", clsAPI.orgID), userID, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);
                    var userData = JsonConvert.DeserializeObject<List<Users>>(result);
                    if (userData.Count > 1)
                    {
                        _Flag = true;
                    }
                    else
                    {
                        _Flag = false;
                    }

                    return _Flag;
                }

                if (htblTestData["RequestType"].ToString() == "Single")
                {
                    //Get All Users
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.user.Replace("{orgid}", clsAPI.orgID), userID, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);

                    var userData = JsonConvert.DeserializeObject<List<Users>>(result);

                    if (userData.Count > 0)
                    {
                        //Find the User that we created and get its ID
                        for (int iEnrolUser = 0; iEnrolUser < userData.Count; iEnrolUser++)
                        {
                            if (userData[iEnrolUser].Profile.F015.ToString().Contains(htblTestData["UserEmail"].ToString()))
                            {
                                string userid = userData[iEnrolUser].ID.ToString();

                                //Use the ID to create a specific URL to get particular user
                                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetSingleUser.Replace("{orgID}", clsAPI.orgID).Replace("{UserID}", userid), userID, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);

                                var singleUserData = JsonConvert.DeserializeObject<Users>(result);

                                if (singleUserData.Profile.F015.Contains(htblTestData["UserEmail"].ToString()))
                                {
                                    _Flag = true;
                                    break;
                                }
                                else { _Flag = false; }
                            }
                            else
                            {
                                _Flag = false;
                            }
                        }
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

        public bool APIPutUser(object[] lStrvalue)
        {

            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            string allCourseResultresult = string.Empty;
            string singleCourseresult = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            Hashtable htblTestData = new Hashtable();
            htblTestData = oGeneric.GetTestData(lStrvalue);
            bool _Flag = false;

            User userID = new User();

            GetAPICredentials oGetAPICredentials = new GetAPICredentials();

            //Get Org ID at runtime
            clsPage oPage = new clsPage(iWebdriver);
            _orgID = oPage.GetOrganizationID();
            clsAPI.orgID = _orgID;

            Thread.Sleep(clsGlobalVariable.iWaitHigh);
            try
            {
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", clsAPI.apiURI + clsAPI.orgCredential.Replace("$", clsAPI.orgID), oGetAPICredentials, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);

                //Get User ID based on Email Address which is passed XML and update Fields Value
                if (htblTestData["Action"].ToString() == "Activate")
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", clsAPI.apiURI + clsAPI.GetUserBasedonEmail.Replace("{orgID}", clsAPI.orgID).Replace("{EmailID}", htblTestData["UserEmail"].ToString() + "&userstatus=inactive"), userID, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);
                }
                else
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", clsAPI.apiURI + clsAPI.GetUserBasedonEmail.Replace("{orgID}", clsAPI.orgID).Replace("{EmailID}", htblTestData["UserEmail"].ToString()), userID, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);
                }

                var singleUserData = JsonConvert.DeserializeObject<List<Users>>(result);

                string userid = singleUserData[0].ID.ToString();

                if (htblTestData["Action"].ToString() == "Update")
                {

                    UpdateUser ouser = new UpdateUser();

                    //List<UpdateUser> oList = new List<UpdateUser>();

                    XDocument doc = XDocument.Load(clsGlobalVariable.ProjectDirectory+ htblTestData["FieldsToBeUpdated"].ToString());

                    List<UpdateUser> oList = doc.Root.Elements()
                                            .Select(x => new UpdateUser()
                                            {
                                                Key = x.Attribute("Key").Value,
                                                Value = x.Attribute("Value").Value
                                            }).ToList();

                    //Update Fields Value
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "PUT", clsAPI.apiURI + clsAPI.UpdateUserProfile.Replace("{orgID}", clsAPI.orgID).Replace("{UserID}", userid), oList, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);
                }
                if (htblTestData["Action"].ToString() == "Activate")
                {

                    //Activate Inactive User
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "PUT", clsAPI.apiURI + clsAPI.UpdateUserProfile.Replace("{orgID}", clsAPI.orgID).Replace("{UserID}", userid) + "/Activate", userID, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);
                }
                if (htblTestData["Action"].ToString() == "Inactivate")
                {

                    //InActivate Active User
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "Delete", clsAPI.apiURI + clsAPI.UpdateUserProfile.Replace("{orgID}", clsAPI.orgID).Replace("{UserID}", userid) + "/Activate", userID, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);
                }


                if (statusCode == "NoContent/204")
                { _Flag = true; }
                else if (statusCode == "OK/200") { _Flag = true; }
                else { _Flag = false; }
                return _Flag;

            }
            catch (Exception e)
            {
                clsException.ExceptionHandler(e, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

                return false;
            }

        }

        public bool APIDelete_PurgeUser(object[] lStrvalue)
        {

            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            string allCourseResultresult = string.Empty;
            string singleCourseresult = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            Hashtable htblTestData = new Hashtable();
            htblTestData = oGeneric.GetTestData(lStrvalue);
            bool _Flag = false;

            User userID = new User();

            GetAPICredentials oGetAPICredentials = new GetAPICredentials();

            //Get Org ID at runtime
            clsPage oPage = new clsPage(iWebdriver);
            _orgID = oPage.GetOrganizationID();
            clsAPI.orgID = _orgID;
            Thread.Sleep(clsGlobalVariable.iWaitHigh);
            try
            {
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", clsAPI.apiURI + clsAPI.orgCredential.Replace("$", clsAPI.orgID), oGetAPICredentials, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);

                //Get User ID based on Email Address which is passed XML and update Fields Value


                if (htblTestData["Action"].ToString() == "DeleteActiveUsers")
                {

                    //Delete User
                    string userid = GetUserID("active", oGeneric, "", statusCode, htblTestData, userID);
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "DELETE", clsAPI.apiURI + clsAPI.UpdateUserProfile.Replace("{orgID}", clsAPI.orgID).Replace("{UserID}", userid), userID, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);
                }
                if (htblTestData["Action"].ToString() == "DeleteInactiveUsers")
                {

                    //Delete User
                    string userid = GetUserID("inactive", oGeneric, "", statusCode, htblTestData, userID);
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "DELETE", clsAPI.apiURI + clsAPI.UpdateUserProfile.Replace("{orgID}", clsAPI.orgID).Replace("{UserID}", userid), userID, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);
                }
                if (htblTestData["Action"].ToString() == "Purge")
                {

                    //Purge on Deleted User
                    string userid = GetUserID("deleted", oGeneric, "", statusCode, htblTestData, userID);
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "DELETE", clsAPI.apiURI + clsAPI.UpdateUserProfile.Replace("{orgID}", clsAPI.orgID).Replace("{UserID}", userid) + "/Purge", userID, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);
                }

                if (statusCode == "NoContent/204" || statusCode == "OK/200" || statusCode == "/504")
                {
                    _Flag = true;
                }
                else
                {
                    _Flag = false;
                }

                return _Flag;
            }
            catch (Exception e)
            {
                clsException.ExceptionHandler(e, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

                return false;
            }
        }

        public bool APIChangePassword(object[] lStrvalue)
        {

            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            string allCourseResultresult = string.Empty;
            string singleCourseresult = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            Hashtable htblTestData = new Hashtable();
            htblTestData = oGeneric.GetTestData(lStrvalue);
            bool _Flag = false;

            User userID = new User();

            GetAPICredentials oGetAPICredentials = new GetAPICredentials();

            //Get Org ID at runtime
            clsPage oPage = new clsPage(iWebdriver);
            _orgID = oPage.GetOrganizationID();
            clsAPI.orgID = _orgID;

            Thread.Sleep(clsGlobalVariable.iWaitHigh);
            try
            {
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", clsAPI.apiURI + clsAPI.orgCredential.Replace("$", clsAPI.orgID), oGetAPICredentials, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);

                ChangePasswordAPI ouser = new ChangePasswordAPI();
                //Get User ID based on Email Address which is passed XML and Change Password
                string userid = GetUserID("active", oGeneric, "", statusCode, htblTestData, userID);
                ouser.newpassword = htblTestData["NewPassword"].ToString();
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "PUT", clsAPI.apiURI + clsAPI.UpdateUserProfile.Replace("{orgID}", clsAPI.orgID).Replace("{UserID}", userid) + "/Password", ouser, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);

                if (statusCode == "NoContent/204")
                { _Flag = true; }
                else if (statusCode == "OK/200") { _Flag = true; }
                else { _Flag = false; }
                return _Flag;
            }
            catch (Exception e)
            {
                clsException.ExceptionHandler(e, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

                return false;
            }

        }

        private string GetUserID(string status, clsGeneric oGeneric, string statusCode, string result, Hashtable htblTestData, User userID)
        {
            string userid;
            try
            {
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", clsAPI.apiURI + clsAPI.GetUserBasedonEmail.Replace("{orgID}", clsAPI.orgID).Replace("{EmailID}", htblTestData["UserEmail"].ToString() + "&userstatus=" + status), userID, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);

                var singleUserData = JsonConvert.DeserializeObject<List<Users>>(result);
                userid = singleUserData[0].ID.ToString();

                return userid;
            }
            catch (Exception e)
            {
                clsException.ExceptionHandler(e, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
                userid = "User_ID_not_Found";
                return userid;
            }
        }

    }
}
