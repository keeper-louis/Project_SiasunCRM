using Kingdee.BOS.Core.DynamicForm.PlugIn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS;

namespace KEEPER.K3.SIASUN.CRM.ClueServicePlugIn
{
    [Description("线索保存插件")]
    public class Save:AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("F_PEJK_ProProcess");//项目进展
            e.FieldKeys.Add("F_PEJK_DDRESON");//丢单原因
        }
        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            //如果分录存在项目进展为丢单，改行丢单原因必填
            if (e.DataEntitys!=null&&e.DataEntitys.Count()>0)
            {
                foreach (DynamicObject item in e.DataEntitys)
                {
                    DynamicObjectCollection ProductDetails = item["PEJK_Cust_ProductDetail"] as DynamicObjectCollection;
                    if (ProductDetails.Count()>0)
                    {
                        foreach (DynamicObject ProductDetail in ProductDetails)
                        {
                            if (Convert.ToString(((DynamicObject)ProductDetail["F_PEJK_ProProcess"])["Name"]).Equals("丢单"))
                            {
                                if (ProductDetail["F_PEJK_DDRESON"] == null || ProductDetail["F_PEJK_DDRESON"].ToString().Equals("") || ProductDetail["F_PEJK_DDRESON"].ToString().Equals(" "))
                                {
                                    throw new KDBusinessException("", string.Format("单据第{0}行，项目进展为丢单，丢单原因未填写！", ProductDetail["Seq"]));
                                }
                            }
                                    
                        }
                    }
                }
            }
        }
    }
}
