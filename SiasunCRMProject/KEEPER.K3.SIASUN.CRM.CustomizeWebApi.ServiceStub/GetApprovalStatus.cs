using Kingdee.BOS.WebApi.ServicesStub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS;
using System.ComponentModel;
using System.Data;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Workflow.ServiceHelper;
using Kingdee.BOS.Orm.DataEntity;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KEEPER.K3.SIASUN.CRM.CustomizeWebApi.ServiceStub
{
    [Description("获取单据在工作流实例中的状态")]
    public class GetApprovalStatus : AbstractWebApiBusinessService
    {
        public GetApprovalStatus(KDServiceContext context) : base(context)
        {
        }

        public Context ctx
        {
            get
            {
                return this.KDContext.Session.AppContext;
            }
        }
        public string getStatus(string parameter)
        {
            string value = HttpContext.Current.Request.Form["Data"];
            JObject jObject = (JObject)JsonConvert.DeserializeObject(value);
            string pKValue = jObject["pKValue"].ToString();
            string FormId = jObject["FormId"].ToString();
            return Judge(ctx, FormId, pKValue);
        }
        /// <summary>
        /// 判断是单据是否在实例运行中
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormId">业务对象标识</param>
        /// <param name="pKValue">单据主键</param>
        private string Judge(Context ctx,string FormId,string pKValue)
        {
            string strSql = string.Format(@"SELECT distinct t_WF_ApprovalAssign.FResult
  FROM t_WF_PiBiMap
 INNER JOIN t_WF_ProcInst
    ON (t_WF_ProcInst.FProcInstId = t_WF_PiBiMap.FProcInstId)
 INNER JOIN t_WF_ActInst
    on (t_WF_ActInst.FProcInstId = t_WF_ProcInst.FProcInstId)
 INNER JOIN t_WF_Assign
    on (t_WF_Assign.FActInstId = t_WF_ActInst.FActInstId)
 INNER JOIN t_WF_Receiver
    on (t_WF_Receiver.FAssignId = t_WF_Assign.FAssignId)
 INNER JOIN t_SEC_User
    ON (t_SEC_User.FUserId = t_WF_Receiver.FReceiverId)
 INNER JOIN t_WF_ApprovalAssign
    on (t_WF_Assign.FAssignId = t_WF_ApprovalAssign.FAssignId)
  LEFT JOIN t_WF_ApprovalItem
    on (t_WF_ApprovalItem.FApprovalAssignId =
       t_WF_ApprovalAssign.FApprovalAssignId AND
       t_WF_ApprovalItem.FReceiverId = t_WF_Receiver.FReceiverId)
 WHERE t_WF_PiBiMap.FObjectTypeId = '{0}'
   AND t_WF_PiBiMap.FKeyValue = '{1}'", FormId, pKValue);
            DynamicObjectCollection doc = DBUtils.ExecuteDynamicObject(ctx, strSql);
            foreach (DynamicObject item in doc)
            {
                if (item["FResult"]==null)
                {
                    return "当前单据在工作流程中，操作失败";
                }
            }
            return null;
            //DataSet ds = DBUtils.ExecuteDataSet(ctx, @"select b.FASSIGNID,b.FAPPROVALASSIGNID,a.FACTINSTID,a.FRECEIVERNAMES
            //                    from t_wf_assign a
            //                    join T_WF_APPROVALASSIGN b on a.fassignid=b.fassignid
            //                    where b.Fobjecttypeid=@FormID
            //                    and b.Fkeyvalue=@pKValue and a.FSTATUS=0",
            //            new List<SqlParam>
            //            {
            //                new SqlParam("@FormID", DbType.String, FormId),
            //                new SqlParam("@pKValue", DbType.String, pKValue)
            //            });
            //DataRow row = ds.Tables[0].Rows.Cast<DataRow>().FirstOrDefault(dr => dr["FRECEIVERNAMES"].ToString().Split(',').Any(r => r == receiverName));
            //if (row == null) throw new Exception("未找到待办任务");
            //string assignId = row["FASSIGNID"].ToString();
            //string approvalAssignId = row["FAPPROVALASSIGNID"].ToString();
            //string _approvalItemId = AssignmentServiceHelper.OpenApprovalItem(ctx, ctx.UserId, assignId, false);
            //var _approvalItem = AssignmentServiceHelper.GetApprovalItemById(ctx, _approvalItemId);
            //if (_approvalItem == null) throw new Exception("待办任务所在的流程实例不在运行中，不能进行处理！");
        }
    }
}
