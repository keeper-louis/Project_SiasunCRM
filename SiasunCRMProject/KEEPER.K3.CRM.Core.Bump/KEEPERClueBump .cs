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
    class KEEPERClueBump : KEEPERBumpAnalysisBase
    {
        public KEEPERClueBump(Context ctx, IBillModel BillModel) : base(ctx, BillModel)
        {

        }

        protected override void Bump()
        {
            FormMetadata formMetaData = MetaDataServiceHelper.GetFormMetaData(base.Context, "CRM_OPP_Clue");
            base.SetBumpData("CRM_OPP_Clue", formMetaData);
        }
    }
}
