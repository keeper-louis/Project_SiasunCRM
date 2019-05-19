using Kingdee.BOS.Core.DynamicForm.PlugIn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using KEEPER.K3.SIASUN.CRM.OppValidators;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS;

namespace KEEPER.K3.SIASUN.CRM.OppServicePlugIn
{
    [Description("输单操作")]
    public class LoseContract:AbstractOperationServicePlugIn
    {

        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FWinReason");
        }

        //输单操作校验器
        //public override void OnAddValidators(AddValidatorsEventArgs e)
        //{
        //    base.OnAddValidators(e);
        //    LoseContractValidator loseValidator = new LoseContractValidator();
        //    loseValidator.EntityKey = "FBillHead";
        //    e.Validators.Add(loseValidator);
        //}

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            if (e.DataEntitys!=null&&e.DataEntitys.Count()>0)
            {
                foreach (DynamicObject item in e.DataEntitys)
                {
                    string reason = Convert.ToString(item["FWinReason"]);
                    if (reason.Equals(" "))
                    {
                        throw new KDBusinessException("", string.Format("商机编号{0}输赢原因没填,输单操作失败！",item["FBillNo"]));
                    }
                }
            }
        }
    }
}
