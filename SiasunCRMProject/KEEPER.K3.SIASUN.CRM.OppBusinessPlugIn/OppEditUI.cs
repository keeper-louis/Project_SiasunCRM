using Kingdee.BOS.Core.Bill.PlugIn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS;

namespace KEEPER.K3.SIASUN.CRM.OppBusinessPlugIn
{
    [Description("商机维护插件")]
    public class OppEditUI:AbstractBillPlugIn
    {
        public override void AfterButtonClick(AfterButtonClickEventArgs e)
        {
            base.AfterButtonClick(e);
        }

        public override void AfterBarItemClick(AfterBarItemClickEventArgs e)
        {
            base.AfterBarItemClick(e);
            if (e.BarItemKey.Equals("tbLose"))
            {
                if (Convert.ToString(this.Model.GetValue("FWinReason")).Equals(" "))
                {
                    throw new KDBusinessException("", string.Format("商机输赢原因没填,输单操作失败！"));
                }
                
            }
            
        }
    }
}
