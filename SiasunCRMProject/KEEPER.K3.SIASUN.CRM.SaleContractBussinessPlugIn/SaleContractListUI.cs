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
using Kingdee.BOS.Orm.DataEntity;

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
            if (salePersonIds!=null)
            {
                var a = from Id in salePersonIds select Id;
                string ids = string.Join(",", a.ToArray());
                string billNoQuery = string.Format(@"/*dialect*/select a.fid from T_CRM_CONTRACT a left join PEJK_PRODETAIL b on a.FID = b.FID where a.FSALERID in ({0}) or b.F_PEJK_MANAGER IN ({0})", ids);
                DynamicObjectCollection doo = DBUtils.ExecuteDynamicObject(this.View.Context, billNoQuery);
                object[] fid = (from c in doo select c[0]).ToArray();
                string billIds = string.Join(",", fid);
                string filter = string.Format(@"fid in ({0})", billIds);
                e.FilterString = e.FilterString.IsNullOrEmpty() ? filter : e.FilterString + " and " + filter;
            }
        }
    }
}
