using Kingdee.BOS.Core.DynamicForm.PlugIn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using KEEPER.K3.CRM.CRMServiceHelper;

namespace KEEPER.K3.SIASUN.CRM.OppServicePlugIn
{
    [Description("商业机会")]
    public class Audit:AbstractOperationServicePlugIn
    {
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            //商业机会审核自动将商机状态更新为执行中。
            object[] pkValues = (from c in e.DataEntitys
                            select c[0]).ToArray();//提交成功的结果
            CRMServiceHelper.setState(base.Context, "T_CRM_Opportunity", "FDOCUMENTSTATUS", "G", "FID", pkValues);
        }
    }
}
