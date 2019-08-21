using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
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
    /// CRM服务契约
    /// </summary>
    [RpcServiceError]
    [ServiceContract]
    public interface ICRMService
    {
        /// <summary>
        /// 获取权限规则下销售员id集合
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<long> getSalerPersonids(Context ctx,long personId);

        /// <summary>
        /// 获取权限规则下项目号id集合
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        List<long> getProjectIds(Context ctx,long personId);


        /// <summary>
        /// 构建用于撞单分析的线索单据数据包
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID">业务对象标识</param>
        /// <param name="fillBillPropertys">填写单据内容</param>
        /// <param name="BillTypeId">单据类型ID</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(ServiceFault))]
        IBillModel installBillPackage(Context ctx, string FormID, Action<IDynamicFormViewService> fillBillPropertys, string BillTypeId);
    }
}
