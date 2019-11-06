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

namespace OppWarningBill
{
    [Description("商机预警总表")]
    public class OppWarningPlugin : SysReportBaseService
    {
        //商机预警总表初始化
        public override void Initialize()
        {
            base.Initialize();

            //设置报表类型：简单报表
            this.ReportProperty.ReportType = Kingdee.BOS.Core.Report.ReportType.REPORTTYPE_NORMAL;
            //设置报表名称
            this.ReportProperty.ReportName = new Kingdee.BOS.LocaleValue("商机预警总表", base.Context.UserLocale.LCID);
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
            //商机编号
            String oppBillNo = null;
            if (customFilter["F_QSNC_OppBillNoFilter"] != null)
            {
                oppBillNo = Convert.ToString(customFilter["F_QSNC_OppBillNoFilter"]);
            }
             
            //用于存储客户名称多选项sql
            StringBuilder customerSql = new StringBuilder();
            //客户名称
            if (customFilter["F_QSNC_CustomerFilter"] != null && ((DynamicObjectCollection)customFilter["F_QSNC_CustomerFilter"]).Count > 0)
            {
                //获取到多选基础资料中所有选中项
                DynamicObjectCollection cols = (DynamicObjectCollection)customFilter["F_QSNC_CustomerFilter"];
                int customerNum = 0;

                if (cols.Count >= 1)
                {
                    customerSql.Append(" IN (");
                }

                foreach (DynamicObject customer in cols)
                {
                    String customerName = Convert.ToString(((DynamicObject)customer["F_QSNC_CustomerFilter"])["Number"]);
                    customerNum++;

                    if (cols.Count == customerNum)
                    {
                        customerSql.Append("'" + customerName + "')");
                    }
                    else
                    {
                        customerSql.Append("'" + customerName + "', ");
                    }
                }
            }

            // 商机负责人所在部门
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

            // 商机负责人
            StringBuilder salerSql = new StringBuilder();
            if (customFilter["F_QSNC_SalerFilter"] != null && ((DynamicObjectCollection)customFilter["F_QSNC_SalerFilter"]).Count > 0)
            {
                //获取到多选基础资料中所有选中项
                DynamicObjectCollection cols1 = (DynamicObjectCollection)customFilter["F_QSNC_SalerFilter"];
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

            StringBuilder sql = new StringBuilder();

            sql.AppendLine(@"/*dialect*/ SELECT ROW_NUMBER() OVER (ORDER BY OPP.FBILLNO) AS FSeq, OPP.FBILLNO AS OPPBILLNO, ");
            sql.AppendLine(" OPP.FOPPNAME AS OPPBILLNAME, ");
            sql.AppendLine(" CUST.FNUMBER AS CUSTOMERID, ");
            sql.AppendLine(" CUSTL.FNAME AS CUSTOMERNAME, ");
            sql.AppendLine(" OPP.FCREATEDATE AS CREATEDATE,	");
            sql.AppendLine(" EMPL.FNAME AS SALER, ");
            sql.AppendLine(" DEPTL.FNAME AS DEPTNAME, ");
            sql.AppendLine(" OPP.FMODIFYDATE AS MODIFYDATE,	");
            sql.AppendLine(" SUSER.FNAME AS MODIFIERNAME ");
            sql.AppendFormat(" INTO {0} ", tableName);
            sql.AppendLine(" FROM T_CRM_OPPORTUNITY OPP	");
            sql.AppendLine(" LEFT JOIN T_BD_CUSTOMER_L CUSTL ON OPP.FCUSTOMERID = CUSTL.FCUSTID	");
            sql.AppendLine(" LEFT JOIN T_BD_CUSTOMER CUST ON CUST.FCUSTID = CUSTL.FCUSTID ");
            sql.AppendLine(" LEFT JOIN T_SEC_user SUSER ON OPP.FMODIFIERID = SUSER.FUSERID ");
            sql.AppendLine(" LEFT JOIN V_BD_SALESMAN SALESMAN on SALESMAN.FID = OPP.FBEMPID	");

            // ----------- 20191028 增加商机负责人所在部门
            sql.AppendLine(" LEFT JOIN T_BD_DEPARTMENT_L DEPTL ON DEPTL.FDEPTID = OPP.FSALEDEPTID ");
            sql.AppendLine(" LEFT JOIN T_BD_DEPARTMENT DEPT ON DEPTL.FDEPTID = DEPT.FDEPTID ");
            // -----------

            sql.AppendLine(" LEFT JOIN T_BD_STAFF STAFF on STAFF.FSTAFFID = SALESMAN.FSTAFFID ");
            sql.AppendLine(" LEFT JOIN T_HR_EMPINFO_L EMPL on EMPL.FID = STAFF.FEMPINFOID ");
            sql.AppendLine(" WHERE EMPL.FLOCALEID = 2052 AND CUSTL.FLOCALEID = 2052 AND OPP.FBILLNO IN ");
            sql.AppendLine(" (SELECT FBILLNO ");
            sql.AppendLine(" FROM T_CRM_Opportunity OPP	");
            sql.AppendLine(" WHERE OPP.FBILLNO NOT IN (SELECT ACT.FSOURCEBILLNO FROM T_CRM_Activity ACT) ");
            sql.AppendLine(" AND DATEDIFF(MONTH, OPP.FSTARTDATE, GETDATE()) >= 3 AND OPP.FBILLNO != ' ') ");
            //sql.AppendLine(" UNION ");
            //sql.AppendLine(" SELECT OPP.FBILLNO	");
            //sql.AppendLine(" FROM T_CRM_Opportunity OPP ");
            //sql.AppendLine(" LEFT JOIN T_CRM_Activity ACT ");
            //sql.AppendLine(" ON OPP.FBILLNO = ACT.FSOURCEBILLNO ");
            //sql.AppendLine(" where DATEDIFF(MONTH, OPP.FSTARTDATE, ACT.FCREATEDATE) >= 3) ");  
            //商机编码
            if (oppBillNo != null && !oppBillNo.Equals(""))
            {
                sql.AppendFormat(" and OPP.FBILLNO = '{0}' ", oppBillNo);
            }
            //客户名称
            if (customFilter["F_QSNC_CustomerFilter"] != null && ((DynamicObjectCollection)customFilter["F_QSNC_CustomerFilter"]).Count > 0)
            {
                sql.Append(" and CUST.FNUMBER ").Append(customerSql);
            }

            // 商机负责人
            if(customFilter["F_QSNC_SalerFilter"] != null && ((DynamicObjectCollection)customFilter["F_QSNC_SalerFilter"]).Count > 0){
                sql.AppendLine(" AND STAFF.FNUMBER ").Append(salerSql);
            }

            // 负责人所在部门
            if (customFilter["F_QSNC_DeptFilter"] != null && ((DynamicObjectCollection)customFilter["F_QSNC_DeptFilter"]).Count > 0)
            {
                sql.AppendLine(" AND DEPT.FNUMBER ").Append(deptSql);
            }

            //销售数据隔离
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

            //商机编号
            header.AddChild("oppbillno", new Kingdee.BOS.LocaleValue("商机编号")).ColIndex = 0;

            //商机名称
            header.AddChild("oppbillname", new Kingdee.BOS.LocaleValue("商机名称")).ColIndex = 1;

            //客户编号
            header.AddChild("customerid", new Kingdee.BOS.LocaleValue("客户编号")).ColIndex = 2;

            //客户名称
            header.AddChild("customername", new Kingdee.BOS.LocaleValue("客户名称")).ColIndex = 3;

            //商机创建日期
            var createDate = header.AddChild("createdate", new Kingdee.BOS.LocaleValue("商机创建日期"));
            createDate.ColIndex = 4;
            createDate.Width = 200;

            //商机负责人
            header.AddChild("saler", new Kingdee.BOS.LocaleValue("商机负责人")).ColIndex = 5;

            // 负责人所属部门
            header.AddChild("DEPTNAME", new Kingdee.BOS.LocaleValue("负责人所属部门")).ColIndex = 6;

            //修改时间
            var modifyDate = header.AddChild("modifydate", new Kingdee.BOS.LocaleValue("修改时间"));
            modifyDate.ColIndex = 7;
            modifyDate.Width = 200;

            //修改人
            header.AddChild("modifiername", new Kingdee.BOS.LocaleValue("修改人")).ColIndex = 8;

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
                //商机编号
                if (customFilter["F_QSNC_OppBillNoFilter"] != null)
                {
                    String oppBillNo = Convert.ToString(customFilter["F_QSNC_OppBillNoFilter"]);
                    result.AddTitle("F_QSNC_OppBillNo", oppBillNo);
                }
                else
                {
                    result.AddTitle("F_QSNC_OppBillNo", "全部");
                }

                // 商机负责人
                if (customFilter["F_QSNC_SalerFilter"] != null && ((DynamicObjectCollection)customFilter["F_QSNC_SalerFilter"]).Count > 0)
                {
                    StringBuilder salerName = new StringBuilder();
                    DynamicObjectCollection cols = (DynamicObjectCollection)customFilter["F_QSNC_SalerFilter"];
                    foreach (DynamicObject customer in cols)
                    {
                        String tmpName = Convert.ToString(((DynamicObject)customer["F_QSNC_SalerFilter"])["Name"]);
                        salerName.Append(tmpName + "; ");
                    }

                    result.AddTitle("F_QSNC_Saler", salerName.ToString());
                }
                else
                {
                    result.AddTitle("F_QSNC_Saler", "全部");
                }

                // 负责人所属部门
                if (customFilter["F_QSNC_DeptFilter"] != null && ((DynamicObjectCollection)customFilter["F_QSNC_DeptFilter"]).Count > 0)
                {
                    StringBuilder deptName = new StringBuilder();
                    DynamicObjectCollection cols = (DynamicObjectCollection)customFilter["F_QSNC_DeptFilter"];
                    foreach (DynamicObject customer in cols)
                    {
                        String tmpName = Convert.ToString(((DynamicObject)customer["F_QSNC_DeptFilter"])["Name"]);
                        deptName.Append(tmpName + "; ");
                    }

                    result.AddTitle("F_QSNC_Dept", deptName.ToString());
                }
                else
                {
                    result.AddTitle("F_QSNC_Dept", "全部");
                }

                //客户名称
                if (customFilter["F_QSNC_CustomerFilter"] != null && ((DynamicObjectCollection)customFilter["F_QSNC_CustomerFilter"]).Count > 0)
                {
                    StringBuilder customerName = new StringBuilder();
                    DynamicObjectCollection cols = (DynamicObjectCollection)customFilter["F_QSNC_CustomerFilter"];
                    foreach (DynamicObject customer in cols)
                    {
                        String tmpName = Convert.ToString(((DynamicObject)customer["F_QSNC_CustomerFilter"])["Name"]);
                        customerName.Append(tmpName + "; ");
                    }

                    result.AddTitle("F_QSNC_CustomerName", customerName.ToString());
                }
                else
                {
                    result.AddTitle("F_QSNC_CustomerName", "全部");
                }
            }

            //当前日期
            result.AddTitle("F_QSNC_CurrentDate", DateTime.Now.ToShortDateString());

            return result;
        }
    }
}