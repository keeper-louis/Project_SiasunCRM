using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.ServiceHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KEEPER.K3.CRM.Core.Bump
{
    public class KEEPERCrmCustBump: KEEPERBumpAnalysisBase
    {
        public KEEPERCrmCustBump(Context ctx, IBillModel BillModel) : base(ctx, BillModel)
    {
        }

        protected override void Bump()
        {
            FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "CRM_CUST");
            base.SetBumpData("CRM_CUST", formMetaData);
        }
    }
}
