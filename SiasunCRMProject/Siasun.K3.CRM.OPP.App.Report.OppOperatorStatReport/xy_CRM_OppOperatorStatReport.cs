using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Kingdee.BOS;
using Kingdee.BOS.Util;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Core.Report.PlugIn;
using Kingdee.BOS.Core.Report.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.App;
using KEEPER.K3.CRM.CRMServiceHelper;

namespace Siasun.K3.CRM.OPP.App.Report.OppOperatorStatReport
{
    [Description("商机统计报表—业务员")]
    public class xy_CRM_OppOperatorStatReport : SysReportBaseService
    {
        private string[] tmpTables;
        public override void Initialize()
        {
            base.Initialize();

            this.ReportProperty.ReportType = ReportType.REPORTTYPE_NORMAL;
            this.ReportProperty.ReportName = new LocaleValue("商机统计报表—业务员", base.Context.UserLocale.LCID);
            this.IsCreateTempTableByPlugin = true;
            this.ReportProperty.IsUIDesignerColumns = false;
            this.ReportProperty.IdentityFieldName = "FIDENTITYID";
            this.ReportProperty.IsGroupSummary = false;
            this.ReportProperty.SimpleAllCols = false;
        }

        public override string GetTableName()
        {
            return base.GetTableName();
        }

        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            base.BuilderReportSqlAndTempTable(filter, tableName);

            // 根据当前用户的UserId  查询出其personId
            StringBuilder sql0 = new StringBuilder();
            sql0.AppendFormat(@"/*dialect*/ SELECT FLINKOBJECT FROM T_SEC_USER WHERE FUSERID = {0} ", this.Context.UserId);
            DynamicObjectCollection collection = DBUtils.ExecuteDynamicObject(this.Context, sql0.ToString());

            StringBuilder salerLimit = new StringBuilder();
            Boolean flag = false;

            if (collection.Count > 0)
            {
                //获取当前用户personId
                DynamicObject personIdObj = (DynamicObject)collection[0];
                int personId = Convert.ToInt32(personIdObj["FLINKOBJECT"]);

                //销售员数据隔离
                if (CRMServiceHelper.getSalerPersonids(this.Context, personId) != null)
                {
                    List<long> salerList = CRMServiceHelper.getSalerPersonids(this.Context, personId);
                    int len = 0;
                    flag = true;

                    if (salerList.Count >= 1)
                    {
                        salerLimit.Append(" IN ( ");
                    }

                    foreach (long salerId in salerList)
                    {
                        len++;
                        if (len == salerList.Count)
                        {
                            salerLimit.Append(" " + salerId + " ) ");
                        }
                        else
                        {
                            salerLimit.Append(" " + salerId + ", ");
                        }
                    }
                }
            }

            IDBService dbservice = ServiceHelper.GetService<IDBService>();
            tmpTables = dbservice.CreateTemporaryTableName(this.Context, 1);
            string tempTable1 = tmpTables[0];

            DynamicObject customFilter = filter.FilterParameter.CustomFilter;
            string fromDate = String.Empty;
            string toDate = String.Empty;
            string deptID = String.Empty;
            string salerID = String.Empty;
            string billStatus = String.Empty;

            if (customFilter["F_xy_FromDate"] != null) fromDate = string.Format("{0:yyyy-MM-dd}", customFilter["F_xy_FromDate"]);
            if (customFilter["F_xy_ToDate"] != null) toDate = string.Format("{0:yyyy-MM-dd}", customFilter["F_xy_ToDate"]);
            //if (customFilter["F_xy_Dept"] != null) deptID = customFilter["F_xy_Dept_Id"].ToString();
            //if (customFilter["F_xy_Saler"] != null) salerID = customFilter["F_xy_Saler_Id"].ToString();
            if (customFilter["F_xy_BillStatus"] != null) billStatus = customFilter["F_xy_BillStatus"].ToString();

            Boolean hasDept = !String.IsNullOrEmpty(deptID);
            Boolean hasSaler = !String.IsNullOrEmpty(salerID);
            Boolean hasBillStatus = !String.IsNullOrEmpty(billStatus);


            StringBuilder deptnumbersql = new StringBuilder();

            if (customFilter["F_PAEZ_DEPT"] != null)
            {

                DynamicObjectCollection cols = (DynamicObjectCollection)customFilter["F_PAEZ_DEPT"];
                int deptsize = 0;
                if (cols.Count >= 1)
                    deptnumbersql.Append("in (");
                foreach (DynamicObject dept in cols)
                {
                    String deptnumber = Convert.ToString(((DynamicObject)dept["F_PAEZ_DEPT"])["Id"]);
                    deptsize = deptsize + 1;
                    if (deptsize == cols.Count)
                        deptnumbersql.Append("'" + deptnumber + "')");
                    else
                        deptnumbersql.Append("'" + deptnumber + "',");


                }
            }

            StringBuilder billstatussql = new StringBuilder();
            StringBuilder salenumbersql = new StringBuilder();
            if (customFilter["F_PAEZ_MulBaseSaler"] != null)
            {
                DynamicObjectCollection cols = (DynamicObjectCollection)customFilter["F_PAEZ_MulBaseSaler"];
                int salesize = 0;
                if (cols.Count >= 1)
                    salenumbersql.Append("in (");
                foreach (DynamicObject onesale in cols)
                {
                    String salenumber = Convert.ToString(((DynamicObject)onesale["F_PAEZ_MulBaseSaler"])["Id"]);
                    salesize = salesize + 1;
                    if (salesize == cols.Count)
                        salenumbersql.Append("'" + salenumber + "')");
                    else
                        salenumbersql.Append("'" + salenumber + "',");

                }
            }


            StringBuilder sql = new StringBuilder();
            sql.Append(" select ROW_NUMBER() OVER(ORDER BY deptNO,empName) FIDENTITYID, ");
            sql.Append("	deptName,empName, ");
            sql.Append("	ISNULL(oppTotalCount,0) oppTotalCount, ");
            sql.Append("	ISNULL(oppRegCount,0) oppRegCount, ");
            sql.Append("	ISNULL(oppWinBillCount,0) oppWinBillCount, ");
            sql.Append("	ISNULL(oppLostBillCount,0) oppLostBillCount, ");
            sql.Append("	ISNULL(oppWinBillIncome,0) oppWinBillIncome, ");
            sql.Append("	ISNULL(oppLostBillIncome,0) oppLostBillIncome, ");
            sql.Append("	ISNULL(oppTotalBillIncome,0) oppTotalBillIncome, ");
            sql.Append("	ISNULL(oppQuota,0) oppQuota, ");
            sql.Append("	ISNULL(oppUnfinishedCount,0) oppUnfinishedCount ");
            sql.Append(" into " + tableName);
            sql.Append(" from ( ");
            sql.Append(" 	select empInfo.*,oppStat.*, ");
            sql.Append(" 		   oppQuota.F_PEJK_OPPQUNTA oppQuota,oppQuota.F_PEJK_OPPQUNTA-oppStat.oppTotalCount oppUnfinishedCount  ");

            //商机统计信息
            sql.Append(" 	from ( ");
            sql.Append(" 		select opp.FBEMPID, ");
            sql.Append(" 		count(1) oppTotalCount, ");//需要确认
            //sql.Append(" 		count(1) oppRegCount, ");
            sql.Append(" 		count(case when opp.FBEMPID = opp.FCREATORID then 1 else 0 end)   oppRegCount, ");
            sql.Append(" 		sum(case when opp.FDOCUMENTSTATUS='E' then 1 else 0 end) oppWinBillCount, ");
            sql.Append(" 		sum(case when opp.FDOCUMENTSTATUS='F' then 1 else 0 end) oppLostBillCount, ");
            sql.Append(" 		sum(case when opp.FDOCUMENTSTATUS='E' then opp.FESTIMATEINCOME else 0 end) oppWinBillIncome, ");
            sql.Append(" 		sum(case when opp.FDOCUMENTSTATUS='F' then opp.FESTIMATEINCOME else 0 end) oppLostBillIncome, ");
            sql.Append(" 		sum(opp.FESTIMATEINCOME) oppTotalBillIncome ");
            sql.Append(" 		from T_CRM_OPPORTUNITY opp ");
            if (hasBillStatus)
            {
                sql.Append("    where CHARINDEX(opp.FDOCUMENTSTATUS, '" + billStatus + "') > 0 ");
            }
            else
            {
                sql.Append(" 		where opp.FDOCUMENTSTATUS>='C' ");
            }
            if (hasDept)
            {
                sql.Append(" 		and opp.FSALEDEPTID='" + deptID + "' ");
            }
            if (hasSaler)
            {
                sql.Append(" 		and opp.FBEMPID='" + salerID + "' ");
            }


            //部门
            if (deptnumbersql != null && deptnumbersql.Length > 0)
            {
                sql.Append(" and    opp.FSALEDEPTID ").Append(deptnumbersql);


            }
            //销售员
            if (salenumbersql != null && salenumbersql.Length > 0)
            {
                sql.Append(" and  opp.FBEMPID ").Append(salenumbersql);


            }


            //销售数据隔离
            if (flag)
            {
                sql.AppendLine(" and opp.FBEMPID ").Append(salerLimit);
            }

            sql.Append(" 		and opp.FSTARTDATE between '" + fromDate + "' and '" + toDate + "' ");
            sql.Append(" 		group by opp.FBEMPID ");
            sql.Append(" 	) oppStat  ");

            //销售指标
            sql.Append(" 	left join ( ");
            sql.Append(" 		select quota_entry.F_PEJK_SALER,quota_entry.F_PEJK_OPPQUNTA from PEJK_SALERQUNTA quota  ");
            sql.Append(" 		inner join PEJK_SALERQUNTAENTRY quota_entry on quota.FID=quota_entry.FID ");
            sql.Append(" 		where quota.FDOCUMENTSTATUS='C' ");
            //if (hasDept)
            //{
            //    sql.Append(" 		and quota.F_PEJK_SALEDEPT='" + deptID + "' ");
            //}
            if (hasSaler)
            {
                sql.Append(" 		and quota_entry.F_PEJK_SALER='" + salerID + "' ");
            }
            //部门
            //if (deptnumbersql != null && deptnumbersql.Length > 0)
            //{
            //    sql.Append(" and    quota.F_PEJK_SALEDEPT ").Append(deptnumbersql);


            //}
            //销售员
            if (salenumbersql != null && salenumbersql.Length > 0)
            {
                sql.Append(" and  quota_entry.F_PEJK_SALER ").Append(salenumbersql);


            }
            //销售数据隔离
            if (flag)
            {
                sql.AppendLine(" and quota_entry.F_PEJK_SALER ").Append(salerLimit);
            }
            sql.Append(" 		and Year(quota.F_PEJK_YEAR)=Year('" + fromDate + "') ");
            sql.Append(" 	) oppQuota on oppStat.FBEMPID=oppQuota.F_PEJK_SALER ");

            //人员信息
            sql.Append(" 	right join ( ");
            sql.Append(" 		select distinct dept.FNUMBER deptNO,deptl.FNAME deptName,saler.fid salerID,empl.fname empName  ");
            sql.Append(" 		from T_CRM_Opportunity opp ");
            sql.Append(" 		inner join  V_BD_SALESMAN saler ");
            sql.Append(" 		on opp.FBEMPID = saler.fid ");
            sql.Append(" 		inner join T_BD_STAFF staff on saler.FSTAFFID=staff.FSTAFFID ");
            sql.Append(" 		inner join T_HR_EMPINFO_L empl on empl.FID=staff.FEMPINFOID and empl.FLOCALEID='2052' ");
            sql.Append(" 		inner join T_BD_DEPARTMENT_L deptl on opp.FSALEDEPTID=deptl.FDEPTID and deptl.FLOCALEID='2052' ");
            sql.Append("        inner join T_BD_DEPARTMENT dept on deptl.FDEPTID=dept.FDEPTID and dept.FDOCUMENTSTATUS='C' ");
            sql.Append(" 		where saler.FDOCUMENTSTATUS='C'  ");
            sql.Append(" 		and saler.FISUSE=1  ");
            sql.Append(" 		and saler.FFORBIDSTATUS='A' ");
            //sql.Append(" 		and saler.FBIZORGID='100041' "); //需要确认
            if (hasDept)
            {
                sql.Append("    and opp.FSALEDEPTID='" + deptID + "' ");
            }
            if (hasSaler)
            {
                sql.Append("    and saler.fid='" + salerID + "' ");
            }

            //部门
            if (deptnumbersql != null && deptnumbersql.Length > 0)
            {
                sql.Append(" and  opp.FSALEDEPTID ").Append(deptnumbersql);


            }
            //销售员
            if (salenumbersql != null && salenumbersql.Length > 0)
            {
                sql.Append(" and  saler.fid ").Append(salenumbersql);


            }

            //销售数据隔离
            if (flag)
            {
                sql.AppendLine(" and saler.fid ").Append(salerLimit);
            }
            sql.Append(" 	) empInfo on oppStat.FBEMPID=empInfo.salerID ");
            sql.Append(" ) tt ");

            DBUtils.ExecuteDynamicObject(this.Context, sql.ToString());

        }

        public override ReportHeader GetReportHeaders(IRptParams filter)
        {
            ReportHeader header = new ReportHeader();
            header.Mergeable = true;

            header.AddChild("deptName", new LocaleValue("销售部门"));
            header.AddChild("empName", new LocaleValue("销售员"));
            header.AddChild("oppTotalCount", new LocaleValue("总商机数量"), SqlStorageType.SqlInt);
            header.AddChild("oppRegCount", new LocaleValue("商机登录数量"), SqlStorageType.SqlInt);
            header.AddChild("oppWinBillCount", new LocaleValue("赢单数量"), SqlStorageType.SqlInt);
            header.AddChild("oppLostBillCount", new LocaleValue("输单数量"), SqlStorageType.SqlInt);
            header.AddChild("oppQuota", new LocaleValue("商机指标"), SqlStorageType.SqlInt);
            header.AddChild("oppUnfinishedCount", new LocaleValue("未完成数量"), SqlStorageType.SqlInt);
            header.AddChild("oppWinBillIncome", new LocaleValue("赢单金额"), SqlStorageType.SqlDecimal);
            header.AddChild("oppLostBillIncome", new LocaleValue("输单金额"), SqlStorageType.SqlDecimal);
            header.AddChild("oppTotalBillIncome", new LocaleValue("预计收入"), SqlStorageType.SqlDecimal);

            return header;
        }

        public override ReportTitles GetReportTitles(IRptParams filter)
        {
            var result = base.GetReportTitles(filter);
            DynamicObject customFilter = filter.FilterParameter.CustomFilter;
            if (customFilter != null)
            {
                if (result == null)
                {
                    result = new ReportTitles();
                }

                if (customFilter["F_xy_FromDate"] != null && customFilter["F_xy_ToDate"] != null)
                {
                    result.AddTitle("F_xy_titleDate", string.Format(@"      日期：{0:yyyy/MM/dd}", customFilter["F_xy_FromDate"]) + " - " + string.Format(@"{0:yyyy/MM/dd}", customFilter["F_xy_ToDate"]));
                }
            }
            return result;
        }

        /// <summary>
        /// 设置报表合计列
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public override List<SummaryField> GetSummaryColumnInfo(IRptParams filter)
        {
            var result = base.GetSummaryColumnInfo(filter);
            DynamicObject customFilter = filter.FilterParameter.CustomFilter;
            result.Add(new SummaryField("oppTotalCount", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("oppRegCount", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("oppWinBillCount", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("oppLostBillCount", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("oppQuota", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("oppUnfinishedCount", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("oppWinBillIncome", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("oppLostBillIncome", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("oppTotalBillIncome", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            return result;
        }

        protected override string GetIdentityFieldIndexSQL(string tableName)
        {
            return base.GetIdentityFieldIndexSQL(tableName);
        }

        protected override void ExecuteBatch(List<string> listSql)
        {
            base.ExecuteBatch(listSql);
        }

        protected override string AnalyzeDspCloumn(IRptParams filter, string tablename)
        {
            return base.AnalyzeDspCloumn(filter, tablename);
        }

        protected override void AfterCreateTempTable(string tablename)
        {
            base.AfterCreateTempTable(tablename);
        }

        protected override string GetSummaryColumsSQL(List<SummaryField> summaryFields)
        {
            return base.GetSummaryColumsSQL(summaryFields);
        }

        protected override System.Data.DataTable GetListData(string sSQL)
        {
            return base.GetListData(sSQL);
        }

        protected override System.Data.DataTable GetReportData(IRptParams filter)
        {
            return base.GetReportData(filter);
        }

        protected override System.Data.DataTable GetReportData(string tablename, IRptParams filter)
        {
            return base.GetReportData(tablename, filter);
        }

        public override int GetRowsCount(IRptParams filter)
        {
            return base.GetRowsCount(filter);
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

        public override void CloseReport()
        {
            base.CloseReport();
        }

        protected override string CreateGroupSummaryData(IRptParams filter, string tablename)
        {
            return base.CreateGroupSummaryData(filter, tablename);
        }

        protected override void CreateTempTable(string sSQL)
        {
            base.CreateTempTable(sSQL);
        }

        public override void DropTempTable()
        {
            base.DropTempTable();
        }

        public override System.Data.DataTable GetList(IRptParams filter)
        {
            return base.GetList(filter);
        }

        public override List<long> GetOrgIdList(IRptParams filter)
        {
            return base.GetOrgIdList(filter);
        }

        public override List<Kingdee.BOS.Core.Metadata.TreeNode> GetTreeNodes(IRptParams filter)
        {
            return base.GetTreeNodes(filter);
        }
    }
}
