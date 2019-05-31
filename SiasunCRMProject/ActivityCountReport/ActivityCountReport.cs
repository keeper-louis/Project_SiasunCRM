using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

using System.Linq;
using System.Text;

namespace SIASUN.K3.Report.ActivityCountReportPlugIn
{
    [Description("年度拜访客户情况")]
    public class ActivityCountReport : SysReportBaseService
    {
        private string[] materialRptTableNames;

        public object DynamicObjectCollectio { get; private set; }

        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            base.BuilderReportSqlAndTempTable(filter, tableName);
            IDBService dbservice = ServiceHelper.GetService<IDBService>();
            materialRptTableNames = dbservice.CreateTemporaryTableName(this.Context, 2);

            DynamicObject cutomerfiler = filter.FilterParameter.CustomFilter;
            String year = null;//年份
            if (cutomerfiler["F_SXINSUN_YEAR"] != null)
            {
                year = Convert.ToInt32(cutomerfiler["F_SXINSUN_YEAR"]).ToString();
            }
            StringBuilder deptnumbersql = new StringBuilder();

            if (cutomerfiler["F_PAEZ_DEPT"] != null)
            {

                DynamicObjectCollection cols = (DynamicObjectCollection)cutomerfiler["F_PAEZ_DEPT"];
                int deptsize = 0;
                if (cols.Count >= 1)
                    deptnumbersql.Append("in (");
                foreach (DynamicObject dept in cols)
                {
                    String deptnumber = Convert.ToString(((DynamicObject)dept["F_PAEZ_DEPT"])["Number"]);
                    deptsize = deptsize + 1;
                    if (deptsize == cols.Count)
                        deptnumbersql.Append("'" + deptnumber + "')");
                    else
                        deptnumbersql.Append("'" + deptnumber + "',");


                }
            }
            StringBuilder salenumbersql = new StringBuilder();
            if (cutomerfiler["F_PAEZ_MulBaseSaler"] != null)
            {
                DynamicObjectCollection cols = (DynamicObjectCollection)cutomerfiler["F_PAEZ_MulBaseSaler"];
                int salesize = 0;
                if (cols.Count >= 1)
                    salenumbersql.Append("in (");
                foreach (DynamicObject onesale in cols)
                {
                    String salenumber = Convert.ToString(((DynamicObject)onesale["F_PAEZ_MulBaseSaler"])["Number"]);
                    salesize = salesize + 1;
                    if (salesize == cols.Count)
                        salenumbersql.Append("'" + salenumber + "')");
                    else
                        salenumbersql.Append("'" + salenumber + "',");

                }
            }

            string temTable1 = materialRptTableNames[0];
            string temTable2 = materialRptTableNames[1];
            // 拼接过滤条件 ： filter
            // 略
            //DynamicObject cutomerfiler = filter.FilterParameter.CustomFilter;


            //if (cutomerfiler["F_JD_Date"] != null)
            //{
            //    strbydate = Convert.ToDateTime(cutomerfiler["F_JD_Date"]).ToString("yyyy-MM-dd 23:59:59");
            //    strbydate2 = Convert.ToString(cutomerfiler["F_JD_Date"]);
            //}

            // 默认排序字段：需要从filter中取用户设置的排序字段
            //string seqFld = string.Format(base.KSQL_SEQ, " t0.FID ");

            // 取数SQL
            // 商机登录部门 

            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.AppendLine("select distinct   secuser.fname username ,emp.fnumber empnumber,empl.fname empname,post.fnumber postnumber,post_l.fname postname,dept.fnumber deptnumber,deptl.FNAME deptname ");
                stringBuilder.AppendFormat(", opp.FBILLNO,opp.FOPPName  ,opp.FCREATEDATE,year(opp.FCREATEDATE) rtyear ,month(opp.FCREATEDATE) rtmonth ,opp.FDOCUMENTSTATUS,activity.FBILLNO  activitybillno ,opp.FCloseStatus,activity.Fname activitytitle ,activity.FACTSTARTTIME \n");
                stringBuilder.AppendFormat("into {0}", temTable1).AppendLine(" \n");
                stringBuilder.AppendLine("from  T_CRM_Activity activity \n ");
                stringBuilder.AppendLine("left join  T_CRM_Opportunity opp on activity.FOPPID=opp.FID \n ");
                stringBuilder.AppendLine("left join  t_sec_user secuser          on opp.FCREATORID= secuser.FUSERID           --用户 \n");
                stringBuilder.AppendLine("left join V_BD_SALESMAN saleman on saleman.fid = opp.FBEMPID           --销售员 \n");
                stringBuilder.AppendLine("left join T_BD_STAFF staff on staff.FSTAFFID = saleman.FSTAFFID   \n");
                stringBuilder.AppendLine("left join  T_HR_EMPINFO emp on emp.fid=staff.FEMPINFOID   \n");
                stringBuilder.AppendLine("left join T_HR_EMPINFO_L empl on empl.FID=emp.FID     \n");
                stringBuilder.AppendLine("left join T_ORG_POST post on post.FPOSTID = staff.FPOSTID  -- 岗位   \n");
                stringBuilder.AppendLine("left join T_ORG_POST_L post_l on post_l.FPOSTID = post.FPOSTID  \n");
                stringBuilder.AppendLine("left join t_bd_department dept on dept.FDEPTID = post.FDEPTID ---- - 部门   \n");
                stringBuilder.AppendLine("left join  t_bd_department_L deptl on deptl.FDEPTID = dept.FDEPTID   \n");
                stringBuilder.AppendLine("where secuser.FTYPE=1  and  empl.FLOCALEID = 2052 and deptl.FLOCALEID = 2052 ");
                if (year != null)
                {
                    stringBuilder.AppendLine(" and  year(activity.FACTSTARTTIME)= ");
                    stringBuilder.AppendLine(year);

                }
                //部门
                if (deptnumbersql != null && deptnumbersql.Length > 0)
                {
                    stringBuilder.AppendLine(" and  dept.fnumber ").Append(deptnumbersql);


                }
                //销售员
                if (salenumbersql != null && salenumbersql.Length > 0)
                {
                    stringBuilder.AppendLine(" and  emp.fnumber ").Append(salenumbersql);


                }
                DBUtils.ExecuteDynamicObject(this.Context, stringBuilder.ToString());

                stringBuilder = new StringBuilder();

                stringBuilder.AppendLine("select    toji2.empnumber, toji2.empname,yue1,yue2,yue3,yue4,yue5,yue6,yue7,yue8,  \n  ");
                stringBuilder.AppendLine("yue9,yue10,yue11,yue12,   \n  ");
                stringBuilder.AppendLine("yue1+yue2+yue3+yue4+yue5+yue6+yue7+yue8+yue9+yue10+yue11+yue12 zoji,isnull(F_PEJK_ACTIVITYQUNTA,0) hdzb,yue1+yue2+yue3+yue4+yue5+yue6+yue7+yue8+yue9+yue10+yue11+yue12-isnull(F_PEJK_ACTIVITYQUNTA,0) hdwcqk   \n  ");
                stringBuilder.AppendFormat("into {0}", temTable2).AppendLine(" \n");
                stringBuilder.AppendLine("from (  \n  ");
                stringBuilder.AppendLine("select  empnumber, empname, sum(yue1) yue1,sum(yue2) yue2,sum(yue3) yue3,sum(yue4) yue4,sum(yue5) yue5,sum(yue6) yue6,sum(yue7) yue7,sum(yue8) yue8,sum(yue9) yue9,sum(yue10) yue10,sum(yue11) yue11,sum(yue12) yue12  \n  ");
                stringBuilder.AppendLine("from (  \n  ");
                stringBuilder.AppendLine("  \n  ");
                stringBuilder.AppendLine("select  empnumber, empname,  \n  ");
                stringBuilder.AppendLine("case when rtmonth=1 then oppcounts else 0 end  yue1 ,  \n  ");
                stringBuilder.AppendLine("case when rtmonth=2 then oppcounts else 0 end  yue2,  \n  ");
                stringBuilder.AppendLine("case when rtmonth=3 then oppcounts else 0 end  yue3,  \n  ");
                stringBuilder.AppendLine("case when rtmonth=4 then oppcounts else 0 end  yue4,  \n  ");
                stringBuilder.AppendLine("case when rtmonth=5 then oppcounts else 0 end  yue5,  \n  ");
                stringBuilder.AppendLine("case when rtmonth=6 then oppcounts else 0 end  yue6,  \n  ");
                stringBuilder.AppendLine("case when rtmonth=7 then oppcounts else 0 end  yue7,  \n  ");
                stringBuilder.AppendLine("case when rtmonth=8 then oppcounts else 0 end  yue8,  \n  ");
                stringBuilder.AppendLine("case when rtmonth=9 then oppcounts else 0 end  yue9,  \n  ");
                stringBuilder.AppendLine("case when rtmonth=10 then oppcounts else 0 end  yue10,  \n  ");
                stringBuilder.AppendLine("case when rtmonth=11 then oppcounts else 0 end  yue11,  \n  ");
                stringBuilder.AppendLine("case when rtmonth=12 then oppcounts else 0 end  yue12  \n  ");
                stringBuilder.AppendLine("from (  \n  ");
                stringBuilder.AppendLine("select  empnumber, empname,rtyear,rtmonth, count (distinct activitybillno)  oppcounts  \n  ");
                stringBuilder.AppendFormat("from {0}  opp \n", temTable1);
                stringBuilder.AppendLine("group by empnumber,empname,rtyear,rtmonth  \n  ");
                stringBuilder.AppendLine(")  toji0  \n  ");
                stringBuilder.AppendLine(") tojimonth  \n  ");
                stringBuilder.AppendLine("group by  empnumber,empname  \n  ");
                stringBuilder.AppendLine("  \n  ");
                stringBuilder.AppendLine(") toji2  \n  ");
                stringBuilder.AppendLine("  \n  ");
                stringBuilder.AppendLine("   \n  ");
                stringBuilder.AppendLine("left join   \n  ");
                stringBuilder.AppendLine("  (  \n  ");
                stringBuilder.AppendLine("select emp.fnumber empnumber,F_PEJK_OPPQUNTA,F_PEJK_OPPTRACKQUNTA ,F_PEJK_ACTIVITYQUNTA  \n  ");
                stringBuilder.AppendLine("from PEJK_SALERQUNTAENTRY SALERQUNTAENTRY   \n  ");
                stringBuilder.AppendLine("left join PEJK_SALERQUNTA SALERQUNTA on SALERQUNTA.fid=SALERQUNTAENTRY.fid  \n  ");
                stringBuilder.AppendLine("left join V_BD_SALESMAN salesman on salesman.fstaffid=SALERQUNTA.F_PEJK_SALER  \n  ");
                stringBuilder.AppendLine("left join T_BD_STAFF staff on staff.FSTAFFID= salesman.fstaffid  \n  ");
                stringBuilder.AppendLine("left join T_HR_EMPINFO emp on staff.FEMPINFOID=emp.FID  \n  ");
                stringBuilder.AppendLine("left join T_HR_EMPINFO_L empl on empl.FID=emp.FID  --员工   \n  ");
                stringBuilder.AppendLine(") zbdj on zbdj.empnumber=toji2.empnumber  \n  ");


                DBUtils.ExecuteDynamicObject(this.Context, stringBuilder.ToString());


                //插入 总计
                stringBuilder = new StringBuilder();
                stringBuilder.AppendFormat("insert into {0}", temTable2);
                stringBuilder.AppendLine("\n select '总计','',sum(yue1) yue1,sum(yue2) yue2,sum(yue3) yue3,sum(yue4)yue4,sum(yue5) yue5,sum(yue6) yue6,sum(yue7) yue7,sum(yue8) yue8,sum(yue9) yue9,sum(yue10) yue10,sum(yue11) yue11,sum(yue12) yue12,sum(zoji) zoji,isnull(sum(hdzb),0) hdzb,isnull(sum(hdwcqk),0) hdwcqk  ");
                stringBuilder.AppendFormat("from {0} ", temTable2);
                stringBuilder.AppendLine(" where   1=1 ");
                stringBuilder.AppendLine("  ");

                DBUtils.ExecuteDynamicObject(this.Context, stringBuilder.ToString());

                stringBuilder = new StringBuilder();
                stringBuilder.AppendFormat(" select ROW_NUMBER() OVER(ORDER BY empnumber) FIDENTITYID,tmp2.* into {0}   from   {1} tmp2  order by empnumber,empname  ", tableName, temTable2);

                DBUtils.ExecuteDynamicObject(this.Context, stringBuilder.ToString());
            }

        }

        public override void CloseReport()
        {
            base.CloseReport();
        }

        public override void CloseReportInstance()
        {
            base.CloseReportInstance();
        }

        public override void DropTempTable()
        {
            base.DropTempTable();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override MoveRptAllPage ExportMoveRptAllPage(IRptParams filter)
        {
            return base.ExportMoveRptAllPage(filter);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override DataTable GetList(IRptParams filter)
        {
            return base.GetList(filter);
        }

        public override List<long> GetOrgIdList(IRptParams filter)
        {
            return base.GetOrgIdList(filter);
        }

        public override ReportHeader GetReportHeaders(IRptParams filter)
        {
            ReportHeader header = new ReportHeader();
            DynamicObject cutomerfiler = filter.FilterParameter.CustomFilter;
            String year = null;//年份
            if (cutomerfiler["F_SXINSUN_YEAR"] != null)
            {
                year = Convert.ToInt32(cutomerfiler["F_SXINSUN_YEAR"]).ToString();
            }
            if (cutomerfiler["F_PAEZ_DEPT"] != null)
            {
                DynamicObjectCollection cols = (DynamicObjectCollection)cutomerfiler["F_PAEZ_DEPT"];
                foreach (DynamicObject dept in cols)
                {
                    String deptnumber = Convert.ToString(((DynamicObject)dept["F_PAEZ_DEPT"])["Number"]);

                }
            }
            if (cutomerfiler["F_PAEZ_MulBaseSaler"] != null)
            {
                DynamicObjectCollection cols = (DynamicObjectCollection)cutomerfiler["F_PAEZ_MulBaseSaler"];
                foreach (DynamicObject onesale in cols)
                {
                    String salenumber = Convert.ToString(((DynamicObject)onesale["F_PAEZ_MulBaseSaler"])["Number"]);

                }
            }

            if (Convert.ToBoolean(cutomerfiler["F_PAEZ_ShowDetail"]) == false)
            {
                // 编号
                //var deptname = header.AddChild("deptnumber", new LocaleValue("部门编码"));

                //var deptnumber = header.AddChild("deptname", new LocaleValue("部门"));

                var empnumber = header.AddChild("empnumber", new LocaleValue("员工编码"));

                var empname = header.AddChild("empname", new LocaleValue("员工"));

                var yue1 = header.AddChild("yue1", new LocaleValue("1月份"));

                //billNo.IsHyperlink = true;          // 支持超链接
                var yue2 = header.AddChild("yue2", new LocaleValue("2月份"));

                var yue3 = header.AddChild("yue3", new LocaleValue("3月份"));


                var yue4 = header.AddChild("yue4", new LocaleValue("4月份"));

                var yue5 = header.AddChild("yue5", new LocaleValue("5月份"));

                var yue6 = header.AddChild("yue6", new LocaleValue("6月份"));

                var yue7 = header.AddChild("yue7", new LocaleValue("7月份"));

                var yue8 = header.AddChild("yue8", new LocaleValue("8月份"));

                var yue9 = header.AddChild("yue9", new LocaleValue("9月份"));


                var yue10 = header.AddChild("yue10", new LocaleValue("10月份"));


                var yue11 = header.AddChild("yue11", new LocaleValue("11月份"));


                var yue12 = header.AddChild("yue12", new LocaleValue("12月份"));

                var zoji = header.AddChild("zoji", new LocaleValue("总计"));
            }

            //var zoji = header.AddChild("gzzj", new LocaleValue("总计"));

            // sum(dlzb) dlzb,sum(dlwqqk) dlwqqk,sum(gzzj) gzzj,sum(gzzb) gzzb,sum(gzzbwqqk) gzzbwqqk,sum(gzhud) gzhud
            else
            {
                var deptname = header.AddChild("deptnumber", new LocaleValue("部门编码"));

                var deptnumber = header.AddChild("deptname", new LocaleValue("部门"));

                var empnumber = header.AddChild("empnumber", new LocaleValue("员工编码"));

                var empname = header.AddChild("empname", new LocaleValue("员工"));

                var yue1 = header.AddChild("yue1", new LocaleValue("1月份"));

                //billNo.IsHyperlink = true;          // 支持超链接
                var yue2 = header.AddChild("yue2", new LocaleValue("2月份"));

                var yue3 = header.AddChild("yue3", new LocaleValue("3月份"));


                var yue4 = header.AddChild("yue4", new LocaleValue("4月份"));

                var yue5 = header.AddChild("yue5", new LocaleValue("5月份"));

                var yue6 = header.AddChild("yue6", new LocaleValue("6月份"));

                var yue7 = header.AddChild("yue7", new LocaleValue("7月份"));

                var yue8 = header.AddChild("yue8", new LocaleValue("8月份"));

                var yue9 = header.AddChild("yue9", new LocaleValue("9月份"));

                var yue10 = header.AddChild("yue10", new LocaleValue("10月份"));

                var yue11 = header.AddChild("yue11", new LocaleValue("11月份"));

                var yue12 = header.AddChild("yue12", new LocaleValue("12月份"));

                var zoji = header.AddChild("zoji", new LocaleValue("总计"));

                var dlzb = header.AddChild("dlzb", new LocaleValue("登录指标"));

                var dlwqqk = header.AddChild("dlwqqk", new LocaleValue("完成情况"));

                var gzzj = header.AddChild("gzzj", new LocaleValue("总计跟踪"));

                var gzzb = header.AddChild("gzzb", new LocaleValue("跟踪指标"));

                var gzzbwqqk = header.AddChild("gzzbwqqk", new LocaleValue("完成情况"));

                var gzhud = header.AddChild("gzhud", new LocaleValue("跟踪更新"));


            }

            return header;
        }

        public override ReportTitles GetReportTitles(IRptParams filter)
        {
            return base.GetReportTitles(filter);
        }

        public override int GetRowsCount(IRptParams filter)
        {
            return base.GetRowsCount(filter);
        }

        public override List<SummaryField> GetSummaryColumnInfo(IRptParams filter)
        {
            return base.GetSummaryColumnInfo(filter);
        }

        public override string GetTableName()
        {
            return base.GetTableName();
        }

        public override List<TreeNode> GetTreeNodes(IRptParams filter)
        {
            return base.GetTreeNodes(filter);
        }

        public override void Initialize()
        {
            base.Initialize();

            // 简单账表类型：普通、树形、分页
            this.ReportProperty.ReportType = ReportType.REPORTTYPE_NORMAL;
            // 报表名称
            this.ReportProperty.ReportName = new LocaleValue("商机登录明细表", base.Context.UserLocale.LCID);
            // 
            this.IsCreateTempTableByPlugin = true;
            // 
            this.ReportProperty.IsUIDesignerColumns = false;
            this.ReportProperty.IdentityFieldName = "FIDENTITYID";
            // 
            this.ReportProperty.IsGroupSummary = false;
            // 
            this.ReportProperty.SimpleAllCols = false;

        }

        public override string ToString()
        {
            return base.ToString();
        }

        protected override void AfterCreateTempTable(string tablename)
        {
            base.AfterCreateTempTable(tablename);
        }

        protected override string AnalyzeDspCloumn(IRptParams filter, string tablename)
        {
            return base.AnalyzeDspCloumn(filter, tablename);
        }

        protected override string BuilderFromWhereSQL(IRptParams filter)
        {
            return base.BuilderFromWhereSQL(filter);
        }

        protected override string BuilderSelectFieldSQL(IRptParams filter)
        {
            return base.BuilderSelectFieldSQL(filter);
        }

        protected override string BuilderTempTableOrderBySQL(IRptParams filter)
        {
            return base.BuilderTempTableOrderBySQL(filter);
        }

        protected override string CreateGroupSummaryData(IRptParams filter, string tablename)
        {
            return base.CreateGroupSummaryData(filter, tablename);
        }

        protected override void CreateTempTable(string sSQL)
        {
            base.CreateTempTable(sSQL);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected override void ExecuteBatch(List<string> listSql)
        {
            base.ExecuteBatch(listSql);
        }

        protected override string GetIdentityFieldIndexSQL(string tableName)
        {
            return base.GetIdentityFieldIndexSQL(tableName);
        }

        protected override DataTable GetListData(string sSQL)
        {
            return base.GetListData(sSQL);
        }

        protected override DataTable GetReportData(IRptParams filter)
        {
            return base.GetReportData(filter);
        }

        protected override DataTable GetReportData(string tablename, IRptParams filter)
        {
            return base.GetReportData(tablename, filter);
        }

        protected override string GetSummaryColumsSQL(List<SummaryField> summaryFields)
        {
            return base.GetSummaryColumsSQL(summaryFields);
        }

        protected override object InvokePluginMethod(string name)
        {
            return base.InvokePluginMethod(name);
        }

        protected override object InvokePluginMethod(string name, object args)
        {
            return base.InvokePluginMethod(name, args);
        }

        protected override object InvokePluginMethod(string name, object args1, object args2)
        {
            return base.InvokePluginMethod(name, args1, args2);
        }
    }

}