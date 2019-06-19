using Kingdee.BOS.Core.Bill.PlugIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.App.Data;
using System.ComponentModel;

namespace KEEPER.K3.SIASUN.CRM.SaleContractBussinessPlugIn
{
    [Description("销售合同表单插件-项目编号携带项目金额")]
    public class SaleContractEditUI:AbstractBillPlugIn
    {
        public override void DataChanged(DataChangedEventArgs e)
        {
            if (e.Field.Key.Equals("F_PEJK_ProNo"))
            {
                if (Convert.ToInt64(e.NewValue) != 0)
                {
                    string strSql = string.Format(@"/*dialect*/select FPAMOUNT from PBZS_SSProject where FPNO = {0}", Convert.ToInt64(e.NewValue));
                    double amount = DBUtils.ExecuteScalar<double>(this.Context, strSql, 0, null);
                    this.Model.SetValue("F_PEJK_ProAmount", amount, e.Row);
                }
                
            }    
            base.DataChanged(e);
        }
    }
}
