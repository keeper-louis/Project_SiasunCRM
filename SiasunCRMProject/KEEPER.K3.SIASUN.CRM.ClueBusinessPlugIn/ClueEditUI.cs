using Kingdee.BOS.Core.Bill.PlugIn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.App.Data;

namespace KEEPER.K3.SIASUN.CRM.ClueBusinessPlugIn
{
    [Description("线索界面插件")]
    public class ClueEditUI:AbstractBillPlugIn
    {
        
        //项目进展过滤
        public override void BeforeF7Select(BeforeF7SelectEventArgs e)
        {
            base.BeforeF7Select(e);
            if (e.FieldKey.ToUpper().Equals("F_PEJK_PROPROCESS"))
            {
                //项目进展按照产品进行过滤
                DynamicObject proCode = this.Model.GetValue("F_PEJK_CRMProCode", e.Row) as DynamicObject;
                if (proCode ==  null)
                {
                    return;
                }
                DynamicObjectCollection ProProcess = proCode["PEJK_Cust_CRMPROENTRY"] as DynamicObjectCollection;
                if (ProProcess == null ||ProProcess.Count() == 0)
                {
                    return;
                }
                List<long> ProprocessIds = new List<long>();
                foreach (var item in ProProcess)
                {
                    ProprocessIds.Add(Convert.ToInt64(item["F_PEJK_ProProcess_Id"]));
                }
                string str = string.Join(",", ProprocessIds);
                string filter = string.Format(" FID IN ({0})", str);
                if (string.IsNullOrEmpty(e.ListFilterParameter.Filter))
                {
                    e.ListFilterParameter.Filter = filter;
                }
                else
                {
                    filter = " And " + filter;
                    e.ListFilterParameter.Filter += filter;
                }
            }
            
        }

        //值更新
        public override void DataChanged(DataChangedEventArgs e)
        {
            base.DataChanged(e);
            if (e.Field.Key.Equals("F_PEJK_Province"))
            {
                string strSql = string.Format(@"/*dialect*/select FPARENTID from T_BAS_ASSISTANTDATAENTRY  where FMASTERID = '{0}'", e.NewValue);
                string parentid = DBUtils.ExecuteScalar<string>(this.Context, strSql, "", null);
                if (parentid.Equals(""))
                {
                    return;
                }
                this.Model.SetItemValueByID("F_PEJK_Region", parentid, 0);
            }
            if (e.Field.Key.Equals("FSalerId"))
            {
                DynamicObject salerObject = this.Model.GetValue("FSalerId") as DynamicObject;
                long depId = Convert.ToInt64(salerObject["DeptId_Id"]);
                string strSql = string.Format(@"/*dialect*/SELECT FDEPTH FROM T_BD_DEPARTMENT WHERE FDEPTID = {0}",depId);
                int depth = DBUtils.ExecuteScalar<int>(base.Context, strSql, 0, null);
                if (depth == 4)
                 {
                    string parentSql = string.Format(@"/*dialect*/select FPARENTID from T_BD_DEPARTMENT where fdeptid = {0}", depId);
                    long parentID = DBUtils.ExecuteScalar<long>(base.Context, parentSql, 0, null);
                    if (parentID>0)
                    {
                        this.Model.SetItemValueByID("FSALEDEPTID", parentID, 0);
                    }
                  }
                
            }
        }

        public override void AfterCreateNewData(EventArgs e)
        {
           base.AfterCreateNewData(e);
            DynamicObject  deptObject = this.Model.GetValue("FSALEDEPTID") as DynamicObject;
            if (deptObject!=null)
            {
                int dtpth = Convert.ToInt32(deptObject["Depth"]);//深度
                if ((DynamicObject)deptObject["ParentID"] != null && dtpth == 4)
                {
                    long parentId = Convert.ToInt64(deptObject["ParentID_Id"]);
                    this.Model.SetItemValueByID("FSALEDEPTID", parentId, 0);
                }
            }
            this.Model.SetValue("F_PAEZ_CheckBox", true);
        }

        
        public override void AuthPermissionBeforeF7Select(AuthPermissionBeforeF7SelectEventArgs e)
        {
            base.AuthPermissionBeforeF7Select(e);
            e.IsIsolationOrg = false;
        }
        
    }
}
