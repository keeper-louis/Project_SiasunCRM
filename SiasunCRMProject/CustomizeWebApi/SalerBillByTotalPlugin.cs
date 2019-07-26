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

namespace Ken.K3.CRM.CustomizeWebApi.ServicesStub
{
    public class SalerBillByTotalPlugin : AbstractWebApiBusinessService
    {
        public SalerBillByTotalPlugin(KDServiceContext context) : base(context)
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
                //生成中间临时表
                String tmpTable1 = AppServiceContext.DBService.CreateTemporaryTableName(ctx);

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
                sql1.AppendLine(@"/*dialect*/ SELECT DEPTNAME, SALERNAME, QUOTA, COMPLETEAMOUNT, ");
                sql1.AppendFormat(" CAST(CONVERT(FLOAT, ROUND((COMPLETEAMOUNT * 1.00 / (QUOTA * 1.00)) * 100, 3)) as varchar)+' %' AS COMPLETERATE INTO {0} ", tmpTable1);
                sql1.AppendLine(" FROM ");
                sql1.AppendLine(" (SELECT DEPTL.FNAME AS DEPTNAME, ");
                sql1.AppendLine(" EMPL.FNAME AS SALERNAME, ");
                sql1.AppendLine(" (SELECT F_PEJK_CONTRACTQUNTA FROM PEJK_SALERQUNTAENTRY WHERE F_PEJK_SALER = RESOLVESALER.F_PEJK_SALER) AS QUOTA, ");
                sql1.AppendLine(" SUM(F_PEJK_DETAILLAMOUNT) AS COMPLETEAMOUNT ");
                sql1.AppendLine(" FROM PEJK_SALECONTRACTENTRY RESOLVESALER ");
                sql1.AppendLine(" LEFT JOIN PEJK_SALECONTRACTS RESOLVEBASIC ON RESOLVESALER.FID = RESOLVEBASIC.FID ");
                sql1.AppendLine(" LEFT JOIN V_BD_SALESMAN SALESMAN ON SALESMAN.FID = RESOLVESALER.F_PEJK_SALER ");
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
                    sql1.AppendFormat(" AND RESOLVEBASIC.F_PEJK_DATE >= '{0}' ", startDate);
                }
                if (!endDate.Equals("") && endDate != null)
                {
                    sql1.AppendFormat(" AND RESOLVEBASIC.F_PEJK_DATE <= '{0}' ", endDate);
                }
                sql1.AppendLine(" GROUP BY F_PEJK_SALER, EMPL.FNAME, DEPTL.FNAME) TMP ");
                DBUtils.ExecuteDynamicObject(ctx, sql1.ToString());


                StringBuilder sql2 = new StringBuilder();
                sql2.AppendFormat(@"/*dialect*/INSERT INTO {0} SELECT '合计' , '', QUOTA, COMPLETEAMOUNT, ", tmpTable1);
                sql2.AppendLine(" CAST(CONVERT(FLOAT, ROUND((COMPLETEAMOUNT * 1.00 / (QUOTA * 1.00)) * 100, 3)) as varchar) + ' %' AS COMPLETERATE ");
                sql2.AppendLine(" FROM ");
                sql2.AppendFormat(" (SELECT SUM(QUOTA) AS QUOTA, SUM(COMPLETEAMOUNT) AS COMPLETEAMOUNT FROM {0}) TMP ", tmpTable1);
                DBUtils.ExecuteDynamicObject(ctx, sql2.ToString());

                StringBuilder sql3 = new StringBuilder();
                sql3.AppendFormat(@"/*dialect*/ SELECT ROW_NUMBER() OVER (ORDER BY DEPTNAME) AS FSeq, DEPTNAME, SALERNAME, CONVERT(FLOAT, ROUND(QUOTA, 2)) AS TOTALQUOTA, CONVERT(FLOAT, ROUND(COMPLETEAMOUNT, 2)) AS TOTALAMOUNT, COMPLETERATE ");
                sql3.AppendFormat(" FROM {0} ", tmpTable1);
                DynamicObjectCollection periodColl = DBUtils.ExecuteDynamicObject(ctx, sql3.ToString());

                foreach (DynamicObject item in periodColl)
                {
                    basedata = new JObject();
                    basedata.Add("FSeq", Convert.ToInt64(item["FSeq"]));
                    basedata.Add("DEPTNAME", Convert.ToInt64(item["DEPTNAME"]));
                    basedata.Add("SALERNAME", Convert.ToInt64(item["SALERNAME"]));
                    basedata.Add("TOTALQUOTA", Convert.ToInt64(item["TOTALQUOTA"]));
                    basedata.Add("TOTALAMOUNT", Convert.ToInt64(item["TOTALAMOUNT"]));
                    basedata.Add("COMPLETERATE", Convert.ToInt64(item["COMPLETERATE"]));
                    jsonRoot.Add("Result", basedata);
                }

                string dropSql2 = string.Format(@"/*dialect*/ drop table {0}", tmpTable1);
                DBUtils.Execute(ctx, dropSql2);

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
