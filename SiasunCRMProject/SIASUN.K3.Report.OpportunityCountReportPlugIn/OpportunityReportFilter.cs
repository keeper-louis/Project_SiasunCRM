using Kingdee.BOS;
using Kingdee.BOS.Core.CommonFilter.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIASUN.K3.Report.OpportunityCountReportPlugIn
{
  public  class OpportunityReportFilter: AbstractCommonFilterPlugIn
    {
        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

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

            ComboFieldEditor comFieldEditor3 = this.View.GetControl<ComboFieldEditor>("F_PAEZ_BillStatus");
            comFieldEditor3.SetComboItems(lstBillStatusItems);
            this.View.Model.SetValue("F_xy_BillStatus", "C");
        }
    }
}
}
