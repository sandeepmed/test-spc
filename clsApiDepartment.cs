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
    class clsApiDepartment
    {
        private IWebDriver iWebdriver;
        private string _ecomTransactionKey = string.Empty;
        private string _ecomLoginKey = string.Empty;
        private string _orgID = string.Empty;

        public clsApiDepartment(IWebDriver iWeb)
        {
            this.iWebdriver = iWeb;
        }


        public bool AddUpdateDeleteDepartment(object[] oValue)
        {
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            Hashtable hTable = new Hashtable();
            hTable = oGeneric.GetTestData(oValue);

            switch (hTable["Function"].ToString().ToUpper().Trim())
            {
                case "UPDATE":
                    return ModifyDepartment(hTable);

                case "ADD":
                    return AddDepartment(hTable);

                case "REMOVE":
                    return RemoveDepartment(hTable);
            }
            return true;
        }

        public bool ModifyDepartment(Hashtable hTable)
        {
            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            string RegionResult = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            string strGroupID = string.Empty;

            List<string> lCourseId = new List<string>();
            bool _Flag = false;
            string _UserID = string.Empty;
            try
            {
                clsPage oPage = new clsPage(iWebdriver);
                clsRegionApi oRegion = new clsRegionApi();
                _orgID = oPage.GetOrganizationID();
                clsAPI.orgID = _orgID;
                //  oRegion.name = hTable["Name"].ToString();

                GetAPICredentials oGetAPICredentials = new GetAPICredentials();

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", _orgID), oGetAPICredentials, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);

                //Get All Regions Data of above organization
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.Region.Replace("{orgid}", _orgID), oRegion, "H1", _ecomLoginKey, _ecomTransactionKey);

                //Convert the JSON data in List and put in a Local variable
                var RegionData = JsonConvert.DeserializeObject<List<GetRegion>>(result);

                //Find for my region and get its id and put in a variable

                string RegID = "";
                for (int iRegions = 0; iRegions < RegionData.Count; iRegions++)
                {
                    if (RegionData[iRegions].Name.Contains(hTable["RegionName"].ToString()))
                    {
                        RegID = RegionData[iRegions].ID.ToString();
                        break;
                    }
                    else
                    {
                        _Flag = false;
                    }
                }

                //Get All Divisions of a Region
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", clsAPI.apiURI + clsAPI.updateRegion.Replace("{orgid}", _orgID).Replace("{regionID}", RegID) + "/divisions", oRegion, "H1", _ecomLoginKey, _ecomTransactionKey);


                var DivisionData = JsonConvert.DeserializeObject<List<GetRegion>>(result);

                //Find for my division and get its id and put in a variable

                string DivID = "";
                for (int iDivs = 0; iDivs < DivisionData.Count; iDivs++)
                {
                    if (DivisionData[iDivs].Name.Contains(hTable["DivisionName"].ToString()))
                    {
                        DivID = DivisionData[iDivs].ID.ToString();
                        break;
                    }
                    else
                    {
                        _Flag = false;
                    }
                }

                //Get All Depts of a Reg and Division
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", clsAPI.apiURI + clsAPI.GetAllDepartments.Replace("{orgid}", _orgID).Replace("{regionID}", RegID).Replace("{divisionID}", DivID), oRegion, "H1", _ecomLoginKey, _ecomTransactionKey);

                var DeptData = JsonConvert.DeserializeObject<List<GetRegion>>(result);

                //Find for my dept and get its id and put in a variable

                string DeptID = "";
                for (int iDepts = 0; iDepts < DeptData.Count; iDepts++)
                {
                    if (DeptData[iDepts].Name.Contains(hTable["OldDeptName"].ToString()))
                    {
                        DeptID = DeptData[iDepts].ID.ToString();
                        break;
                    }
                    else
                    {
                        _Flag = false;
                    }
                }

                Thread.Sleep(clsGlobalVariable.iWaitMedium);

                if (hTable["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                {
                    clsDepartmentApi DepartmentData = new clsDepartmentApi();

                    DepartmentData.Name = hTable["NewDeptName"].ToString();

                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "PUT", clsAPI.apiURI + clsAPI.GetSingleDepartment.Replace("{orgid}", _orgID).Replace("{regionID}", RegID).Replace("{divisionID}", DivID).Replace("{deptID}", DeptID), DepartmentData, "H1", _ecomLoginKey, _ecomTransactionKey);
                    if (statusCode == "NoContent/204")
                    { _Flag = true; }
                    else { _Flag = false; }
                }
            }
            catch (Exception e)
            {
                clsException.ExceptionHandler(e, iWebdriver, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name);

                return false;

            }

            return true;


        }

        /// Function to ADD a Department
        public bool AddDepartment(Hashtable hTable)
        {
            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            string RegionResult = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            string strGroupID = string.Empty;

            List<string> lCourseId = new List<string>();
            bool _Flag = false;
            string _UserID = string.Empty;
            try
            {
                clsPage oPage = new clsPage(iWebdriver);
                clsRegionApi oRegion = new clsRegionApi();
                _orgID = oPage.GetOrganizationID();
                clsAPI.orgID = _orgID;
                //  oRegion.name = hTable["Name"].ToString();

                GetAPICredentials oGetAPICredentials = new GetAPICredentials();

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", _orgID), oGetAPICredentials, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);

                //Get All Regions Data of above organization
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.Region.Replace("{orgid}", _orgID), oRegion, "H1", _ecomLoginKey, _ecomTransactionKey);

                //Convert the JSON data in List and put in a Local variable
                var RegionData = JsonConvert.DeserializeObject<List<GetRegion>>(result);

                //Find for my region and get its id and put in a variable

                string RegID = "";
                for (int iRegions = 0; iRegions < RegionData.Count; iRegions++)
                {
                    if (RegionData[iRegions].Name.Contains(hTable["RegionName"].ToString()))
                    {
                        RegID = RegionData[iRegions].ID.ToString();
                        _Flag = true;
                        break;
                    }
                    else
                    {
                        _Flag = false;
                    }
                }


                if (_Flag == false && hTable["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                {
                    clsGlobalVariable.strExceptionReport = "Region Id not available";
                    return false;
                }

                //Get All Divisions of a Region
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", clsAPI.apiURI + clsAPI.updateRegion.Replace("{orgid}", _orgID).Replace("{regionID}", RegID) + "/divisions", oRegion, "H1", _ecomLoginKey, _ecomTransactionKey);


                var DivisionData = JsonConvert.DeserializeObject<List<GetRegion>>(result);

                //Find for my division and get its id and put in a variable

                string DivID = "";
                for (int iDivs = 0; iDivs < DivisionData.Count; iDivs++)
                {
                    if (DivisionData[iDivs].Name.Contains(hTable["DivisionName"].ToString()))
                    {
                        DivID = DivisionData[iDivs].ID.ToString();
                        _Flag = true;
                        break;
                    }
                    else
                    {
                        _Flag = false;
                    }
                }

                if (_Flag == false && hTable["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                {
                    clsGlobalVariable.strExceptionReport = "Division Id not available";
                    return false;
                }


                if (hTable["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                {
                    clsDepartmentApi DepartmentData = new clsDepartmentApi();

                    DepartmentData.Name = hTable["DeptName"].ToString();

                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "POST", clsAPI.apiURI + clsAPI.GetSingleDepartment.Replace("{orgid}", _orgID).Replace("{regionID}", RegID).Replace("{divisionID}", DivID).Replace("/{deptID}", ""), DepartmentData, "H1", _ecomLoginKey, _ecomTransactionKey);
                    if (statusCode == "Created/201")
                    {
                        _Flag = true;
                    }
                    else { _Flag = false; }
                }

                else if
                    (hTable["TestCaseType"].ToString().ToUpper() == "NEGATIVE")
                {
                    clsDepartmentApi DepartmentData = new clsDepartmentApi();

                    DepartmentData.Name = hTable["DeptName"].ToString();

                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "POST", clsAPI.apiURI + clsAPI.GetSingleDepartment.Replace("{orgid}", _orgID).Replace("{regionID}", RegID).Replace("{divisionID}", DivID).Replace("/{deptID}", ""), DepartmentData, "H1", _ecomLoginKey, _ecomTransactionKey);
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
        public bool RemoveDepartment(Hashtable hTable)
        {
            clsGlobalVariable.strExceptionReport = string.Empty;
            string statusCode = string.Empty;
            string result = string.Empty;
            string RegionResult = string.Empty;
            clsGeneric oGeneric = new clsGeneric();
            string URI = string.Empty;
            string strGroupID = string.Empty;

            List<string> lCourseId = new List<string>();
            bool _Flag = false;
            string _UserID = string.Empty;
            try
            {
                clsPage oPage = new clsPage(iWebdriver);
                clsRegionApi oRegion = new clsRegionApi();
                _orgID = oPage.GetOrganizationID();
                clsAPI.orgID = _orgID;
                //  oRegion.name = hTable["Name"].ToString();

                GetAPICredentials oGetAPICredentials = new GetAPICredentials();

                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.orgCredential.Replace("$", _orgID), oGetAPICredentials, "H2", "", "");

                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(result);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());

                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);

                //Get All Regions Data of above organization
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", ApplicationSettings.APIURI() + clsAPI.Region.Replace("{orgid}", _orgID), oRegion, "H1", _ecomLoginKey, _ecomTransactionKey);

                //Convert the JSON data in List and put in a Local variable
                var RegionData = JsonConvert.DeserializeObject<List<GetRegion>>(result);

                //Find for my region and get its id and put in a variable

                string RegID = "";
                for (int iRegions = 0; iRegions < RegionData.Count; iRegions++)
                {
                    if (RegionData[iRegions].Name.Contains(hTable["RegionName"].ToString()))
                    {
                        RegID = RegionData[iRegions].ID.ToString();
                        break;
                    }
                    else
                    {
                        _Flag = false;
                    }
                }



                if (_Flag == false && hTable["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                {
                    clsGlobalVariable.strExceptionReport = "Region Id not available";
                    return false;
                }
                //Get All Divisions of a Region
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", clsAPI.apiURI + clsAPI.updateRegion.Replace("{orgid}", _orgID).Replace("{regionID}", RegID) + "/divisions", oRegion, "H1", _ecomLoginKey, _ecomTransactionKey);


                var DivisionData = JsonConvert.DeserializeObject<List<GetRegion>>(result);

                //Find for my division and get its id and put in a variable

                string DivID = "";
                for (int iDivs = 0; iDivs < DivisionData.Count; iDivs++)
                {
                    if (DivisionData[iDivs].Name.Contains(hTable["DivisionName"].ToString()))
                    {
                        DivID = DivisionData[iDivs].ID.ToString();
                        break;
                    }
                    else
                    {
                        _Flag = false;
                    }
                }


                if (_Flag == false && hTable["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                {
                    clsGlobalVariable.strExceptionReport = "Division Id not available";
                    return false;
                }

                //Get All Depts of a Reg and Division
                oGeneric.GetApiResponseCodeData(out statusCode, out result, "GET", clsAPI.apiURI + clsAPI.GetAllDepartments.Replace("{orgid}", _orgID).Replace("{regionID}", RegID).Replace("{divisionID}", DivID), oRegion, "H1", _ecomLoginKey, _ecomTransactionKey);

                var DeptData = JsonConvert.DeserializeObject<List<GetRegion>>(result);

                //Find for my dept and get its id and put in a variable

                string DeptID = "";
                for (int iDepts = 0; iDepts < DeptData.Count; iDepts++)
                {
                    if (DeptData[iDepts].Name.Contains(hTable["DeptName"].ToString()))
                    {
                        DeptID = DeptData[iDepts].ID.ToString();
                        break;
                    }
                    else
                    {
                        _Flag = false;
                    }
                }

                Thread.Sleep(clsGlobalVariable.iWaitMedium);

                if (hTable["TestCaseType"].ToString().ToUpper() == "POSITIVE")
                {
                    clsDepartmentApi DepartmentData = new clsDepartmentApi();

                    DepartmentData.Name = hTable["DeptName"].ToString();

                    oGeneric.GetApiResponseCodeData(out statusCode, out result, "PUT", clsAPI.apiURI + clsAPI.GetSingleDepartment.Replace("{orgid}", _orgID).Replace("{regionID}", RegID).Replace("{divisionID}", DivID).Replace("{deptID}", DeptID), DepartmentData, "H1", _ecomLoginKey, _ecomTransactionKey);
                    if (statusCode == "NoContent/204")
                    { _Flag = true; }
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

    }
}
