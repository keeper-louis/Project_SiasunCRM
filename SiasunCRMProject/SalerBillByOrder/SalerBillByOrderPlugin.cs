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
    [Description("销售员销售报表(按单汇总)")]
    public class SalerBillByTotalPlugin : SysReportBaseService
    {
        private String[] materialRptTableNames;

        //报表初始化
        public override void Initialize()
        {
            base.Initialize();

            //设置报表类型：简单报表
            this.ReportProperty.ReportType = Kingdee.BOS.Core.Report.ReportType.REPORTTYPE_NORMAL;
            //设置报表名称
            this.ReportProperty.ReportName = new Kingdee.BOS.LocaleValue("销售员销售报表(按单汇总)", base.Context.UserLocale.LCID);
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


            //生成中间临时表
            IDBService dbservice = ServiceHelper.GetService<IDBService>();
            materialRptTableNames = dbservice.CreateTemporaryTableName(this.Context, 1);
            String tmpTable1 = materialRptTableNames[0];

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
                    String salerNumber = Convert.ToString(((DynamicObject)saler["F_QSNC_SalerFilter"])["Number"]);
                    salerNum++;

                    if (cols1.Count == salerNum)
                    {
                        salerSql.Append("'" + salerNumber + "')");
                    }
                    else
                    {
                        salerSql.Append("'" + salerNumber + "', ");
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
                    String deptNumber = Convert.ToString(((DynamicObject)dept["F_QSNC_DeptFilter"])["Number"]);
                    deptNum++;

                    if (cols2.Count == deptNum)
                    {
                        deptSql.Append("'" + deptNumber + "')");
                    }
                    else
                    {
                        deptSql.Append("'" + deptNumber + "', ");
                    }
                }
            }

            //报表sql
            StringBuilder sql1 = new StringBuilder();
            sql1.AppendFormat(@"/*dialect*/ SELECT DEPTNAME, SALERNAME, QUOTA, CONTRACTNUM, COMPLETEAMOUNT INTO {0}", tmpTable1);
            sql1.AppendLine(" FROM ");
            sql1.AppendLine(" (SELECT DEPTL.FNAME AS DEPTNAME, ");
            sql1.AppendLine(" EMPL.FNAME AS SALERNAME, ");
            sql1.AppendLine(" (SELECT F_PEJK_CONTRACTQUNTA FROM PEJK_SALERQUNTAENTRY WHERE F_PEJK_SALER = RESOLVESALER.F_QSNC_SALER) AS QUOTA,");
            sql1.AppendLine(" F_QSNC_CONTRACTNUM AS CONTRACTNUM,");
            sql1.AppendLine(" F_QSNC_DISTAMOUNT AS COMPLETEAMOUNT");
            sql1.AppendLine(" FROM QSNC_SaleResolve_Saler RESOLVESALER");
            sql1.AppendLine(" LEFT JOIN QSNC_SaleResolve_Basic RESOLVEBASIC ON RESOLVEBASIC.FID = RESOLVESALER.FID");
            sql1.AppendLine(" LEFT JOIN V_BD_SALESMAN SALESMAN ON SALESMAN.FID = RESOLVESALER.F_QSNC_SALER");
            sql1.AppendLine(" LEFT JOIN T_BD_DEPARTMENT_L DEPTL ON DEPTL.FDEPTID = SALESMAN.FDEPTID");
            sql1.AppendLine(" LEFT JOIN T_BD_DEPARTMENT DEPT ON DEPTL.FDEPTID = DEPT.FDEPTID ");
            sql1.AppendLine(" LEFT JOIN T_BD_STAFF STAFF ON STAFF.FSTAFFID = SALESMAN.FSTAFFID");
            sql1.AppendLine(" LEFT JOIN T_HR_EMPINFO_L EMPL ON EMPL.FID = STAFF.FEMPINFOID");
            sql1.AppendLine(" WHERE DEPTL.FLOCALEID = 2052 AND EMPL.FLOCALEID = 2052 ");

            //判断起始日期是否有效
            if (dyFilter["F_QSNC_StartDateFilter"] != null)
            {
                startDate = Convert.ToDateTime(dyFilter["F_QSNC_StartDateFilter"]).ToString("yyyy-MM-dd 00:00:00");
                sql1.AppendFormat(" AND RESOLVEBASIC.F_QSNC_DATE >= '{0}' ", startDate);
            }
            //判断截止日期是否有效
            if (dyFilter["F_QSNC_EndDateFilter"] != null)
            {
                endDate = Convert.ToDateTime(dyFilter["F_QSNC_EndDateFilter"]).ToString("yyyy-MM-dd 23:59:59");
                sql1.AppendFormat(" AND RESOLVEBASIC.F_QSNC_DATE <= '{0}' ", endDate);
            }

            //判断销售员条件
            if (dyFilter["F_QSNC_SalerFilter"] != null && ((DynamicObjectCollection)dyFilter["F_QSNC_SalerFilter"]).Count > 0)
            {
                sql1.AppendLine(" AND STAFF.FNUMBER ").Append(salerSql);
            }

            if (flag)
            {
                sql1.AppendLine(" and SALESMAN.FID ").Append(salerLimit);
            }

            //判断销售部门条件
            if (dyFilter["F_QSNC_DeptFilter"] != null && ((DynamicObjectCollection)dyFilter["F_QSNC_DeptFilter"]).Count > 0)
            {
                sql1.AppendLine(" AND DEPT.FNUMBER ").Append(deptSql);
            }
            sql1.AppendLine(" ) TMP ");

            DBUtils.ExecuteDynamicObject(this.Context, sql1.ToString());
            // tmpTable1 : DEPTNAME  SALERNAME  QUOTA  COMPLETEAMOUNT  COMPLETERATE

            StringBuilder sql2 = new StringBuilder();
            sql2.AppendFormat(@"/*dialect*/INSERT INTO {0} SELECT '合计' , '', TOTALQUOTA, '', TOTALAMOUNT ", tmpTable1);
            sql2.AppendLine(" FROM ");
            sql2.AppendFormat(" (SELECT SUM(QUOTA) AS TOTALQUOTA, SUM(COMPLETEAMOUNT) AS TOTALAMOUNT FROM {0}) TMP ", tmpTable1);
            DBUtils.ExecuteDynamicObject(this.Context, sql2.ToString());

            StringBuilder sql3 = new StringBuilder();
            sql3.AppendFormat(@"/*dialect*/ SELECT ROW_NUMBER() OVER (ORDER BY DEPTNAME) AS FSeq, DEPTNAME, SALERNAME, CONVERT(FLOAT, ROUND(QUOTA, 2)) AS TOTALQUOTA, CONTRACTNUM, CONVERT(FLOAT, ROUND(COMPLETEAMOUNT, 2)) AS TOTALAMOUNT, CAST(CONVERT(FLOAT, ROUND((COMPLETEAMOUNT * 1.00 / (QUOTA * 1.00)) * 100, 3)) as varchar) + ' %' AS COMPLETERATE INTO {0} ", tableName);
            sql3.AppendFormat(" FROM {0} ", tmpTable1);
            DBUtils.ExecuteDynamicObject(this.Context, sql3.ToString());
        }

        //构建报表列
        public override ReportHeader GetReportHeaders(IRptParams filter)
        {
            ReportHeader header = new ReportHeader();

            //销售部门
            var department = header.AddChild("DEPTNAME", new Kingdee.BOS.LocaleValue("销售部门"));
            department.ColIndex = 0;

            //销售员
            var salesman = header.AddChild("SALERNAME", new Kingdee.BOS.LocaleValue("销售员"));
            salesman.ColIndex = 1;

            //指标
            var quota = header.AddChild("TOTALQUOTA", new Kingdee.BOS.LocaleValue("指标"));
            quota.ColIndex = 2;

            //销售合同编号
            var contractNumber = header.AddChild("CONTRACTNUM", new Kingdee.BOS.LocaleValue("销售合同编号"));
            contractNumber.ColIndex = 3;

            //完成金额
            var amount = header.AddChild("TOTALAMOUNT", new Kingdee.BOS.LocaleValue("完成金额"));
            amount.ColIndex = 4;

            //完成占比
            var rate = header.AddChild("COMPLETERATE", new Kingdee.BOS.LocaleValue("完成占比"));
            rate.ColIndex = 5;

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
                    result.AddTitle("F_QSNC_StartDate", DateTime.Now.ToShortDateString());
                }
                else
                {
                    result.AddTitle("F_QSNC_StartDate", Convert.ToString(dyFilter["F_QSNC_StartDateFilter"]));
                }

                //截止日期
                if (dyFilter["F_QSNC_EndDateFilter"] == null)
                {
                    result.AddTitle("F_QSNC_EndDate", DateTime.Now.ToShortDateString());
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