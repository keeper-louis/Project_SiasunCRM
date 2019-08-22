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
using System.ComponentModel;
using Kingdee.K3.CRM.Entity;
using System.Collections;

namespace KEEPER.K3.SIASUN.CRM.CustomizeWebApi.ServiceStub
{
    [Description("移动CRM撞单分析接口")]
    public class BumpWebApiService : AbstractWebApiBusinessService
    {
        string F_PEJK_ClueName = "";//线索主题编码
        string FRemarks = "";// 需求内容
        string FCustomerName = "";//客户名称
        string F_PEJK_ReqDeptId = "";//需求部门
        string FContactName = "";//联系人名称
        string FPhone = ""; //移动电话
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
            JObject jsonRoot = new JObject();//根节点
            JObject mBEntry = new JObject();//model中单据体，存储普通变量，baseData
            JArray entrys = new JArray();//单个model中存储多行分录体集合，存储mBentry
            string value = HttpContext.Current.Request.Form["Data"];//获取前端传递过来的单据必要信息
            JObject jObject = (JObject)JsonConvert.DeserializeObject(value);//反序列化成JObject对象
            F_PEJK_ClueName = jObject["F_PEJK_ClueName"]!=null&& !jObject["F_PEJK_ClueName"].Equals("")? jObject["F_PEJK_ClueName"].ToString():"";
            FRemarks = jObject["FRemarks"] != null && !jObject["FRemarks"].Equals("") ? jObject["FRemarks"].ToString() : "";
            FCustomerName = jObject["FCustomerName"] != null && !jObject["FCustomerName"].Equals("") ? jObject["FCustomerName"].ToString() : "";
            F_PEJK_ReqDeptId = jObject["F_PEJK_ReqDeptId"] != null && !jObject["F_PEJK_ReqDeptId"].Equals("") ? jObject["F_PEJK_ReqDeptId"].ToString() : "";
            FContactName = jObject["FContactName"] != null && !jObject["FContactName"].Equals("") ? jObject["FContactName"].ToString() : ""; 
            FPhone = jObject["FPhone"] != null && !jObject["FPhone"].Equals("") ? jObject["FPhone"].ToString() : ""; 
            Action<IDynamicFormViewService> fillBillPropertys = new Action<IDynamicFormViewService>(fillPropertys);
            IBillModel BillNodel = CRMServiceHelper.installBumpBillData(Ctx, "CRM_OPP_Clue", fillBillPropertys);
            IKEEPERBumpAnalysisCommon bumpCommon = KEEPERBumpAnalysisFactory.CreateBumpAnalysis(Ctx, BillNodel, BillNodel.BusinessInfo.GetForm().Id);
            BumpAnalysisResultEntrity resultEntry = bumpCommon.ResultEntrity;
            Dictionary<string, Hashtable> bumpResult = resultEntry.DicMacthDesc;
            if (bumpResult.Count<=0)
            {
                jsonRoot.Add("bumpResult","fasle");
                return JsonConvert.SerializeObject(jsonRoot);
            }
            foreach (Hashtable item in bumpResult.Values)
            {
                mBEntry = new JObject();
                foreach (DictionaryEntry de in item)
                {
                    mBEntry.Add(Convert.ToString(de.Key), Convert.ToString(de.Value));
                }
                entrys.Add(mBEntry);
            }
            jsonRoot.Add("bumpResult", entrys);
            return JsonConvert.SerializeObject(jsonRoot);


        }

        //填充线索的字段内容
        private void fillPropertys(IDynamicFormViewService dynamicFormView)
        {
            //线索主题ZT001
            dynamicFormView.UpdateValue("F_PEJK_ClueName", 0, F_PEJK_ClueName);
            //需求内容new clue
            dynamicFormView.UpdateValue("FRemarks", 0, FRemarks);
            //客户名称东方集团
            dynamicFormView.UpdateValue("FCustomerName", 0, FCustomerName);
            //需求部门1
            dynamicFormView.UpdateValue("F_PEJK_ReqDeptId", 0, F_PEJK_ReqDeptId);
            //联系人名称KEEPER
            dynamicFormView.UpdateValue("FContactName", 0, FContactName);
            //电话1024-666669
            dynamicFormView.UpdateValue("FPhone", 0, FPhone);

            

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
