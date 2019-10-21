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

namespace ClueTransBill
{
    [Description("线索转换分析表")]
    public class ClueTransBillPlugin : SysReportBaseService
    {
        private string[] materialRptTableNames;

        //线索转化分析表初始化
        public override void Initialize()
        {
            base.Initialize();

            //设置报表类型：简单报表
            this.ReportProperty.ReportType = Kingdee.BOS.Core.Report.ReportType.REPORTTYPE_NORMAL;
            //设置报表名称
            this.ReportProperty.ReportName = new Kingdee.BOS.LocaleValue("线索转化分析表", base.Context.UserLocale.LCID);
            this.IsCreateTempTableByPlugin = true;
            this.ReportProperty.IsUIDesignerColumns = false;
            this.ReportProperty.IsGroupSummary = true;
            this.ReportProperty.SimpleAllCols = false;

            //设置报表主键字段名
            this.ReportProperty.IdentityFieldName = "FSeq";

        }

        //向临时表插入报表数据
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            base.BuilderReportSqlAndTempTable(filter, tableName);

            // 根据当前用户的UserId  查询出其personId
            StringBuilder sql0 = new StringBuilder();
            sql0.AppendFormat(@"/*dialect*/ SELECT FLINKOBJECT FROM T_SEC_USER WHERE FUSERID = {0} ", this.Context.UserId);
            DynamicObjectCollection collection = DBUtils.ExecuteDynamicObject(this.Context, sql0.ToString());

            StringBuilder salerLimit = new StringBuilder();
            Boolean flag0 = false;

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
                    flag0 = true;

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
            materialRptTableNames = dbservice.CreateTemporaryTableName(this.Context, 4);
            string tmpTable1 = materialRptTableNames[0];
            string tmpTable2 = materialRptTableNames[1];
            string tmpTable3 = materialRptTableNames[2];
            string tmpTable4 = materialRptTableNames[3];

            //过滤条件：起始日期/截至日期/部门/业务员
            DynamicObject dyFilter = filter.FilterParameter.CustomFilter;
            String startDate = "";    //起始日期
            String endDate = "";      //截至日期

            bool flag = false;

            //业务员
            StringBuilder salerSql = new StringBuilder();
            if (dyFilter["F_QSNC_SalesmanFilter"] != null && ((DynamicObjectCollection)dyFilter["F_QSNC_SalesmanFilter"]).Count > 0)
            {
                //获取到多选基础资料中所有选中项
                DynamicObjectCollection cols1 = (DynamicObjectCollection)dyFilter["F_QSNC_SalesmanFilter"];
                int salerNum = 0;

                if (cols1.Count >= 1)
                {
                    salerSql.Append(" IN (");
                }

                foreach (DynamicObject saler in cols1)
                {
                    String salerNumber = Convert.ToString(((DynamicObject)saler["F_QSNC_SalesmanFilter"])["Number"]);
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
            if (dyFilter["F_QSNC_DepartmentFilter"] != null && ((DynamicObjectCollection)dyFilter["F_QSNC_DepartmentFilter"]).Count > 0)
            {
                //获取到多选基础资料中所有选中项
                DynamicObjectCollection cols2 = (DynamicObjectCollection)dyFilter["F_QSNC_DepartmentFilter"];
                int deptNum = 0;

                if (cols2.Count >= 1)
                {
                    deptSql.Append(" IN (");
                }

                foreach (DynamicObject dept in cols2)
                {
                    String deptNumber = Convert.ToString(((DynamicObject)dept["F_QSNC_DepartmentFilter"])["Number"]);
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

            //查询出商机中所有的执行部门id -- finish
            //StringBuilder sql1 = new StringBuilder();
            //sql1.AppendFormat(@"/*dialect*/ select oppexedept.F_PEJK_EXECUTEDEPTID as exedeptid into {0} ", tmpTable1);
            //sql1.AppendLine(" from PEJK_OPP_ExecuteDept oppexedept ");
            //sql1.AppendLine(" left join T_CRM_Opportunity opp ");
            //sql1.AppendLine(" on oppexedept.FID = opp.FID ");
            //DBUtils.ExecuteDynamicObject(this.Context, sql1.ToString());

            // 查询线索中所有执行部门id
            //StringBuilder sql1 = new StringBuilder();
            //sql1.AppendFormat(@"/*dialect*/ SELECT DISTINCT E.F_PEJK_EXECUTEDEPTID as exedeptid FROM PEJK_ExecuteDept E LEFT JOIN T_CRM_Clue C ON E.FID = C.FID ", tmpTable1);
            //DBUtils.ExecuteDynamicObject(this.Context, sql1.ToString());

            //根据商机中的执行部门，查找每个部门下销售员的 线索数量/商机数量/转化率
            StringBuilder sql2 = new StringBuilder();
            sql2.AppendFormat(@"/*dialect*/ select deptl.FDEPTID deptid, salesman.fid salerid, cluenumber, oppnumber, convert(float,round((oppnumber * 1.00 / (cluenumber * 1.00)) * 100, 2)) as conversionrate into {0} ", tmpTable2);
            sql2.AppendLine(" from(select FCREATORID, count(cluetmp.FCREATORID) cluenumber, sum(cluetmp.status) oppnumber from ");
            sql2.AppendLine(" (select clue.FCREATORID, ");
            sql2.AppendLine(" case when clue.FBILLNO in (select opp.FSOURCEBILLNO from T_CRM_Opportunity opp left join V_BD_SALESMAN salesman on salesman.fid = opp.FBEMPID left join T_BD_STAFF staff on staff.FSTAFFID = salesman.FSTAFFID inner join T_HR_EMPINFO emp on staff.FEMPINFOID = emp.FID LEFT JOIN T_SEC_USER U ON U.FLINKOBJECT = EMP.FPERSONID where U.FUSERID = clue.FCREATORID) then 1 else 0 end as status ");
            sql2.AppendLine(" from T_CRM_Clue clue where 1 = 1 and clue.FCREATORID != 0 ");

            //判断起始日期是否有效
            if (dyFilter["f_qsnc_startdatefilter"] != null)
            {
                startDate = Convert.ToDateTime(dyFilter["f_qsnc_startdatefilter"]).ToString("yyyy-MM-dd 00:00:00");
                sql2.AppendFormat(" and clue.F_PEJK_BIZDATE >= '{0}' ", startDate);
            }
            //判断截止日期是否有效
            if (dyFilter["f_qsnc_enddatefilter"] != null)
            {
                endDate = Convert.ToDateTime(dyFilter["f_qsnc_enddatefilter"]).ToString("yyyy-MM-dd 23:59:59");
                sql2.AppendFormat(" and clue.F_PEJK_BIZDATE <= '{0}' ", endDate);
            }

            sql2.AppendLine(" ) cluetmp ");
            sql2.AppendLine(" group by cluetmp.FCREATORID) tmp ");
            sql2.AppendLine(" LEFT JOIN T_SEC_USER U ON U.FUSERID = tmp.FCREATORID ");
            sql2.AppendLine(" LEFT JOIN T_HR_EMPINFO EMP ON U.FLINKOBJECT = EMP.FPERSONID ");
            sql2.AppendLine(" left join T_BD_STAFF staff on staff.FEMPINFOID = emp.FID ");
            sql2.AppendLine(" left join V_BD_SALESMAN salesman ");
            sql2.AppendLine(" on staff.FSTAFFID = salesman.FSTAFFID ");
            sql2.AppendLine(" left join T_BD_DEPARTMENT_L deptl ");
            sql2.AppendLine(" on deptl.FDEPTID = salesman.FDEPTID ");
            //sql2.AppendFormat(" where deptl.FDEPTID in (select exedeptid from {0}) and deptl.FLOCALEID = 2052 ", tmpTable1);
            sql2.AppendLine(" where deptl.FLOCALEID = 2052 ");
            if (flag0)
            {
                sql2.AppendLine(" and salesman.fid ").Append(salerLimit);
            }

            DBUtils.ExecuteDynamicObject(this.Context, sql2.ToString());

            //查询出所有部门小计
            StringBuilder sql3 = new StringBuilder();
            sql3.AppendFormat(@"/*dialect*/ select deptid, sum(cluenumber) as totalclue, sum(oppnumber) as totalopp into {0} from {1} group by deptid ", tmpTable3, tmpTable2);
            DBUtils.ExecuteDynamicObject(this.Context, sql3.ToString());

            //将销售员名称进行连表查询
            StringBuilder sql4 = new StringBuilder();
            sql4.AppendFormat(@"/*dialect*/ select deptid, empl.FNAME as saler, cluenumber, oppnumber, conversionrate into {0} ", tmpTable4);
            sql4.AppendFormat(" from {0} ", tmpTable2);
            sql4.AppendLine(" left join V_BD_SALESMAN salesman on salesman.fid = salerid ");
            sql4.AppendLine(" left join T_BD_STAFF staff on staff.FSTAFFID = salesman.FSTAFFID ");
            sql4.AppendLine(" inner join T_HR_EMPINFO emp on staff.FEMPINFOID = emp.FID ");
            sql4.AppendLine(" left join T_HR_EMPINFO_L empl on empl.FID = emp.FID ");
            sql4.AppendLine(" where empl.FLOCALEID = 2052 ");
            //判断业务员条件是否有效
            if (dyFilter["F_QSNC_SalesmanFilter"] != null && ((DynamicObjectCollection)dyFilter["F_QSNC_SalesmanFilter"]).Count > 0)
            {
                sql4.AppendLine(" and staff.FNUMBER").Append(salerSql);
                flag = true;
            }
            if (flag0)
            {
                sql4.AppendLine(" and salesman.fid ").Append(salerLimit);
            }

            DBUtils.ExecuteDynamicObject(this.Context, sql4.ToString());

            if (!flag)
            {
                //将部门小计插入总表中
                StringBuilder sql5 = new StringBuilder();
                sql5.AppendFormat(@"/*dialect*/ insert into {0} select deptid, '小计', totalclue, totalopp, convert(float,round((totalopp * 1.00 / (totalclue * 1.00)) * 100, 2)) as conversionrate from {1} ", tmpTable4, tmpTable3);
                DBUtils.ExecuteDynamicObject(this.Context, sql5.ToString());
            }

            //显示部门
            StringBuilder sql6 = new StringBuilder();
            sql6.AppendFormat(@"/*dialect*/ select row_number() over (order by deptl.FNAME) as FSeq, deptl.FNAME as department, saler, cluenumber, oppnumber, cast(conversionrate as varchar)+' %' as rate into {0} from {1} ", tableName, tmpTable4);
            sql6.AppendLine(" left join t_bd_department_L deptl ");
            sql6.AppendLine(" on deptl.FDEPTID = deptid ");
            sql6.AppendLine(" left join t_bd_department dept ");
            sql6.AppendLine(" on deptl.FDEPTID = dept.FDEPTID ");
            sql6.AppendLine(" where deptl.FLOCALEID = 2052 ");
            //判断部门条件是否有效
            if (dyFilter["F_QSNC_DepartmentFilter"] != null && ((DynamicObjectCollection)dyFilter["F_QSNC_DepartmentFilter"]).Count > 0)
            {
                sql6.AppendLine(" and dept.FNUMBER").Append(deptSql);
            }
            DBUtils.ExecuteDynamicObject(this.Context, sql6.ToString());
        }

        //构建报表列
        public override ReportHeader GetReportHeaders(IRptParams filter)
        {
            ReportHeader header = new ReportHeader();

            //部门
            var department = header.AddChild("department", new Kingdee.BOS.LocaleValue("部门"));
            department.ColIndex = 0;

            //业务员
            var salesman = header.AddChild("saler", new Kingdee.BOS.LocaleValue("业务员"));
            salesman.ColIndex = 1;

            //线索数量
            var clueNumber = header.AddChild("cluenumber", new Kingdee.BOS.LocaleValue("线索数量"));
            clueNumber.ColIndex = 2;

            //转化商机数量
            var oppNumber = header.AddChild("oppnumber", new Kingdee.BOS.LocaleValue("转化商机数量"));
            oppNumber.ColIndex = 3;

            //转化率
            var conversionRate = header.AddChild("rate", new Kingdee.BOS.LocaleValue("转化率"));
            conversionRate.ColIndex = 4;

            return header;
        }

        //准备报表的表头信息
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

                //部门
                if (dyFilter["F_QSNC_DepartmentFilter"] != null && ((DynamicObjectCollection)dyFilter["F_QSNC_DepartmentFilter"]).Count > 0)
                {
                    StringBuilder deptName = new StringBuilder();
                    DynamicObjectCollection cols = (DynamicObjectCollection)dyFilter["F_QSNC_DepartmentFilter"];
                    foreach (DynamicObject dept in cols)
                    {
                        String tmpName = Convert.ToString(((DynamicObject)dept["F_QSNC_DepartmentFilter"])["Name"]);
                        deptName.Append(tmpName + "; ");
                    }

                    result.AddTitle("F_QSNC_Department", deptName.ToString());
                }
                else
                {
                    result.AddTitle("F_QSNC_Department", "全部");
                }

                //业务员
                if (dyFilter["F_QSNC_SalesmanFilter"] != null && ((DynamicObjectCollection)dyFilter["F_QSNC_SalesmanFilter"]).Count > 0)
                {
                    StringBuilder salerName = new StringBuilder();
                    DynamicObjectCollection cols = (DynamicObjectCollection)dyFilter["F_QSNC_SalesmanFilter"];
                    foreach (DynamicObject saler in cols)
                    {
                        String tmpName = Convert.ToString(((DynamicObject)saler["F_QSNC_SalesmanFilter"])["Name"]);
                        salerName.Append(tmpName + "; ");
                    }

                    result.AddTitle("F_QSNC_Salesman", salerName.ToString());
                }
                else
                {
                    result.AddTitle("F_QSNC_Salesman", "全部");
                }
            }

            result.AddTitle("F_QSNC_BillDate", DateTime.Now.ToShortDateString());//报表日期

            return result;
        }
    }
}