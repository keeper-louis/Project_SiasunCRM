using Kingdee.BOS;
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
    }
}
