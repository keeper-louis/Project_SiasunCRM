using Kingdee.BOS;
using Kingdee.BOS.Core.CommonFilter.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm.PlugIn.ControlModel;
using Kingdee.BOS.Core.Metadata;
using System;
using System.Collections.Generic;

namespace Siasun.K3.CRM.OPP.App.Report.OppOperatorStatReport
{
  public   class DeptIsolationOrg : AbstractCommonFilterPlugIn
    {
        public override void BeforeF7Select(BeforeF7SelectEventArgs e)
        {
            base.BeforeF7Select(e);
        }
        public override void AuthPermissionBeforeF7Select(AuthPermissionBeforeF7SelectEventArgs e)
        {
            base.AuthPermissionBeforeF7Select(e);
            e.IsIsolationOrg = false;//是否进行组织隔离
        }
    }
}
