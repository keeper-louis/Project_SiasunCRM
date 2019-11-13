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

            // 计算当前登录用户销售员可见范围
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

            // -----------------------------------------------------------------------------------------------------------------------------------------------

            //生成中间临时表
            IDBService dbservice = ServiceHelper.GetService<IDBService>();
            materialRptTableNames = dbservice.CreateTemporaryTableName(this.Context, 10);
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
                    String salerNumber = Convert.ToString(((DynamicObject)saler["F_QSNC_SalesmanFilter"])["Id"]);
                    salerNum++;

                    if (cols1.Count == salerNum)
                    {
                        salerSql.Append(salerNumber + ")");
                    }
                    else
                    {
                        salerSql.Append(salerNumber + ", ");
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





            // ---------------------------------------------------------------------------------------------------------------
            // tmpTable1 存放的是线索中的部门、线索的创建人、线索是否转化为商机（1：是；0：否）
            StringBuilder tmpSQL1 = new StringBuilder();
            tmpSQL1.AppendFormat(@"/*dialect*/ select clue.FCREATORID, 
                                                       clue.FSALEDEPTID, 
	                                                   case when clue.FBILLNO in (select opp.FSOURCEBILLNO 
								                                                  from T_CRM_Opportunity opp 
								                                                  inner join V_BD_SALESMAN saler on saler.fid = opp.FBEMPID 
								                                                  inner join T_BD_STAFF staff on staff.FSTAFFID = saler.FSTAFFID 
								                                                  inner join T_HR_EMPINFO emp on staff.FEMPINFOID = emp.FID 
								                                                  inner JOIN T_SEC_USER U ON U.FLINKOBJECT = EMP.FPERSONID 
								                                                  where U.FUSERID = clue.FCREATORID) then 1 else 0 end as status
                                    into {0}
                                    from T_CRM_Clue clue 
                                    where clue.FCREATORID != 0 
                                    and clue.FSALEDEPTID != 0 ", tmpTable1);
            //判断起始日期是否有效
            if (dyFilter["f_qsnc_startdatefilter"] != null)
            {
                startDate = Convert.ToDateTime(dyFilter["f_qsnc_startdatefilter"]).ToString("yyyy-MM-dd 00:00:00");
                tmpSQL1.AppendFormat(" and clue.F_PEJK_BIZDATE >= '{0}' ", startDate);
            }
            //判断截止日期是否有效
            if (dyFilter["f_qsnc_enddatefilter"] != null)
            {
                endDate = Convert.ToDateTime(dyFilter["f_qsnc_enddatefilter"]).ToString("yyyy-MM-dd 23:59:59");
                tmpSQL1.AppendFormat(" and clue.F_PEJK_BIZDATE <= '{0}' ", endDate);
            }
            DBUtils.ExecuteDynamicObject(this.Context, tmpSQL1.ToString());

            // --------------------------------------------------------------------------------------------------------------------
            // 统计每一个销售员的线索数量、商机数量
            StringBuilder tmpSQL2 = new StringBuilder();
            tmpSQL2.AppendFormat(@"/*dialect*/ SELECT FCREATORID, FSALEDEPTID, COUNT(FCREATORID) CLUENUMBER, SUM(status) OPPNUMBER
                                                INTO {0}
                                                FROM {1}
                                                GROUP BY FCREATORID, FSALEDEPTID ", tmpTable2, tmpTable1);
            DBUtils.ExecuteDynamicObject(this.Context, tmpSQL2.ToString());

            // --------------------------------------------------------------------------------------------------------------------
            // 计算每一名销售员的转化率
            StringBuilder tmpSQL3 = new StringBuilder();
            tmpSQL3.AppendFormat(@"/*dialect*/ SELECT DISTINCT DEPTL.FNAME DEPTNAME, 
                                                      EMPL.FNAME SALERNAME, 
                                                      CLUENUMBER, 
                                                      OPPNUMBER, 
                                                      CONVERT(FLOAT,ROUND((OPPNUMBER * 1.00 / (CLUENUMBER * 1.00)) * 100, 2)) RATE 
                                               INTO {0} 
                                               FROM {1} TMP
                                               INNER JOIN T_SEC_USER U ON U.FUSERID = TMP.FCREATORID
                                               INNER JOIN T_HR_EMPINFO EMP ON U.FLINKOBJECT = EMP.FPERSONID
                                               INNER JOIN T_BD_STAFF STAFF on STAFF.FEMPINFOID = EMP.FID
                                               INNER JOIN V_BD_SALESMAN SALESMAN ON SALESMAN.FSTAFFID = STAFF.FSTAFFID
                                               INNER JOIN T_HR_EMPINFO_L EMPL ON EMPL.FID = EMP.FID
                                               INNER JOIN t_bd_department_L DEPTL ON DEPTL.FDEPTID = FSALEDEPTID
                                               INNER JOIN t_bd_department DEPT ON DEPTL.FDEPTID = DEPT.FDEPTID 
                                               WHERE TMP.FCREATORID <> 0 
                                               ", tmpTable3, tmpTable2);
            // 进行销售员数据隔离
            if (flag0)
            {
                tmpSQL3.AppendLine(" AND SALESMAN.FID ").Append(salerLimit);
            }

            // 进行销售员过滤条件过滤
            if (dyFilter["F_QSNC_SalesmanFilter"] != null && ((DynamicObjectCollection)dyFilter["F_QSNC_SalesmanFilter"]).Count > 0)
            {
                tmpSQL3.AppendLine(" and SALESMAN.FID ").Append(salerSql);
            }

            // 进行部门条件过滤
            //判断部门条件是否有效
            if (dyFilter["F_QSNC_DepartmentFilter"] != null && ((DynamicObjectCollection)dyFilter["F_QSNC_DepartmentFilter"]).Count > 0)
            {
                tmpSQL3.AppendLine(" AND DEPT.FNUMBER ").Append(deptSql);
            }

            DBUtils.ExecuteDynamicObject(this.Context, tmpSQL3.ToString());

            // 根据部门进行线索及商机的汇总
            // ---------------------------------------------------------------------------------------------------------------------
            // 获取到部门小计
            StringBuilder tmpSQL4 = new StringBuilder();
            tmpSQL4.AppendFormat(@"/*dialect*/ SELECT DEPTNAME, SUM(CLUENUMBER) TOTALCLUE, SUM(OPPNUMBER) TOTALOPP INTO {0} FROM {1} GROUP BY DEPTNAME ", tmpTable4, tmpTable3);
            DBUtils.ExecuteDynamicObject(this.Context, tmpSQL4.ToString());


            // 将部门小计斤系插入明细表
            // ----------------------------------------------------------------------------------------------------------------------
            StringBuilder tmpSQL5 = new StringBuilder();
            tmpSQL5.AppendFormat(@"/*dialect*/ INSERT INTO {0} SELECT DEPTNAME + ' - 小计', '', TOTALCLUE, TOTALOPP, CONVERT(FLOAT,ROUND((TOTALOPP * 1.00 / (TOTALCLUE * 1.00)) * 100, 2)) TOTALRATE FROM {1} ", tmpTable3, tmpTable4);
            DBUtils.ExecuteDynamicObject(this.Context, tmpSQL5.ToString());

            // ------------------------------------------------------------------------------------------------------------------------
            // 将总体结果进行插入系统提供的tablename中
            StringBuilder tmpSQl6 = new StringBuilder();
            tmpSQl6.AppendFormat(@"/*dialect*/ SELECT ROW_NUMBER() OVER (ORDER BY DEPTNAME, SALERNAME) AS FSeq, DEPTNAME, SALERNAME, CLUENUMBER, OPPNUMBER, CONVERT(varchar(60), RATE)+'%' RATE1 INTO {0} FROM {1} ORDER BY DEPTNAME, SALERNAME ", tableName, tmpTable3);
            DBUtils.ExecuteDynamicObject(this.Context, tmpSQl6.ToString());
        }

        //构建报表列
        public override ReportHeader GetReportHeaders(IRptParams filter)
        {
            ReportHeader header = new ReportHeader();

            //部门
            var department = header.AddChild("DEPTNAME", new Kingdee.BOS.LocaleValue("部门"));
            department.ColIndex = 0;
            department.Width = 200;

            //业务员
            var salesman = header.AddChild("SALERNAME", new Kingdee.BOS.LocaleValue("业务员"));
            salesman.ColIndex = 1;

            //线索数量
            var clueNumber = header.AddChild("CLUENUMBER", new Kingdee.BOS.LocaleValue("线索数量"));
            clueNumber.ColIndex = 2;

            //转化商机数量
            var oppNumber = header.AddChild("OPPNUMBER", new Kingdee.BOS.LocaleValue("转化商机数量"));
            oppNumber.ColIndex = 3;

            //转化率
            var conversionRate = header.AddChild("RATE1", new Kingdee.BOS.LocaleValue("转化率"));
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