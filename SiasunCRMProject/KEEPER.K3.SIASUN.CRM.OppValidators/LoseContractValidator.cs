using Kingdee.BOS.Core.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.Core;
using Kingdee.BOS.Util;
using Kingdee.BOS.Orm.DataEntity;

namespace KEEPER.K3.SIASUN.CRM.OppValidators
{
    [Description("Opp输单操作校验器")]
    public class LoseContractValidator : AbstractValidator
    {
        public override void Validate(ExtendedDataEntity[] dataEntities, ValidateContext validateContext, Context ctx)
        {
            if (dataEntities.IsNullOrEmpty() || dataEntities.Length == 0)
            {
                return;
            }
            foreach (ExtendedDataEntity item in dataEntities)
            {
                DynamicObject requestDynamic = item.DataEntity;
                string reason = Convert.ToString(requestDynamic["FWinReason"]);
               
                if (reason.Equals(" "))
                {
                    string msg = string.Format("商机编号{0}输赢原因没填", requestDynamic["FBillNo"]);
                    var errInfo = new ValidationErrorInfo(
                                    item.BillNo,
                                    item.DataEntity["Id"].ToString(),
                                    item.DataEntityIndex,
                                    item.RowIndex,
                                    "ARValid019",
                                    msg,
                                    " ",
                                    Kingdee.BOS.Core.Validation.ErrorLevel.Error);
                    validateContext.AddError(item.DataEntity, errInfo);
                    
                }
            }
        }
    }
}
