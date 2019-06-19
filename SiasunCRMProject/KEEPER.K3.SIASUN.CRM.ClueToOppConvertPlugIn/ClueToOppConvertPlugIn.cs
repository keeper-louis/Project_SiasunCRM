using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.Metadata.ConvertElement.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Core.Metadata.FormElement;
using Kingdee.BOS.Core;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.App;
using Kingdee.BOS.App.Data;

namespace KEEPER.K3.SIASUN.CRM.ClueToOppConvertPlugIn
{
    [Description("线索转商机执行部门单据体携带")]
    public class ClueToOppConvertPlugIn : AbstractConvertPlugIn
    {
        /// <summary>
        /// 主单据体的字段携带完毕，与源单的关联关系创建好之后，触发此事件
        /// </summary>
        /// <param name="e"></param>
        public override void OnAfterCreateLink(CreateLinkEventArgs e)
        {
            // 预先获取一些必要的元数据，后续代码要用到：
            // 源单第二单据体,执行部门
            Entity srcSecondEntity = e.SourceBusinessInfo.GetEntity("F_PEJK_ExecuteDept");


            // 目标单第一单据体，产品明细
            //Entity mainEntity = e.TargetBusinessInfo.GetEntity("FEntity");

            // 目标单第二单据体，执行部门
            Entity secondEntity = e.TargetBusinessInfo.GetEntity("F_PEJK_OppExecuteDept");

            // 目标单关联子单据体
            //Entity linkEntity = null;
            //Form form = e.TargetBusinessInfo.GetForm();
            //if (form.LinkSet != null
            //    && form.LinkSet.LinkEntitys != null
            //    && form.LinkSet.LinkEntitys.Count != 0)
            //{
            //    linkEntity = e.TargetBusinessInfo.GetEntity(
            //        form.LinkSet.LinkEntitys[0].Key);
            //}

            //if (linkEntity == null)
            //{
            //    return;
            //}

            // 获取生成的全部下游单据
            ExtendedDataEntity[] billDataEntitys = e.TargetExtendedDataEntities.FindByEntityKey("FBillHead");

            // 对下游单据，逐张单据进行处理
            //foreach (var item in billDataEntitys)
            //{
            //    DynamicObject dataObject = item.DataEntity;
            //    // 定义一个集合，用于收集本单对应的源单内码
            //    HashSet<long> srcBillIds = new HashSet<long>();
            //    //开始到主单据体中，读取关联的源单内码
            //    DynamicObjectCollection mainEntryRows =
            //        mainEntity.DynamicProperty.GetValue(dataObject) as DynamicObjectCollection;
            //    foreach (var mainEntityRow in mainEntryRows)
            //    {
            //        DynamicObjectCollection linkRows =
            //            linkEntity.DynamicProperty.GetValue(mainEntityRow) as DynamicObjectCollection;
            //        foreach (var linkRow in linkRows)
            //        {
            //            long srcBillId = Convert.ToInt64(linkRow["SBillId"]);
            //            if (srcBillId != 0
            //                && srcBillIds.Contains(srcBillId) == false)
            //            {
            //                srcBillIds.Add(srcBillId);
            //            }
            //        }
            //    }


            //DynamicObject linkRows =
            //    linkEntity.DynamicProperty.GetValue(dataObject) as DynamicObject;
            //    long srcBillId = Convert.ToInt64(linkRows["SBillId"]);
            //    if (srcBillId != 0
            //        && srcBillIds.Contains(srcBillId) == false)
            //    {
            //        srcBillIds.Add(srcBillId);
            //    }
            //定义一个集合，用于收集本单对应的源单内码
            HashSet<long> srcBillIds = new HashSet<long>();
            foreach (var item in billDataEntitys)
            {
                DynamicObject dataObject = item.DataEntity;
                if (Convert.ToString(dataObject["FSourceBillNo"])!=null&& Convert.ToString(dataObject["FSourceBillNo"])!=" ")
                {
                    string strSql = string.Format(@"/*dialect*/select FID from T_CRM_Clue where FBILLNO = '{0}'", Convert.ToString(dataObject["FSourceBillNo"]));
                    long srcBillId = DBUtils.ExecuteScalar<long>(this.Context, strSql, 0, null);
                    if (srcBillId != 0
                    && srcBillIds.Contains(srcBillId) == false)
                    {
                        srcBillIds.Add(srcBillId);
                    }
                    if (srcBillIds.Count == 0)
                    {
                        continue;
                    }
                    // 开始加载源单第二单据体上的字段

                    // 确定需要加载的源单字段（仅加载需要携带的字段）
                    List<SelectorItemInfo> selector = new List<SelectorItemInfo>();
                    selector.Add(new SelectorItemInfo("F_PEJK_ExecuteDeptId"));
                    // TODO: 继续添加其他需要携带的字段，示例代码略
                    // 设置过滤条件
                    string filter = string.Format(" {0} IN ({1}) ",
                        e.SourceBusinessInfo.GetForm().PkFieldName,
                        string.Join(",", srcBillIds));
                    OQLFilter filterObj = OQLFilter.CreateHeadEntityFilter(filter);

                    // 读取源单
                    IViewService viewService = ServiceHelper.GetService<IViewService>();
                    var srcBillObjs = viewService.Load(this.Context,
                        e.SourceBusinessInfo.GetForm().Id,
                        selector,
                        filterObj);

                    // 开始把源单单据体数据，填写到目标单上
                    DynamicObjectCollection secondEntryRows =
                        secondEntity.DynamicProperty.GetValue(dataObject) as DynamicObjectCollection;
                    secondEntryRows.Clear();    // 删除空行

                    foreach (var srcBillObj in srcBillObjs)
                    {
                        DynamicObjectCollection srcEntryRows =
                            srcSecondEntity.DynamicProperty.GetValue(srcBillObj) as DynamicObjectCollection;

                        foreach (var srcEntryRow in srcEntryRows)
                        {
                            // 目标单添加新行，并接受源单字段值
                            DynamicObject newRow = new DynamicObject(secondEntity.DynamicObjectType);
                            secondEntryRows.Add(newRow);
                            // 填写字段值
                            newRow["F_PEJK_ExecuteDeptId"] = srcEntryRow["F_PEJK_ExecuteDeptId"];
                            // TODO: 逐个填写其他字段值，示例代码略
                        }
                    }
                }
                
                //string strSql = string.Format(@"/*dialect*/select fsbillid from T_CRM_Opportunity_LK where fid = {0}", Convert.ToInt64(dataObject["id"]));
                
                
                
                
            }



        }
    }
}
