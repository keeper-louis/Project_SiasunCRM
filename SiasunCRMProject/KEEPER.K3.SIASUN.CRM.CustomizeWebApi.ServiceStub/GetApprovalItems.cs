using Kingdee.BOS.WebApi.ServicesStub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS;
using System.Data;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Workflow.ServiceHelper;
using Kingdee.BOS.Workflow.Assignment;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Web;

namespace KEEPER.K3.SIASUN.CRM.CustomizeWebApi.ServiceStub
{
    [Description("获取审批项")]
    public class GetApprovalItems : AbstractWebApiBusinessService
    {
        public GetApprovalItems(KDServiceContext context) : base(context)
        {
        }
        public Context ctx
        {
            get
            {
                return this.KDContext.Session.AppContext;
            }
        }
        public string getItems(string parameters)
        {
            string value = HttpContext.Current.Request.Form["Data"];
            JObject jObject = (JObject)JsonConvert.DeserializeObject(value);
            string pKValue = jObject["pKValue"].ToString();
            string FormId = jObject["FormId"].ToString();
            string receiverName = jObject["UserName"].ToString();
            return items(ctx, FormId, pKValue,receiverName);
        }

        /// <summary>
        /// 获取当前处理人审批项
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="formId">业务对象标识</param>
        /// <param name="pKValue">业务对象内码</param>
        /// <param name="receiverName">接收人</param>
        private string items(Context ctx, string formId, string pKValue, string receiverName)
        {
            DataSet ds = DBUtils.ExecuteDataSet(ctx, @"select b.FASSIGNID,b.FAPPROVALASSIGNID,a.FRECEIVERNAMES
                                from t_wf_assign a
                                join T_WF_APPROVALASSIGN b on a.fassignid=b.fassignid
                                where b.Fobjecttypeid=@FormID
                                and b.Fkeyvalue=@pKValue and a.FSTATUS=0",
                        new List<SqlParam>
                        {
                            new SqlParam("@FormID", DbType.String, formId),
                            new SqlParam("@pKValue", DbType.String, pKValue)
                        });
            DataRow row = ds.Tables[0].Rows.Cast<DataRow>().FirstOrDefault(dr => dr["FRECEIVERNAMES"].ToString().Split(',').Any(r => r == receiverName));
            if (row == null) return "待办任务没有找到审批项";
            string assignId = row["FASSIGNID"].ToString();
            string approvalAssignId = row["FAPPROVALASSIGNID"].ToString();
            string _approvalItemId = AssignmentServiceHelper.OpenApprovalItem(ctx, ctx.UserId, assignId, false);
            var _approvalItem = AssignmentServiceHelper.GetApprovalItemById(ctx, _approvalItemId);
            List<AssignResult> list = _approvalItem.Actions.ToList();
            string result = string.Empty;
            JArray entrys = new JArray();
            foreach (AssignResult item in list)
            {
                JObject mBEntry = new JObject();
                mBEntry.Add("ApprovalItem", item.Name.ToString());
                entrys.Add(mBEntry);
            }
            return JsonConvert.SerializeObject(entrys);
        }
    }
}
