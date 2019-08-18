using Kingdee.BOS;
using Kingdee.BOS.Authentication;
using Kingdee.BOS.ServiceFacade.KDServiceClient.User;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS.WebApi.Client;
using Kingdee.BOS.WebApi.ServicesStub;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Web;

namespace Ken.K3.CRM.CustomizeWebApi.ServicesStub
{
    public class CRMService : AbstractWebApiBusinessService
    {
        public CRMService(KDServiceContext context) : base(context)
        {
        }
        public string APPtest(string parameter)
        {

            string value = HttpContext.Current.Request.Form["Data"];
            JObject jObject = (JObject)JsonConvert.DeserializeObject(value);
            string DBID = jObject["DBID"].ToString();
            string UserName = jObject["UserName"].ToString();
            string PassWord = jObject["PassWord"].ToString();
            string SearchType = jObject["SearchType"].ToString();
            string PersonId = jObject["PersonId"].ToString();


            JObject jsonRoot = new JObject();
            JArray Arr = new JArray();
            JObject basedata = new JObject();
            KEEPER.K3.App.CRMService crm = new KEEPER.K3.App.CRMService();
            List<long> ids = new List<long>();
            string sContent = "";


            Context ctx = getContext(UserName, PassWord, 2052, DBID, "http://localhost/K3Cloud/");
            ApiClient client = new ApiClient("http://localhost/K3Cloud/");
            bool bLogin = client.Login(DBID, UserName, PassWord, 2052);
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
            if (ids != null)
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
