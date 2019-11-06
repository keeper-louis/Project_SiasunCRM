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

namespace Siasun.K3.CRM.OPP.App.Report.SalesTargetReport
{
    [Description("销售人员指标报表（按月）")]
    public class xy_CRM_SalesTarget_Report : SysReportBaseService
    {
        public override void Initialize()
        {
            base.Initialize();

            this.ReportProperty.ReportType = ReportType.REPORTTYPE_NORMAL;
            this.ReportProperty.ReportName = new LocaleValue("销售人员指标报表（按月）", base.Context.UserLocale.LCID);
            this.IsCreateTempTableByPlugin = true;
            this.ReportProperty.IsUIDesignerColumns = false;
            this.ReportProperty.IdentityFieldName = "FIDENTITYID";
            this.ReportProperty.IsGroupSummary = false;
            this.ReportProperty.SimpleAllCols = false;
            //this.ReportProperty.PrimaryKeyFieldName = "deptName";
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


            DynamicObject customFilter = filter.FilterParameter.CustomFilter;
            String year = string.Empty;
            String saleDeptID = string.Empty;
            String salerID = string.Empty;
            if (customFilter["F_xy_Year"] != null) year = string.Format("{0:yyyy}", customFilter["F_xy_Year"]);
            if (customFilter["F_xy_Dept"] !=null && customFilter["F_xy_Dept_Id"] != null) saleDeptID = customFilter["F_xy_Dept_Id"].ToString();
            if (customFilter["F_xy_Saler"] != null && customFilter["F_xy_Saler_Id"] != null) salerID = customFilter["F_xy_Saler_Id"].ToString();



            StringBuilder deptnumbersql = new StringBuilder();

            if (customFilter["F_PAEZ_DEPT"] != null)
            {

                DynamicObjectCollection cols = (DynamicObjectCollection)customFilter["F_PAEZ_DEPT"];
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
                    String salenumber = Convert.ToString(((DynamicObject)onesale["F_PAEZ_MulBaseSaler"])["Number"]);
                    salesize = salesize + 1;
                    if (salesize == cols.Count)
                        salenumbersql.Append("'" + salenumber + "')");
                    else
                        salenumbersql.Append("'" + salenumber + "',");

                }
            }

            StringBuilder s = new StringBuilder();
            s.Append(@" select ROW_NUMBER() OVER(ORDER BY deptNO,empName,rowType) FIDENTITYID,deptName,empName,rowType,rowTypeText,salesQuota, ");
            s.Append(@" 	actual_1,CASE WHEN salesQuota>0 THEN round((actual_1/salesQuota),2)*100 ELSE 0 END percent_1, ");
            s.Append(@" 	actual_2,CASE WHEN salesQuota>0 THEN round((actual_2/salesQuota),2)*100 ELSE 0 END percent_2, ");
            s.Append(@" 	actual_3,CASE WHEN salesQuota>0 THEN round((actual_3/salesQuota),2)*100 ELSE 0 END percent_3, ");
            s.Append(@" 	actual_4,CASE WHEN salesQuota>0 THEN round((actual_4/salesQuota),2)*100 ELSE 0 END percent_4, ");
            s.Append(@" 	actual_5,CASE WHEN salesQuota>0 THEN round((actual_5/salesQuota),2)*100 ELSE 0 END percent_5, ");
            s.Append(@" 	actual_6,CASE WHEN salesQuota>0 THEN round((actual_6/salesQuota),2)*100 ELSE 0 END percent_6, ");
            s.Append(@" 	actual_7,CASE WHEN salesQuota>0 THEN round((actual_7/salesQuota),2)*100 ELSE 0 END percent_7, ");
            s.Append(@" 	actual_8,CASE WHEN salesQuota>0 THEN round((actual_8/salesQuota),2)*100 ELSE 0 END percent_8, ");
            s.Append(@" 	actual_9,CASE WHEN salesQuota>0 THEN round((actual_9/salesQuota),2)*100 ELSE 0 END percent_9, ");
            s.Append(@" 	actual_10,CASE WHEN salesQuota>0 THEN round((actual_10/salesQuota),2)*100 ELSE 0 END percent_10, ");
            s.Append(@" 	actual_11,CASE WHEN salesQuota>0 THEN round((actual_11/salesQuota),2)*100 ELSE 0 END percent_11, ");
            s.Append(@" 	actual_12,CASE WHEN salesQuota>0 THEN round((actual_12/salesQuota),2)*100 ELSE 0 END percent_12  ");
            s.Append(@" into " + tableName);
            s.Append(@" from ( ");
            s.Append(@" 	select case when saledept.fnumber  is null then case when dept.FDEPTH=3 then dept.fnumber else dept_3.fnumber end else saledept.fnumber end deptNO,case when saledeptl.FNAME is null then case when  dept.FDEPTH=3 then deptl.fname else deptl_3.fname end  else saledeptl.FNAME end deptName ,empl.fname empName,	'1' rowType,N'线索转商机' rowTypeText,isnull(quota_entry.F_PEJK_OPPQUNTA,0) salesQuota, ");
            s.Append(@" 	ISNULL(actual_1,0) actual_1,ISNULL(actual_2,0) actual_2,ISNULL(actual_3,0) actual_3,ISNULL(actual_4,0) actual_4, ");
            s.Append(@" 	ISNULL(actual_5,0) actual_5,ISNULL(actual_6,0) actual_6,ISNULL(actual_7,0) actual_7,ISNULL(actual_8,0) actual_8, ");
            s.Append(@" 	ISNULL(actual_9,0) actual_9,ISNULL(actual_10,0) actual_10,ISNULL(actual_11,0) actual_11,ISNULL(actual_12,0) actual_12 ");
            s.Append(@" 	from T_HR_EMPINFO emp   ");
            s.Append(@" 	inner join T_HR_EMPINFO_L empl on empl.FID=emp.FID and empl.FLOCALEID='2052'  ");
            s.Append(@" 	left join T_BD_STAFF staff on staff.FEMPINFOID=emp.FID  ");
            s.Append(@" 	inner join V_BD_SALESMAN saleman on staff.FSTAFFID=saleman.FSTAFFID   ");
            //s.Append(@"                                        and oper.FBIZORGID='100041'  ");
            s.Append(@" 	left join T_ORG_POST post on post.FPOSTID=staff.FPOSTID  ");
            s.Append(@" 	left join T_ORG_POST_L post_l on post_l.FPOSTID=post.FPOSTID and post_l.FLOCALEID='2052'  ");
            s.Append(@"  
		inner join t_bd_department dept  on post.FDEPTID=dept.FDEPTID

        left join  t_bd_department_L deptl on deptl.FDEPTID = dept.FDEPTID

		inner join t_bd_department dept_3  on dept.FPARENTID=dept_3.FDEPTID

        left join  t_bd_department_L deptl_3 on deptl_3.FDEPTID = dept_3.FDEPTID ");
            s.Append(@" 	left join PEJK_SALERQUNTA quota on Year(quota.F_PEJK_YEAR)='" + year + "' and quota.FDOCUMENTSTATUS='C' ");
            if (!string.IsNullOrEmpty(saleDeptID)) { s.Append(@" and quota.F_PEJK_SALEDEPT='" + saleDeptID + "' "); }

            s.Append(@"    inner join PEJK_SALERQUNTAENTRY quota_entry on quota.FID=quota_entry.FID and saleman.fid=quota_entry.F_PEJK_SALER ");
            s.Append(@" 	left join (	 ");
            s.Append(@" 		select saledeptid, salerID, ");
            s.Append(@" 		SUM(CASE WHEN curmonth =1 THEN opp_count ELSE 0 END) actual_1,    ");
            s.Append(@" 		SUM(CASE WHEN curmonth <=2 THEN opp_count ELSE 0 END) actual_2,   ");
            s.Append(@" 		SUM(CASE WHEN curmonth <=3 THEN opp_count ELSE 0 END) actual_3,   ");
            s.Append(@" 		SUM(CASE WHEN curmonth <=4 THEN opp_count ELSE 0 END) actual_4,   ");
            s.Append(@" 		SUM(CASE WHEN curmonth <=5 THEN opp_count ELSE 0 END) actual_5,   ");
            s.Append(@" 		SUM(CASE WHEN curmonth <=6 THEN opp_count ELSE 0 END) actual_6,   ");
            s.Append(@" 		SUM(CASE WHEN curmonth <=7 THEN opp_count ELSE 0 END) actual_7,   ");
            s.Append(@" 		SUM(CASE WHEN curmonth <=8 THEN opp_count ELSE 0 END) actual_8,   ");
            s.Append(@" 		SUM(CASE WHEN curmonth <=9 THEN opp_count ELSE 0 END) actual_9,   ");
            s.Append(@" 		SUM(CASE WHEN curmonth <=10 THEN opp_count ELSE 0 END) actual_10,   ");
            s.Append(@" 		SUM(CASE WHEN curmonth <=11 THEN opp_count ELSE 0 END) actual_11,   ");
            s.Append(@" 		SUM(CASE WHEN curmonth <=12 THEN opp_count ELSE 0 END) actual_12 ");
            s.Append(@" 		from ( ");
            s.Append(@" 			select opp.FSALEDEPTID saledeptid, opp.FBEMPID salerID,MONTH(opp.FSTARTDATE) curmonth,count(1) opp_count  ");
            s.Append(@" 			from T_CRM_OPPORTUNITY opp ");
            s.Append(@" 			inner join  T_CRM_CLUE clue on opp.FSOURCEBILLNO=clue.FBILLNO and clue.FSALERID=opp.FBEMPID  ");
            s.Append(@" 			where YEAR(opp.FSTARTDATE)='" + year + "' ");
            if (!string.IsNullOrEmpty(saleDeptID)) { s.Append(@" 			and opp.FSALEDEPTID='" + saleDeptID + "' "); }
            if (!string.IsNullOrEmpty(salerID)) { s.Append(@" 			and opp.FBEMPID='" + salerID + "' "); }
            if (flag)
            {
                s.AppendLine(" and opp.FBEMPID ").Append(salerLimit);
            }
            s.Append(@" 			group by opp.FSALEDEPTID,opp.FBEMPID,MONTH(opp.FSTARTDATE) ");
            s.Append(@" 			) t1 ");
            s.Append(@" 		group by saledeptid,salerID ");
            s.Append(@" 	) t2 on saleman.fid=t2.salerID ");

            s.Append(@" 	left join t_bd_department saledept on saledept.FDEPTID=t2.saledeptid and saledept.FDOCUMENTSTATUS='C' ");
            if (!string.IsNullOrEmpty(saleDeptID)) { s.Append(" and dept.FDEPTID='" + saleDeptID + "' "); }
            s.Append(@" 	left join t_bd_department_L saledeptl on saledeptl.FDEPTID=saledept.FDEPTID and saledeptl.FLOCALEID='2052'  ");


            s.Append(@" 	where 1=1 and t2.actual_12 != 0  ");
            if (!string.IsNullOrEmpty(saleDeptID)) { s.Append(" and saledept.FDEPTID='" + saleDeptID + "' "); }
            //部门
            if (deptnumbersql != null && deptnumbersql.Length > 0)
            {
                s.Append(" and saledept.fnumber " + deptnumbersql + " ");
            }
            //销售员
            if (salenumbersql != null && salenumbersql.Length > 0)
            {
                s.Append(" and  staff.fnumber ").Append(salenumbersql);


            }
            if (!string.IsNullOrEmpty(salerID)) { s.Append(@"           and saleman.fid='" + salerID + "'"); }
            if (flag)
            {
                s.AppendLine(" and saleman.fid ").Append(salerLimit);
            }
            s.Append(@"  ");
            s.Append(@" 	union all  ");
            s.Append(@"  ");
            s.Append(@" 	select case when saledept.fnumber  is null then case when dept.FDEPTH=3 then dept.fnumber else dept_3.fnumber end else saledept.fnumber end deptNO,case when saledeptl.FNAME is null then case when  dept.FDEPTH=3 then deptl.fname else deptl_3.fname end  else saledeptl.FNAME end deptName,empl.fname empName,	'2' rowType,N'商机转合同' rowTypeText,isnull(quota_entry.F_PEJK_CONTRACTQUNTA,0) salesQuota, ");
            s.Append(@" 	ISNULL(actual_1,0) actual_1,ISNULL(actual_2,0) actual_2,ISNULL(actual_3,0) actual_3,ISNULL(actual_4,0) actual_4, ");
            s.Append(@" 	ISNULL(actual_5,0) actual_5,ISNULL(actual_6,0) actual_6,ISNULL(actual_7,0) actual_7,ISNULL(actual_8,0) actual_8, ");
            s.Append(@" 	ISNULL(actual_9,0) actual_9,ISNULL(actual_10,0) actual_10,ISNULL(actual_11,0) actual_11,ISNULL(actual_12,0) actual_12 ");
            s.Append(@" 	from T_HR_EMPINFO emp   ");
            s.Append(@" 	inner join T_HR_EMPINFO_L empl on empl.FID=emp.FID and empl.FLOCALEID='2052'  ");
            s.Append(@" 	left join T_BD_STAFF staff on staff.FEMPINFOID=emp.FID  ");
            s.Append(@" 	inner join V_BD_SALESMAN saleman on staff.FSTAFFID=saleman.FSTAFFID  ");
            //s.Append(@"                                        and oper.FBIZORGID='100041'  ");
            s.Append(@" 	left join T_ORG_POST post on post.FPOSTID=staff.FPOSTID  ");
            s.Append(@" 	left join T_ORG_POST_L post_l on post_l.FPOSTID=post.FPOSTID and post_l.FLOCALEID='2052'  ");
            s.Append(@"  
		inner join t_bd_department dept  on post.FDEPTID=dept.FDEPTID

        left join  t_bd_department_L deptl on deptl.FDEPTID = dept.FDEPTID

		inner join t_bd_department dept_3  on dept.FPARENTID=dept_3.FDEPTID

        left join  t_bd_department_L deptl_3 on deptl_3.FDEPTID = dept_3.FDEPTID ");
            s.Append(@" 	join PEJK_SALERQUNTA quota on Year(quota.F_PEJK_YEAR)='" + year + "' and quota.FDOCUMENTSTATUS='C' ");
            if (!string.IsNullOrEmpty(saleDeptID)) { s.Append(@"     and quota.F_PEJK_SALEDEPT='" + saleDeptID + "' "); }
            s.Append(@" 	inner join PEJK_SALERQUNTAENTRY quota_entry on quota.FID=quota_entry.FID and saleman.fid=quota_entry.F_PEJK_SALER ");
            s.Append(@" 	left join (	 ");
            s.Append(@" 		select saledeptid,salerID, ");
            s.Append(@" 		SUM(CASE WHEN curmonth =1 THEN totalcount ELSE 0 END) actual_1,    ");
            s.Append(@" 		SUM(CASE WHEN curmonth <=2 THEN totalcount ELSE 0 END) actual_2,   ");
            s.Append(@" 		SUM(CASE WHEN curmonth <=3 THEN totalcount ELSE 0 END) actual_3,   ");
            s.Append(@" 		SUM(CASE WHEN curmonth <=4 THEN totalcount ELSE 0 END) actual_4,   ");
            s.Append(@" 		SUM(CASE WHEN curmonth <=5 THEN totalcount ELSE 0 END) actual_5,   ");
            s.Append(@" 		SUM(CASE WHEN curmonth <=6 THEN totalcount ELSE 0 END) actual_6,   ");
            s.Append(@" 		SUM(CASE WHEN curmonth <=7 THEN totalcount ELSE 0 END) actual_7,   ");
            s.Append(@" 		SUM(CASE WHEN curmonth <=8 THEN totalcount ELSE 0 END) actual_8,   ");
            s.Append(@" 		SUM(CASE WHEN curmonth <=9 THEN totalcount ELSE 0 END) actual_9,   ");
            s.Append(@" 		SUM(CASE WHEN curmonth <=10 THEN totalcount ELSE 0 END) actual_10, ");
            s.Append(@" 		SUM(CASE WHEN curmonth <=11 THEN totalcount ELSE 0 END) actual_11, ");
            s.Append(@" 		SUM(CASE WHEN curmonth <=12 THEN totalcount ELSE 0 END) actual_12  ");
            s.Append(@" 		from( ");
            s.Append(@" 			select opp.FSALEDEPTID saledeptid,con.FSALERID salerID,MONTH(con.FDATE) curmonth,count(distinct con_r.F_PEJK_SOURCEBILLNO) totalcount   ");
            s.Append(@" 			from T_CRM_CONTRACT con ");
            s.Append(@" 			inner join PEJK_GOODSDEATIL con_r on con.FID=con_r.FID  ");
           // s.Append(@" 			inner join T_CRM_OPPORTUNITY opp on con_r.F_PEJK_SOURCEBILLNO=opp.FBILLNO --and opp.FBEMPID=con.FSALERID  ");
            s.Append(@" 			inner join T_CRM_OPPORTUNITY opp on con_r.F_PEJK_SOURCEBILLNO=opp.FBILLNO   ");
            s.Append(@" 			where YEAR(con.FDATE)='" + year + "' ");
            if (!string.IsNullOrEmpty(saleDeptID)) { s.Append(@" 			and con.FSALEDEPTID='" + saleDeptID + "' "); }
            if (!string.IsNullOrEmpty(salerID)) { s.Append(@" 			and con.FSALERID='" + salerID + "' "); }
            if (flag)
            {
                s.AppendLine(" and con.FSALERID ").Append(salerLimit);
            }
            s.Append(@" 			group by opp.FSALEDEPTID ,con.FSALERID,MONTH(con.FDATE) ");
            s.Append(@" 			) t3 ");
            s.Append(@" 		group by saledeptid,salerID ");
            s.Append(@" 	) t4 on saleman.fid=t4.salerID ");
            s.Append(@" 	left join t_bd_department saledept on saledept.FDEPTID=t4.saledeptid and saledept.FDOCUMENTSTATUS='C' ");
            if (!string.IsNullOrEmpty(saleDeptID)) { s.Append(" and saledept.FDEPTID='" + saleDeptID + "' "); }
            s.Append(@" 	left join t_bd_department_L saledeptl on saledeptl.FDEPTID=saledept.FDEPTID and saledeptl.FLOCALEID='2052'  ");

            s.Append(@" 	where 1=1 and t4.actual_12 != 0 ");
            if (!string.IsNullOrEmpty(saleDeptID)) { s.Append(@"           and saledept.FDEPTID='" + saleDeptID + "' "); }

            //部门
            if (deptnumbersql != null && deptnumbersql.Length > 0)
            {
                s.Append(" and saledept.fnumber " + deptnumbersql + " ");
            }
            //销售员
            if (salenumbersql != null && salenumbersql.Length > 0)
            {
                s.Append(" and  staff.fnumber ").Append(salenumbersql);


            }
            if (!string.IsNullOrEmpty(salerID)) { s.Append(@"           and saleman.fid='" + salerID + "'"); }
            if (flag)
            {
                s.AppendLine(" and saleman.fid ").Append(salerLimit);
            }
            s.Append(@" ) t5 ");
            s.Append(@" order by deptNO,empName,rowType ");

            string sql = s.ToString();
            DBUtils.ExecuteDynamicObject(this.Context, sql);
        }

        public override ReportHeader GetReportHeaders(IRptParams filter)
        {
            ReportHeader header = new ReportHeader();
            header.Mergeable = true;

            //ListHeader deptName = header.AddChild();
            //deptName.Caption = new LocaleValue("销售部门");
            //deptName.AddChild("deptName", new LocaleValue("销售部门"));
            //deptName.Mergeable = true;
            header.AddChild("deptName", new LocaleValue("销售部门"));
            header.AddChild("empName", new LocaleValue("销售员"));
            header.AddChild("rowTypeText", new LocaleValue("类型"));
            header.AddChild("salesQuota", new LocaleValue("目标"), SqlStorageType.SqlInt);

            ListHeader month_1 = header.AddChild();
            month_1.Caption = new LocaleValue("1月");
            month_1.AddChild("actual_1", new LocaleValue("实际"), SqlStorageType.SqlInt);
            month_1.AddChild("percent_1", new LocaleValue("完成百分比"), SqlStorageType.SqlInt);

            ListHeader month_2 = header.AddChild();
            month_2.Caption = new LocaleValue("2月");
            month_2.AddChild("actual_2", new LocaleValue("实际"), SqlStorageType.SqlInt);
            month_2.AddChild("percent_2", new LocaleValue("完成百分比"), SqlStorageType.SqlInt);

            ListHeader month_3 = header.AddChild();
            month_3.Caption = new LocaleValue("3月");
            month_3.AddChild("actual_3", new LocaleValue("实际"), SqlStorageType.SqlInt);
            month_3.AddChild("percent_3", new LocaleValue("完成百分比"), SqlStorageType.SqlInt);

            ListHeader month_4 = header.AddChild();
            month_4.Caption = new LocaleValue("4月");
            month_4.AddChild("actual_4", new LocaleValue("实际"), SqlStorageType.SqlInt);
            month_4.AddChild("percent_4", new LocaleValue("完成百分比"), SqlStorageType.SqlInt);

            ListHeader month_5 = header.AddChild();
            month_5.Caption = new LocaleValue("5月");
            month_5.AddChild("actual_5", new LocaleValue("实际"), SqlStorageType.SqlInt);
            month_5.AddChild("percent_5", new LocaleValue("完成百分比"), SqlStorageType.SqlInt);

            ListHeader month_6 = header.AddChild();
            month_6.Caption = new LocaleValue("6月");
            month_6.AddChild("actual_6", new LocaleValue("实际"), SqlStorageType.SqlInt);
            month_6.AddChild("percent_6", new LocaleValue("完成百分比"), SqlStorageType.SqlInt);

            ListHeader month_7 = header.AddChild();
            month_7.Caption = new LocaleValue("7月");
            month_7.AddChild("actual_7", new LocaleValue("实际"), SqlStorageType.SqlInt);
            month_7.AddChild("percent_7", new LocaleValue("完成百分比"), SqlStorageType.SqlInt);

            ListHeader month_8 = header.AddChild();
            month_8.Caption = new LocaleValue("8月");
            month_8.AddChild("actual_8", new LocaleValue("实际"), SqlStorageType.SqlInt);
            month_8.AddChild("percent_8", new LocaleValue("完成百分比"), SqlStorageType.SqlInt);

            ListHeader month_9 = header.AddChild();
            month_9.Caption = new LocaleValue("9月");
            month_9.AddChild("actual_9", new LocaleValue("实际"), SqlStorageType.SqlInt);
            month_9.AddChild("percent_9", new LocaleValue("完成百分比"), SqlStorageType.SqlInt);

            ListHeader month_10 = header.AddChild();
            month_10.Caption = new LocaleValue("10月");
            month_10.AddChild("actual_10", new LocaleValue("实际"), SqlStorageType.SqlInt);
            month_10.AddChild("percent_10", new LocaleValue("完成百分比"), SqlStorageType.SqlInt);

            ListHeader month_11 = header.AddChild();
            month_11.Caption = new LocaleValue("11月");
            month_11.AddChild("actual_11", new LocaleValue("实际"), SqlStorageType.SqlInt);
            month_11.AddChild("percent_11", new LocaleValue("完成百分比"), SqlStorageType.SqlInt);

            ListHeader month_12 = header.AddChild();
            month_12.Caption = new LocaleValue("12月");
            month_12.AddChild("actual_12", new LocaleValue("实际"), SqlStorageType.SqlInt);
            month_12.AddChild("percent_12", new LocaleValue("完成百分比"), SqlStorageType.SqlInt);

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
                result.AddTitle("F_xy_Year", "年度: " + string.Format("{0:yyyy}", customFilter["F_xy_Year"]) +"      ");
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
            //result.Add(new SummaryField("FQty", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
            //result.Add(new SummaryField("FALLAMOUNT", Kingdee.BOS.Core.Enums.BOSEnums.Enu_SummaryType.SUM));
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
