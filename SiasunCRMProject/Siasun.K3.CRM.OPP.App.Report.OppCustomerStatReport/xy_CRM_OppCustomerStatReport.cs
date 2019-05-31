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

namespace Siasun.K3.CRM.OPP.App.Report.OppCustomerStatReport
{
    [Description("商机统计报表—客户")]
    public class xy_CRM_OppCustomerStatReport : SysReportBaseService
    {
        private string[] userTempTable;
        public override void Initialize()
        {
            base.Initialize();

            this.ReportProperty.ReportType = ReportType.REPORTTYPE_NORMAL;
            this.ReportProperty.ReportName = new LocaleValue("商机统计报表—客户", base.Context.UserLocale.LCID);
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

            IDBService dbservice = ServiceHelper.GetService<IDBService>();
            userTempTable = dbservice.CreateTemporaryTableName(this.Context, 1);
            string tempTable1 = userTempTable[0];

            DynamicObject customFilter = filter.FilterParameter.CustomFilter;
            string customerIndustryID = String.Empty;
            string customerRelationshipID = String.Empty;
            string endUserID = String.Empty;
            string salesDeptID = String.Empty;
            string fromDate = String.Empty;
            string toDate = String.Empty;
            
            if (customFilter["F_xy_CustomerIndustry"] != null) customerIndustryID = customFilter["F_xy_CustomerIndustry_Id"].ToString();
            if (customFilter["F_xy_CustomerRelationship"] != null) customerRelationshipID = customFilter["F_xy_CustomerRelationship_Id"].ToString();
            if (customFilter["F_xy_EndUser"] != null) endUserID = customFilter["F_xy_EndUser_Id"].ToString();
            if (customFilter["F_xy_SalesDept"] != null) salesDeptID = customFilter["F_xy_SalesDept_Id"].ToString();
            if (customFilter["F_xy_FromDate"] != null) fromDate = string.Format("{0:yyyy-MM-dd}", customFilter["F_xy_FromDate"]);
            if (customFilter["F_xy_ToDate"] != null) toDate = string.Format("{0:yyyy-MM-dd}", customFilter["F_xy_ToDate"]);

            Boolean hasCustomerIndustry = !String.IsNullOrEmpty(customerIndustryID);
            Boolean hasCustomerRelationship = !String.IsNullOrEmpty(customerRelationshipID);
            Boolean hasEndUser = !String.IsNullOrEmpty(endUserID);
            Boolean hasSaleDept = !String.IsNullOrEmpty(salesDeptID);

            if (hasEndUser)
            {
                String s = String.Format(" select distinct fid into {0} from T_CRM_OpportunityProduct where F_PEJK_FINALU='{1}' ", tempTable1, endUserID);
                DBUtils.ExecuteDynamicObject(this.Context, s);
            }

            StringBuilder sql = new StringBuilder();
            sql.Append(" select ROW_NUMBER() over(order by customerNO) FIDENTITYID, ");
            sql.Append("        customerNo,customerName,oppTotalCount,oppWinBillCount,oppLostBillCount, ");
            sql.Append("        oppTotalCount-oppWinBillCount-oppLostBillCount oppUnfinishedCount, ");
            sql.Append(" 	    oppWinBillIncome,oppTotalIncome ");
            sql.Append(" into " + tableName);
            sql.Append(" from ( ");
            sql.Append(" 	select  cust.FNUMBER customerNO,cust_l.FNAME customerName, ");
            sql.Append(" 			count(opp.FID) oppTotalCount, ");
            sql.Append(" 			sum(case when opp.FDOCUMENTSTATUS='E' then 1 else 0 end) oppWinBillCount, ");
            sql.Append(" 			sum(case when opp.FDOCUMENTSTATUS='F' then 1 else 0 end) oppLostBillCount, ");
            sql.Append(" 			sum(case when opp.FDOCUMENTSTATUS='E' then opp.FESTIMATEINCOME else 0 end) oppWinBillIncome, ");
            sql.Append(" 			sum(opp.FESTIMATEINCOME) oppTotalIncome ");
            sql.Append(" 	from T_CRM_OPPORTUNITY opp ");
            sql.Append(" 	inner join T_BD_CUSTOMER cust on opp.FCUSTOMERID=cust.FCUSTID  ");
            sql.Append(" 	inner join T_BD_CUSTOMER_L cust_l on opp.FCUSTOMERID=cust_l.FCUSTID ");
            if (hasEndUser)
            {
                sql.Append(" 	inner join "+ tempTable1 +" prod on prod.FID=opp.FID ");
            }

            sql.Append(" 	where opp.FDOCUMENTSTATUS >='C' ");
            sql.Append(" 	and opp.F_PEJK_AUDITDATE between '"+ fromDate +"' and '"+ toDate +"' ");

            if (hasCustomerIndustry)
            {
                sql.Append(" 	and opp.F_PEJK_CUSTINDUSTRY='"+ customerIndustryID +"' ");
            }
            if (hasCustomerRelationship)
            {
                sql.Append(" 	and opp.F_PEJK_CUSTSHIP='"+ customerRelationshipID +"' ");
            }
            if (hasSaleDept)
            {
                sql.Append(" 	and opp.FSALEDEPTID='"+ salesDeptID +"' ");
            }

            sql.Append(" 	group by cust.FNUMBER,cust_l.FNAME ");
            sql.Append(" ) tt ");
 

            DBUtils.ExecuteDynamicObject(this.Context, sql.ToString());
        }

        public override ReportHeader GetReportHeaders(IRptParams filter)
        {
            ReportHeader header = new ReportHeader();
            header.Mergeable = true;

            header.AddChild("customerNo", new LocaleValue("客户代码"));
            header.AddChild("customerName", new LocaleValue("客户名称"));
            header.AddChild("oppTotalCount", new LocaleValue("总商机数量"), SqlStorageType.SqlInt);
            header.AddChild("oppWinBillCount", new LocaleValue("赢单数量"), SqlStorageType.SqlInt);
            header.AddChild("oppLostBillCount", new LocaleValue("输单数量"), SqlStorageType.SqlInt);
            header.AddChild("oppUnfinishedCount", new LocaleValue("未完成数量"), SqlStorageType.SqlInt);
            header.AddChild("oppWinBillIncome", new LocaleValue("赢单金额"), SqlStorageType.SqlDecimal);
            header.AddChild("oppTotalIncome", new LocaleValue("预计收入"), SqlStorageType.SqlDecimal);

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
                if (customFilter["F_xy_SalesDept"] != null)
                {
                    result.AddTitle("F_xy_titleSalesDept", ((DynamicObject)customFilter["F_xy_SalesDept"])["Name"].ToString());
                }
                if (customFilter["F_xy_FromDate"] !=null && customFilter["F_xy_ToDate"]!=null)
                {
                    result.AddTitle("F_xy_titleDate", string.Format(@"{0:yyyy/MM/dd}", customFilter["F_xy_FromDate"]) + " - " + string.Format(@"{0:yyyy/MM/dd}", customFilter["F_xy_ToDate"]));
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
            result.Add(new SummaryField("oppTotalCount", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("oppWinBillCount", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("oppLostBillCount", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("oppUnfinishedCount", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("oppWinBillIncome", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            result.Add(new SummaryField("oppTotalIncome", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
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
