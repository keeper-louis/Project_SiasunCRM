using KEEPER.K3.CRM.CRMServiceHelper;
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

namespace SIASUN.K3.CRM.APP.Report
{

    [Description("商机登录明细表")]
    public class OpportunityCountReport : SysReportBaseService
    {
        private string[] materialRptTableNames;

        public object DynamicObjectCollectio { get; private set; }

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
                DynamicObject personIdObj = (DynamicObject) collection[0];
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
            if (Convert.ToBoolean(cutomerfiler["F_PAEZ_ShowDetail"]) == true)
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.AppendLine("select distinct   secuser.fname username ,emp.fnumber empnumber,empl.fname empname,post.fnumber postnumber,post_l.fname postname,dept.fnumber deptnumber,deptl.FNAME deptname ");
                stringBuilder.AppendFormat(", opp.FBILLNO,opp.FOPPName  ,opp.FCREATEDATE,year(opp.FCREATEDATE) rtyear ,month(opp.FCREATEDATE) rtmonth ,opp.FDOCUMENTSTATUS,activity.FBILLNO  activitybillno ,opp.FCloseStatus \n");
                stringBuilder.AppendFormat("into {0}", temTable1).AppendLine(" \n");
                stringBuilder.AppendLine("from T_CRM_Opportunity opp \n ");
                stringBuilder.AppendLine("left join T_CRM_Activity activity on activity.FOPPID = opp.FID \n ");
                stringBuilder.AppendLine(" left join V_BD_SALESMAN saler on opp.FBEMPID = saler.FID ");
                stringBuilder.AppendLine(" left join T_BD_STAFF staff on saler.FSTAFFID=staff.FSTAFFID ");
                stringBuilder.AppendLine("inner  join T_HR_EMPINFO emp on staff.FEMPINFOID=emp.FID \n");
                stringBuilder.AppendLine("left join T_HR_EMPINFO_L empl on empl.FID=emp.FID  \n");
                stringBuilder.AppendLine("left join T_ORG_POST post on post.FPOSTID=staff.FPOSTID \n");
                stringBuilder.AppendLine("left join T_ORG_POST_L post_l on post_l.FPOSTID=post.FPOSTID  ");
                stringBuilder.AppendLine(" left join  t_sec_user secuser on secuser.FLINKOBJECT=emp.FPERSONID ");

                // 商机中的销售部门
                stringBuilder.AppendLine("left join t_bd_department dept on dept.FDEPTID=opp.FSALEDEPTID  -----部门  \n");
                stringBuilder.AppendLine("left join  t_bd_department_L deptl on deptl.FDEPTID=dept.FDEPTID \n");
                stringBuilder.AppendLine("where secuser.FTYPE=1 AND empl.FLOCALEID = 2052 AND post_l.FLOCALEID = 2052 AND deptl.FLOCALEID = 2052 ");
                if (year != null && !year.Equals("0"))
                {
                    stringBuilder.AppendLine(" and  year(opp.FCREATEDATE)= ");
                    stringBuilder.AppendLine(year);

                }
                if (deptnumbersql != null && deptnumbersql.Length > 0)
                {
                    stringBuilder.AppendLine(" and  dept.fnumber ").Append(deptnumbersql);


                }
                if (salenumbersql != null && salenumbersql.Length > 0)
                {
                    stringBuilder.AppendLine(" and  emp.fnumber ").Append(salenumbersql);
                }
                //销售员数据隔离
                if (flag)
                {
                    stringBuilder.AppendLine(" and OPP.FBEMPID ").Append(salerLimit);
                }

                DBUtils.ExecuteDynamicObject(this.Context, stringBuilder.ToString());

                stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("select   toji2.deptnumber,  toji2.deptname, toji2.empnumber, toji2.empname,yue1,yue2,yue3,yue4,yue5,yue6,yue7,yue8,\n");
                stringBuilder.AppendLine("yue9,yue10,yue11,yue12, \n");
                stringBuilder.AppendLine("yue1+yue2+yue3+yue4+yue5+yue6+yue7+yue8+yue9+yue10+yue11+yue12 zoji,isnull(F_PEJK_OPPQUNTA,0) dlzb,yue1+yue2+yue3+yue4+yue5+yue6+yue7+yue8+yue9+yue10+yue11+yue12-isnull(F_PEJK_OPPQUNTA,0) dlwqqk,gzzj gzzj ,isnull(F_PEJK_OPPTRACKQUNTA,0) gzzb,  gzzj-isnull(F_PEJK_OPPTRACKQUNTA,0) gzzbwqqk,activitycount gzhud\n");
                stringBuilder.AppendFormat("into {0}", temTable2).AppendLine(" \n");
                stringBuilder.AppendLine("from (\n");
                stringBuilder.AppendLine("select deptnumber, deptname,empnumber, empname, sum(yue1) yue1,sum(yue2) yue2,sum(yue3) yue3,sum(yue4) yue4,sum(yue5) yue5,sum(yue6) yue6,sum(yue7) yue7,sum(yue8) yue8,sum(yue9) yue9,sum(yue10) yue10,sum(yue11) yue11,sum(yue12) yue12\n");
                stringBuilder.AppendLine("from (\n");
                stringBuilder.AppendLine("\n");
                stringBuilder.AppendLine("select deptnumber, deptname,empnumber, empname,\n");
                stringBuilder.AppendLine("case when rtmonth=1 then oppcounts else 0 end  yue1 ,\n");
                stringBuilder.AppendLine("case when rtmonth=2 then oppcounts else 0 end  yue2,\n");
                stringBuilder.AppendLine("case when rtmonth=3 then oppcounts else 0 end  yue3,\n");
                stringBuilder.AppendLine("case when rtmonth=4 then oppcounts else 0 end  yue4,\n");
                stringBuilder.AppendLine("case when rtmonth=5 then oppcounts else 0 end  yue5,\n");
                stringBuilder.AppendLine("case when rtmonth=6 then oppcounts else 0 end  yue6,\n");
                stringBuilder.AppendLine("case when rtmonth=7 then oppcounts else 0 end  yue7,\n");
                stringBuilder.AppendLine("case when rtmonth=8 then oppcounts else 0 end  yue8,\n");
                stringBuilder.AppendLine("case when rtmonth=9 then oppcounts else 0 end  yue9,\n");
                stringBuilder.AppendLine("case when rtmonth=10 then oppcounts else 0 end  yue10,\n");
                stringBuilder.AppendLine("case when rtmonth=11 then oppcounts else 0 end  yue11,\n");
                stringBuilder.AppendLine("case when rtmonth=12 then oppcounts else 0 end  yue12\n");
                stringBuilder.AppendLine(" \n");
                stringBuilder.AppendLine("from (\n");
                stringBuilder.AppendLine(" \n");
                stringBuilder.AppendLine("select deptnumber, deptname,empnumber, empname,rtyear,rtmonth, count (distinct FBILLNO)  oppcounts\n");
                stringBuilder.AppendFormat("from {0}  opp \n", temTable1);
                stringBuilder.AppendLine("group by deptnumber, deptname,empnumber,empname,rtyear,rtmonth\n");
                stringBuilder.AppendLine(")  toji0\n");
                stringBuilder.AppendLine(") tojimonth\n");
                stringBuilder.AppendLine("group by deptnumber, deptname,empnumber,empname\n");
                stringBuilder.AppendLine("\n");
                stringBuilder.AppendLine(") toji2\n");
                stringBuilder.AppendLine("\n");
                stringBuilder.AppendLine(" \n");
                stringBuilder.AppendLine("left join \n");
                stringBuilder.AppendLine("  (\n");
                stringBuilder.AppendLine("select emp.fnumber empnumber,F_PEJK_OPPQUNTA,F_PEJK_OPPTRACKQUNTA \n");
                stringBuilder.AppendLine("from PEJK_SALERQUNTAENTRY SALERQUNTAENTRY \n");
                stringBuilder.AppendLine("left join PEJK_SALERQUNTA SALERQUNTA on SALERQUNTA.fid=SALERQUNTAENTRY.fid \n");
                stringBuilder.AppendLine("left join V_BD_SALESMAN salesman on salesman.fstaffid=SALERQUNTAENTRY.F_PEJK_SALER \n");
                stringBuilder.AppendLine("left join T_BD_STAFF staff on staff.FSTAFFID= salesman.fstaffid \n");
                stringBuilder.AppendLine("left join T_HR_EMPINFO emp on staff.FEMPINFOID=emp.FID \n");
                stringBuilder.AppendLine("left join T_HR_EMPINFO_L empl on empl.FID=emp.FID  --员工 \n");
                stringBuilder.AppendLine(") zbdj on zbdj.empnumber=toji2.empnumber\n");
                stringBuilder.AppendLine("\n");
                stringBuilder.AppendLine("left join \n");
                stringBuilder.AppendLine("(\n");
                stringBuilder.AppendLine("select deptnumber, deptname,empnumber, empname, sum(gzzj) gzzj,sum(activitycount ) activitycount \n");
                stringBuilder.AppendLine("from (\n");
                stringBuilder.AppendLine("\n");
                stringBuilder.AppendLine("select deptnumber, deptname,empnumber, empname ,  count (distinct FBILLNO)   gzzj,0  activitycount \n");
                stringBuilder.AppendFormat("from {0} opp \n", temTable1);
                stringBuilder.AppendLine("where opp.FDOCUMENTSTATUS is not null \n");
                stringBuilder.AppendLine("and opp.FCloseStatus<>0 and opp.FDOCUMENTSTATUS>='C' \n");
                stringBuilder.AppendLine("group by deptnumber, deptname,empnumber,empname  \n");
                stringBuilder.AppendLine("\n");
                stringBuilder.AppendLine("union all\n");
                stringBuilder.AppendLine("\n");
                stringBuilder.AppendLine("select deptnumber, deptname,empnumber, empname ,  0   gzzj,count(distinct activitybillno) activitycount \n");
                stringBuilder.AppendFormat("from {0} opp  \n", temTable1);
                stringBuilder.AppendLine("where opp.FDOCUMENTSTATUS is not null \n");
                //stringBuilder.AppendLine("and opp.FDOCUMENTSTATUS='E' \n");
                stringBuilder.AppendLine("group by deptnumber, deptname,empnumber,empname  \n");
                stringBuilder.AppendLine("\n");
                stringBuilder.AppendLine(") zjtongji\n");
                stringBuilder.AppendLine("group by deptnumber, deptname,empnumber,empname  \n");
                stringBuilder.AppendLine("\n");
                stringBuilder.AppendLine(") zitongji2  on zitongji2.empnumber=toji2.empnumber \n");

                DBUtils.ExecuteDynamicObject(this.Context, stringBuilder.ToString());
                //插入部门小计
                //
                stringBuilder = new StringBuilder();
                stringBuilder.AppendFormat("insert into {0} ", temTable2);
                stringBuilder.AppendLine(" select deptnumber,deptname,'部门小计','',sum(yue1) yue1,sum(yue2) yue2,sum(yue3) yue3,sum(yue4)yue4,sum(yue5) yue5,sum(yue6) yue6,sum(yue7) yue7,sum(yue8) yue8,sum(yue9) yue9,sum(yue10) yue10,sum(yue11) yue11,sum(yue12) yue12,sum(zoji) zoji,sum(dlzb) dlzb,sum(dlwqqk) dlwqqk,sum(gzzj) gzzj,sum(gzzb) gzzb,sum(gzzbwqqk) gzzbwqqk,sum(gzhud) gzhud ");
                stringBuilder.AppendFormat(" from {0} ", temTable2);
                stringBuilder.AppendLine(" group by deptnumber, deptname ");
                DBUtils.ExecuteDynamicObject(this.Context, stringBuilder.ToString());
                //插入 总计
                stringBuilder = new StringBuilder();
                stringBuilder.AppendFormat("insert into {0}", temTable2);
                stringBuilder.AppendLine("\n select '总计','','','',sum(yue1) yue1,sum(yue2) yue2,sum(yue3) yue3,sum(yue4)yue4,sum(yue5) yue5,sum(yue6) yue6,sum(yue7) yue7,sum(yue8) yue8,sum(yue9) yue9,sum(yue10) yue10,sum(yue11) yue11,sum(yue12) yue12,sum(zoji) zoji,sum(dlzb) dlzb,sum(dlwqqk) dlwqqk,sum(gzzj) gzzj,sum(gzzb) gzzb,sum(gzzbwqqk) gzzbwqqk,sum(gzhud) gzhud ");
                stringBuilder.AppendFormat("from {0} ", temTable2);
                stringBuilder.AppendLine(" where empnumber<>'部门小计' ");
                stringBuilder.AppendLine(" group by deptnumber, deptname ");

                DBUtils.ExecuteDynamicObject(this.Context, stringBuilder.ToString());

                stringBuilder = new StringBuilder();
                stringBuilder.AppendFormat(" select ROW_NUMBER() OVER(ORDER BY deptnumber) FIDENTITYID,tmp2.* into {0}   from   {1} tmp2  order by deptnumber,deptname,empnumber,empname ", tableName, temTable2);

                DBUtils.ExecuteDynamicObject(this.Context, stringBuilder.ToString());
            }
            else
            {
                // 取数SQL
                // 商机登录部门 
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.AppendLine("select distinct   secuser.fname username ,emp.fnumber empnumber,empl.fname empname,post.fnumber postnumber,post_l.fname postname,dept.fnumber deptnumber,deptl.FNAME deptname ");
                stringBuilder.AppendFormat(", opp.FBILLNO,opp.FOPPName  ,opp.FCREATEDATE,year(opp.FCREATEDATE) rtyear ,month(opp.FCREATEDATE) rtmonth ,opp.FDOCUMENTSTATUS,activity.FBILLNO  activitybillno,opp.FCloseStatus  \n");
                stringBuilder.AppendFormat("into {0}", temTable1).AppendLine(" \n");
                stringBuilder.AppendLine("from T_CRM_Opportunity opp \n ");
                stringBuilder.AppendLine("left join T_CRM_Activity activity on activity.FOPPID = opp.FID \n ");
                stringBuilder.AppendLine(" left join V_BD_SALESMAN saler on opp.FBEMPID = saler.FID ");
                stringBuilder.AppendLine(" left join T_BD_STAFF staff on saler.FSTAFFID=staff.FSTAFFID ");
                stringBuilder.AppendLine("inner  join T_HR_EMPINFO emp on staff.FEMPINFOID=emp.FID \n");
                stringBuilder.AppendLine("left join T_HR_EMPINFO_L empl on empl.FID=emp.FID  \n");
                stringBuilder.AppendLine("left join T_ORG_POST post on post.FPOSTID=staff.FPOSTID \n");
                stringBuilder.AppendLine("left join T_ORG_POST_L post_l on post_l.FPOSTID=post.FPOSTID  ");
                stringBuilder.AppendLine(" left join  t_sec_user secuser on secuser.FLINKOBJECT=emp.FPERSONID ");

                // 商机中的销售部门
                stringBuilder.AppendLine("left join t_bd_department dept on dept.FDEPTID=opp.FSALEDEPTID  -----部门  \n");
                stringBuilder.AppendLine("left join  t_bd_department_L deptl on deptl.FDEPTID=dept.FDEPTID \n");
                stringBuilder.AppendLine("where secuser.FTYPE=1 AND empl.FLOCALEID = 2052 AND post_l.FLOCALEID = 2052 AND deptl.FLOCALEID = 2052  ");

                if (year != null && !year.Equals("0"))
                {
                    stringBuilder.AppendLine(" and  year(opp.FCREATEDATE)= ");
                    stringBuilder.AppendLine(year);

                }
                if (deptnumbersql != null && deptnumbersql.Length > 0)
                {
                    stringBuilder.AppendLine(" and  dept.fnumber ").Append(deptnumbersql);


                }
                if (salenumbersql != null && salenumbersql.Length > 0)
                {
                    stringBuilder.AppendLine(" and  emp.fnumber ").Append(salenumbersql);


                }
                DBUtils.ExecuteDynamicObject(this.Context, stringBuilder.ToString());

                stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("select   deptnumber,  deptname, yue1,yue2,yue3,yue4,yue5,yue6,yue7,yue8, \n");
                stringBuilder.AppendLine("yue9,yue10,yue11,yue12,  \n");
                stringBuilder.AppendLine("yue1+yue2+yue3+yue4+yue5+yue6+yue7+yue8+yue9+yue10+yue11+yue12 zoji \n");
                stringBuilder.AppendFormat("into {0} ", temTable2);
                stringBuilder.AppendLine("from ( \n");
                stringBuilder.AppendLine("select deptnumber, deptname,  sum(yue1) yue1,sum(yue2) yue2,sum(yue3) yue3,sum(yue4) yue4,sum(yue5) yue5,sum(yue6) yue6,sum(yue7) yue7,sum(yue8) yue8,sum(yue9) yue9,sum(yue10) yue10,sum(yue11) yue11,sum(yue12) yue12\n");
                stringBuilder.AppendLine("from ( \n");
                stringBuilder.AppendLine("select deptnumber, deptname,case when rtmonth=1 then oppcounts else 0 end  yue1 , \n");
                stringBuilder.AppendLine("case when rtmonth=2 then oppcounts else 0 end  yue2, \n");
                stringBuilder.AppendLine("case when rtmonth=3 then oppcounts else 0 end  yue3, \n");
                stringBuilder.AppendLine("case when rtmonth=4 then oppcounts else 0 end  yue4, \n");
                stringBuilder.AppendLine("case when rtmonth=5 then oppcounts else 0 end  yue5, \n");
                stringBuilder.AppendLine("case when rtmonth=6 then oppcounts else 0 end  yue6, \n");
                stringBuilder.AppendLine("case when rtmonth=7 then oppcounts else 0 end  yue7, \n");
                stringBuilder.AppendLine("case when rtmonth=8 then oppcounts else 0 end  yue8, \n");
                stringBuilder.AppendLine("case when rtmonth=9 then oppcounts else 0 end  yue9, \n");
                stringBuilder.AppendLine("case when rtmonth=10 then oppcounts else 0 end  yue10, \n");
                stringBuilder.AppendLine("case when rtmonth=11 then oppcounts else 0 end  yue11, \n");
                stringBuilder.AppendLine("case when rtmonth=12 then oppcounts else 0 end  yue12 \n");
                stringBuilder.AppendLine("  \n");
                stringBuilder.AppendLine("from ( \n");
                stringBuilder.AppendLine("select deptnumber, deptname,rtyear,rtmonth, count (distinct FBILLNO)  oppcounts \n");
                stringBuilder.AppendFormat("from {0} \n", temTable1);
                stringBuilder.AppendLine("group by deptnumber, deptname,rtyear,rtmonth \n");
                stringBuilder.AppendLine(" \n");
                stringBuilder.AppendLine(") toji \n");
                stringBuilder.AppendLine("  ) toji0 \n");
                stringBuilder.AppendLine("    group by deptnumber, deptname  \n");

                stringBuilder.AppendLine(")  toji2 \n");
                DBUtils.ExecuteDynamicObject(this.Context, stringBuilder.ToString());
                //插入部门小计
                //
                //stringBuilder = new StringBuilder();
                //stringBuilder.AppendFormat("insert into {0} ", temTable2);
                //stringBuilder.AppendLine(" select deptnumber, deptname,'部门小计','',sum(yue1) yue1,sum(yue2) yue2,sum(yue3) yue3,sum(yue4)yue4,sum(yue5) yue5,sum(yue6) yue6,sum(yue7) yue7,sum(yue8) yue8,sum(yue9) yue9,sum(yue10) yue10,sum(yue11) yue11,sum(yue12) yue12, sum(zoji) zoji ");
                //stringBuilder.AppendFormat(" from {0} ", temTable2);
                //stringBuilder.AppendLine(" group by deptnumber, deptname ");
                //DBUtils.ExecuteDynamicObject(this.Context, stringBuilder.ToString());
                //插入 总计
                stringBuilder = new StringBuilder();
                stringBuilder.AppendFormat("insert into {0}", temTable2);
                stringBuilder.AppendLine("\n select '总计','',sum(yue1) yue1,sum(yue2) yue2,sum(yue3) yue3,sum(yue4)yue4,sum(yue5) yue5,sum(yue6) yue6,sum(yue7) yue7,sum(yue8) yue8,sum(yue9) yue9,sum(yue10) yue10,sum(yue11) yue11,sum(yue12) yue12,sum(zoji) zoji ");
                stringBuilder.AppendFormat("from {0} ", temTable2);
                stringBuilder.AppendLine(" group by deptnumber, deptname ");
                DBUtils.ExecuteDynamicObject(this.Context, stringBuilder.ToString());

                stringBuilder = new StringBuilder();
                stringBuilder.AppendFormat(" select ROW_NUMBER() OVER(ORDER BY deptnumber) FIDENTITYID,tmp2.* into {0}   from   {1} tmp2  order by deptnumber,deptname  ", tableName, temTable2);

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
                var deptname = header.AddChild("deptnumber", new LocaleValue("部门编码"));

                var deptnumber = header.AddChild("deptname", new LocaleValue("部门"));

                //var empnumber = header.AddChild("empnumber", new LocaleValue("员工编码"));

                // var empname = header.AddChild("empname", new LocaleValue("员工"));

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
