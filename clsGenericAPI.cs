using System.Threading;
using Newtonsoft.Json;
using ILMS.ApplicationSettingsManager;
using System;

namespace ILMS
{
    class clsGenericAPI
    {
        private string _ecomTransactionKey = string.Empty;
        private string _ecomLoginKey = string.Empty;
        private string _orgID = string.Empty;
        string statusCode = string.Empty;
        string result = string.Empty;

        public void GetORGCredentials(out string _ecomTransactionKey, out string _ecomLoginKey, string orgID)
        {
            _ecomTransactionKey = string.Empty;
            _ecomLoginKey = string.Empty;
            try
            {
                GetAPICredentials oGetAPICredentials = new GetAPICredentials();
                string status = string.Empty;
                string resultAPI = string.Empty;
                clsGeneric oGeneric = new clsGeneric();

                Thread.Sleep(clsGlobalVariable.iWaitHigh);

                //Get Org ID at runtime
                oGeneric.GetApiResponseCodeData(out status, out resultAPI, "GET", clsAPI.apiURI + clsAPI.orgCredential.Replace("$", orgID), oGetAPICredentials, "H2", "", "");
                var orgCredential = JsonConvert.DeserializeObject<GetAPICredentials>(resultAPI);
                Crypto3DES _des = new Crypto3DES(ApplicationSettings.EComModuleEncKey());
                _ecomTransactionKey = _des.Decrypt3DES(orgCredential.TransactionKey);
                _ecomLoginKey = _des.Decrypt3DES(orgCredential.LoginID);
            }
            catch  { }

        }
    }
}
