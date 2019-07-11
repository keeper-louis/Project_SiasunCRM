using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Workflow.Assignment;
using Kingdee.BOS.Workflow.Interface;
using Kingdee.BOS.Workflow.Models.ApprovalAssignment;
using Kingdee.BOS.Workflow.Models.EnumStatus;
using Kingdee.BOS.Workflow.ServiceHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Kingdee.BOS.WebApi.Client;
using Kingdee.BOS.WebApi.ServicesStub;
using Kingdee.K3.CRM.Contracts;
using Kingdee.K3.CRM.Core;
using Kingdee.K3.CRM.Entity;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS;
using Kingdee.BOS.ServiceFacade.KDServiceClient.User;
using Kingdee.BOS.Authentication;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace CustomizeWebApi
{
    public class Workflow : AbstractWebApiBusinessService
    {
        public Workflow(KDServiceContext context) : base(context)
        {
        }

        public string APPtest(string parameter)
        {
            JObject Jo = (JObject)JsonConvert.DeserializeObject(parameter);
            string ServerUrl = "http://localhost/K3Cloud/";//服务地址
            string DBID = Jo["DBID"].ToString();
            string UserName = Jo["UserName"].ToString();
            string PassWord = Jo["PassWord"].ToString();
            int ICID = Convert.ToInt32("2052");
            string Fobjecttypeid = Jo["Fobjecttypeid"].ToString();
            string Fkeyvalue = Jo["Fkeyvalue"].ToString();
            string isApprove = Jo["isApprove"].ToString();
            string disposition = Jo["disposition"].ToString();
            string actionName = "打回发起人";

            string reason = "";
            string sContent = "";

            Context ctx = getContext(UserName, PassWord, ICID, DBID, ServerUrl);

            ApiClient client = new ApiClient(ServerUrl);
            bool bLogin = client.Login(DBID, UserName, PassWord, ICID);
            if (bLogin)//登录成功
            {
                if (isApprove.Equals("0"))
                {
                    reason = ApproveBill(ctx, Fobjecttypeid, Fkeyvalue, UserName, disposition);
                }
                else if (isApprove.Equals("1"))
                {
                    reason = RejectBill(ctx, Fobjecttypeid, Fkeyvalue, UserName, disposition, false, actionName);
                }
            }
            else
            {
                reason = "登录失败";
            }
            if (reason.Equals(""))
            {

                JObject jsonRoot = new JObject();
                JObject Result = new JObject();
                JObject ResponseStatus = new JObject();
                JArray Errors = new JArray();
                JObject basedata = new JObject();


                basedata.Add("FieldName", "");
                Errors.Add(basedata);
                basedata = new JObject();
                basedata.Add("Message", reason);
                Errors.Add(basedata);
                basedata = new JObject();
                basedata.Add("DIndex", "0");
                Errors.Add(basedata);

                ResponseStatus.Add("IsSuccess", "true");
                ResponseStatus.Add("Errors", Errors);

                Result.Add("ResponseStatus", ResponseStatus);

                jsonRoot.Add("Result", Result);

                sContent = JsonConvert.SerializeObject(jsonRoot);


            }
            else
            {
                JObject jsonRoot = new JObject();
                JObject Result = new JObject();
                JObject ResponseStatus = new JObject();
                JArray Errors = new JArray();
                JObject basedata = new JObject();


                basedata.Add("FieldName", "");
                Errors.Add(basedata);
                basedata = new JObject();
                basedata.Add("Message", reason);
                Errors.Add(basedata);
                basedata = new JObject();
                basedata.Add("DIndex", "0");
                Errors.Add(basedata);

                ResponseStatus.Add("IsSuccess", "false");
                ResponseStatus.Add("Errors", Errors);

                Result.Add("ResponseStatus", ResponseStatus);

                jsonRoot.Add("Result", Result);

                sContent = JsonConvert.SerializeObject(jsonRoot);


            }

            return sContent;
        }

        /// <summary>
        /// 审批单据，actionName为空时，自动寻找第一个同意审批项；
        /// 有多个类型为通过的审批项时，可用actionName指定审批项，如actionName="审批通过"
        /// </summary>
        /// <param name="formId">单据FormId</param>
        /// <param name="pKValue">单据主键</param>
        /// <param name="receiverName">处理人</param>
        /// <param name="disposition">审批意见</param>
        /// <param name="isApprovalFlow">是否为审批流</param>
        public string ApproveBill(Context ctx, string formId, string pKValue, string receiverName, string disposition, bool isApprovalFlow = false, string actionName = null)
        {
            string reason = "";
            List<AssignResult> assignResults = GetApproveActions(ctx, formId, pKValue, receiverName);
            AssignResult approvalAssignResults = assignResults.FirstOrDefault(r => r.ApprovalType == AssignResultApprovalType.Forward);
            if (approvalAssignResults == null)
            {
                reason = "未找到审批项";
                return reason;
            }
            reason = SubmitWorkflow(ctx, formId, pKValue, receiverName, approvalAssignResults.Id, disposition, isApprovalFlow);
            return reason;
        }

        /// <summary>
        /// 驳回单据，actionName为空时，自动寻找一个驳回审批项；
        /// 有多个类型为驳回的审批项时（如驳回、打回发起人），可用actionName指定审批项，如actionName="打回发起人"，"终止流程"
        /// </summary>
        public string RejectBill(Context ctx, string formId, string pKValue, string receiverName, string disposition, bool isApprovalFlow = false, string actionName = null)
        {
            string reason = "";
            List<AssignResult> assignResults = GetApproveActions(ctx, formId, pKValue, receiverName);
            assignResults = assignResults.Where(r => r.Name.Any(p => p.Value == actionName)).ToList();
            if (!string.IsNullOrEmpty(actionName))
                assignResults = assignResults.Where(r => r.Name.Any(p => p.Value == actionName)).ToList();
            else
                assignResults = assignResults.OrderBy(r => r.Name.Any(p => p.Value == "打回发起人")).ToList();
            AssignResult rejectAssignResults = assignResults.FirstOrDefault(r => r.ApprovalType == AssignResultApprovalType.Reject);

            if (rejectAssignResults == null)
            {
                reason = "未找到驳回审批项";
                return reason;
            }
            reason = SubmitWorkflow(ctx, formId, pKValue, receiverName, rejectAssignResults.Id, disposition, isApprovalFlow);
            return reason;

        }

        private List<AssignResult> GetApproveActions(Context ctx, string formId, string pKValue, string receiverName)
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
            if (row == null) return new List<AssignResult> { };

            string assignId = row["FASSIGNID"].ToString();
            string approvalAssignId = row["FAPPROVALASSIGNID"].ToString();
            string _approvalItemId = AssignmentServiceHelper.OpenApprovalItem(ctx, ctx.UserId, assignId, false);
            var _approvalItem = AssignmentServiceHelper.GetApprovalItemById(ctx, _approvalItemId);
            return _approvalItem.Actions.ToList();
        }


        private string SubmitWorkflow(Context ctx, string formId, string pKValue, string receiverName, string actionResult, string disposition, bool isApprovalFlow)
        {
            string reason = "";
            DataSet ds = DBUtils.ExecuteDataSet(ctx, @"select b.FASSIGNID,b.FAPPROVALASSIGNID,a.FACTINSTID,a.FRECEIVERNAMES
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
            if (row == null)
            {
                reason = "未找到待办任务";
                return reason;
            }



            string assignId = row["FASSIGNID"].ToString();
            string approvalAssignId = row["FAPPROVALASSIGNID"].ToString();
            string _approvalItemId = AssignmentServiceHelper.OpenApprovalItem(ctx, ctx.UserId, assignId, false);
            FormMetadata formMetadata = MetaDataServiceHelper.GetFormMetaData(ctx, formId);
            DynamicObject ObjData = BusinessDataServiceHelper.LoadSingle(ctx, pKValue, formMetadata.BusinessInfo.GetDynamicObjectType());
            BusinessInfo businessInfo = formMetadata.BusinessInfo;

            var _approvalItem = AssignmentServiceHelper.GetApprovalItemById(ctx, _approvalItemId);
            if (_approvalItem == null)
            {
                reason = "待办任务所在的流程实例不在运行中，不能进行处理！";
                return reason;
            }
            _approvalItem.ObjData = ObjData;
            _approvalItem.ReceiverPostId = 0;
            _approvalItem.ActionResult = actionResult;
            _approvalItem.Disposition = disposition.ToString();
            AssignResult assignResult = _approvalItem.Actions != null ? _approvalItem.Actions.FirstOrDefault(i => i.Id == actionResult) : null;
            AssignResultApprovalType approvalType = assignResult != null ? assignResult.ApprovalType : AssignResultApprovalType.None;
            _approvalItem.ActionResultType = approvalType;
            _approvalItem.Status = ApprovalItemStatus.Completed;
            DateTime timeNow = TimeServiceHelper.GetSystemDateTime(ctx);
            _approvalItem.CompletedTime = timeNow;

            ObjectActivityInstance _activityInstance = AssignmentServiceHelper.ConvertActivityModel(
                    ctx, businessInfo, approvalAssignId, _approvalItem);

            var option = OperateOption.Create();


            ApprovalAssignmentContext assignCtx = new ApprovalAssignmentContext()
            {
                ApprovalItems = new List<ApprovalItem>() { _approvalItem },
                Info = businessInfo,
                Option = option
            };
            assignCtx.NextActHandler = null;
            assignCtx.RejectReturn = false;
            assignCtx.ActivityInstance = _activityInstance;
            if (actionResult == AssignResultApprovalType.Reject.ToString())
            {
                string actInstId = row["FACTINSTID"].ToString();
                Kingdee.BOS.Workflow.App.Core.ProcInstService procInstService = new Kingdee.BOS.Workflow.App.Core.ProcInstService();
                var rejectActivityIds = procInstService.GetBackActInstList(ctx, actInstId, true).Select(r => r.ActivityId);
                if (!rejectActivityIds.Any())
                {

                    reason = "无驳回节点";
                    return reason;
                }
                assignCtx.Target = rejectActivityIds.FirstOrDefault();
            }
            ApprovalAssignmentServiceHelper.SubmitApprovalItem(ctx, assignCtx);
            reason = "";
            return reason;


        }
        public static Context getContext(string UserName, string PassWord, int ICID, string DBID, string ServerUrl)
        {


            UserServiceProxy proxy = new UserServiceProxy();// 引用Kingdee.BOS.ServiceFacade.KDServiceClient.dll
            proxy.HostURL = @"http://localhost/K3Cloud/";//k/3cloud地址

            LoginInfo logininfo = new LoginInfo();
            logininfo.Username = UserName;
            logininfo.Password = PassWord;
            logininfo.Lcid = ICID;
            logininfo.AcctID = DBID;

            Context ctx = proxy.ValidateUser("http://localhost/K3Cloud/", logininfo).Context;//guid为业务数据中心dbid，可以去管理中心数据库查询一下t_bas_datacenter_l表查找，后面需要用户名和密码

            return ctx;
        }
    }
}
