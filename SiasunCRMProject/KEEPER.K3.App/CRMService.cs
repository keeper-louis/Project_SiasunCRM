using KEEPER.K3.CRM.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace KEEPER.K3.App
{
    public class CRMService : ICRMService
    {
        /// <summary>
        /// 获取数据规则下项目信息id集合
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="personId"></param>
        /// <returns></returns>
        public List<long> getProjectIds(Context ctx, long personId)
        {
            List<long> proIds = new List<long>();
            List<long> salerIds = getSalerPersonids(ctx, personId);
            if (salerIds == null)
            {
                return null;
            }
            string Ids = string.Join(",", salerIds);
            string strSql = string.Format(@"/*dialect*/SELECT DISTINCT AA.F_PEJK_PRONO FROM PEJK_SALECONTRACTS AA INNER JOIN PEJK_SALECONTRACTENTRY BB ON AA.FID = BB.FID WHERE BB.F_PEJK_SALER IN ({0})",Ids);
            DynamicObjectCollection col = DBUtils.ExecuteDynamicObject(ctx, strSql);
            if (col == null || col.Count() == 0)
            {
                return null;
            }
            foreach (var item in col)
            {
                proIds.Add(Convert.ToInt64(item["F_PEJK_PRONO"]));
            }
            return proIds;
            
        }

        /// <summary>
        /// 获取数据规则下销售员id集合
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="personId"></param>
        /// <returns></returns>
        public List<long> getSalerPersonids(Context ctx, long personId)
        {
            List<long> salerPersonIds = new List<long>();
            //判断personId是否在CRM汇报关系设置表中
            string strSql_1 = string.Format(@"/*dialect*/SELECT * FROM PEJK_RPTSHIP WHERE F_PEJK_MANAGER = {0}",personId);
            DynamicObjectCollection  headCol = DBUtils.ExecuteDynamicObject(ctx, strSql_1);
            if (headCol == null || headCol.Count==0)
            {
                string strSql_2 = string.Format(@"/*dialect*/SELECT * FROM PEJK_RPTSHIPENTRY WHERE F_PEJK_TEAMMEMBER = {0}",personId);
                DynamicObjectCollection entryCol = DBUtils.ExecuteDynamicObject(ctx, strSql_2);
                if (entryCol == null || entryCol.Count ==0)
                {
                    return null;
                }
            }
            string sql = string.Format(@"/*dialect*/SELECT B.F_PEJK_TEAMMEMBER
  FROM PEJK_RPTSHIP A
 INNER JOIN PEJK_RPTSHIPENTRY B
    ON A.FID = B.FID
 WHERE A.F_PEJK_MANAGER = {0}

UNION ALL

SELECT BB.F_PEJK_TEAMMEMBER
  FROM PEJK_RPTSHIP A
 INNER JOIN PEJK_RPTSHIPENTRY B
    ON A.FID = B.FID
 INNER JOIN PEJK_RPTSHIP AA
    ON B.F_PEJK_TEAMMEMBER = AA.F_PEJK_MANAGER
 INNER JOIN PEJK_RPTSHIPENTRY BB
    ON AA.FID = BB.FID
 WHERE A.F_PEJK_MANAGER = {0}

UNION ALL

SELECT BBB.F_PEJK_TEAMMEMBER
  FROM PEJK_RPTSHIP A
 INNER JOIN PEJK_RPTSHIPENTRY B
    ON A.FID = B.FID
 INNER JOIN PEJK_RPTSHIP AA
    ON B.F_PEJK_TEAMMEMBER = AA.F_PEJK_MANAGER
 INNER JOIN PEJK_RPTSHIPENTRY BB
    ON AA.FID = BB.FID
 INNER JOIN PEJK_RPTSHIP AAA
    ON BB.F_PEJK_TEAMMEMBER = AAA.F_PEJK_MANAGER
 INNER JOIN PEJK_RPTSHIPENTRY BBB
    ON AAA.FID = BBB.FID
 WHERE A.F_PEJK_MANAGER = {0}

UNION ALL

SELECT BBBB.F_PEJK_TEAMMEMBER
  FROM PEJK_RPTSHIP A
 INNER JOIN PEJK_RPTSHIPENTRY B
    ON A.FID = B.FID
 INNER JOIN PEJK_RPTSHIP AA
    ON B.F_PEJK_TEAMMEMBER = AA.F_PEJK_MANAGER
 INNER JOIN PEJK_RPTSHIPENTRY BB
    ON AA.FID = BB.FID
 INNER JOIN PEJK_RPTSHIP AAA
    ON BB.F_PEJK_TEAMMEMBER = AAA.F_PEJK_MANAGER
 INNER JOIN PEJK_RPTSHIPENTRY BBB
    ON AAA.FID = BBB.FID
 INNER JOIN PEJK_RPTSHIP AAAA
    ON BBB.F_PEJK_TEAMMEMBER = AAAA.F_PEJK_MANAGER
 INNER JOIN PEJK_RPTSHIPENTRY BBBB
    ON AAAA.FID = BBBB.FID
 WHERE A.F_PEJK_MANAGER = {0}
UNION ALL
SELECT {0}", personId);
            DynamicObjectCollection personIds = DBUtils.ExecuteDynamicObject(ctx, sql);
            if (personIds != null&& personIds.Count()>0)
            {
                foreach (var item in personIds)
                {
                    salerPersonIds.Add(Convert.ToInt64(item["F_PEJK_TEAMMEMBER"]));
                }
                string Ids = string.Join(",", salerPersonIds);
                string strSql = string.Format(@"/*dialect*/SELECT V.FID
  FROM T_BD_PERSON A
 INNER JOIN T_BD_STAFF B
    ON A.FPERSONID = B.FPERSONID
 INNER JOIN V_BD_SALESMAN V
    ON B.FSTAFFID = V.FSTAFFID
 WHERE A. FPERSONID IN ({0})
",Ids);
                DynamicObjectCollection salerIds = DBUtils.ExecuteDynamicObject(ctx, strSql);
                if (salerIds != null && salerIds.Count() > 0)
                {
                    salerPersonIds.Clear();
                    foreach (var item in salerIds)
                    {
                        salerPersonIds.Add(Convert.ToInt64(item["FID"]));
                    }
                    return salerPersonIds;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 构建移动端撞单分析单据对象
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FormID"></param>
        /// <param name="fillBillPropertys"></param>
        /// <param name="BillTypeId"></param>
        /// <returns></returns>
        public IBillModel installBillPackage(Context ctx, string FormID, Action<IDynamicFormViewService> fillBillPropertys, string BillTypeId = "")
        {
            FormMetadata Meta = MetaDataServiceHelper.Load(ctx, FormID) as FormMetadata;//获取元数据
            Form form = Meta.BusinessInfo.GetForm();
            IDynamicFormViewService dynamicFormViewService = (IDynamicFormViewService)Activator.CreateInstance(Type.GetType("Kingdee.BOS.Web.Import.ImportBillView,Kingdee.BOS.Web"));
            // 创建视图加载参数对象，指定各种参数，如FormId, 视图(LayoutId)等
            BillOpenParameter openParam = new BillOpenParameter(form.Id, Meta.GetLayoutInfo().Id);
            openParam.Context = ctx;
            openParam.ServiceName = form.FormServiceName;
            openParam.PageId = Guid.NewGuid().ToString();
            openParam.FormMetaData = Meta;
            openParam.Status = OperationStatus.ADDNEW;
            openParam.CreateFrom = CreateFrom.Default;
            // 单据类型
            openParam.DefaultBillTypeId = BillTypeId;
            openParam.SetCustomParameter("ShowConfirmDialogWhenChangeOrg", false);
            // 插件
            List<AbstractDynamicFormPlugIn> plugs = form.CreateFormPlugIns();
            openParam.SetCustomParameter(FormConst.PlugIns, plugs);
            PreOpenFormEventArgs args = new PreOpenFormEventArgs(ctx, openParam);
            foreach (var plug in plugs)
            {
                plug.PreOpenForm(args);
            }
            // 动态领域模型服务提供类，通过此类，构建MVC实例
            IResourceServiceProvider provider = form.GetFormServiceProvider(false);

            dynamicFormViewService.Initialize(openParam, provider);
            IBillView billView = dynamicFormViewService as IBillView;
            ((IBillViewService)billView).LoadData();

            // 触发插件的OnLoad事件：
            // 组织控制基类插件，在OnLoad事件中，对主业务组织改变是否提示选项进行初始化。
            // 如果不触发OnLoad事件，会导致主业务组织赋值不成功
            DynamicFormViewPlugInProxy eventProxy = billView.GetService<DynamicFormViewPlugInProxy>();
            eventProxy.FireOnLoad();
            if (fillBillPropertys != null)
            {
                fillBillPropertys(dynamicFormViewService);
            }
            // 设置FormId
            form = billView.BillBusinessInfo.GetForm();
            if (form.FormIdDynamicProperty != null)
            {
                form.FormIdDynamicProperty.SetValue(billView.Model.DataObject, form.Id);
            }
            return billView.Model;
        }
    }
}
