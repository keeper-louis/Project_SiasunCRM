using Kingdee.BOS.Contracts.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.Report;
using Kingdee.BOS.App.Data;
using System.ComponentModel;
using Kingdee.BOS.Orm.DataEntity;
using KEEPER.K3.CRM.CRMServiceHelper;

namespace SaleControlBill
{
    [Description("销售监控报表")]
    public class SaleControlBillPlugin : SysReportBaseService
    {
        //销售监控报表
        public override void Initialize()
        {
            base.Initialize();

            //设置报表类型：简单报表
            this.ReportProperty.ReportType = Kingdee.BOS.Core.Report.ReportType.REPORTTYPE_NORMAL;
            //设置报表名称
            this.ReportProperty.ReportName = new Kingdee.BOS.LocaleValue("销售监控报表", base.Context.UserLocale.LCID);
            this.IsCreateTempTableByPlugin = true;
            this.ReportProperty.IsUIDesignerColumns = false;
            this.ReportProperty.IsGroupSummary = true;
            this.ReportProperty.SimpleAllCols = false;

            //设置报表主键字段名
            this.ReportProperty.IdentityFieldName = "FSeq";
        }

        //向临时表中插入报表数据
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


            DynamicObject customFilter = filter.FilterParameter.CustomFilter;

            //部门
            StringBuilder deptSql = new StringBuilder();
            if (customFilter["F_QSNC_DeptFilter"] != null && ((DynamicObjectCollection)customFilter["F_QSNC_DeptFilter"]).Count > 0)
            {
                //获取到多选基础资料中所有选中项
                DynamicObjectCollection cols2 = (DynamicObjectCollection)customFilter["F_QSNC_DeptFilter"];
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

            StringBuilder sql = new StringBuilder();

            sql.AppendLine(@"/*dialect*/ SELECT ROW_NUMBER() OVER (ORDER BY CLUE.FCREATEDATE) AS FSeq, CONVERT(varchar(100), CLUE.FCREATEDATE, 23) AS 'firstDate', ");    //首次录入时间
            sql.AppendLine(" CONVERT(varchar(100), OPP.FMODIFYDATE, 23) AS 'updateDate', ");                       //更新日期 
            sql.AppendLine(" (SELECT (ASS1.FDATAVALUE + ' ' + ASS2.FDATAVALUE) ");
            sql.AppendLine(" FROM T_BD_CUSTOMEREXT CUSTEXT ");
            sql.AppendLine(" LEFT JOIN T_BAS_ASSISTANTDATAENTRY_L ASS1 ");
            sql.AppendLine(" ON ASS1.FENTRYID = CUSTEXT.FPROVINCE ");
            sql.AppendLine(" LEFT JOIN T_BAS_ASSISTANTDATAENTRY_L ASS2 ");
            sql.AppendLine(" ON ASS2.FENTRYID = CUSTEXT.FCITY ");
            sql.AppendLine(" WHERE ASS1.FLOCALEID = 2052 AND ASS2.FLOCALEID = 2052 AND CUSTEXT.FCUSTID = OPP.FCUSTOMERID) AS 'area', "); // CRM客户--区域
            sql.AppendLine(" EMPL.FNAME AS 'saler', ");                                 //负责人
            sql.AppendLine(" DEPTL.FNAME AS 'department', ");                           //部门
            sql.AppendLine(" CUST.F_PEJK_AGENT AS 'agent', ");                          //代理商
            //sql.AppendLine(" FINUSER.FNAME AS 'user', ");                               //最终用户
            sql.AppendLine(" F_PEJK_SBSYDZ AS 'address', ");                            //设备使用地址
            sql.AppendLine(" F_PEJK_CRMPRONAME AS 'category', ");                       //产品类别
            sql.AppendLine(" F_PEJK_GGXH AS 'model', ");                                //产品型号
            sql.AppendLine(" F_PEJK_SPECPARAM AS 'special', ");                         //特殊参数要求
            sql.AppendLine(" CAST(F_PEJK_QTY AS INT) AS 'count', ");                    //台数(整数)
            sql.AppendLine(" (CONVERT(FLOAT, F_PEJK_PRICE, 2) / 10000) AS 'price', ");  //单价
            sql.AppendLine(" (CONVERT(FLOAT, F_PEJK_AMOUNT, 2) / 10000) AS 'amount', "); //总金额
            sql.AppendLine(" PROJECTPRO.FNAME AS 'progress', ");                        //项目进展
            sql.AppendLine(" F_PEJK_DDRESON AS 'reason', ");                            //丢单原因
            sql.AppendLine(" F_PEJK_XYBJH AS 'plan', ");                                //下一步计划
            sql.AppendLine(" F_PEJK_ZZJZDS AS 'rival',	 ");                            //主要竞争对手
            sql.AppendLine(" F_PEJK_JZDSCPXH AS 'rivalModel', ");                       //竞争对手产品型号
            sql.AppendLine(" (CONVERT(FLOAT, F_PEJK_JZPRICE, 2) / 10000) AS 'rivalPrice', ");//竞争型号单价
            sql.AppendLine(" F_PEJK_DATE AS 'orderDate', ");                            //预计下单时间
            sql.AppendLine(" F_PEJK_REMARKDETAIL AS 'remark' ");                        //备注
            sql.AppendFormat(" INTO {0} ", tableName);
            sql.AppendLine(" FROM T_CRM_OpportunityProduct OPPPRO ");
            sql.AppendLine(" LEFT JOIN T_CRM_Opportunity OPP ");
            sql.AppendLine(" ON OPPPRO.FID = OPP.FID ");
            sql.AppendLine(" LEFT JOIN PEJK_PROJECTPROCESS_L PROJECTPRO	");
            sql.AppendLine(" ON PROJECTPRO.FID = OPPPRO.F_PEJK_PROPROCESS ");
            //sql.AppendLine(" LEFT JOIN PEJK_FINALU_L FINUSER ");
            //sql.AppendLine(" ON FINUSER.FID = OPPPRO.F_PEJK_FINALU ");
            sql.AppendLine(" LEFT JOIN T_CRM_Clue CLUE ");
            sql.AppendLine(" ON OPP.FSOURCEBILLNO = CLUE.FBILLNO ");
            sql.AppendLine(" LEFT JOIN T_CRM_Clue_Cust CUST ");
            sql.AppendLine(" ON CLUE.FID = CUST.FID ");
            sql.AppendLine(" LEFT JOIN V_BD_SALESMAN SALESMAN ");
            sql.AppendLine(" ON SALESMAN.FID = OPP.FBEMPID ");
            sql.AppendLine(" LEFT JOIN T_BD_STAFF STAFF ");
            sql.AppendLine(" ON STAFF.FSTAFFID = SALESMAN.FSTAFFID ");
            sql.AppendLine(" INNER JOIN T_HR_EMPINFO EMP ");
            sql.AppendLine(" ON STAFF.FEMPINFOID = EMP.FID ");
            sql.AppendLine(" LEFT JOIN T_HR_EMPINFO_L EMPL ");
            sql.AppendLine(" ON  EMPL.FID = EMP.FID ");
            sql.AppendLine(" LEFT JOIN T_BD_DEPARTMENT_L DEPTL ");
            sql.AppendLine(" ON DEPTL.FDEPTID = SALESMAN.FDEPTID ");
            sql.AppendLine(" LEFT JOIN T_BD_DEPARTMENT DEPT ON DEPTL.FDEPTID = DEPT.FDEPTID ");
            sql.AppendLine(" where EMPL.FLOCALEID = 2052 AND DEPTL.FLOCALEID = 2052 AND PROJECTPRO.FLOCALEID = 2052 AND FINUSER.FLOCALEID = 2052 ");
            //判断销售部门条件
            if (customFilter["F_QSNC_DeptFilter"] != null && ((DynamicObjectCollection)customFilter["F_QSNC_DeptFilter"]).Count > 0)
            {
                sql.AppendLine(" AND DEPT.FNUMBER").Append(deptSql);
            }
            if (flag)
            {
                sql.AppendLine(" and SALESMAN.FID ").Append(salerLimit);
            }

            DBUtils.ExecuteDynamicObject(this.Context, sql.ToString());
        }

        //构建报表列
        public override ReportHeader GetReportHeaders(IRptParams filter)
        {
            ReportHeader header = new ReportHeader();

            //首次录入时间
            var firstDate = header.AddChild("firstDate", new Kingdee.BOS.LocaleValue("首次录入时间"));
            firstDate.ColIndex = 0;
            firstDate.Width = 100;

            //更新日期
            var updateDate = header.AddChild("updateDate", new Kingdee.BOS.LocaleValue("更新日期"));
            updateDate.ColIndex = 1;
            updateDate.Width = 100;

            //区域
            var area = header.AddChild("area", new Kingdee.BOS.LocaleValue("区域"));
            area.ColIndex = 2;
            area.Width = 150;

            //负责人
            header.AddChild("saler", new Kingdee.BOS.LocaleValue("负责人")).ColIndex = 3;

            //部门
            header.AddChild("department", new Kingdee.BOS.LocaleValue("部门")).ColIndex = 4;

            //代理商
            header.AddChild("agent", new Kingdee.BOS.LocaleValue("代理商")).ColIndex = 5;

            ////最终用户
            //header.AddChild("user", new Kingdee.BOS.LocaleValue("最终用户")).ColIndex = 6;

            //设备使用地址
            var address = header.AddChild("address", new Kingdee.BOS.LocaleValue("设备使用地址"));
            address.ColIndex = 6;
            address.Width = 150;

            //产品类别
            var category = header.AddChild("category", new Kingdee.BOS.LocaleValue("产品类别"));
            category.ColIndex = 7;
            category.Width = 150;

            //产品型号
            var model = header.AddChild("model", new Kingdee.BOS.LocaleValue("产品型号"));
            model.ColIndex = 8;
            model.Width = 150;

            //特殊参数要求
            var special = header.AddChild("special", new Kingdee.BOS.LocaleValue("特殊参数要求"));
            special.ColIndex = 9;
            special.Width = 150;
            //台数
            header.AddChild("count", new Kingdee.BOS.LocaleValue("台数")).ColIndex = 10;

            //单价
            header.AddChild("price", new Kingdee.BOS.LocaleValue("单价(万元)")).ColIndex = 11;

            //总金额
            header.AddChild("amount", new Kingdee.BOS.LocaleValue("总金额(万元)")).ColIndex = 12;

            //项目进展
            var progress = header.AddChild("progress", new Kingdee.BOS.LocaleValue("项目进展"));
            progress.ColIndex = 13;
            progress.Width = 150;

            //丢单原因
            var reason = header.AddChild("reason", new Kingdee.BOS.LocaleValue("丢单原因"));
            reason.ColIndex = 14;
            reason.Width = 150;

            //下一步计划
            var plan = header.AddChild("plan", new Kingdee.BOS.LocaleValue("下一步计划"));
            plan.ColIndex = 15;
            plan.Width = 150;

            //主要竞争对手
            var rival = header.AddChild("rival", new Kingdee.BOS.LocaleValue("主要竞争对手"));
            rival.ColIndex = 16;
            rival.Width = 150;

            //竞争对手产品型号
            var rivalModel = header.AddChild("rivalModel", new Kingdee.BOS.LocaleValue("竞争对手产品型号"));
            rivalModel.ColIndex = 17;
            rivalModel.Width = 150;

            //竞争型号单价
            header.AddChild("rivalPrice", new Kingdee.BOS.LocaleValue("竞争型号单价(万元)")).ColIndex = 18;

            //预计下单时间
            var orderDate = header.AddChild("orderDate", new Kingdee.BOS.LocaleValue("预计下单时间"));
            orderDate.ColIndex = 19;
            orderDate.Width = 100;

            //备注
            var remark = header.AddChild("remark", new Kingdee.BOS.LocaleValue("备注"));
            remark.ColIndex = 20;
            remark.Width = 150;

            return header;
        }

        //准备报表的表头信息
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

                //销售部门
                if (customFilter["F_QSNC_DeptFilter"] != null && ((DynamicObjectCollection)customFilter["F_QSNC_DeptFilter"]).Count > 0)
                {
                    StringBuilder deptName = new StringBuilder();
                    DynamicObjectCollection cols = (DynamicObjectCollection)customFilter["F_QSNC_DeptFilter"];
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
            }

            //当前日期
            result.AddTitle("F_QSNC_CurrentDate", DateTime.Now.ToShortDateString());

            return result;
        }
    }
}