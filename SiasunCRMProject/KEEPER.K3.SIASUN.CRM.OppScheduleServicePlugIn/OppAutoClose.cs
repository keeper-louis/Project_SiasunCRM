using Kingdee.BOS.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using System.ComponentModel;
using KEEPER.K3.CRM.CRMServiceHelper;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Orm.DataEntity;

namespace KEEPER.K3.SIASUN.CRM.OppScheduleServicePlugIn
{
    [Description("180天后商机自动关闭")]
    public class OppAutoClose : IScheduleService
    {
        DateTime date = new DateTime();//元年
        public void Run(Context ctx, Schedule schedule)
        {
            //读取系统公共参数
            //服务参数
            //组织内码:如果参数与组织无关，设置为0
            //long orgId = ctx.CurrentOrganizationInfo.ID;
            long orgId = 100041;
            //会计账簿内码：如果参数与账簿无关，设置为0
            long acctBookId = 0;
            //系统参数对象标识
            string parameterObjId = "PEJK_OppAutoClose";//商机自动关闭期限
            //选项字段的绑定实体属性
            string parameterCloseDueTime = "F_PEJK_CloseDueTime";//自动关闭期限
            string parameterActivityWarning = "F_PEJK_ActivityWarning";//活动期限预警

            //读取系统参数：自动关闭期限返回值可能为null
            var CloseDueTime = SystemParameterServiceHelper.GetParamter(
                ctx,
                orgId,
                acctBookId,
                parameterObjId,
                parameterCloseDueTime
                );

            //读取系统参数：预警期限返回值可能为null
            var ActivityWarningTime = SystemParameterServiceHelper.GetParamter(
                ctx,
                orgId,
                acctBookId,
                parameterObjId,
                parameterActivityWarning
                );

            double CloseTime = 0;
            double WarningTime = 0;
            if (CloseDueTime != null)
            {
                CloseTime = Convert.ToDouble(CloseDueTime);
            }
            if (ActivityWarningTime != null)
            {
                WarningTime = Convert.ToDouble(ActivityWarningTime);
            }
            //判断商机状态为执行中的商机，最大的活动时间和当前时间的间隔是否大于CloseTime，如果大于自动关闭商机，如果商机一条活动也没有通过审核时间和CloseTime进行对比。
            string strSql = string.Format("/*dialect*/select opp.FID,opp.F_PEJK_AuditDate,max(act.FACTSTARTTIME) as FACTSTARTTIME from T_CRM_Opportunity opp left join T_CRM_Activity act on opp.FID = act.FOPPID where opp.FDOCUMENTSTATUS = 'G' and opp.FCLOSESTATUS = 0 group by opp.FID,opp.F_PEJK_AuditDate");
            DynamicObjectCollection  dbcol = DBUtils.ExecuteDynamicObject(ctx, strSql);
            if (dbcol!=null&&dbcol.Count()>0)
            {
                List<object> ids = new List<object>();
                foreach (DynamicObject item in dbcol)
                {
                    DateTime AuditDate = Convert.ToDateTime(item["F_PEJK_AuditDate"]);
                    DateTime ACTSTARTTIME = Convert.ToDateTime(item["FACTSTARTTIME"]);
                    if (ACTSTARTTIME == date)
                    {
                        string strSQL = string.Format(@"/*dialect*/update T_CRM_Opportunity set F_PEJK_WarningDate = '{0}' where FID = {1}", AuditDate.AddDays(WarningTime), Convert.ToInt64(item["FID"]));
                        DBUtils.Execute(ctx, strSQL);
                        TimeSpan span = DateTime.Now.Subtract(AuditDate);
                        if (span.Days<0)
                        {
                            continue;
                        }
                        if (span.Days+1 > CloseTime)
                        {
                            ids.Add(Convert.ToInt64(item["FID"]));
                        }
                    }
                    else
                    {
                        string strSQL = string.Format(@"/*dialect*/update T_CRM_Opportunity set F_PEJK_WarningDate = '{0}' where FID = {1}", ACTSTARTTIME.AddDays(WarningTime), Convert.ToInt64(item["FID"]));
                        DBUtils.Execute(ctx, strSQL);
                        TimeSpan span = DateTime.Now.Subtract(ACTSTARTTIME);
                        int days = span.Days + 1;
                        if (span.Days < 0)
                        {
                            continue;
                        }
                        if (span.Days + 1 > CloseTime)
                        {
                            ids.Add(Convert.ToInt64(item["FID"]));
                        }
                        
                    }
                }
                if (ids !=null && ids.Count()>0)
                {
                    object[] pkValues = ids.ToArray();
                    KEEPER.K3.CRM.CRMServiceHelper.CRMServiceHelper.setState(ctx, "T_CRM_Opportunity", "FCLOSESTATUS", "1", "FID", pkValues);
                }
                
            }
        }
    }
}
