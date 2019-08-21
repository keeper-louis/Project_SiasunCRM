using Kingdee.BOS.WebApi.ServicesStub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS;
using KEEPER.K3.CRM.CRMServiceHelper;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Bill;
using KEEPER.K3.CRM.Core.Bump;

namespace KEEPER.K3.SIASUN.CRM.CustomizeWebApi.ServiceStub
{
    public class BumpWebApiService : AbstractWebApiBusinessService
    {
        public BumpWebApiService(KDServiceContext context) : base(context)
        {

        }
        public Context Ctx
        {
            get
            {
                return this.KDContext.Session.AppContext;
            }
        }
        //构建线索单据数据包
        public string bumpAnalyse(string parameterJson)
        {
            //string value = HttpContext.Current.Request.Form["Data"];//获取前端传递过来的单据必要信息
            //JObject jObject = (JObject)JsonConvert.DeserializeObject(value);//反序列化成JObject对象
            Action<IDynamicFormViewService> fillBillPropertys = new Action<IDynamicFormViewService>(fillPropertys);
            IBillModel BillNodel = CRMServiceHelper.installBumpBillData(Ctx, "CRM_OPP_Clue", fillBillPropertys);
            IKEEPERBumpAnalysisCommon bumpCommon = KEEPERBumpAnalysisFactory.CreateBumpAnalysis(Ctx, BillNodel, BillNodel.BusinessInfo.GetForm().Id);
            return null;
        }

        //填充线索的字段内容
        private void fillPropertys(IDynamicFormViewService dynamicFormView)
        {
            //线索主题ZT001
            dynamicFormView.UpdateValue("F_PEJK_ClueName", 0, "ZT001");
            //需求内容new clue
            dynamicFormView.UpdateValue("FRemarks", 0, "new clue");
            //客户名称东方集团
            dynamicFormView.UpdateValue("FCustomerName", 0, "东方集团");
            //需求部门1
            dynamicFormView.UpdateValue("F_PEJK_ReqDeptId", 0, "1");
            //联系人名称KEEPER
            dynamicFormView.UpdateValue("FContactName", 0, "KEEPER");
            //电话1024-666669
            dynamicFormView.UpdateValue("FPhone", 0, "024-666669");

            

            //新增分录
            //((IBillView)dynamicFormView).Model.CreateNewEntryRow("FEntity");
            //如果预知有多条分录，可以使用这个方法进行批量新增
            //((IBillView)dynamicFormView).Model.BatchCreateNewEntryRow("FEntity",100);
            //dynamicFormView.SetItemValueByNumber("FExpenseItemID", "CI001", 1);
            //申请金额：固定值：10000
            //dynamicFormView.UpdateValue("FOrgAmount", 1, 20000);
        }
    }
}
