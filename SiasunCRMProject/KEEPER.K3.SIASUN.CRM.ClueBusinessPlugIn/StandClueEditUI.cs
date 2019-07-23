using KEEPER.K3.CRM.Core.Bump;
using KEEPER.K3.CRM.Entity;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS.Core.Bill.PlugIn.Args;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.List;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.CRM.Core;
using Kingdee.K3.CRM.Core.Bump;
using Kingdee.K3.CRM.Entity;
using Kingdee.K3.CRM.ServiceHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KEEPER.K3.SIASUN.CRM.ClueBusinessPlugIn
{
    [Description("标准产品线索插件反编译-表单插件")]
    public class StandClueEditUI: CRMBaseBillPlugIn
    {
        // Fields
        public IDynamicFormView _ActivityView;
        public string _viewPageId = "";
        private bool is_bumped;

        // Methods
        private void AddActivityForm()
        {
            this._ActivityView = base.View.GetView(this._viewPageId);
            if (this._ActivityView != null)
            {
                this._ActivityView.Close();
                base.View.SendDynamicFormAction(this._ActivityView);
            }
            ListShowParameter param = new ListShowParameter
            {
                FormId = "CRM_ACTIVITY",
                OpenStyle = {
                TagetKey = "FActPanel",
                ShowType = ShowType.InContainer
            },
                PageId = this._viewPageId = Guid.NewGuid().ToString(),
                ParentPageId = base.View.PageId,
                IsShowQuickFilter = false
            };
            string str = "";
            if (base.View.Model.GetPKValue() != null)
            {
                str = base.View.Model.GetPKValue().ToString();
            }
            param.CustomParams.Add("ClueID", str);
            base.View.ShowForm(param);
        }

        public override void AfterBindData(EventArgs e)
        {
            base.AfterBindData(e);
            this.AddActivityForm();
        }

        public override void AfterCreateNewData(EventArgs e)
        {
            long num = CommonServiceHelper.GetSellerIdFromUserId(base.Context, base.Context.CurrentOrganizationInfo.ID, base.Context.UserId);
            if (num != 0L)
            {
                base.View.Model.SetValue("FSalerId", num);
                base.View.InvokeFieldUpdateService("FSalerId", 0);
            }
            if ((base.View.ParentFormView != null) && (base.View.ParentFormView.OpenParameter != null))
            {
                object customParameter = base.View.ParentFormView.OpenParameter.GetCustomParameter("MarketingID");
                if (customParameter != null)
                {
                    base.View.Model.SetValue("FMARACTIVITYID", customParameter.ToString());
                    base.View.InvokeFieldUpdateService("FMARACTIVITYID", 0);
                }
                object obj3 = base.View.ParentFormView.OpenParameter.GetCustomParameter("WXMarketingID");
                if (obj3 != null)
                {
                    base.View.Model.SetValue("FWeiXinMarketingId", obj3.ToString());
                    base.View.InvokeFieldUpdateService("FWeiXinMarketingId", 0);
                }
            }
        }

        public override void AfterSave(AfterSaveEventArgs e)
        {
            base.AfterSave(e);
            if (base.View.OpenParameter.Status == OperationStatus.ADDNEW)
            {
                this.AddActivityForm();
            }
        }

        public override void BarItemClick(BarItemClickEventArgs e)
        {
            string str2;
            base.BarItemClick(e);
            if (((str2 = e.BarItemKey.ToUpperInvariant()) != null) && (str2 == "TBBUMPSETTING"))
            {
                string id = base.View.BusinessInfo.GetForm().Id;
                if (!CommonHelper.CheckPermission(base.Context, id, "545c77b3761524"))
                {
                    base.View.ShowMessage(ResManager.LoadKDString("您没有撞单设置的权限！", "006008030001275", SubSystemType.CRM, new object[0]), MessageBoxType.Notice);
                }
                else
                {
                    BillShowParameter param = new BillShowParameter
                    {
                        FormId = "CRM_BumpAnalysisSetting",
                        OpenStyle = { ShowType = ShowType.Modal },
                        PageId = SequentialGuid.NewGuid().ToString(),
                        CustomParams = { {
                        "FormID",
                        id
                    } },
                        CustomComplexParams = { {
                        "EnableExpdate",
                        false
                    } }
                    };
                    List<string> list = new string[] { "FBillHead", "FCRM_Clue_Cust", "FCRM_Clue_Contact" }.ToList<string>();
                    param.CustomComplexParams.Add("FormTables", list);
                    base.View.ShowForm(param);
                }
            }
        }

        public override void BeforeF7Select(BeforeF7SelectEventArgs e)
        {
            string str2 = e.FieldKey.ToUpperInvariant();
            if (str2 != null)
            {
                if (str2 != "FCUSTOMERNAME")
                {
                    if (str2 != "FCONTACTNAME")
                    {
                        return;
                    }
                }
                else
                {
                    this.GetF8Value("CRM_CUST", "FCustomerName", "FCustomerID", "");
                    long lCustid = 0L;
                    DynamicObject obj2 = (DynamicObject)base.View.Model.GetValue("FCustomerID");
                    if ((obj2 != null) && (obj2["Id"] != null))
                    {
                        lCustid = Convert.ToInt64(obj2["Id"]);
                    }
                    this.GetCustInfo(lCustid);
                    return;
                }
                long num2 = 0L;
                DynamicObject obj3 = (DynamicObject)base.View.Model.GetValue("FCustomerID");
                if ((obj3 != null) && (obj3["Id"] != null))
                {
                    num2 = Convert.ToInt64(obj3["Id"]);
                }
                string strFilter = "1=0";
                if (num2 > 0L)
                {
                    strFilter = $" EXISTS(SELECT 1 FROM T_CRM_CONTACT WHERE T0.FCONTACTID=T_CRM_CONTACT.FCONTACTID AND T_CRM_CONTACT.FCUSTOMERID={num2})";
                }
                this.GetF8Value("CRM_CUST_Contact", "FContactName", "FContactID", strFilter);
                long lCONTACTID = 0L;
                DynamicObject obj4 = (DynamicObject)base.View.Model.GetValue("FContactID");
                if ((obj4 != null) && (obj4["Id"] != null))
                {
                    lCONTACTID = Convert.ToInt64(obj4["Id"]);
                }
                this.GetContactInfo(lCONTACTID);
            }
        }

        public override void BeforeSave(BeforeSaveEventArgs e)
        {
            base.BeforeSave(e);
            if (base.View.Model.GetValue("FEMail") != null)
            {
                string str = base.View.Model.GetValue("FEMail").ToString().Trim();
                if ((str.Length > 0) && !StringHelper.IsEmail(str))
                {
                    base.View.ShowMessage(ResManager.LoadKDString("电子邮箱地址不合法！", "006021030001076", SubSystemType.CRM, new object[0]), MessageBoxType.Notice);
                    e.Cancel = true;
                    return;
                }
            }
            if (!this.is_bumped)
            {
                this.bump(e);
            }
            else
            {
                this.is_bumped = !this.is_bumped;
            }
        }

        public override void BeforeSetItemValueByNumber(BeforeSetItemValueByNumberArgs e)
        {
            string str;
            if (((str = e.BaseDataFieldKey.ToUpper()) != null) && (((str == "FCUSTOMERID") || (str == "FCONTACTID")) || ((str == "FMARACTIVITYID") || (str == "FWEIXINMARKETINGID"))))
            {
                e.IsShowApproved = false;
            }
            base.BeforeSetItemValueByNumber(e);
        }

        public void bump(BeforeSaveEventArgs e)
        {
            Action<FormResult> action = null;
            IKEEPERBumpAnalysisCommon bumpCommon = KEEPERBumpAnalysisFactory.CreateBumpAnalysis(base.Context, base.View.Model, this.Model.BusinessInfo.GetForm().Id);
            if (bumpCommon.IsShowResult)
            {
                BillShowParameter param = new BillShowParameter
                {
                    FormId = "CRM_BumpAnalysisShow",
                    OpenStyle = { ShowType = ShowType.Modal },
                    PageId = SequentialGuid.NewGuid().ToString(),
                    CustomComplexParams = { {
                    "ResultEntrity",
                    bumpCommon.ResultEntrity
                } }
                };
                e.Cancel = true;
                if (action == null)
                {
                    action = delegate (FormResult result) {
                        if (result.ReturnData != null)
                        {
                            if (((bool)result.ReturnData) && !bumpCommon.IsAllowSave)
                            {
                                this.View.ShowMessage(ResManager.LoadKDString("撞单后单据不允许保存", "006021030001299", SubSystemType.CRM, new object[0]), MessageBoxType.Notice);
                            }
                            else if (((bool)result.ReturnData) && bumpCommon.IsAllowSave)
                            {
                                this.is_bumped = true;
                                this.View.InvokeFormOperation(FormOperationEnum.Save);
                            }
                        }
                    };
                }
                base.View.ShowForm(param, action);
            }
        }

        public override void DataChanged(DataChangedEventArgs e)
        {
            string str3 = e.Field.Key.ToUpperInvariant();
            if (str3 != null)
            {
                if (str3 != "FCUSTOMERNAME")
                {
                    if (str3 != "FCONTACTNAME")
                    {
                        return;
                    }
                }
                else
                {
                    base.View.Model.SetValue("FContactName", "");
                    base.View.Model.SetValue("FContactID", 0);
                    string str = "";
                    if (base.View.Model.GetValue("FCustomerName") != null)
                    {
                        str = base.View.Model.GetValue("FCustomerName").ToString().Trim();
                    }
                    if (str.Length == 0)
                    {
                        base.View.Model.SetValue("FCustomerID", 0);
                        return;
                    }
                    long customerIDByName = ClueServiceHelper.GetCustomerIDByName(base.Context, str);
                    base.View.Model.SetValue("FCustomerID", customerIDByName);
                    this.GetCustInfo(customerIDByName);
                    return;
                }
                long num2 = 0L;
                DynamicObject obj2 = (DynamicObject)base.View.Model.GetValue("FCustomerID");
                if ((obj2 != null) && (obj2["Id"] != null))
                {
                    num2 = Convert.ToInt64(obj2["Id"]);
                }
                if (num2 == 0L)
                {
                    base.View.Model.SetValue("FContactID", 0);
                }
                else
                {
                    string str2 = "";
                    if (base.View.Model.GetValue("FContactName") != null)
                    {
                        str2 = base.View.Model.GetValue("FContactName").ToString().Trim();
                    }
                    if (str2.Length == 0)
                    {
                        base.View.Model.SetValue("FContactID", 0);
                    }
                    else
                    {
                        long num3 = ClueServiceHelper.GetContactIDByName(base.Context, num2, str2);
                        base.View.Model.SetValue("FContactID", num3);
                        this.GetContactInfo(num3);
                    }
                }
            }
        }

        private void GetContactInfo(long lCONTACTID)
        {
            List<SelectorItemInfo> selector = new List<SelectorItemInfo> {
            new SelectorItemInfo("FTel"),
            new SelectorItemInfo("FEMail"),
            new SelectorItemInfo("FDuty"),
            new SelectorItemInfo("FMobile")
        };
            OQLFilter ofilter = OQLFilter.CreateHeadEntityFilter($"FCONTACTID={lCONTACTID}");
            DynamicObject[] objArray = BusinessDataServiceHelper.Load(base.Context, "CRM_CUST_Contact", selector, ofilter);
            string str = "";
            string str2 = "";
            string str3 = "";
            string str4 = "";
            if ((objArray != null) && (objArray.Length > 0))
            {
                DynamicObject obj2 = objArray[0];
                str = obj2["FTel"].ToString();
                str3 = obj2["FEMail"].ToString();
                str2 = obj2["Mobile"].ToString();
                DynamicObjectCollection objects = obj2["T_CRM_Contact"] as DynamicObjectCollection;
                if ((objects != null) && (objects.Count > 0))
                {
                    DynamicObject obj3 = objects[0];
                    if (obj3["FDuty"] != null)
                    {
                        str4 = obj3["FDuty"].ToString();
                    }
                }
                base.View.Model.SetValue("FPhone", str);
                base.View.Model.SetValue("FMobile", str2);
                base.View.Model.SetValue("FEmail", str3);
                base.View.Model.SetValue("FDuty", str4);
            }
        }

        private void GetCustInfo(long lCustid)
        {
            List<SelectorItemInfo> selector = new List<SelectorItemInfo> {
            new SelectorItemInfo("FWEBSITE"),
            new SelectorItemInfo("FFAX"),
            new SelectorItemInfo("FADDRESS")
        };
            OQLFilter ofilter = OQLFilter.CreateHeadEntityFilter($"FCustID={lCustid}");
            DynamicObject[] objArray = BusinessDataServiceHelper.Load(base.Context, "CRM_CUST", selector, ofilter);
            string str = "";
            string str2 = "";
            string str3 = "";
            if ((objArray != null) && (objArray.Length > 0))
            {
                DynamicObject obj2 = objArray[0];
                str = obj2["WEBSITE"].ToString();
                str2 = obj2["FAX"].ToString();
                str3 = obj2["ADDRESS"].ToString();
                base.View.Model.SetValue("FHomePage", str);
                base.View.Model.SetValue("FFax", str2);
                base.View.Model.SetValue("FAddress", str3);
            }
        }

        private void GetF8Value(string strFormId, string strFieldOfName, string strFieldOfId, string strFilter)
        {
            ListShowParameter param = new ListShowParameter
            {
                FormId = strFormId,
                MultiSelect = false,
                IsShowApproved = false,
                IsShowUsed = true,
                IsLookUp = true
            };
            FormMetadata metadata = (FormMetadata)MetaDataServiceHelper.Load(base.Context, strFormId, true);
            BusinessInfo businessInfo = metadata.BusinessInfo;
            string str = CRMAllocationServiceHelper.GetFilter(base.Context, businessInfo, param.SqlParams);
            if (str.Length > 0)
            {
                if (strFilter.Length > 0)
                {
                    strFilter = strFilter + " AND " + str;
                }
                else
                {
                    strFilter = str;
                }
            }
            param.ListFilterParameter.Filter = strFilter;
            param.PageId = Guid.NewGuid().ToString();
            param.ParentPageId = base.View.PageId;
            param.ListType = 2;
            base.View.ShowForm(param, delegate (FormResult result) {
                if (result.ReturnData != null)
                {
                    ListSelectedRowCollection returnData = result.ReturnData as ListSelectedRowCollection;
                    if (returnData.Count != 0)
                    {
                        ListSelectedRow row = returnData[0];
                        this.View.Model.SetValue(strFieldOfId, row.PrimaryKeyValue);
                        this.View.Model.BeginIniti();
                        this.View.Model.SetValue(strFieldOfName, row.Name);
                        this.View.Model.EndIniti();
                        this.View.UpdateView(strFieldOfName);
                        if (strFieldOfName.ToUpperInvariant() == "FCUSTOMERNAME")
                        {
                            this.GetCustInfo(Convert.ToInt64(row.PrimaryKeyValue));
                        }
                        else if (strFieldOfName.ToUpperInvariant() == "FCONTACTNAME")
                        {
                            this.GetContactInfo(Convert.ToInt64(row.PrimaryKeyValue));
                        }
                    }
                }
            });
        }
    }
}
