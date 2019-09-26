using Kingdee.BOS.Core.List.PlugIn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.List.PlugIn.Args;
using KEEPER.K3.CRM.CRMServiceHelper;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Util;

namespace KEEPER.K3.SIASUN.CRM.OppBusinessPlugIn
{
    [Description("商业机会列表界面插件")]
    public class OppListUI: AbstractListPlugIn
    {
        public override void PrepareFilterParameter(FilterArgs e)
        {
            base.PrepareFilterParameter(e);
            long userId = this.Model.Context.UserId;
            string strSql = string.Format(@"/*dialect*/ SELECT FLINKOBJECT FROM T_SEC_USER WHERE FUSERID = {0}", userId);
            long personId = DBUtils.ExecuteScalar<long>(this.Context, strSql, 0, null);
            List<long> salePersonIds = CRMServiceHelper.getSalerPersonids(this.Context, personId);
            if (salePersonIds!=null)
            {
                var a = from Id in salePersonIds select Id;
                string ids = string.Join(",", a.ToArray());
                string filter = string.Format(@"FBEMPID in ({0})", ids);
                e.FilterString = e.FilterString.IsNullOrEmpty() ? filter : e.FilterString + " and " + filter;
            }
        }
    }
}
