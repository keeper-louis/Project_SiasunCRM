using Kingdee.BOS.Core.DynamicForm.PlugIn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using KEEPER.K3.CRM.CRMServiceHelper;
using Kingdee.BOS.Core.DynamicForm.Operation;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Orm;

namespace KEEPER.K3.SIASUN.CRM.ClueServicePlugIn
{
    [Description("线索审核,自动生成审核状态CRM客户CRM联系人审核中状态商业机会")]
    public class Audit: AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("F_PEJK_ClubType");
            e.FieldKeys.Add("F_PEJK_ExecuteDeptId");
            e.FieldKeys.Add("FCustomerID");
            e.FieldKeys.Add("FContactID");
        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            List<long> CRMCustIds = new List<long>();//线索转化成CRM客户的源单ID集合
            List<long> CRMLinkIds = new List<long>();//线索转化成CRM联系人的源单ID集合
            List<long> CRMOppIds = new List<long>();//线索转化成商业机会的源单ID集合
            if (e.DataEntitys!=null&&e.DataEntitys.Count()>0)
            {
                foreach (DynamicObject item in e.DataEntitys)
                {
                    //线索类型是是转商机客户
                    if (Convert.ToInt64(item["F_PEJK_ClubType"])==1)
                    {
                        //判断CRM客户内码，为空
                        if (Convert.ToInt64(((DynamicObject)((DynamicObjectCollection)item["CRM_Clue_Cust"])[0])["FCustomerID_Id"]) == 0)
                        {
                            CRMCustIds.Add(Convert.ToInt64(item["Id"]));
                        }
                        //判断CRM联系人内，为空
                        if (Convert.ToInt64(((DynamicObject)((DynamicObjectCollection)item["CRM_Clue_Contact"])[0])["FContactID_Id"]) == 0)
                        {
                            CRMLinkIds.Add(Convert.ToInt64(item["Id"]));
                        }
                        //判断执行部门小于等于1行
                        if (((DynamicObjectCollection)item["PEJK_Cust_ExecuteDept"]).Count()<=1)
                        {
                            //生成商机
                            CRMOppIds.Add(Convert.ToInt64(item["Id"]));
                        }
                    }
                    //线索类型事非转商机客户
                    else
                    {
                        //判断CRM客户内码，为空
                        if (Convert.ToInt64(((DynamicObject)((DynamicObjectCollection)item["CRM_Clue_Cust"])[0])["FCustomerID_Id"]) == 0)
                        {
                            CRMCustIds.Add(Convert.ToInt64(item["Id"]));
                        }
                        //判断CRM联系人内，为空
                        if (Convert.ToInt64(((DynamicObject)((DynamicObjectCollection)item["CRM_Clue_Contact"])[0])["FContactID_Id"]) == 0)
                        {
                            CRMLinkIds.Add(Convert.ToInt64(item["Id"]));
                        }
                    }
                }
                if (CRMCustIds.Count()>0)
                {
                    //下推CRM客户CRM_CUST
                    ConvertOperationResult operationCustResult = CRMServiceHelper.ConvertBills(base.Context, this.BusinessInfo.GetForm().Id, "CRM_CUST", CRMCustIds);
                    //获取下推生成的下游单据数据包
                    DynamicObject[] targetCustBillObjs = (from p in operationCustResult.TargetDataEntities select p.DataEntity).ToArray();
                    if (targetCustBillObjs.Length == 0)
                    {
                        // 未下推成功目标单，抛出错误，中断审核
                        throw new KDBusinessException("", string.Format("由{0}自动下推{1}，没有成功生成数据包，自动下推失败！", this.BusinessInfo.GetForm().Id, "CRM_CUST"));
                    }
                    // 对下游单据数据包，进行适当的修订，以避免关键字段为空，自动保存失败
                    // 示例代码略
                    //var saveResult = CRMServiceHelper.Save(base.Context, "CRM_OPP_Opportunity", targetBillObjs);
                    var saveCustResult = CRMServiceHelper.Save(base.Context, "CRM_CUST", targetCustBillObjs);
                    // 判断自动保存结果：只有操作成功，才会继续
                    if (this.CheckOpResult(saveCustResult, OperateOption.Create()))
                    {
                        object[] ids = (from c in saveCustResult.SuccessDataEnity
                                        select c[0]).ToArray();//保存成功的结果
                        if (ids.Count()>0)
                        {
                            IOperationResult submitCustResult = CRMServiceHelper.Submit(base.Context, "CRM_CUST", ids);
                            if (this.CheckOpResult(submitCustResult, OperateOption.Create()))
                            {
                                object[] ips = (from c in submitCustResult.SuccessDataEnity
                                                select c[0]).ToArray();//提交成功的结果
                                if (ips.Count()>0)
                                {
                                    IOperationResult auditCustResult = CRMServiceHelper.Audit(base.Context, "CRM_CUST", ips);
                                    if (this.CheckOpResult(auditCustResult,OperateOption.Create()))
                                    {
                                        
                                    }
                                }
                            }
                        } 
                    }
                }
                if (CRMLinkIds.Count()>0)
                {
                    //下推CRM联系人CRM_CUST_Contact
                    ConvertOperationResult operationContactResult = CRMServiceHelper.ConvertBills(base.Context, this.BusinessInfo.GetForm().Id, "CRM_CUST_Contact", CRMLinkIds);
                    //获取下推生成的下游单据数据包
                    DynamicObject[] targetContactBillObjs = (from p in operationContactResult.TargetDataEntities select p.DataEntity).ToArray();
                    if (targetContactBillObjs.Length == 0)
                    {
                        // 未下推成功目标单，抛出错误，中断审核
                        throw new KDBusinessException("", string.Format("由{0}自动下推{1}，没有成功生成数据包，自动下推失败！", this.BusinessInfo.GetForm().Id, "CRM_CUST_Contact"));
                    }
                    // 对下游单据数据包，进行适当的修订，以避免关键字段为空，自动保存失败
                    // 示例代码略
                    //var saveResult = CRMServiceHelper.Save(base.Context, "CRM_OPP_Opportunity", targetBillObjs);
                    var saveContactResult = CRMServiceHelper.Save(base.Context, "CRM_CUST_Contact", targetContactBillObjs);
                    // 判断自动保存结果：只有操作成功，才会继续
                    if (this.CheckOpResult(saveContactResult, OperateOption.Create()))
                    {
                        object[] ids = (from c in saveContactResult.SuccessDataEnity
                                        select c[0]).ToArray();//保存成功的结果
                        if (ids.Count() > 0)
                        {
                            IOperationResult submitContactResult = CRMServiceHelper.Submit(base.Context, "CRM_CUST_Contact", ids);
                            if (this.CheckOpResult(submitContactResult, OperateOption.Create()))
                            {
                                object[] ips = (from c in submitContactResult.SuccessDataEnity
                                                select c[0]).ToArray();//提交成功的结果
                                if (ips.Count() > 0)
                                {
                                    IOperationResult auditContactResult = CRMServiceHelper.Audit(base.Context, "CRM_CUST_Contact", ips);
                                    if (this.CheckOpResult(auditContactResult, OperateOption.Create()))
                                    {
                                        
                                    }
                                }
                            }
                        }
                    }
                }
                if (CRMOppIds.Count()>0)
                {
                    //下推商业机会CRM_OPP_Opportunity
                    ConvertOperationResult operationOppResult = CRMServiceHelper.ConvertBills(base.Context, this.BusinessInfo.GetForm().Id, "CRM_OPP_Opportunity", CRMOppIds);
                    //获取下推生成的下游单据数据包
                    DynamicObject[] targetOppBillObjs = (from p in operationOppResult.TargetDataEntities select p.DataEntity).ToArray();
                    if (targetOppBillObjs.Length == 0)
                    {
                        // 未下推成功目标单，抛出错误，中断审核
                        throw new KDBusinessException("", string.Format("由{0}自动下推{1}，没有成功生成数据包，自动下推失败！", this.BusinessInfo.GetForm().Id, "CRM_OPP_Opportunity"));
                    }
                    // 对下游单据数据包，进行适当的修订，以避免关键字段为空，自动保存失败
                    // 示例代码略
                    //var saveResult = CRMServiceHelper.Save(base.Context, "CRM_OPP_Opportunity", targetBillObjs);
                    //var draftOppResult = CRMServiceHelper.Draft(base.Context, "CRM_OPP_Opportunity", targetOppBillObjs);
                    var saveOppResult = CRMServiceHelper.Save(base.Context, "CRM_OPP_Opportunity", targetOppBillObjs);
                    // 判断自动保存结果：只有操作成功，才会继续
                    //if (this.CheckOpResult(draftOppResult, OperateOption.Create()))
                    //{

                    //}
                    if (this.CheckOpResult(saveOppResult, OperateOption.Create()))
                    {
                        object[] ids = (from c in saveOppResult.SuccessDataEnity
                                        select c[0]).ToArray();//保存成功的结果
                        if (ids.Count() > 0)
                        {
                            IOperationResult submitOppResult = CRMServiceHelper.Submit(base.Context, "CRM_OPP_Opportunity", ids);
                            if (this.CheckOpResult(submitOppResult, OperateOption.Create()))
                            {

                            }
                        }
                    }
                }











            }



            
            //获取下推生成的下游单据数据包
            //
            
            

            
            
        }

        private bool CheckOpResult(IOperationResult opResult, OperateOption opOption)
        {
            bool isSuccess = false;
            if (opResult.IsSuccess == true)
            {
                // 操作成功
                isSuccess = true;
            }
            else
            {
                if (opResult.InteractionContext != null
                    && opResult.InteractionContext.Option.GetInteractionFlag().Count > 0)
                {// 有交互性提示
                    // 传出交互提示完整信息对象
                    //this.OperationResult.InteractionContext = opResult.InteractionContext;
                    // 传出本次交互的标识，
                    // 用户在确认继续后，会重新进入操作；
                    // 将以此标识取本交互是否已经确认过，避免重复交互
                    //this.OperationResult.Sponsor = opResult.Sponsor;
                    // 抛出交互错误，把交互信息传递给前端
                    new KDInteractionException(opOption, opResult.Sponsor);
                }
                else
                {
                    // 操作失败，拼接失败原因，然后抛出中断
                    opResult.MergeValidateErrors();
                    if (opResult.OperateResult == null)
                    {// 未知原因导致提交失败
                        throw new KDBusinessException("", "未知原因导致自动提交、审核失败！");
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("自动操作失败：");
                        foreach (var operateResult in opResult.OperateResult)
                        {
                            sb.AppendLine(operateResult.Message);
                        }
                        throw new KDBusinessException("", sb.ToString());
                    }
                }
            }
            return isSuccess;
        }
    }
}
