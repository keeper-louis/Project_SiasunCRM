using Kingdee.BOS;
using Kingdee.BOS.Core.Bill;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KEEPER.K3.CRM.Core.Bump
{
    public class KEEPERBumpAnalysisFactory
    {
        // Methods
        public static IKEEPERBumpAnalysisCommon CreateBumpAnalysis(Context ctx, IBillModel BillModel, string strFormid)
        {
            IKEEPERBumpAnalysisCommon common = null;
            string str = strFormid;
            if (str == null)
            {
                return common;
            }
            if (str != "CRM_OPP_Opportunity")
            {
                if (str != "CRM_CUST")
                {
                    if (str != "CRM_OPP_Clue")
                    {
                        return common;
                    }
                    return new KEEPERClueBump(ctx, BillModel);
                }
            }
            else
            {
                return new KEEPEROpportunityBump(ctx, BillModel);
            }
            return new KEEPERCrmCustBump(ctx, BillModel);
        }
    }
}
