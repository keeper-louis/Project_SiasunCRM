
using System;
using Kingdee.BOS.WebApi.Client;
using Kingdee.BOS.WebApi.ServicesStub;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS;
using Kingdee.BOS.ServiceFacade.KDServiceClient.User;
using Kingdee.BOS.Authentication;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using KEEPER.K3.App;
using System.Collections.Generic;

namespace CustomizeWebApi
{
    public class CRMService : AbstractWebApiBusinessService
    {
        public CRMService(KDServiceContext context) : base(context)
        {
        }
        public string APPtest(string parameter)
        {
            JObject Jo = (JObject)JsonConvert.DeserializeObject(parameter);
            string ServerUrl = "http://localhost/K3Cloud/";//服务地址
            string DBID = Jo["DBID"].ToString();
            string UserName = Jo["UserName"].ToString();
            string PassWord = Jo["PassWord"].ToString();
            int ICID = Convert.ToInt32("2052");
            string SearchType = Jo["SearchType"].ToString();
            string PersonId = Jo["PersonId"].ToString();

            string sContent = "";

            JObject jsonRoot = new JObject();
            JArray Arr = new JArray();
            JObject basedata = new JObject();
            KEEPER.K3.App.CRMService crm = new KEEPER.K3.App.CRMService();
            List<long> ids = new List<long>();

            Context ctx = getContext(UserName, PassWord, ICID, DBID, ServerUrl);

            ApiClient client = new ApiClient(ServerUrl);
            bool bLogin = client.Login(DBID, UserName, PassWord, ICID);
            if (bLogin)//登录成功
            {
                if (SearchType.Equals("0"))
                {
                    ids = crm.getSalerPersonids(ctx, Convert.ToInt64(PersonId));
                    jsonRoot.Add("IsSuccess", "true");
                    jsonRoot.Add("Reason", "");
                }
                else if (SearchType.Equals("1"))
                {
                    ids = crm.getProjectIds(ctx, Convert.ToInt64(PersonId));
                    jsonRoot.Add("IsSuccess", "true");
                    jsonRoot.Add("Reason", "");
                }
                else
                {
                    jsonRoot.Add("IsSuccess", "false");
                    jsonRoot.Add("Reason", "请传入正确的参数");
                }
            }
            else
            {
                jsonRoot.Add("IsSuccess", "false");
                jsonRoot.Add("Reason", "登录失败");
            }
            Arr.Add(ids.ToArray());
            jsonRoot.Add("Ids", Arr);

            sContent = JsonConvert.SerializeObject(jsonRoot);

            return sContent;
        }


        public static Context getContext(string UserName, string PassWord, int ICID, string DBID, string ServerUrl)
        {


            UserServiceProxy proxy = new UserServiceProxy();// 引用Kingdee.BOS.ServiceFacade.KDServiceClient.dll
            proxy.HostURL = @"http://localhost/K3Cloud/";//k/3cloud地址

            LoginInfo logininfo = new LoginInfo();
            logininfo.Username = UserName;
            logininfo.Password = PassWord;
            logininfo.Lcid = ICID;
            logininfo.AcctID = DBID;

            Context ctx = proxy.ValidateUser("http://localhost/K3Cloud/", logininfo).Context;//guid为业务数据中心dbid，可以去管理中心数据库查询一下t_bas_datacenter_l表查找，后面需要用户名和密码

            return ctx;
        }


    }
}
