using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts.Report;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.App;
using System.ComponentModel;
using KEEPER.K3.CRM.CRMServiceHelper;

namespace SalerBillByTotal
{
    [Description("销售员销售报表(汇总)")]
    public class SalerBillByTotalPlugin : SysReportBaseService
    {
        //临时表数组
        private String[] materialRptTableNames;

        //报表初始化   
        public override void Initialize()
        {
            base.Initialize();

            //设置报表类型：简单报表
            this.ReportProperty.ReportType = Kingdee.BOS.Core.Report.ReportType.REPORTTYPE_NORMAL;
            //设置报表名称
            this.ReportProperty.ReportName = new Kingdee.BOS.LocaleValue("销售员销售报表(汇总)", base.Context.UserLocale.LCID);
            this.IsCreateTempTableByPlugin = true;
            this.ReportProperty.IsUIDesignerColumns = false;
            this.ReportProperty.IsGroupSummary = true;
            this.ReportProperty.SimpleAllCols = false;

            //设置报表主键字段名
            this.ReportProperty.IdentityFieldName = "FSeq";
        }

        //执行查询sql 按时间 销售员 销售部门进行过滤
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

                //获取当前用户权限内的销售数据
                if (CRMServiceHelper.getSalerPersonids(this.Context, personId) != null)
                {
                    //获取当前用户可见的销售员集合
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


            //生成中间临时表
            IDBService dbservice = ServiceHelper.GetService<IDBService>();
            materialRptTableNames = dbservice.CreateTemporaryTableName(this.Context, 10);
            string tmpTable1 = materialRptTableNames[0];
            string tmpTable2 = materialRptTableNames[1];
            string tmpTable3 = materialRptTableNames[2];
            string tmpTable4 = materialRptTableNames[3];
            string tmpTable5 = materialRptTableNames[4];
            string tmpTable6 = materialRptTableNames[5];

            //过滤条件：起始日期/截至日期/部门/业务员
            DynamicObject dyFilter = filter.FilterParameter.CustomFilter;
            String startDate = "";    //起始日期
            String endDate = "";      //截至日期

            //销售员
            StringBuilder salerSql = new StringBuilder();
            if (dyFilter["F_QSNC_SalerFilter"] != null && ((DynamicObjectCollection)dyFilter["F_QSNC_SalerFilter"]).Count > 0)
            {
                //获取到多选基础资料中所有选中项
                DynamicObjectCollection cols1 = (DynamicObjectCollection)dyFilter["F_QSNC_SalerFilter"];
                int salerNum = 0;

                if (cols1.Count >= 1)
                {
                    salerSql.Append(" IN (");
                }

                foreach (DynamicObject saler in cols1)
                {
                    String salerNumber = Convert.ToString(((DynamicObject)saler["F_QSNC_SalerFilter"])["Id"]);
                    salerNum++;

                    if (cols1.Count == salerNum)
                    {
                        salerSql.Append(salerNumber + ") ");
                    }
                    else
                    {
                        salerSql.Append(salerNumber + ", ");
                    }
                }
            }

            //部门
            StringBuilder deptSql = new StringBuilder();
            if (dyFilter["F_QSNC_DeptFilter"] != null && ((DynamicObjectCollection)dyFilter["F_QSNC_DeptFilter"]).Count > 0)
            {
                //获取到多选基础资料中所有选中项
                DynamicObjectCollection cols2 = (DynamicObjectCollection)dyFilter["F_QSNC_DeptFilter"];
                int deptNum = 0;

                if (cols2.Count >= 1)
                {
                    deptSql.Append(" IN (");
                }

                foreach (DynamicObject dept in cols2)
                {
                    String deptNumber = Convert.ToString(((DynamicObject)dept["F_QSNC_DeptFilter"])["Id"]);
                    deptNum++;

                    if (cols2.Count == deptNum)
                    {
                        deptSql.Append(deptNumber + ") ");
                    }
                    else
                    {
                        deptSql.Append(deptNumber + ", ");
                    }
                }
            }

            //报表sql

            String yearFilter = "(";
            if (dyFilter["F_QSNC_StartDateFilter"] != null)
            {
                String year1 = Convert.ToDateTime(dyFilter["F_QSNC_StartDateFilter"]).Year.ToString();
                yearFilter += "'" + year1 + "',";
            }
            if (dyFilter["F_QSNC_EndDateFilter"] != null)
            {
                String year2 = Convert.ToDateTime(dyFilter["F_QSNC_EndDateFilter"]).Year.ToString();
                yearFilter += "'" + year2 + "',";
            }
            if (dyFilter["F_QSNC_StartDateFilter"] != null || dyFilter["F_QSNC_EndDateFilter"] != null)
            {
                yearFilter = yearFilter.TrimEnd(',');
            }
            yearFilter += ")";

            // ---------------------------------------------------------------------------------
            // tmpTable1中存放所有进行合同指标规划的 销售部门、销售员、合同指标
            StringBuilder sql1 = new StringBuilder();
            sql1.AppendFormat(@"/*dialect*/ SELECT  F_PEJK_SALEDEPT, SQE.F_PEJK_SALER, SUM(F_PEJK_CONTRACTQUNTA) YEARQUOTA INTO {0} FROM PEJK_SALERQUNTAENTRY SQE LEFT JOIN PEJK_SALERQUNTA SQH ON SQE.FID = SQH.FID WHERE SQH.FDOCUMENTSTATUS = 'C' AND SQH.F_PEJK_SALEDEPT != 0 ", tmpTable1);

            if (dyFilter["F_QSNC_StartDateFilter"] != null || dyFilter["F_QSNC_EndDateFilter"] != null)
            {
                sql1.AppendFormat(" AND datepart(yyyy,F_PEJK_YEAR) IN {0} ", yearFilter);
            }

            sql1.AppendLine(" GROUP BY F_PEJK_SALEDEPT, SQE.F_PEJK_SALER ");

            DBUtils.ExecuteDynamicObject(this.Context, sql1.ToString());

            // ---------------------------------------------------------------------------------
            // tmpTable2存放所有进行销售合同分解的中销售员、合同分解金额
            StringBuilder sql2 = new StringBuilder();
            sql2.AppendFormat(@"/*dialect*/ SELECT F_PEJK_SALER, SUM(F_PEJK_DETAILLAMOUNT) FINISHAMOUNT INTO {0} FROM PEJK_SALECONTRACTENTRY E LEFT JOIN PEJK_SALECONTRACTS H ON E.FID = H.FID WHERE F_PEJK_SALER != 0 AND FDOCUMENTSTATUS = 'C' GROUP BY F_PEJK_SALER ", tmpTable2);


            if (dyFilter["F_QSNC_StartDateFilter"] != null)
            {
                startDate = Convert.ToDateTime(dyFilter["F_QSNC_StartDateFilter"]).ToString("yyyy-MM-dd 00:00:00");
                sql1.AppendFormat(" AND H.F_PEJK_DATE >= '{0}' ", startDate);
            }
            if (dyFilter["F_QSNC_EndDateFilter"] != null)
            {
                endDate = Convert.ToDateTime(dyFilter["F_QSNC_EndDateFilter"]).ToString("yyyy-MM-dd 23:59:59");
                sql1.AppendFormat(" AND H.F_PEJK_DATE <= '{0}' ", endDate);
            }
            DBUtils.ExecuteDynamicObject(this.Context, sql2.ToString());

            // ---------------------------------------------------------------------------------
            // tmpTable3 
            StringBuilder sql3 = new StringBuilder();
            sql3.AppendFormat(@"/*dialect*/ SELECT TMP1.F_PEJK_SALEDEPT, TMP1.F_PEJK_SALER, TMP1.YEARQUOTA QUOTA, ISNULL(TMP2.FINISHAMOUNT, 0) COMPLETEAMOUNT INTO {0} FROM {1} TMP1 LEFT JOIN {2} TMP2 ON TMP1.F_PEJK_SALER = TMP2.F_PEJK_SALER ", tmpTable3, tmpTable1, tmpTable2);
            DBUtils.ExecuteDynamicObject(this.Context, sql3.ToString());

            // ---------------------------------------------------------------------------------
            // tmpTable4 
            StringBuilder sql4 = new StringBuilder();
            sql4.AppendFormat(@"/*dialect*/ SELECT DEPTL.FNAME DEPTNAME, SALESMANL.FNAME SALERNAME, QUOTA, COMPLETEAMOUNT, CASE WHEN QUOTA = 0 THEN '0 %' ELSE CAST(CONVERT(FLOAT, ROUND((COMPLETEAMOUNT * 1.00 / (QUOTA * 1.00)) * 100, 3)) as varchar)+' %' END AS COMPLETERATE INTO {0} FROM {1} TMP1
                                            LEFT JOIN V_BD_SALESMAN_L SALESMANL ON SALESMANL.FID = TMP1.F_PEJK_SALER
                                            LEFT JOIN T_BD_DEPARTMENT_L DEPTL ON DEPTL.FDEPTID = TMP1.F_PEJK_SALEDEPT
                                            LEFT JOIN T_BD_DEPARTMENT DEPT ON DEPTL.FDEPTID = DEPT.FDEPTID WHERE SALESMANL.FNAME IS NOT NULL ", tmpTable4, tmpTable3);

            //判断销售员条件
            if (dyFilter["F_QSNC_SalerFilter"] != null && ((DynamicObjectCollection)dyFilter["F_QSNC_SalerFilter"]).Count > 0)
            {
                sql1.AppendLine(" AND TMP1.F_PEJK_SALER ").Append(salerSql);
            }

            //销售员数据隔离
            if (flag)
            {
                sql1.AppendLine(" AND TMP1.F_PEJK_SALER ").Append(salerLimit);
            }

            //判断销售部门条件
            if (dyFilter["F_QSNC_DeptFilter"] != null && ((DynamicObjectCollection)dyFilter["F_QSNC_DeptFilter"]).Count > 0)
            {
                sql1.AppendLine(" AND TMP1.F_PEJK_SALEDEPT ").Append(deptSql);
            }
            DBUtils.ExecuteDynamicObject(this.Context, sql4.ToString());


            StringBuilder sql5 = new StringBuilder();
            sql5.AppendFormat(@"/*dialect*/INSERT INTO {0} SELECT '最终合计' DEPTNAME, '' SALERNAME, ISNULL(TOTALQUOTA, 0) TOTALQUOTA1, ISNULL(TOTALCOMPLETEAMOUNT, 0) TOTALCOMPLETEAMOUNT1, CASE WHEN TOTALQUOTA IS NULL THEN '0 %' ELSE CAST(CONVERT(FLOAT, ROUND((TOTALCOMPLETEAMOUNT * 1.00 / (TOTALQUOTA * 1.00)) * 100, 3)) as varchar) + ' %' END AS COMPLETERATE
            FROM 
            (SELECT SUM(QUOTA) TOTALQUOTA, SUM(COMPLETEAMOUNT) TOTALCOMPLETEAMOUNT FROM {1}) TMP  ", tmpTable4, tmpTable4);
            DBUtils.ExecuteDynamicObject(this.Context, sql5.ToString());

            StringBuilder sql6 = new StringBuilder();
            sql6.AppendFormat(@"/*dialect*/ SELECT ROW_NUMBER() OVER (ORDER BY DEPTNAME) AS FSeq, DEPTNAME, SALERNAME, CONVERT(FLOAT, ROUND(QUOTA, 2)) AS TOTALQUOTA, CONVERT(FLOAT, ROUND(COMPLETEAMOUNT, 2)) AS TOTALAMOUNT, COMPLETERATE INTO {0} ", tableName);
            sql6.AppendFormat(" FROM {0}  ", tmpTable4);
            DBUtils.ExecuteDynamicObject(this.Context, sql6.ToString());
        }

        //构建报表列
        public override ReportHeader GetReportHeaders(IRptParams filter)
        {
            ReportHeader header = new ReportHeader();

            //销售部门
            var department = header.AddChild("DEPTNAME", new Kingdee.BOS.LocaleValue("销售部门"));
            department.ColIndex = 0;
            department.Width = 150;

            //销售员
            var salesman = header.AddChild("SALERNAME", new Kingdee.BOS.LocaleValue("销售员"));
            salesman.ColIndex = 1;
            salesman.Width = 120;

            //指标
            var quota = header.AddChild("TOTALQUOTA", new Kingdee.BOS.LocaleValue("指标"));
            quota.ColIndex = 2;
            quota.Width = 150;

            //完成金额
            var amount = header.AddChild("TOTALAMOUNT", new Kingdee.BOS.LocaleValue("完成金额"));
            amount.ColIndex = 3;
            amount.Width = 150;

            //完成占比
            var rate = header.AddChild("COMPLETERATE", new Kingdee.BOS.LocaleValue("完成占比"));
            rate.ColIndex = 4;
            rate.Width = 150;

            return header;
        }

        //反写报表过滤信息
        public override ReportTitles GetReportTitles(IRptParams filter)
        {
            var result = base.GetReportTitles(filter);
            DynamicObject dyFilter = filter.FilterParameter.CustomFilter;

            if (dyFilter != null)
            {
                if (result == null)
                {
                    result = new ReportTitles();
                }
                //反写过滤条件
                //起始日期
                if (dyFilter["F_QSNC_StartDateFilter"] == null)
                {
                    result.AddTitle("F_QSNC_StartDate", "");
                }
                else
                {
                    result.AddTitle("F_QSNC_StartDate", Convert.ToString(dyFilter["F_QSNC_StartDateFilter"]));
                }

                //截止日期
                if (dyFilter["F_QSNC_EndDateFilter"] == null)
                {
                    result.AddTitle("F_QSNC_EndDate", "");
                }
                else
                {
                    result.AddTitle("F_QSNC_EndDate", Convert.ToString(dyFilter["F_QSNC_EndDateFilter"]));
                }

                //销售部门
                if (dyFilter["F_QSNC_DeptFilter"] != null && ((DynamicObjectCollection)dyFilter["F_QSNC_DeptFilter"]).Count > 0)
                {
                    StringBuilder deptName = new StringBuilder();
                    DynamicObjectCollection cols = (DynamicObjectCollection)dyFilter["F_QSNC_DeptFilter"];
                    foreach (DynamicObject dept in cols)
                    {
                        String tmpName = Convert.ToString(((DynamicObject)dept["F_QSNC_DeptFilter"])["Name"]);
                        deptName.Append(tmpName + "; ");
                    }

                    result.AddTitle("F_QSNC_Department", deptName.ToString());
                }
                else
                {
                    result.AddTitle("F_QSNC_Department", "全部");
                }

                //销售员
                if (dyFilter["F_QSNC_SalerFilter"] != null && ((DynamicObjectCollection)dyFilter["F_QSNC_SalerFilter"]).Count > 0)
                {
                    StringBuilder salerName = new StringBuilder();
                    DynamicObjectCollection cols = (DynamicObjectCollection)dyFilter["F_QSNC_SalerFilter"];
                    foreach (DynamicObject saler in cols)
                    {
                        String tmpName = Convert.ToString(((DynamicObject)saler["F_QSNC_SalerFilter"])["Name"]);
                        salerName.Append(tmpName + "; ");
                    }

                    result.AddTitle("F_QSNC_Saler", salerName.ToString());
                }
                else
                {
                    result.AddTitle("F_QSNC_Saler", "全部");
                }
            }

            return result;
        }
    }
}