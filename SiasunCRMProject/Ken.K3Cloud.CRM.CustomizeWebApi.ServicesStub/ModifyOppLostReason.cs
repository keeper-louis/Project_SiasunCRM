using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Authentication;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Msg;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceFacade.KDServiceClient.User;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.WebApi.Client;
using Kingdee.BOS.WebApi.ServicesStub;
using Kingdee.K3.CRM.Contracts;
using Kingdee.K3.CRM.Core;
using Kingdee.K3.CRM.Entity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Ken.K3Cloud.CRM.CustomizeWebApi.ServicesStub
{
    public class ModifyOppLostReason : AbstractWebApiBusinessService
    {
        public ModifyOppLostReason(KDServiceContext context) : base(context)
        {
        }

        public string APPtest(string parameter)
        {
            string s = System.Web.HttpContext.Current.Request.Form["Data"];
            JObject Jo = (JObject)JsonConvert.DeserializeObject(s);
            string DBID = Jo["DBID"].ToString();
            string UserName = Jo["UserName"].ToString();
            string PassWord = Jo["PassWord"].ToString();
            int BillId = Convert.ToInt32(Jo["BillId"].ToString());
            string LostReason = Jo["LostReason"].ToString();

            string reason = "";
            string sContent = "";

            Context ctx = getContext(UserName, PassWord, 2052, DBID, "http://localhost/K3Cloud/");

            K3CloudApiClient client = new K3CloudApiClient("http://localhost/K3Cloud/");
            var ret = client.ValidateLogin(DBID, UserName, PassWord, 2052);
            var result = JObject.Parse(ret)["LoginResultType"].Value<int>();

            if (result == 1)//登录成功
            {
                string strSql = string.Format(@"/*dialect*/update T_CRM_Opportunity set FWinReason={1}  where FID='{0}'", BillId, LostReason);
                DBUtils.Execute(ctx, strSql);
                reason = "填写输赢原因成功";
            }
            else
            {
                reason = "登录失败";
            }

            JObject jsonRoot = new JObject();
            JObject Result = new JObject();
            JObject ResponseStatus = new JObject();
            JArray Errors = new JArray();
            JObject basedata = new JObject();


            basedata.Add("FieldName", "");
            Errors.Add(basedata);
            basedata = new JObject();
            basedata.Add("Message", reason);
            Errors.Add(basedata);
            basedata = new JObject();
            basedata.Add("DIndex", "0");
            Errors.Add(basedata);

            ResponseStatus.Add("IsSuccess", "true");
            ResponseStatus.Add("Errors", Errors);

            Result.Add("ResponseStatus", ResponseStatus);

            jsonRoot.Add("Result", Result);

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
