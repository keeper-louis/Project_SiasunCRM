using System;
using Kingdee.BOS.WebApi.Client;
using Kingdee.BOS.WebApi.ServicesStub;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS;
using Kingdee.BOS.ServiceFacade.KDServiceClient.User;
using Kingdee.BOS.Authentication;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Web;
using System.Text;
using System.Linq;
using Kingdee.BOS.App.Data;
using Kingdee.K3.MFG.App;
using Kingdee.BOS.Orm.DataEntity;

namespace CustomizeWebApi
{
    public class SalerBillByOrderPlugin : AbstractWebApiBusinessService
    {
        public SalerBillByOrderPlugin(KDServiceContext context) : base(context)
        {
        }
        public string APPtest(string parameter)
        {
            string value = HttpContext.Current.Request.Form["Data"];
            JObject jObject = (JObject)JsonConvert.DeserializeObject(value);
            string DBID = jObject["DBID"].ToString();
            string UserName = jObject["UserName"].ToString();
            string PassWord = jObject["PassWord"].ToString();

            //过滤条件：起始日期/截至日期/部门/业务员

            string PersonId = jObject["PersonId"].ToString();
            string SalesId = jObject["SalesId"].ToString();
            string DeptId = jObject["DeptId"].ToString();
            string startDate = jObject["startDate"].ToString();
            string endDate = jObject["endDate"].ToString();

            JObject jsonRoot = new JObject();
            JArray Arr = new JArray();
            JObject basedata = new JObject();
            KEEPER.K3.App.CRMService crm = new KEEPER.K3.App.CRMService();
            string sContent = "";

            Context ctx = getContext(UserName, PassWord, 2052, DBID, "http://localhost/K3Cloud/");
            ApiClient client = new ApiClient("http://localhost/K3Cloud/");
            bool bLogin = client.Login(DBID, UserName, PassWord, 2052);
            if (bLogin)//登录成功
            {

                string salerLimit = "";
                Boolean flag = false;
                //获取当前用户权限内的销售数据
                if (crm.getSalerPersonids(ctx, Convert.ToInt64(PersonId)) != null)
                {
                    //获取当前用户可见的销售员集合
                    List<long> salerList = crm.getSalerPersonids(ctx, Convert.ToInt64(PersonId));
                    if (salerList != null)
                    {
                        var a = from Id in salerList select Id;
                        string ids = string.Join(",", a.ToArray());
                        salerLimit = string.Format(@"and SALESMAN.FID in ({0})", ids);
                    }

                }

                //报表sql
                StringBuilder sql1 = new StringBuilder();
                sql1.AppendFormat(@"/*dialect*/ SELECT ROW_NUMBER() OVER (ORDER BY DEPTL.FNAME) AS FSeq, DEPTL.FNAME AS DEPTNAME, EMPL.FNAME AS SALERNAME, FBILLNO, CONVERT(FLOAT, ROUND(F_PEJK_SUMAMOUNT, 2)) AS SUMAMOUNT, F_PEJK_PRONO, CONVERT(FLOAT, ROUND(F_PEJK_FPAmount, 2)) AS FPAMOUNT, CAST(CONVERT(FLOAT, ROUND(F_PEJK_SumRatio * 100, 3)) as varchar) + ' %' AS RATIO ");
                sql1.AppendLine(" FROM PEJK_PRODETAIL DETAIL ");
                sql1.AppendLine(" LEFT JOIN T_CRM_CONTRACT CONTRACT ON DETAIL.FID = CONTRACT.FID ");
                sql1.AppendLine(" LEFT JOIN V_BD_SALESMAN SALESMAN ON SALESMAN.FID = CONTRACT.FSALERID ");
                sql1.AppendLine(" LEFT JOIN T_BD_DEPARTMENT_L DEPTL ON DEPTL.FDEPTID = SALESMAN.FDEPTID ");
                sql1.AppendLine(" LEFT JOIN T_BD_DEPARTMENT DEPT ON DEPTL.FDEPTID = DEPT.FDEPTID ");
                sql1.AppendLine(" LEFT JOIN T_BD_STAFF STAFF ON STAFF.FSTAFFID = SALESMAN.FSTAFFID ");
                sql1.AppendLine(" LEFT JOIN T_HR_EMPINFO_L EMPL ON EMPL.FID = STAFF.FEMPINFOID ");
                sql1.AppendLine(" WHERE DEPTL.FLOCALEID = 2052 AND EMPL.FLOCALEID = 2052 ");
                if (flag)
                {
                    sql1.AppendLine(salerLimit);
                }
                if (!SalesId.Equals("") && SalesId != null)
                {
                    sql1.AppendLine(" and STAFF.fstaffid in (").Append(SalesId).Append(") ");
                }
                if (!DeptId.Equals("") && DeptId != null)
                {
                    sql1.AppendLine(" and AND DEPT.fdeptid in (").Append(DeptId).Append(") ");
                }
                if (!startDate.Equals("") && startDate != null)
                {
                    sql1.AppendFormat(" AND CONTRACT.FDATE >= '{0}' ", startDate);
                }
                if (!endDate.Equals("") && endDate != null)
                {
                    sql1.AppendFormat(" AND CONTRACT.FDATE <= '{0}' ", endDate);
                }

                DynamicObjectCollection periodColl = DBUtils.ExecuteDynamicObject(ctx, sql1.ToString());

                foreach (DynamicObject item in periodColl)
                {
                    basedata = new JObject();
                    basedata.Add("FSeq", Convert.ToInt64(item["FSeq"]));
                    basedata.Add("DEPTNAME", Convert.ToInt64(item["DEPTNAME"]));
                    basedata.Add("SALERNAME", Convert.ToInt64(item["SALERNAME"]));
                    basedata.Add("FBILLNO", Convert.ToInt64(item["FBILLNO"]));
                    basedata.Add("SUMAMOUNT", Convert.ToInt64(item["SUMAMOUNT"]));
                    basedata.Add("F_PEJK_PRONO", Convert.ToInt64(item["F_PEJK_PRONO"]));
                    basedata.Add("FPAMOUNT", Convert.ToInt64(item["FPAMOUNT"]));
                    basedata.Add("RATIO", Convert.ToInt64(item["RATIO"]));
                    jsonRoot.Add("Result", basedata);
                }

            }
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
