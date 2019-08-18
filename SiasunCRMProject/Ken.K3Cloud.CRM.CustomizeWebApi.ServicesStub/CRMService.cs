
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

namespace Ken.K3Cloud.CRM.CustomizeWebApi.ServicesStub
{
    public class CRMService : AbstractWebApiBusinessService
    {
        public CRMService(KDServiceContext context) : base(context)
        {
        }
        public string APPtest(string parameter)
        {
            string s = System.Web.HttpContext.Current.Request.Form["Data"];
            JObject Jo = (JObject)JsonConvert.DeserializeObject(s);
            string DBID = Jo["DBID"].ToString();
            string UserName = Jo["UserName"].ToString();
            string PassWord = Jo["PassWord"].ToString();
            string SearchType = Jo["SearchType"].ToString();
            string PersonId = Jo["PersonId"].ToString();

            string sContent = "";

            JObject jsonRoot = new JObject();
            JArray Arr = new JArray();
            JObject basedata = new JObject();
            KEEPER.K3.App.CRMService crm = new KEEPER.K3.App.CRMService();
            List<long> ids = new List<long>();

            Context ctx = getContext(UserName, PassWord, 2052, DBID, "http://localhost/K3Cloud/");

            K3CloudApiClient client = new K3CloudApiClient("http://localhost/K3Cloud/");
            var ret = client.ValidateLogin(DBID, UserName, PassWord, 2052);
            var result = JObject.Parse(ret)["LoginResultType"].Value<int>();

            if (result == 1)//登录成功
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
            if (ids!=null)
            {
                Arr.Add(ids.ToArray());
            }

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
