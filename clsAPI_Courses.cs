/*
Created By: Niranjan Kashikar
Created On: 05/July/2016
Purpose: To Automate all APIs related to Course resource. For example: Get All Courses, Get a single Course, Modify Course's due date settings, etc...
*/

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
    class clsAPI_Courses
    {
        private IWebDriver iWebdriver;
        private string _ecomTransactionKey = string.Empty;
        private string _ecomLoginKey = string.Empty;
        private string _orgID = string.Empty;

        public clsAPI_Courses(IWebDriver iWeb)
        {
            this.iWebdriver = iWeb;
        }

        public bool APIGetCourses(object[] lStrvalue)
        {
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

                //Get Org ID at runtime
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", clsAPI.apiURI + clsAPI.orgCredential.Replace("$", clsAPI.orgID), oGetAPICredentials, "H2", "", "");
                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());
                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);


                //API will be hit to get all courses
                if (htblTestData["RequestType"].ToString() == "All")
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", clsAPI.apiURI + clsAPI.GetAllCourses.Replace("{orgid}", clsAPI.orgID), userID, htblTestData["TestCaseType"].ToString(), _ecomLoginKey, _ecomTransactionKey);
                    var userData = JsonConvert.DeserializeObject<List<clsAPICourse>>(result);
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

                //API will be hit to get single course
                if (htblTestData["RequestType"].ToString() == "Single")
                {
                    //Get All Users
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", clsAPI.apiURI + clsAPI.GetAllCourses.Replace("{orgid}", clsAPI.orgID), userID, htblTestData["TestCaseType"].ToString(), _ecomLoginKey, _ecomTransactionKey);

                    var courseData = JsonConvert.DeserializeObject<List<clsAPICourse>>(result);

                    if (courseData.Count > 0)
                    {
                        //Find the User that we created and get its ID
                        for (int iCourses = 0; iCourses < courseData.Count; iCourses++)
                        {
                            if (courseData[iCourses].Name.ToString().Contains(htblTestData["CourseName"].ToString()))
                            {
                                string courseid = courseData[iCourses].ID.ToString();

                                //Use the ID to create a specific URL to get particular user
                                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", clsAPI.apiURI + clsAPI.GetSingleCourse.Replace("{orgid}", clsAPI.orgID).Replace("{CourseID}", courseid), userID, htblTestData["TestCaseType"].ToString(), _ecomLoginKey, _ecomTransactionKey);

                                var singleCourseData = JsonConvert.DeserializeObject<clsAPICourse>(result);

                                if (singleCourseData.Name.Contains(htblTestData["CourseName"].ToString()))
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

        public bool ActivateDeactiveCourse(string courseName, string FlagActiveInactive)
        {
            bool _flag = false;
            string courseId = string.Empty;

            try
            {
                clsGlobalVariable.strExceptionReport = string.Empty;
                string statusCode = string.Empty;
                string result = string.Empty;
                GetAPICredentials oGetAPICredentials = new GetAPICredentials();

                //Get Org ID at runtime
                clsPage oPage = new clsPage(iWebdriver);
                _orgID = oPage.GetOrganizationID();
                clsAPI.orgID = _orgID;

                clsGeneric oGeneric = new clsGeneric();

                Thread.Sleep(clsGlobalVariable.iWaitHigh);

                //Get Org ID at runtime
                clsGenericAPI oGenericAPI = new clsGenericAPI();
                /////////////geting or credential
                oGenericAPI.GetORGCredentials(out _ecomTransactionKey, out _ecomLoginKey, _orgID);


                //////getting all courses
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", clsAPI.apiURI + clsAPI.GetAllCourses.Replace("{orgid}", clsAPI.orgID), oGetAPICredentials, "H1", _ecomLoginKey, _ecomTransactionKey);

                var courseData = JsonConvert.DeserializeObject<List<clsAPICourse>>(result);

                for (int iUser = 0; iUser < courseData.Count; iUser++)
                {
                    if (courseData[iUser].Name == courseName.ToString())
                    {
                        courseId = courseData[iUser].ID;
                        break;
                    }
                }

                /////////////////////////////activating deactivating course
                if (FlagActiveInactive.ToUpper() == "ACTIVATE")
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "PUT", clsAPI.apiURI + clsAPI.GetSingleCourse.Replace("{orgid}", clsAPI.orgID).Replace("{CourseID}", courseId) + "/Activate", oGetAPICredentials, "H1", _ecomLoginKey, _ecomTransactionKey);
                }
                else
                {
                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "DELETE", clsAPI.apiURI + clsAPI.GetSingleCourse.Replace("{orgid}", clsAPI.orgID).Replace("{CourseID}", courseId) + "/Activate", oGetAPICredentials, "H1", _ecomLoginKey, _ecomTransactionKey);

                }
                if (statusCode.Contains("204"))
                    _flag = true;
            }
            catch (Exception e)
            {
                clsException.ExceptionHandler(e, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

                return false;
            }



            return _flag;

        }



        public bool ActivateDeactiveCoursefromXML(object[] oValue)
        {
            clsGeneric oGeneric = new clsGeneric();
            Hashtable htblTestData = new Hashtable();
            htblTestData = oGeneric.GetTestData(oValue);

            return ActivateDeactiveCourse(htblTestData["CourseName"].ToString(), htblTestData["CourseStatus"].ToString());

        }
    }
}
