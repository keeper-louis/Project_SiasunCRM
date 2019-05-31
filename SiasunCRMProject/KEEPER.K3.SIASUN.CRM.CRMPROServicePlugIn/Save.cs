using Kingdee.BOS.Core.DynamicForm.PlugIn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;

namespace KEEPER.K3.SIASUN.CRM.CRMPROServicePlugIn
{
    [Description("CRM产品保存")]
    public class Save:AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("F_PEJK_ProProcess");
        }

        /// <summary>
        /// CRM产品保存前校验，项目进展必填
        /// </summary>
        /// <param name="e"></param>
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            if (e.DataEntitys!=null && e.DataEntitys.Count()>0)
            {
                foreach (DynamicObject item in e.DataEntitys)
                {
                    DynamicObjectCollection entity = item["PEJK_Cust_CRMPROENTRY"] as DynamicObjectCollection;
                    if (entity.Count() > 0 && ((DynamicObject)entity[0]["F_PEJK_ProProcess"]) != null)
                    {

                    }
                    else
                    {
                        throw new Exception("项目进展为必填项！！！");
                    }
                }
            }
        }
    }
}
