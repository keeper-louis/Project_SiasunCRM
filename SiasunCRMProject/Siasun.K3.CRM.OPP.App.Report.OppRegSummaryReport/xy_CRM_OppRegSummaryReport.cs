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

namespace Siasun.K3.CRM.OPP.App.Report.OppRegSummaryReport
{
    [Description("商机登录汇总表")]
    public class xy_CRM_OppRegSummaryReport : SysReportBaseService
    {
        public override void Initialize()
        {
            base.Initialize();

            this.ReportProperty.ReportType = ReportType.REPORTTYPE_NORMAL;
            this.ReportProperty.ReportName = new LocaleValue("商机登录汇总表", base.Context.UserLocale.LCID);
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

            DynamicObject customFilter = filter.FilterParameter.CustomFilter;

            if (customFilter["F_xy_Dimension"] == null || customFilter["F_xy_Year"] == null)
            {
                throw new Exception("未选择维度或年份条件");
            }

            string dimension = customFilter["F_xy_Dimension"].ToString();
            string year = customFilter["F_xy_Year"].ToString();
            string billStatus = customFilter["F_xy_BillStatus"].ToString();
            string sql = ElementGenerator.GetSqlByDimension(dimension,year, billStatus,tableName);
            DBUtils.ExecuteDynamicObject(this.Context, sql);
        }

        public override ReportHeader GetReportHeaders(IRptParams filter)
        {
            DynamicObject customFilter = filter.FilterParameter.CustomFilter;

            if (customFilter["F_xy_Dimension"] == null || customFilter["F_xy_Year"] == null)
            {
                throw new Exception("未选择维度或年份条件");
            }

            string dimension = customFilter["F_xy_Dimension"].ToString();
            ReportHeader header = ElementGenerator.GetHeaderByDimension(dimension);

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

                string dimension = string.Empty;
                string desc = string.Empty;
                if (customFilter["F_xy_Dimension"] != null)
                {
                    dimension = customFilter["F_xy_Dimension"].ToString();
                }

                desc = ElementGenerator.GetTitleByDimension(dimension);

                result.AddTitle("F_xy_Title", "年度公司商机登录数量" + desc);
                result.AddTitle("F_xy_Year", "年度: " + string.Format("{0:yyyy}", customFilter["F_xy_Year"]) + "      ");
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
            if (customFilter["F_xy_Dimension"] == null || customFilter["F_xy_Year"] == null)
            {
                throw new Exception("未选择维度或年份条件");
            }

            string dimension = customFilter["F_xy_Dimension"].ToString();
            if (dimension=="1" || dimension=="3" || dimension=="4")
            {
                result.Add(new SummaryField("oppCount", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            }
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
