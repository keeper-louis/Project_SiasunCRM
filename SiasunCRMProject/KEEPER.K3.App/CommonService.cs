using KEEPER.K3.CRM.Contracts;
using KEEPER.K3.CRM.Core.Entity;
using Kingdee.BOS;
using Kingdee.BOS.App;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Const;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.Operation;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Interaction;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.Metadata.ConvertElement.ServiceArgs;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.BOS.Workflow.Contracts;
using Kingdee.BOS.Workflow.Models.EnumStatus;
using Kingdee.BOS.Workflow.Models.Template;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KEEPER.K3.App
{
    public class CommonService: ICommonService
    {
        /// <summary>
        /// 暂存单据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="dyObject"></param>
        /// <returns></returns>
        public IOperationResult DraftBill(Context ctx, string FormID, DynamicObject[] dyObject)
        {
            IMetaDataService metaService = ServiceHelper.GetService<IMetaDataService>();//元数据服务
            FormMetadata Meta = metaService.Load(ctx, FormID) as FormMetadata;//获取元数据
            OperateOption DraftOption = OperateOption.Create();
            IOperationResult DraftResult = BusinessDataServiceHelper.Draft(ctx, Meta.BusinessInfo, dyObject, DraftOption, "Draft");
            return DraftResult;
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID">业务对象标识</param>
        /// <param name="dyObject">保存业务对象集合</param>
        /// <returns></returns>
        public IOperationResult SaveBill(Context ctx, string FormID, DynamicObject[] dyObject)
        {
            IMetaDataService metaService = ServiceHelper.GetService<IMetaDataService>();
            FormMetadata targetBillMeta = metaService.Load(ctx, FormID) as FormMetadata;
            // 构建保存操作参数：设置操作选项值，忽略交互提示
            OperateOption saveOption = OperateOption.Create();
            // 忽略全部需要交互性质的提示，直接保存；
            //saveOption.SetIgnoreWarning(true);              // 忽略交互提示
            //saveOption.SetInteractionFlag(this.Option.GetInteractionFlag());        // 如果有交互，传入用户选择的交互结果
            // using Kingdee.BOS.Core.Interaction;
            //saveOption.SetIgnoreInteractionFlag(this.Option.GetIgnoreInteractionFlag());
            //// 如下代码，强制要求忽略交互提示(演示案例不需要，注释掉)
            saveOption.SetIgnoreWarning(true);
            //// using Kingdee.BOS.Core.Interaction;
            saveOption.SetIgnoreInteractionFlag(true);
            // 调用保存服务，自动保存
            ISaveService saveService = ServiceHelper.GetService<ISaveService>();
            IOperationResult saveResult = saveService.Save(ctx, targetBillMeta.BusinessInfo, dyObject, saveOption, "Save");
            ISubmitService save1Service = ServiceHelper.GetService<ISubmitService>();
            

            return saveResult;
        }

        /// <summary>
        /// 另一种保存服务
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="dyObject"></param>
        /// <returns></returns>
        public IOperationResult BatchSaveBill(Context ctx, string FormID, DynamicObject[] dyObject)
        {
            IMetaDataService metaService = ServiceHelper.GetService<IMetaDataService>();//元数据服务
            FormMetadata Meta = metaService.Load(ctx, FormID) as FormMetadata;//获取元数据
            OperateOption SaveOption = OperateOption.Create();
            IOperationResult SaveResult = BusinessDataServiceHelper.Save(ctx, Meta.BusinessInfo, dyObject, SaveOption, "Save");
            return SaveResult;
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public IOperationResult SubmitBill(Context ctx, string FormID, object[] ids)
        {
            IMetaDataService metaService = ServiceHelper.GetService<IMetaDataService>();//元数据服务
            FormMetadata Meta = metaService.Load(ctx, FormID) as FormMetadata;//获取元数据
            OperateOption submitOption = OperateOption.Create();
            IOperationResult submitResult = BusinessDataServiceHelper.Submit(ctx, Meta.BusinessInfo, ids, "Submit", submitOption);
            return submitResult;

            //IMetaDataService metaService = ServiceHelper.GetService<IMetaDataService>();
            //FormMetadata targetBillMeta = metaService.Load(ctx, FormID) as FormMetadata;
            //// 构建保存操作参数：设置操作选项值，忽略交互提示
            //OperateOption submitOption = OperateOption.Create();
            //// 忽略全部需要交互性质的提示，直接保存；
            ////saveOption.SetIgnoreWarning(true);              // 忽略交互提示
            ////saveOption.SetInteractionFlag(this.Option.GetInteractionFlag());        // 如果有交互，传入用户选择的交互结果
            //// using Kingdee.BOS.Core.Interaction;
            ////saveOption.SetIgnoreInteractionFlag(this.Option.GetIgnoreInteractionFlag());
            ////// 如下代码，强制要求忽略交互提示(演示案例不需要，注释掉)
            //submitOption.SetIgnoreWarning(true);
            ////// using Kingdee.BOS.Core.Interaction;
            //submitOption.SetIgnoreInteractionFlag(true);
            //// 调用保存服务，自动保存
            //ISubmitService submitService = ServiceHelper.GetService<ISubmitService>();
            //IOperationResult submitResult = submitService.Submit(ctx, targetBillMeta.BusinessInfo, ids, "Submit", submitOption);
            //return submitResult;
        }

        /// <summary>
        /// 提交进入工作流
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public IOperationResult SubmitWorkFlowBill(Context ctx, string FormID, string billId)
        {
            IMetaDataService metaService = ServiceHelper.GetService<IMetaDataService>();//元数据服务
            FormMetadata Meta = metaService.Load(ctx, FormID) as FormMetadata;//获取元数据
            // 首先判断单据是否已经有未完成的工作流
            IProcInstService procInstService = Kingdee.BOS.Workflow.Contracts.ServiceFactory.GetProcInstService(ctx);
                bool isExist = procInstService.CheckUnCompletePrcInstExsit(ctx, FormID, billId);
                if (isExist)
                {
                    throw new KDBusinessException("AutoSubmit-001", "该单据已经启动了流程，不允许重复提交！");
                }
                // 读取单据的工作流配置模板
                IWorkflowTemplateService wfTemplateService = Kingdee.BOS.Workflow.Contracts.ServiceFactory.GetWorkflowTemplateService(ctx);
                List<FindPrcResult> findProcResultList = wfTemplateService.GetPrcListByFormID(
                                FormID, new string[] { billId }, ctx);
                if (findProcResultList == null || findProcResultList.Count == 0)
                {
                    throw new KDBusinessException("AutoSubmit-002", "查找单据适用的流程模板失败，不允许提交工作流！");
                }

                // 设置提交参数：忽略操作过程中的警告，避免与用户交互
                OperateOption submitOption = OperateOption.Create();
                submitOption.SetIgnoreWarning(true);
                IOperationResult submitResult = null;

                FindPrcResult findProcResult = findProcResultList[0];
                if (findProcResult.Result == TemplateResultType.Error)
                {
                    throw new KDBusinessException("AutoSubmit-003", "单据不符合流程启动条件，不允许提交工作流！");
                }
                else if (findProcResult.Result != TemplateResultType.Normal)
                {// 本单无适用的流程图，直接走传统审批
                    ISubmitService submitService = ServiceHelper.GetService<ISubmitService>();
                    submitResult = submitService.Submit(ctx, Meta.BusinessInfo,
                        new object[] { billId }, "Submit", submitOption);
                }
                else
                {// 走工作流
                    IBOSWorkflowService wfService = Kingdee.BOS.Workflow.Contracts.ServiceFactory.GetBOSWorkflowService(ctx);
                    submitResult = wfService.ListSubmit(ctx, Meta.BusinessInfo,
                        0, new object[] { billId }, findProcResultList, submitOption);
                }
            return submitResult;
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        public IOperationResult AuditBill(Context ctx, string FormID, object[] ids)
        {
            IMetaDataService metaService = ServiceHelper.GetService<IMetaDataService>();//元数据服务
            FormMetadata Meta = metaService.Load(ctx, FormID) as FormMetadata;//获取元数据
            OperateOption AuditOption = OperateOption.Create();
            AuditOption.SetIgnoreWarning(true);
            AuditOption.SetIgnoreInteractionFlag(true);
            IOperationResult AuditResult = BusinessDataServiceHelper.Audit(ctx, Meta.BusinessInfo, ids, AuditOption);
            return AuditResult;
        }

        /// <summary>
        /// 转换业务对象装填
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="tableName">状态字段所在物理表名</param>
        /// <param name="fieldName">状态字段名</param>
        /// <param name="fieldValue">要转换成的状态值</param>
        /// <param name="pkFieldName">物料表主键列名</param>
        /// <param name="pkFieldValues">主键值集合</param>
        public void setState(Context ctx, string tableName,string fieldName,string fieldValue,string pkFieldName, object[] pkFieldValues)
        {
            //获取数据服务
            IBusinessDataService businessDataService = ServiceHelper.GetService<IBusinessDataService>();
            businessDataService.SetState(ctx, tableName, fieldName, fieldValue, pkFieldName, pkFieldValues);
        }


        /// <summary>
        /// 整单下推
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="SourceFormId"></param>
        /// <param name="TargetFormId"></param>
        /// <param name="sourceBillIds"></param>
        /// <returns></returns>
        public ConvertOperationResult ConvertBills(Context ctx, string SourceFormId, string TargetFormId, List<long> sourceBillIds)
        {
            // 获取源单与目标单的转换规则
            IConvertService convertService = ServiceHelper.GetService<IConvertService>();
            var rules = convertService.GetConvertRules(ctx, SourceFormId, TargetFormId);
            if (rules == null || rules.Count == 0)
            {
                throw new KDBusinessException("", string.Format("未找到{0}到{1}之间，启用的转换规则，无法自动下推！", SourceFormId, TargetFormId));
            }
            // 取勾选了默认选项的规则
            var rule = rules.FirstOrDefault(t => t.IsDefault);
            // 如果无默认规则，则取第一个
            if (rule == null)
            {
                rule = rules[0];
            }
            // 开始构建下推参数：
            // 待下推的源单数据行
            List<ListSelectedRow> srcSelectedRows = new List<ListSelectedRow>();
            int rowkey = -1;
            foreach (long billId in sourceBillIds)
            {
                //把待下推的源单内码，逐个创建ListSelectedRow对象，添加到集合中
                srcSelectedRows.Add(new ListSelectedRow(billId.ToString(), string.Empty, rowkey++, SourceFormId));
            }
            // 指定目标单单据类型:情况比较复杂，直接留空，会下推到默认的单据类型
            string targetBillTypeId = string.Empty;
            // 指定目标单据主业务组织：情况更加复杂，
            // 建议在转换规则中，配置好主业务组织字段的映射关系：运行时，由系统根据映射关系，自动从上游单据取主业务组织，避免由插件指定
            long targetOrgId = 0;
            // 自定义参数字典：把一些自定义参数，传递到转换插件中；转换插件再根据这些参数，进行特定处理
            Dictionary<string, object> custParams = new Dictionary<string, object>();
            //custParams.Add("1", 1);
            //custParams.Add("2", 2);
            // 组装下推参数对象
            PushArgs pushArgs = new PushArgs(rule, srcSelectedRows.ToArray())
            {
                 TargetBillTypeId = targetBillTypeId,
                 TargetOrgId = targetOrgId,
                 CustomParams = custParams
            };
            // 调用下推服务，生成下游单据数据包
            ConvertOperationResult operationResult = convertService.Push(ctx, pushArgs, OperateOption.Create());
            return operationResult;
        }


        /// <summary>
        /// 构建数据包
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="fillBillPropertys"></param>
        /// <returns></returns>
        public DynamicObject installBillPackage(Context ctx, string FormID, Action<IDynamicFormViewService> fillBillPropertys,string BillTypeId)
        {
            FormMetadata Meta = MetaDataServiceHelper.Load(ctx, FormID) as FormMetadata;//获取元数据
            Form form = Meta.BusinessInfo.GetForm();
            IDynamicFormViewService dynamicFormViewService = (IDynamicFormViewService)Activator.CreateInstance(Type.GetType("Kingdee.BOS.Web.Import.ImportBillView,Kingdee.BOS.Web"));
            // 创建视图加载参数对象，指定各种参数，如FormId, 视图(LayoutId)等
            BillOpenParameter openParam = new BillOpenParameter(form.Id, Meta.GetLayoutInfo().Id);
            openParam.Context = ctx;
            openParam.ServiceName = form.FormServiceName;
            openParam.PageId = Guid.NewGuid().ToString();
            openParam.FormMetaData = Meta;
            openParam.Status = OperationStatus.ADDNEW;
            openParam.CreateFrom = CreateFrom.Default;
            // 单据类型
            openParam.DefaultBillTypeId = BillTypeId;
            openParam.SetCustomParameter("ShowConfirmDialogWhenChangeOrg", false);
            // 插件
            List<AbstractDynamicFormPlugIn> plugs = form.CreateFormPlugIns();
            openParam.SetCustomParameter(FormConst.PlugIns, plugs);
            PreOpenFormEventArgs args = new PreOpenFormEventArgs(ctx, openParam);
            foreach (var plug in plugs)
            {
                plug.PreOpenForm(args);
            }
            // 动态领域模型服务提供类，通过此类，构建MVC实例
            IResourceServiceProvider provider = form.GetFormServiceProvider(false);

            dynamicFormViewService.Initialize(openParam, provider);
            IBillView billView = dynamicFormViewService as IBillView;
            ((IBillViewService)billView).LoadData();

            // 触发插件的OnLoad事件：
            // 组织控制基类插件，在OnLoad事件中，对主业务组织改变是否提示选项进行初始化。
            // 如果不触发OnLoad事件，会导致主业务组织赋值不成功
            DynamicFormViewPlugInProxy eventProxy = billView.GetService<DynamicFormViewPlugInProxy>();
            eventProxy.FireOnLoad();
            if (fillBillPropertys != null)
            {
                fillBillPropertys(dynamicFormViewService);
            }
            // 设置FormId
            form = billView.BillBusinessInfo.GetForm();
            if (form.FormIdDynamicProperty != null)
            {
                form.FormIdDynamicProperty.SetValue(billView.Model.DataObject, form.Id);
            }
            return billView.Model.DataObject;
        }



        /// <summary>
        /// 判断操作结果是否成功，如果不成功，则直接抛错中断进程
        /// </summary>
        /// <param name="opResult">操作结果</param>
        /// <param name="opOption">操作参数</param>
        /// <returns></returns>
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


        /// <summary>
        /// 创建临时表
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="createSql"></param>
        /// <returns></returns>
        private string CreateTempTalbe(Context ctx, string createSql)
        {
            string tableName = ServiceHelper.GetService<IDBService>().CreateTemporaryTableName(ctx);
            createSql = string.Format(createSql, tableName);
            try
            {
                DBUtils.Execute(ctx, createSql);
            }
            catch (Exception)
            {

                throw;
            }
            return tableName;
        }

        
    }
}


