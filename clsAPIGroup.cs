/*
Created By: Niranjan Kashikar
Date: 06/13/2016
Purpose: To Get / Update / Create Group related data using APIs
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System.Collections;
using ILMS.ApplicationSettingsManager;

namespace ILMS
{
    class clsAPIGroup
    {
        private IWebDriver iWebdriver;
        private string _ecomTransactionKey = string.Empty;
        private string _ecomLoginKey = string.Empty;
        private string _orgID = string.Empty;

        public clsAPIGroup(IWebDriver iWeb)
        {
            this.iWebdriver = iWeb;
        }

        public bool GetGroups(object[] lStrValue)
        {
            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            string allCourseResultresult = string.Empty;
            string singleCourseresult = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            Hashtable htblTestData = new Hashtable();
            htblTestData = oGeneric.GetTestData(lStrValue);
            bool _Flag = false;

            clsGroupAPI clGrpData = new clsGroupAPI();

            GetAPICredentials oGetAPICredentials = new GetAPICredentials();
            try
            {
                //Get Org ID at runtime
                clsPage oPage = new clsPage(iWebdriver);
                _orgID = oPage.GetOrganizationID();
                clsAPI.orgID = _orgID;

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", clsAPI.orgID), oGetAPICredentials, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);

                if (htblTestData["RequestType"].ToString() == "All")
                {

                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetAllGroups.Replace("{orgid}", clsAPI.orgID), clGrpData, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);
                    var groupData = JsonConvert.DeserializeObject<List<clsGroupAPI>>(result);
                    if (groupData.Count > 1)
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
                    //Get All Groups
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetAllGroups.Replace("{orgid}", clsAPI.orgID), clGrpData, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);

                    var groupData = JsonConvert.DeserializeObject<List<clsGroupAPI>>(result);

                    if (groupData.Count > 0)
                    {
                        //Find the Group that we created and get its ID
                        for (int iEnrolUser = 0; iEnrolUser < groupData.Count; iEnrolUser++)
                        {
                            if (groupData[iEnrolUser].Name.ToString().Contains(htblTestData["GroupName"].ToString()))
                            {
                                var groupID = groupData[iEnrolUser].ID.ToString();

                                //Use the ID to create a specific URL to get particular group
                                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetGroupByID.Replace("{orgid}", clsAPI.orgID).Replace("{GroupID}", groupID), clGrpData, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);

                                var singleGroupData = JsonConvert.DeserializeObject<clsGroupAPI>(result);

                                if (singleGroupData.Name.Contains(htblTestData["GroupName"].ToString()))
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

                if (htblTestData["RequestType"].ToString() == "Members")
                {
                    //Get Members of a Group
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetAllGroups.Replace("{orgid}", clsAPI.orgID), clGrpData, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);

                    var groupData = JsonConvert.DeserializeObject<List<clsGroupAPI>>(result);


                    if (groupData.Count > 0)
                    {
                        //Find the Group that we created and get its ID
                        for (int iEnrolUser = 0; iEnrolUser < groupData.Count; iEnrolUser++)
                        {
                            if (groupData[iEnrolUser].Name.ToString().Contains(htblTestData["GroupName"].ToString()))
                            {
                                var groupID = groupData[iEnrolUser].ID.ToString();

                                //Use the ID to create a specific URL to get particular group
                                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetGroupMembers.Replace("{orgid}", clsAPI.orgID).Replace("{GroupID}", groupID), clGrpData, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);

                                var GroupMemberData = JsonConvert.DeserializeObject<List<clsGroupAPI>>(result);
                                for (int MemberCnt = 0; MemberCnt < GroupMemberData.Count; MemberCnt++)
                                {
                                    var MemberID = GroupMemberData[MemberCnt].ID.ToString();

                                    if (MemberID != "")
                                    {
                                        _Flag = true;
                                        break;
                                    }
                                    else { _Flag = false; }
                                }
                            }
                            else
                            {
                                _Flag = false;

                            }

                            if (_Flag == true)
                            {
                                break;
                            }
                        }
                    }
                }


                if (htblTestData["RequestType"].ToString() == "ExpInclListUsers")
                {
                    //Get Members of a Group
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetAllGroups.Replace("{orgid}", clsAPI.orgID), clGrpData, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);

                    var groupData = JsonConvert.DeserializeObject<List<clsGroupAPI>>(result);


                    if (groupData.Count > 0)
                    {
                        //Find the Group that we created and get its ID
                        for (int iEnrolUser = 0; iEnrolUser < groupData.Count; iEnrolUser++)
                        {
                            if (groupData[iEnrolUser].Name.ToString().Contains(htblTestData["GroupName"].ToString()))
                            {
                                var groupID = groupData[iEnrolUser].ID.ToString();

                                //Use the ID to create a specific URL to get particular group
                                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetGroupExplicitInclUsers.Replace("{orgid}", clsAPI.orgID).Replace("{GroupID}", groupID), clGrpData, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);

                                var GroupMemberData = JsonConvert.DeserializeObject<List<clsGroupAPI>>(result);
                                for (int MemberCnt = 0; MemberCnt < GroupMemberData.Count; MemberCnt++)
                                {
                                    var MemberID = GroupMemberData[MemberCnt].ID.ToString();

                                    if (MemberID != "")
                                    {
                                        _Flag = true;
                                        break;
                                    }
                                    else { _Flag = false; }
                                }
                            }
                            else
                            {

                                _Flag = false;

                            }

                            if (_Flag == true)
                            {
                                break;
                            }
                        }
                    }
                }


                if (htblTestData["RequestType"].ToString() == "ExpExclListUsers")
                {
                    //Get Members of a Group
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetAllGroups.Replace("{orgid}", clsAPI.orgID), clGrpData, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);

                    var groupData = JsonConvert.DeserializeObject<List<clsGroupAPI>>(result);

                    if (groupData.Count > 0)
                    {
                        //Find the Group that we created and get its ID
                        for (int iEnrolUser = 0; iEnrolUser < groupData.Count; iEnrolUser++)
                        {
                            if (groupData[iEnrolUser].Name.ToString().Contains(htblTestData["GroupName"].ToString()))
                            {
                                var groupID = groupData[iEnrolUser].ID.ToString();

                                //Use the ID to create a specific URL to get particular group
                                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetGroupExplicitExclUsers.Replace("{orgid}", clsAPI.orgID).Replace("{GroupID}", groupID), clGrpData, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);

                                var GroupMemberData = JsonConvert.DeserializeObject<List<clsGroupAPI>>(result);
                                for (int MemberCnt = 0; MemberCnt < GroupMemberData.Count; MemberCnt++)
                                {
                                    var MemberID = GroupMemberData[MemberCnt].ID.ToString();

                                    if (MemberID != "")
                                    {
                                        _Flag = true;
                                        break;
                                    }
                                    else { _Flag = false; }
                                }
                            }
                            else
                            {

                                _Flag = false;

                            }

                            if (_Flag == true)
                            {
                                break;
                            }
                        }
                    }
                }

                if (htblTestData["RequestType"].ToString() == "GetCourses")
                {
                    //Get Courses of a Group

                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetAllGroups.Replace("{orgid}", clsAPI.orgID), clGrpData, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);

                    var groupData = JsonConvert.DeserializeObject<List<clsGroupAPI>>(result);

                    if (groupData.Count > 0)
                    {
                        //Find the Group that we created and get its ID
                        for (int iEnrolUser = 0; iEnrolUser < groupData.Count; iEnrolUser++)
                        {
                            if (groupData[iEnrolUser].Name.ToString().Contains(htblTestData["GroupName"].ToString()))
                            {
                                var groupID = groupData[iEnrolUser].ID.ToString();

                                //Use the ID to create a specific URL to get particular group
                                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.GetGroupCourses.Replace("{orgID}", clsAPI.orgID).Replace("{groupID}", groupID), clGrpData, htblTestData["MethodType"].ToString(), _ecomLoginKey, _ecomTransactionKey);

                                var GroupCourse = JsonConvert.DeserializeObject<List<GroupCourseAPI>>(result);
                                for (int CourseCnt = 0; CourseCnt < GroupCourse.Count; CourseCnt++)
                                {
                                    var CourseID = GroupCourse[CourseCnt].Name.ToString();

                                    if (CourseID == htblTestData["CourseName"].ToString())
                                    {
                                        _Flag = true;
                                        break;
                                    }
                                    else { _Flag = false; }
                                }

                            }
                            else
                            {
                                _Flag = false;

                            }

                            if (_Flag == true)
                            {
                                break;
                            }
                        }
                    }
                }

                return _Flag;
            }
            catch (Exception er)
            {
                clsException.ExceptionHandler(er, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);
                return false;
            }
        }


    }
}
