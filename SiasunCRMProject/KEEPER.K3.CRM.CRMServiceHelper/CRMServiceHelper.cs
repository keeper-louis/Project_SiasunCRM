using KEEPER.K3.CRM.Contracts;
using KEEPER.K3.CRM.Core.Entity;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.Operation;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KEEPER.K3.CRM.CRMServiceHelper
{
    public class CRMServiceHelper
    {

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="dyObject"></param>
        /// <returns></returns>
        public static IOperationResult Save(Context ctx, string FormID, DynamicObject[] dyObject)
        {
            ICommonService service = ServiceFactory.GetService<ICommonService>(ctx);
            IOperationResult saveResult = service.SaveBill(ctx, FormID, dyObject);
            return saveResult;
        }

        /// <summary>
        /// 另一种服务保存
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="dyObject"></param>
        /// <returns></returns>
        public static IOperationResult BatchSave(Context ctx, string FormID, DynamicObject[] dyObject)
        {
            ICommonService service = ServiceFactory.GetService<ICommonService>(ctx);
            IOperationResult saveResult = service.BatchSaveBill(ctx, FormID, dyObject);
            return saveResult;
        }

        
        
        /// <summary>
        /// 业务对象提交
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID">业务对象标识</param>
        /// <param name="ids">业务对象ID集合</param>
        /// <returns></returns>
        public static IOperationResult Submit(Context ctx, string formID, Object[] ids)
        {
            ICommonService service = ServiceFactory.GetService<ICommonService>(ctx);
            IOperationResult submitResult = service.SubmitBill(ctx, formID, ids);
            return submitResult;
        }


        /// <summary>
        /// 审核业务对象
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="formID">业务对象标识</param>
        /// <param name="ids">业务对象ID集合</param>
        /// <returns></returns>
        public static IOperationResult Audit(Context ctx, string formID, Object[] ids)
        {
            ICommonService service = ServiceFactory.GetService<ICommonService>(ctx);
            IOperationResult auditResult = service.AuditBill(ctx, formID, ids);
            return auditResult;
        }

        /// <summary>
        /// 业务对象状态转换
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="tableName"></param>
        /// <param name="fieldName"></param>
        /// <param name="fieldValue"></param>
        /// <param name="pkFieldName"></param>
        /// <param name="pkFieldValues"></param>
        public static void setState(Context ctx, string tableName, string fieldName, string fieldValue, string pkFieldName, object[] pkFieldValues)
        {
            ICommonService service = ServiceFactory.GetService<ICommonService>(ctx);
            service.setState(ctx, tableName, fieldName, fieldValue, pkFieldName, pkFieldValues);
        }
        /// <summary>
        /// 整单批量下推
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="SourceFormId"></param>
        /// <param name="TargetFormId"></param>
        /// <param name="sourceBillIds"></param>
        /// <returns></returns>
        public static ConvertOperationResult ConvertBills(Context ctx, string SourceFormId, string TargetFormId, List<long> sourceBillIds)
        {
            ICommonService service = ServiceFactory.GetService<ICommonService>(ctx);
            return service.ConvertBills(ctx, SourceFormId, TargetFormId, sourceBillIds);
        }
        /// <summary>
        /// 构建业务对象数据包
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID">对象标识</param>
        /// <param name="fillBillPropertys">填充业务对象属性委托对象</param>
        /// <returns></returns>
        public static DynamicObject CreateBillMode(Context ctx, string FormID, Action<IDynamicFormViewService> fillBillPropertys)
        {
            ICommonService service = ServiceFactory.GetService<ICommonService>(ctx);
            DynamicObject model = service.installBillPackage(ctx, FormID, fillBillPropertys, "");
            return model;
        }

    }
}
