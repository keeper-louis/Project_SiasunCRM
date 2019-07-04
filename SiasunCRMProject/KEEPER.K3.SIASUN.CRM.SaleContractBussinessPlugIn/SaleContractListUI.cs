using Kingdee.BOS.Core.List.PlugIn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.List.PlugIn.Args;
using Kingdee.BOS.App.Data;
using KEEPER.K3.CRM.CRMServiceHelper;
using Kingdee.BOS.Util;

namespace KEEPER.K3.SIASUN.CRM.SaleContractBussinessPlugIn
{
    [Description("销售合同列表插件")]
    public class SaleContractListUI:AbstractListPlugIn
    {
        public override void PrepareFilterParameter(FilterArgs e)
        {
            base.PrepareFilterParameter(e);
            long userId = this.Model.Context.UserId;
            string strSql = string.Format(@"/*dialect*/ SELECT FLINKOBJECT FROM T_SEC_USER WHERE FUSERID = {0}", userId);
            long personId = DBUtils.ExecuteScalar<long>(this.Context, strSql, 0, null);
            List<long> salePersonIds = CRMServiceHelper.getSalerPersonids(this.Context, personId);
            var a = from Id in salePersonIds select Id;
            string ids = string.Join(",", a.ToArray());
            string filter = string.Format(@"FSALERID in ({0})", ids);
            e.FilterString = e.FilterString.IsNullOrEmpty() ? filter : e.FilterString + " and " + filter;
        }
    }
}
