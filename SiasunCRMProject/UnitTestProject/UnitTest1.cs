using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KEEPER.K3.CRM.CRMServiceHelper;
using Kingdee.BOS;
using Kingdee.BOS.ServiceFacade.KDServiceClient.User;
using Kingdee.BOS.Authentication;
using System.Collections.Generic;
using KEEPER.K3.CRM.Contracts;
using Kingdee.BOS.WebApi.Client;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            //UserServiceProxy proxy = new UserServiceProxy();// 引用Kingdee.BOS.ServiceFacade.KDServiceClient.dll
            //proxy.HostURL = @"http://127.0.0.1/K3cloud/";//k/3cloud地址
            //LoginInfo loginInfo = new LoginInfo();
            //loginInfo.Username = "scy";
            //loginInfo.Password = "666666";
            //loginInfo.Lcid = 2052;
            //loginInfo.AcctID = "5c42b8435b2962";
            ////Context ctx = proxy.ValidateUser("", "5c42b8435b2962", "scy", "666666", 2052).Context;
            //Context ctx = proxy.ValidateUser("http://127.0.0.1/K3cloud/", loginInfo).Context;
            ////List<long> salerIds = CRMServiceHelper.getSalerPersonids(ctx, 101789);
            ApiClient client = new ApiClient("http://127.0.0.1/k3cloud/");
            string dbId = "5ce2487aaf66cf";
            bool bLogin = client.Login(dbId, "李姝莉", "111111", 2052);
            if (bLogin)
            {
                //var aa = client.Execute<string>("KEEPER.K3.SIASUN.CRM.CustomizeWebApi.ServiceStub.GetApprovalStatus.getStatus,KEEPER.K3.SIASUN.CRM.CustomizeWebApi.ServiceStub", null);
                var bb = client.Execute<string>("KEEPER.K3.SIASUN.CRM.CustomizeWebApi.ServiceStub.GetApprovalItems.getItems,KEEPER.K3.SIASUN.CRM.CustomizeWebApi.ServiceStub", null);
            }

        }
    }
}
