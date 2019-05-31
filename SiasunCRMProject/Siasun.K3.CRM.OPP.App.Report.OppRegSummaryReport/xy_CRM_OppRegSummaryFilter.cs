using Kingdee.BOS;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.CommonFilter.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Siasun.K3.CRM.OPP.App.Report.OppRegSummaryReport
{
    public class xy_CRM_OppRegSummaryFilter : AbstractCommonFilterPlugIn
    {
        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //维度下拉列表
            List<EnumItem> lstDimensionItems = new List<EnumItem>();
            EnumItem item0 = new EnumItem();
            item0.Value = "1";
            item0.Caption = new LocaleValue("产业");
            lstDimensionItems.Add(item0);
            EnumItem item1 = new EnumItem();
            item1.Value = "2";
            item1.Caption = new LocaleValue("月份");
            lstDimensionItems.Add(item1);
            EnumItem item2 = new EnumItem();
            item2.Value = "3";
            item2.Caption = new LocaleValue("省份");
            lstDimensionItems.Add(item2);
            EnumItem item3 = new EnumItem();
            item3.Value = "4";
            item3.Caption = new LocaleValue("区域");
            lstDimensionItems.Add(item3);

            ComboFieldEditor comFieldEditor = this.View.GetControl<ComboFieldEditor>("F_xy_Dimension");
            comFieldEditor.SetComboItems(lstDimensionItems);
            this.View.Model.SetValue("F_xy_Dimension", "1");

            //年份下拉列表
            int startYear = 2015;
            int currentYear = DateTime.Now.Year;
            List<EnumItem> lstEnumItems = new List<EnumItem>();
            for (int i = startYear; i <= currentYear; i++)
            {
                EnumItem item = new EnumItem();
                item.Value = i.ToString();
                item.Caption = new LocaleValue(i.ToString());
                lstEnumItems.Add(item);
            }
            ComboFieldEditor comFieldEditor2 = this.View.GetControl<ComboFieldEditor>("F_xy_Year");
            comFieldEditor2.SetComboItems(lstEnumItems);
            this.View.Model.SetValue("F_xy_Year", currentYear.ToString());

            //单据状态下拉列表
            List<EnumItem> lstBillStatusItems = new List<EnumItem>();

            EnumItem statusItem0 = new EnumItem();
            statusItem0.Value = "Z";
            statusItem0.Caption = new LocaleValue("暂存");
            lstBillStatusItems.Add(statusItem0);

            EnumItem statusItem1 = new EnumItem();
            statusItem1.Value = "A";
            statusItem1.Caption = new LocaleValue("创建");
            lstBillStatusItems.Add(statusItem1);

            EnumItem statusItem2 = new EnumItem();
            statusItem2.Value = "B";
            statusItem2.Caption = new LocaleValue("提交");
            lstBillStatusItems.Add(statusItem2);

            EnumItem statusItem3 = new EnumItem();
            statusItem3.Value = "C";
            statusItem3.Caption = new LocaleValue("审核");
            lstBillStatusItems.Add(statusItem3);

            EnumItem statusItem4 = new EnumItem();
            statusItem4.Value = "D";
            statusItem4.Caption = new LocaleValue("重新审核");
            lstBillStatusItems.Add(statusItem4);

            EnumItem statusItem5 = new EnumItem();
            statusItem5.Value = "E";
            statusItem5.Caption = new LocaleValue("赢单");
            lstBillStatusItems.Add(statusItem5);

            EnumItem statusItem6 = new EnumItem();
            statusItem6.Value = "F";
            statusItem6.Caption = new LocaleValue("输单");
            lstBillStatusItems.Add(statusItem6);

            EnumItem statusItem7 = new EnumItem();
            statusItem7.Value = "G";
            statusItem7.Caption = new LocaleValue("执行中");
            lstBillStatusItems.Add(statusItem7);

            EnumItem statusItem8 = new EnumItem();
            statusItem8.Value = "H";
            statusItem8.Caption = new LocaleValue("评估中");
            lstBillStatusItems.Add(statusItem8);

            ComboFieldEditor comFieldEditor3 = this.View.GetControl<ComboFieldEditor>("F_xy_BillStatus");
            comFieldEditor3.SetComboItems(lstBillStatusItems);
            this.View.Model.SetValue("F_xy_BillStatus", "C");


            
        }
    }
}
