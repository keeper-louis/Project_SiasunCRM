using KEEPER.K3.CRM.Core.Entity;
using Kingdee.BOS;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.Operation;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace KEEPER.K3.CRM.Contracts
{
    /// <summary>
    /// 服务契约
    /// </summary>
    [RpcServiceError]
    [ServiceContract]
    public interface ICommonService
    {
        /// <summary>
        /// 暂存单据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="dyObject"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult DraftBill(Context ctx, string FormID, DynamicObject[] dyObject);

        /// <summary>
        /// 保存单据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult SaveBill(Context ctx, string FormID, DynamicObject[] dyObject);

        /// <summary>
        /// 批量保存单据(另外一种保存服务的调用)
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult BatchSaveBill(Context ctx, string FormID, DynamicObject[] dyObject);

        /// <summary>
        /// 提交单据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult SubmitBill(Context ctx, string FormID, object[] ids);



        /// <summary>
        /// 审核单据
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IOperationResult AuditBill(Context ctx, string FormID, object[] ids);


        /// <summary>
        /// 转换业务对象装填
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="tableName">状态字段所在物理表名</param>
        /// <param name="fieldName">状态字段名</param>
        /// <param name="fieldValue">要转换成的状态值</param>
        /// <param name="pkFieldName">物料表主键列名</param>
        /// <param name="pkFieldValues">主键值集合</param>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        void setState(Context ctx, string tableName, string fieldName, string fieldValue, string pkFieldName, object[] pkFieldValues);





        /// <summary>
        /// 下推 按照单据内码ID集合
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="SourceFormId"></param>
        /// <param name="TargetFormId"></param>
        /// <param name="sourceBillIds"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        ConvertOperationResult ConvertBills(Context ctx,  string SourceFormId, string TargetFormId, List<long> sourceBillIds);



        /// <summary>
        /// 构建单据数据包
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID">业务对象标识</param>
        /// <param name="fillBillPropertys">填写单据内容</param>
        /// <param name="BillTypeId">单据类型ID</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        DynamicObject installBillPackage(Context ctx, string FormID, Action<IDynamicFormViewService> fillBillPropertys,string BillTypeId);


    }
}
