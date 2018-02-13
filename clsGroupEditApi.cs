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
    class clsGroupEditApi
    {

        private IWebDriver iWebdriver;
        private string _ecomTransactionKey = string.Empty;
        private string _ecomLoginKey = string.Empty;
        private string _orgID = string.Empty;

        public clsGroupEditApi(IWebDriver iWeb)
        {
            this.iWebdriver = iWeb;
        }


        public bool AddDeleteMemberFrmGroup(object[] oValue)
        {


            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            Hashtable hTable = new Hashtable();
            hTable = oGeneric.GetTestData(oValue);

            switch (hTable["Function"].ToString().ToUpper().Trim())
            {
                case "ADD":

                    return AddUser(hTable);
                //break;
                case "REMOVE":
                    return RemoveUser(hTable);
                    //break;
            }
            return true;
        }



        public bool ExplicitAddDeleteMemberFrmGroup(object[] oValue)
        {


            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            Hashtable hTable = new Hashtable();
            hTable = oGeneric.GetTestData(oValue);

            switch (hTable["Function"].ToString().ToUpper().Trim())
            {
                case "ADD":

                    return ExpAddUser(hTable);
                //break;
                case "REMOVE":
                    return ExpRemoveUser(hTable);
                    //break;
            }
            return true;
        }



        public bool AddUser(Hashtable hTable)
        {

            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            string GroupResult = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            string strGroupID = string.Empty;

            List<string> lCourseId = new List<string>();
            bool _Flag = false;
            string _UserID = string.Empty;
            try
            {
                clsPage oPage = new clsPage(iWebdriver);

                _orgID = oPage.GetOrganizationID();
                clsAPI.orgID = _orgID;


                Users oUser = new Users();
                clsGroupAPI clGrpData = new clsGroupAPI();
                GetAPICredentials oGetAPICredentials = new GetAPICredentials();
                clsGroupUser oGrpUser = new clsGroupUser();

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", _orgID), oUser, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);



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

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetAllGroups.Replace("{orgid}", _orgID), clGrpData, "H1", _ecomLoginKey, _ecomTransactionKey);

                var groupData = JsonConvert.DeserializeObject<List<clsGroupAPI>>(result);

                if (groupData.Count > 0)
                {
                    for (int iEnrolUser = 0; iEnrolUser < groupData.Count; iEnrolUser++)
                    {
                        if (groupData[iEnrolUser].Name.ToString().Contains(hTable["GroupName"].ToString()))
                        {
                            strGroupID = groupData[iEnrolUser].ID.ToString();
                            _Flag = true; break;
                        }
                        else
                        {
                            _Flag = false;

                        }
                    }
                }

                if (_Flag == false)
                {
                    clsGlobalVariable.strExceptionReport = "GroupID not present.";
                    return false;
                }
                else
                {
                    _Flag = false;

                    oGrpUser.userid = _UserID;
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.AddUserToGroup.Replace("{orgID}", _orgID).Replace("{groupID}", strGroupID), oGrpUser, "H1", _ecomLoginKey, _ecomTransactionKey);


                    var grpData = JsonConvert.DeserializeObject<List<Users>>(result);


                    oGeneric.GetApiResponseCodeData(out statusCode, out GroupResult, "POST", ApplicationSettings.APIURI() + clsAPI.AddUserToGroup.Replace("{orgID}", _orgID).Replace("{groupID}", strGroupID), oGrpUser, "H1", _ecomLoginKey, _ecomTransactionKey);


                    Thread.Sleep(clsGlobalVariable.iWaitHigh);
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.AddUserToGroup.Replace("{orgID}", _orgID).Replace("{groupID}", strGroupID), oGrpUser, "H1", _ecomLoginKey, _ecomTransactionKey);

                    var AddedMemberData = JsonConvert.DeserializeObject<List<Users>>(result);


                    if (hTable["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                    {
                        if (grpData.Count == 0 && AddedMemberData.Count > groupData.Count)
                        {
                            return true;

                        }

                        for (int iNew = 0; iNew < AddedMemberData.Count; iNew++)
                        {

                            if (AddedMemberData[iNew].Profile.F015.ToString() == hTable["UserMailID"].ToString())
                            {
                                return true;
                            }
                        }

                    }
                    else
                    {

                        if (GroupResult.Contains("ErrorCode") || GroupResult.Contains("Invalid URI."))
                        {
                            if (result.Contains("Invalid URI."))
                            {
                                return true;
                            }


                            SuperAdminCoursePushDownErrorCode oError = new SuperAdminCoursePushDownErrorCode();
                            var error = JsonConvert.DeserializeObject<List<SuperAdminCoursePushDownError>>(GroupResult);

                            foreach (string errorcode in oError.coursePushDownErrorCode)
                            {
                                for (int i = 0; i < error.Count; i++)
                                {
                                    if (error[i].ErrorCode == errorcode) { _Flag = true; }
                                    else
                                    {

                                        _Flag = false;
                                    }
                                }
                                if (_Flag == true) break;
                            }
                            return _Flag;
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


        public bool ExpAddUser(Hashtable hTable)
        {

            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            string GroupResult = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            string strGroupID = string.Empty;

            List<string> lCourseId = new List<string>();
            bool _Flag = false;
            string _UserID = string.Empty;
            try
            {
                clsPage oPage = new clsPage(iWebdriver);

                _orgID = oPage.GetOrganizationID();
                clsAPI.orgID = _orgID;
                //clsAPI.orgID = "2808";
                //_orgID = "2808";

                Users oUser = new Users();
                clsGroupAPI clGrpData = new clsGroupAPI();
                GetAPICredentials oGetAPICredentials = new GetAPICredentials();
                clsGroupUser oGrpUser = new clsGroupUser();

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", _orgID), oUser, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);



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

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetAllGroups.Replace("{orgid}", _orgID), clGrpData, "H1", _ecomLoginKey, _ecomTransactionKey);

                var groupData = JsonConvert.DeserializeObject<List<clsGroupAPI>>(result);

                if (groupData.Count > 0)
                {
                    for (int iEnrolUser = 0; iEnrolUser < groupData.Count; iEnrolUser++)
                    {
                        if (groupData[iEnrolUser].Name.ToString().Contains(hTable["GroupName"].ToString()))
                        {
                            strGroupID = groupData[iEnrolUser].ID.ToString();
                            _Flag = true; break;
                        }
                        else
                        {
                            _Flag = false;

                        }
                    }
                }

                if (_Flag == false)
                {
                    clsGlobalVariable.strExceptionReport = "GroupID not present.";
                    return false;
                }
                else
                {
                    _Flag = false;

                    oGrpUser.userid = _UserID;
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetGroupExplicitInclUsers.Replace("{orgid}", _orgID).Replace("{GroupID}", strGroupID), oGrpUser, "H1", _ecomLoginKey, _ecomTransactionKey);


                    var grpData = JsonConvert.DeserializeObject<List<Users>>(result);


                    oGeneric.GetApiResponseCodeData(out statusCode, out GroupResult, "POST", ApplicationSettings.APIURI() + clsAPI.GetGroupExplicitInclUsers.Replace("{orgid}", _orgID).Replace("{GroupID}", strGroupID), oGrpUser, "H1", _ecomLoginKey, _ecomTransactionKey);


                    Thread.Sleep(clsGlobalVariable.iWaitHigh);
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetGroupExplicitInclUsers.Replace("{orgid}", _orgID).Replace("{GroupID}", strGroupID), oGrpUser, "H1", _ecomLoginKey, _ecomTransactionKey);

                    var AddedMemberData = JsonConvert.DeserializeObject<List<Users>>(result);


                    if (hTable["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                    {
                        if (hTable["RemoveFromList"].ToString().ToUpper() == "YES")
                        {
                            oGeneric.GetApiResponseCodeData(out statusCode, out GroupResult, "DELETE", ApplicationSettings.APIURI() + clsAPI.GetGroupExplicitInclUsers.Replace("{orgid}", _orgID).Replace("{GroupID}", strGroupID) + "/" + _UserID, oGrpUser, "H1", _ecomLoginKey, _ecomTransactionKey);

                            Thread.Sleep(clsGlobalVariable.iWaitMedium);

                            oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetGroupExplicitInclUsers.Replace("{orgid}", _orgID).Replace("{GroupID}", strGroupID), oGrpUser, "H1", _ecomLoginKey, _ecomTransactionKey);

                            var removedMemberData = JsonConvert.DeserializeObject<List<Users>>(result);
                            for (int iOld = 0; iOld < removedMemberData.Count; iOld++)
                            {
                                if (AddedMemberData[iOld].Profile.F015.ToString() == hTable["UserMailID"].ToString())
                                {
                                    _Flag = false; break;

                                }
                                else { _Flag = true; }

                            }

                            if (_Flag == true)
                            { return true; }
                        }


                        if (grpData.Count == 0 && AddedMemberData.Count > groupData.Count)
                        {
                            return true;

                        }

                        for (int iNew = 0; iNew < AddedMemberData.Count; iNew++)
                        {

                            if (AddedMemberData[iNew].Profile.F015.ToString() == hTable["UserMailID"].ToString())
                            {
                                return true;
                            }
                        }

                    }
                    else
                    {

                        if (GroupResult.Contains("ErrorCode") || GroupResult.Contains("Invalid URI."))
                        {
                            if (result.Contains("Invalid URI."))
                            {
                                return true;
                            }


                            SuperAdminCoursePushDownErrorCode oError = new SuperAdminCoursePushDownErrorCode();
                            var error = JsonConvert.DeserializeObject<List<SuperAdminCoursePushDownError>>(GroupResult);

                            foreach (string errorcode in oError.coursePushDownErrorCode)
                            {
                                for (int i = 0; i < error.Count; i++)
                                {
                                    if (error[i].ErrorCode == errorcode) { _Flag = true; break; }
                                    else
                                    {

                                        _Flag = false;
                                    }
                                }
                                if (_Flag == true) { break; }
                            }

                            if (_Flag == true) { return true; } else { return false; }
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


        public bool RemoveUser(Hashtable hTable)
        {
            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            string strGroupID = string.Empty;
            string GroupResult = string.Empty;
            List<string> lCourseId = new List<string>();
            bool _Flag = false;
            string _UserID = string.Empty;
            try
            {
                clsPage oPage = new clsPage(iWebdriver);

                _orgID = oPage.GetOrganizationID();
                clsAPI.orgID = _orgID;


                Users oUser = new Users();
                clsGroupAPI clGrpData = new clsGroupAPI();
                GetAPICredentials oGetAPICredentials = new GetAPICredentials();
                clsGroupUser oGrpUser = new clsGroupUser();

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", _orgID), oUser, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);



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
                Thread.Sleep(clsGlobalVariable.iWaitHigh);
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetAllGroups.Replace("{orgid}", _orgID), clGrpData, "H1", _ecomLoginKey, _ecomTransactionKey);

                var groupData = JsonConvert.DeserializeObject<List<clsGroupAPI>>(result);

                if (groupData.Count > 0)
                {
                    for (int iEnrolUser = 0; iEnrolUser < groupData.Count; iEnrolUser++)
                    {
                        if (groupData[iEnrolUser].Name.ToString().Contains(hTable["GroupName"].ToString()))
                        {
                            strGroupID = groupData[iEnrolUser].ID.ToString();
                            _Flag = true; break;
                        }
                        else
                        {
                            _Flag = false;

                        }
                    }
                }

                if (_Flag == false)
                {
                    clsGlobalVariable.strExceptionReport = "GroupID not present.";
                    return false;
                }
                else
                {
                    _Flag = false;

                    oGrpUser.userid = _UserID;
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.AddUserToGroup.Replace("{orgID}", _orgID).Replace("{groupID}", strGroupID), oGrpUser, "H1", _ecomLoginKey, _ecomTransactionKey);


                    var grpData = JsonConvert.DeserializeObject<List<Users>>(result);


                    oGeneric.GetApiResponseCodeData(out statusCode, out GroupResult, "DELETE", ApplicationSettings.APIURI() + clsAPI.RemoveUserToGroup.Replace("{orgID}", _orgID).Replace("{groupID}", strGroupID).Replace("{userID}", _UserID), oGrpUser, "H1", _ecomLoginKey, _ecomTransactionKey);


                    Thread.Sleep(clsGlobalVariable.iWaitHigh);
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.AddUserToGroup.Replace("{orgID}", _orgID).Replace("{groupID}", strGroupID), oGrpUser, "H1", _ecomLoginKey, _ecomTransactionKey);

                    var AddedMemberData = JsonConvert.DeserializeObject<List<Users>>(result);
                    if (result == "") { return true; }

                    if (hTable["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                    {


                        for (int iNew = 0; iNew < AddedMemberData.Count; iNew++)
                        {

                            for (int iOld = 0; iOld < AddedMemberData.Count; iOld++)
                            {
                                if (AddedMemberData[iNew].Profile.F015.ToString() != hTable["UserMailID"].ToString())
                                {
                                    _Flag = true;

                                }
                                else { _Flag = false; break; }

                            }
                            if (_Flag == false) { break; }
                        }


                        if (_Flag == false) { return true; } else { return false; }

                    }
                    else
                    {
                        if (GroupResult.Contains("ErrorCode") || GroupResult.Contains("Invalid URI."))
                        {
                            if (GroupResult.Contains("Invalid URI."))
                            {
                                return true;
                            }


                            SuperAdminCoursePushDownErrorCode oError = new SuperAdminCoursePushDownErrorCode();
                            var error = JsonConvert.DeserializeObject<List<SuperAdminCoursePushDownError>>(result);

                            foreach (string errorcode in oError.coursePushDownErrorCode)
                            {
                                for (int i = 0; i < error.Count; i++)
                                {
                                    if (error[i].ErrorCode == errorcode) { return true; }
                                    else
                                    {

                                        return false;
                                    }
                                }
                            }
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


        public bool ExpRemoveUser(Hashtable hTable)
        {
            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            string strGroupID = string.Empty;
            string GroupResult = string.Empty;
            List<string> lCourseId = new List<string>();
            bool _Flag = false;
            string _UserID = string.Empty;
            try
            {
                clsPage oPage = new clsPage(iWebdriver);

                _orgID = oPage.GetOrganizationID();
                clsAPI.orgID = _orgID;

                Users oUser = new Users();
                clsGroupAPI clGrpData = new clsGroupAPI();
                GetAPICredentials oGetAPICredentials = new GetAPICredentials();
                clsGroupUser oGrpUser = new clsGroupUser();

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", _orgID), oUser, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);



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
                Thread.Sleep(clsGlobalVariable.iWaitHigh);
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetAllGroups.Replace("{orgid}", _orgID), clGrpData, "H1", _ecomLoginKey, _ecomTransactionKey);

                var groupData = JsonConvert.DeserializeObject<List<clsGroupAPI>>(result);

                if (groupData.Count > 0)
                {
                    for (int iEnrolUser = 0; iEnrolUser < groupData.Count; iEnrolUser++)
                    {
                        if (groupData[iEnrolUser].Name.ToString().Contains(hTable["GroupName"].ToString()))
                        {
                            strGroupID = groupData[iEnrolUser].ID.ToString();
                            _Flag = true; break;
                        }
                        else
                        {
                            _Flag = false;

                        }
                    }
                }

                if (_Flag == false)
                {
                    clsGlobalVariable.strExceptionReport = "GroupID not present.";
                    return false;
                }
                else
                {
                    _Flag = false;

                    oGrpUser.userid = _UserID;
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetGroupExplicitExclUsers.Replace("{orgid}", _orgID).Replace("{GroupID}", strGroupID), oGrpUser, "H1", _ecomLoginKey, _ecomTransactionKey);


                    var grpData = JsonConvert.DeserializeObject<List<Users>>(result);


                    oGeneric.GetApiResponseCodeData(out statusCode, out GroupResult, "POST", ApplicationSettings.APIURI() + clsAPI.GetGroupExplicitExclUsers.Replace("{orgid}", _orgID).Replace("{GroupID}", strGroupID).Replace("{userID}", _UserID), oGrpUser, "H1", _ecomLoginKey, _ecomTransactionKey);


                    Thread.Sleep(clsGlobalVariable.iWaitHigh);
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetGroupExplicitExclUsers.Replace("{orgid}", _orgID).Replace("{GroupID}", strGroupID), oGrpUser, "H1", _ecomLoginKey, _ecomTransactionKey);

                    var AddedMemberData = JsonConvert.DeserializeObject<List<Users>>(result);


                    if (hTable["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                    {


                        for (int iNew = 0; iNew < AddedMemberData.Count; iNew++)
                        {

                            for (int iOld = 0; iOld < AddedMemberData.Count; iOld++)
                            {
                                if (AddedMemberData[iNew].Profile.F015.ToString() == hTable["UserMailID"].ToString())
                                {
                                    _Flag = true; break;

                                }
                                else { _Flag = false; }

                            }

                        }

                        if (hTable["RemoveFromList"].ToString().ToUpper() == "YES")
                        {
                            oGeneric.GetApiResponseCodeData(out statusCode, out GroupResult, "DELETE", ApplicationSettings.APIURI() + clsAPI.GetGroupExplicitExclUsers.Replace("{orgid}", _orgID).Replace("{GroupID}", strGroupID) + "/" + _UserID, oGrpUser, "H1", _ecomLoginKey, _ecomTransactionKey);

                            Thread.Sleep(clsGlobalVariable.iWaitMedium);

                            oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetGroupExplicitExclUsers.Replace("{orgid}", _orgID).Replace("{GroupID}", strGroupID), oGrpUser, "H1", _ecomLoginKey, _ecomTransactionKey);

                            var removedMemberData = JsonConvert.DeserializeObject<List<Users>>(result);
                            for (int iOld = 0; iOld < removedMemberData.Count; iOld++)
                            {
                                if (AddedMemberData[iOld].Profile.F015.ToString() == hTable["UserMailID"].ToString())
                                {
                                    _Flag = false; break;

                                }
                                else { _Flag = true; }

                            }


                        }


                        if (_Flag == true) { return true; } else { return false; }
                    }
                    else
                    {
                        if (GroupResult.Contains("ErrorCode") || GroupResult.Contains("Invalid URI."))
                        {
                            if (GroupResult.Contains("Invalid URI."))
                            {
                                return true;
                            }


                            SuperAdminCoursePushDownErrorCode oError = new SuperAdminCoursePushDownErrorCode();
                            var error = JsonConvert.DeserializeObject<List<SuperAdminCoursePushDownError>>(GroupResult);

                            foreach (string errorcode in oError.coursePushDownErrorCode)
                            {
                                for (int i = 0; i < error.Count; i++)
                                {
                                    if (error[i].ErrorCode == errorcode)
                                    { _Flag = true; break; }
                                    else
                                    {

                                        _Flag = false;
                                    }
                                }
                                if (_Flag == true) break;
                            }
                            return _Flag;
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


        public bool AddRemoveCourse(object[] oValue)
        {
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            Hashtable hTable = new Hashtable();
            hTable = oGeneric.GetTestData(oValue);

            switch (hTable["Function"].ToString().ToUpper().Trim())
            {
                case "ADD":

                    return AddCourse(hTable);
                //break;
                case "REMOVE":
                    return RemoveCourse(hTable);
                    //break;
            }
            return true;

        }


        public bool AddCourse(Hashtable hTable)
        {

            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            string GroupResult = string.Empty;
            string CourseId = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            string strGroupID = string.Empty;

            List<string> lCourseId = new List<string>();
            bool _Flag = false;
            string _UserID = string.Empty;

            try
            {
                clsPage oPage = new clsPage(iWebdriver);

                _orgID = oPage.GetOrganizationID();
                clsAPI.orgID = _orgID = "2808";


                Users oUser = new Users();
                clsGroupAPI clGrpData = new clsGroupAPI();
                GetAPICredentials oGetAPICredentials = new GetAPICredentials();
                clsGroupUser oGrpUser = new clsGroupUser();

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", _orgID), oUser, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);


                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetAllGroups.Replace("{orgid}", _orgID), clGrpData, "H1", _ecomLoginKey, _ecomTransactionKey);

                var groupData = JsonConvert.DeserializeObject<List<clsGroupAPI>>(result);

                if (groupData.Count > 0)
                {
                    for (int iEnrolUser = 0; iEnrolUser < groupData.Count; iEnrolUser++)
                    {
                        if (groupData[iEnrolUser].Name.ToString().Contains(hTable["GroupName"].ToString()))
                        {
                            strGroupID = groupData[iEnrolUser].ID.ToString();
                            _Flag = true; break;
                        }
                        else
                        {
                            _Flag = false;

                        }
                    }
                }
                if (_Flag == false)
                {
                    clsGlobalVariable.strExceptionReport = "GroupID not present.";
                    return false;
                }
                else
                {

                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + "/organizations/{orgid}/courses".Replace("{orgid}", _orgID), clGrpData, "H1", _ecomLoginKey, _ecomTransactionKey);

                    var CourseData = JsonConvert.DeserializeObject<List<GetAllCourses>>(result);

                    for (int iCourse = 0; iCourse < CourseData.Count; iCourse++)
                    {
                        if (CourseData[iCourse].Name.ToString() == hTable["CourseName"].ToString()) { CourseId = CourseData[iCourse].ID.ToString(); break; }
                    }

                    if (CourseId == "")
                    {
                        clsGlobalVariable.strExceptionReport = "Course ID not available";
                        return false;
                    }
                    else
                    {

                        clsGroupCourse oclsGroupCourse = new clsGroupCourse();
                        oclsGroupCourse.courseid = CourseId;
                        oGeneric.GetApiResponseCodeData(out statusCode, out GroupResult, "POST", ApplicationSettings.APIURI() + clsAPI.GetGroupCourses.Replace("{orgid}", _orgID).Replace("{GroupID}", strGroupID), oclsGroupCourse, "H1", _ecomLoginKey, _ecomTransactionKey);

                        Thread.Sleep(clsGlobalVariable.iWaitMedium);


                        oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetGroupCourses.Replace("{orgid}", _orgID).Replace("{GroupID}", strGroupID), oclsGroupCourse, "H1", _ecomLoginKey, _ecomTransactionKey);

                        if (hTable["TestCaseType"].ToString().ToUpper() == "NEGATIVE" && (GroupResult.Contains("ErrorCode") || GroupResult.Contains("Invalid URI")))
                        {
                            if (GroupResult.Contains("Invalid URI")) { return true; }

                            SuperAdminCoursePushDownErrorCode oError = new SuperAdminCoursePushDownErrorCode();
                            var error = JsonConvert.DeserializeObject<List<SuperAdminCoursePushDownError>>(GroupResult);

                            foreach (string errorcode in oError.coursePushDownErrorCode)
                            {
                                for (int i = 0; i < error.Count; i++)
                                {
                                    if (error[i].ErrorCode == errorcode) { _Flag = true; break; }
                                    else
                                    {

                                        _Flag = false;
                                    }
                                }
                                if (_Flag == true) break;
                            }

                            if (_Flag == true) { return true; } else { return false; }
                        }
                        var CourseAddedData = JsonConvert.DeserializeObject<List<clsGetGroupCourse>>(result);

                        for (int iCourse = 0; iCourse < CourseAddedData.Count; iCourse++)
                        {
                            if (CourseAddedData[iCourse].Name.ToString() == hTable["CourseName"].ToString()) { _Flag = true; break; } else { _Flag = false; }
                        }

                        return _Flag;

                    }

                }
                return true;
            }
            catch (Exception e)
            {
                clsException.ExceptionHandler(e, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

                return false;
            }
           
        }

        public bool RemoveCourse(Hashtable hTable)
        {

            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            string GroupResult = string.Empty;
            string CourseId = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            string strGroupID = string.Empty;

            List<string> lCourseId = new List<string>();
            bool _Flag = false;
            string _UserID = string.Empty;

            try
            {
                clsPage oPage = new clsPage(iWebdriver);

                _orgID = oPage.GetOrganizationID();
                clsAPI.orgID = _orgID = "2808";


                Users oUser = new Users();
                clsGroupAPI clGrpData = new clsGroupAPI();
                GetAPICredentials oGetAPICredentials = new GetAPICredentials();
                clsGroupUser oGrpUser = new clsGroupUser();

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", _orgID), oUser, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);


                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetAllGroups.Replace("{orgid}", _orgID), clGrpData, "H1", _ecomLoginKey, _ecomTransactionKey);

                var groupData = JsonConvert.DeserializeObject<List<clsGroupAPI>>(result);

                if (groupData.Count > 0)
                {
                    for (int iEnrolUser = 0; iEnrolUser < groupData.Count; iEnrolUser++)
                    {
                        if (groupData[iEnrolUser].Name.ToString().Contains(hTable["GroupName"].ToString()))
                        {
                            strGroupID = groupData[iEnrolUser].ID.ToString();
                            _Flag = true; break;
                        }
                        else
                        {
                            _Flag = false;

                        }
                    }
                }
                if (_Flag == false)
                {
                    clsGlobalVariable.strExceptionReport = "GroupID not present.";
                    return false;
                }
                else
                {

                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + "/organizations/{orgid}/courses".Replace("{orgid}", _orgID), clGrpData, "H1", _ecomLoginKey, _ecomTransactionKey);

                    var CourseData = JsonConvert.DeserializeObject<List<GetAllCourses>>(result);

                    for (int iCourse = 0; iCourse < CourseData.Count; iCourse++)
                    {
                        if (CourseData[iCourse].Name.ToString() == hTable["CourseName"].ToString()) { CourseId = CourseData[iCourse].ID.ToString(); break; }
                    }

                    if (CourseId == "")
                    {
                        clsGlobalVariable.strExceptionReport = "Course ID not available";
                        return false;
                    }
                    else
                    {

                        clsGroupCourse oclsGroupCourse = new clsGroupCourse();
                        oclsGroupCourse.courseid = CourseId;
                        oGeneric.GetApiResponseCodeData(out statusCode, out GroupResult, "DELETE", ApplicationSettings.APIURI() + clsAPI.DeleteCourseGroupCourses.Replace("{orgid}", _orgID).Replace("{GroupID}", strGroupID).Replace("{CourseID}", CourseId), oclsGroupCourse, "H1", _ecomLoginKey, _ecomTransactionKey);

                        Thread.Sleep(clsGlobalVariable.iWaitMedium);


                        oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetGroupCourses.Replace("{orgid}", _orgID).Replace("{GroupID}", strGroupID), oclsGroupCourse, "H1", _ecomLoginKey, _ecomTransactionKey);

                        if (hTable["TestCaseType"].ToString().ToUpper() == "NEGATIVE" && (GroupResult.Contains("ErrorCode") || GroupResult.Contains("Invalid URI")))
                        {
                            if (GroupResult.Contains("Invalid URI")) { return true; }

                            SuperAdminCoursePushDownErrorCode oError = new SuperAdminCoursePushDownErrorCode();
                            var error = JsonConvert.DeserializeObject<List<SuperAdminCoursePushDownError>>(GroupResult);

                            foreach (string errorcode in oError.coursePushDownErrorCode)
                            {
                                for (int i = 0; i < error.Count; i++)
                                {
                                    if (error[i].ErrorCode == errorcode) { _Flag = true; }
                                    else
                                    {

                                        _Flag = false;
                                    }
                                }
                            }

                            if (_Flag == true) { return true; } else { return false; }
                        }
                        var CourseAddedData = JsonConvert.DeserializeObject<List<clsGetGroupCourse>>(result);
                        if (result == "")
                        {
                            return true;
                        }


                        for (int iCourse = 0; iCourse < CourseAddedData.Count; iCourse++)
                        {
                            if (CourseAddedData[iCourse].Name.ToString() == hTable["CourseName"].ToString()) { _Flag = false; break; } else { _Flag = true; }
                        }

                        return _Flag;

                    }

                }

            }
            catch (Exception e)
            {
                clsException.ExceptionHandler(e, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

                return false;
            }
            return true;


           
        }


    }
}
