using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.K3.CRM.Entity;
using Kingdee.BOS.Core.Bill;
using Kingdee.BOS;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Core.Metadata.FieldElement;
using System.Collections;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Util;
using KEEPER.K3.CRM.Entity;

namespace KEEPER.K3.CRM.Core.Bump
{
    public abstract class KEEPERBumpAnalysisBase : IKEEPERBumpAnalysisCommon
    {
        // Fields
        private IBillModel billmodel;
        private Context m_ctx;

        // Methods
        public KEEPERBumpAnalysisBase(Context ctx, IBillModel BillModel)
        {
            this.m_ctx = ctx;
            this.billmodel = BillModel;
            this.Bump();
        }

        protected virtual void Bump()
        {
            throw new NotImplementedException();
        }

        public BumpAnalysisResultEntrity Get_Bumpfields(string FormID, FormMetadata entitryMeta)
        {
            BumpAnalysisResultEntrity entrity = new BumpAnalysisResultEntrity();
            entrity.BumpAnalysisFields = new Dictionary<string, BumpAnalysisFields>();
            string strFilter = $"FBumpFormID='{FormID}'";
            DynamicObject[] objArray = BusinessDataServiceHelper.Load(this.Context, "CRM_BumpAnalysisSettingEntity", null, OQLFilter.CreateHeadEntityFilter(strFilter));
            if (objArray.Length == 0)
            {
                return null;
            }
            DynamicObjectCollection objects = (objArray != null) ? ((DynamicObjectCollection)objArray[0]["FEntity"]) : null;
            List<Field> list = new List<Field>();
            List<FieldAppearance> list2 = new List<FieldAppearance>();
            List<BumpTypeField> list3 = new List<BumpTypeField>();
            if (entrity.ParaFields == null)
            {
                entrity.ParaFields = new Hashtable();
            }
            string str2 = "";
            foreach (DynamicObject obj2 in objects)
            {
                if (obj2["settype"].ToString() == "1")
                {
                    if ((bool)obj2["BUMPSHOW"])
                    {
                        string key = obj2["FIELDNAME"].ToString();
                        Field item = entitryMeta.BusinessInfo.GetField(key);
                        if (item != null)
                        {
                            list.Add(item);
                        }
                        FieldAppearance fieldAppearance = entitryMeta.GetLayoutInfo().GetFieldAppearance(key);
                        if (fieldAppearance != null)
                        {
                            list2.Add(fieldAppearance);
                        }
                        if (item != null)
                        {
                            string str4 = item.EntityKey + "_" + item.PropertyName;
                            if (!entrity.BumpAnalysisFields.ContainsKey(str4))
                            {
                                BumpAnalysisFields fields = new BumpAnalysisFields(str4, new BumpTypeField(item), fieldAppearance, true, false);
                                entrity.BumpAnalysisFields.Add(str4, fields);
                            }
                            else
                            {
                                entrity.BumpAnalysisFields[str4].BumpFields = new BumpTypeField(item);
                                entrity.BumpAnalysisFields[str4].LayoutInfoAppearance = fieldAppearance;
                                entrity.BumpAnalysisFields[str4].IsShowField = true;
                            }
                        }
                    }
                    if ((bool)obj2["SELECTED"])
                    {
                        string str5 = obj2["FIELDNAME"].ToString();
                        string str6 = this.GetFieldData(this.BillModel.GetValue(str5)).Replace("'", "''");
                        string str7 = obj2["Matching"].ToString();
                        if (!string.IsNullOrEmpty(str6))
                        {
                            str2 = str2 + "or " + str5 + ((str7 == "100") ? ("='" + str6.ToString() + "' ") : (" LIKE '%" + str6.ToString() + "%' "));
                        }
                        Field field = entitryMeta.BusinessInfo.GetField(str5);
                        if (field != null)
                        {
                            list3.Add(new BumpTypeField(field, str7));
                        }
                        if (field != null)
                        {
                            string str8 = field.EntityKey + "_" + field.PropertyName;
                            FieldAppearance appearance2 = entitryMeta.GetLayoutInfo().GetFieldAppearance(str5);
                            if (!entrity.BumpAnalysisFields.ContainsKey(str8))
                            {
                                BumpAnalysisFields fields2 = new BumpAnalysisFields(str8, new BumpTypeField(field, str7), appearance2, false, true);
                                entrity.BumpAnalysisFields.Add(str8, fields2);
                            }
                            else
                            {
                                entrity.BumpAnalysisFields[str8].BumpFields = new BumpTypeField(field, str7);
                                entrity.BumpAnalysisFields[str8].IsBumpField = true;
                            }
                        }
                    }
                }
                else if ((obj2["settype"].ToString() == "2") && ((bool)obj2["selected"]))
                {
                    string str9 = obj2["FIELDNAME"].ToString();
                    string str10 = obj2["Matching"].ToString();
                    entrity.ParaFields.Add(str9, str10);
                }
            }
            entrity.BumpFields = list3;
            entrity.BusinessInfoField = list;
            entrity.LayoutInfoAppearance = list2;
            return entrity;
        }

        public string GetFieldData(object value)
        {
            if (value != null)
            {
                if (value is DynamicObject)
                {
                    return ((DynamicObject)value)["Id"].ToString();
                }
                if (value is LocaleValue)
                {
                    return ((LocaleValue)value)[this.Context.UserLocale.LCID];
                }
                if (!(value is DynamicObjectCollection))
                {
                    return value.ToString();
                }
                string str = "";
                foreach (DynamicObject obj2 in (DynamicObjectCollection)value)
                {
                    str = str + this.GetFieldData(obj2[obj2.DynamicObjectType.ToString()]) + ",";
                }
                if (!string.IsNullOrEmpty(str))
                {
                    return str.Substring(0, str.Length - 1);
                }
            }
            return "";
        }

        public string GetReslutFileter(BumpAnalysisResultEntrity ResultEntrity, FormMetadata entitryMeta)
        {
            string str8;
            string str = "";
            foreach (BumpTypeField field in ResultEntrity.BumpFields)
            {
                string key = field.field.FieldName;
                string str3 = this.GetFieldData(this.BillModel.GetValue(key)).Trim();
                string str4 = field.matching;
                if (!string.IsNullOrEmpty(str3))
                {
                    str = str + "or " + key + ((str4 == "100") ? ("='" + str3.ToString() + "' ") : (" LIKE '%" + str3.ToString() + "%' "));
                }
            }
            if (str != "")
            {
                str = "(" + str.Substring(2, str.Length - 2) + ") ";
            }
            string fieldName = entitryMeta.BusinessInfo.GetBillNoField().FieldName;
            string fieldData = this.GetFieldData(this.BillModel.GetValue(fieldName));
            if (fieldData.Trim().Length > 0)
            {
                fieldData = fieldData.Replace("'", "''");
                string str7 = str;
                str = str7 + " and " + fieldName + "<>'" + fieldData + "'";
            }
            if ((ResultEntrity.ParaFields["FExpdate"] == null) || ((str8 = entitryMeta.Id) == null))
            {
                return str;
            }
            if (str8 != "CRM_CUST")
            {
                if (str8 != "CRM_OPP_Opportunity")
                {
                    return str;
                }
            }
            else
            {
                return (str + $" and (DATEDIFF(D,FLastContactDate,GETDATE())<{Convert.ToDouble(ResultEntrity.ParaFields["FExpdate"])} or FLastContactDate is null)");
            }
            return (str + $" and (DATEDIFF(D,flastcondate,GETDATE())<{Convert.ToDouble(ResultEntrity.ParaFields["FExpdate"])} or flastcondate is null)");
        }

        public bool isBump(BumpAnalysisResultEntrity ResultEntrity)
        {
            bool flag = false;
            if ((ResultEntrity == null) || (ResultEntrity.BumpFields.Count == 0))
            {
                return false;
            }
            IEnumerable<IDataEntityProperty> dirtyProperties = this.BillModel.DataObject.DataEntityState.GetDirtyProperties();
            using (List<BumpTypeField>.Enumerator enumerator = ResultEntrity.BumpFields.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Func<IDataEntityProperty, bool> predicate = null;
                    BumpTypeField field = enumerator.Current;
                    if (predicate == null)
                    {
                        predicate = p => p.Name.Trim().ToUpper() == field.field.DynamicProperty.Name.Trim().ToUpper();
                    }
                    IEnumerable<IDataEntityProperty> enumerable2 = dirtyProperties.Where<IDataEntityProperty>(predicate);
                    if (!enumerable2.IsNullOrEmpty() && (enumerable2.Count<IDataEntityProperty>() > 0))
                    {
                        this.isdirtybump = true;
                        return true;
                    }
                }
            }
            if (this.BillModel.DataObject.DynamicObjectType.Properties.Contains("MultiLanguageText"))
            {
                LocalDynamicObjectCollection objects = (LocalDynamicObjectCollection)this.BillModel.DataObject["MultiLanguageText"];
                if ((objects == null) || (objects.Count <= 0))
                {
                    return flag;
                }
                foreach (DynamicObject obj2 in objects)
                {
                    dirtyProperties = obj2.DataEntityState.GetDirtyProperties();
                    using (List<BumpTypeField>.Enumerator enumerator3 = ResultEntrity.BumpFields.GetEnumerator())
                    {
                        while (enumerator3.MoveNext())
                        {
                            Func<IDataEntityProperty, bool> func2 = null;
                            BumpTypeField field = enumerator3.Current;
                            if (func2 == null)
                            {
                                func2 = p => p.Name.Trim().ToUpper() == field.field.DynamicProperty.Name.Trim().ToUpper();
                            }
                            IEnumerable<IDataEntityProperty> enumerable3 = dirtyProperties.Where<IDataEntityProperty>(func2);
                            if (!enumerable3.IsNullOrEmpty() && (enumerable3.Count<IDataEntityProperty>() > 0))
                            {
                                this.isdirtybump = true;
                                return true;
                            }
                        }
                    }
                }
            }
            return flag;
        }

        public void SetBumpData(string FormID, FormMetadata entitryMeta)
        {
            this.ResultEntrity = this.Get_Bumpfields(FormID, entitryMeta);
            if (this.isBump(this.ResultEntrity))
            {
                string reslutFileter = this.GetReslutFileter(this.ResultEntrity, entitryMeta);
                if ((this.ResultEntrity.ParaFields["FOpenMsgSave"] != null) && (this.ResultEntrity.ParaFields["FOpenMsgSave"].ToString() == "1"))
                {
                    this.IsAllowSave = true;
                }
                this.SetData(entitryMeta, reslutFileter);
                this.IsShowResult = this.ResultEntrity.DataValue.Count > 0;
            }
        }

        public void SetData(FormMetadata entitryMeta, string str_result_Filter)
        {
            new List<DynamicObject>();
            DynamicObject[] objArray = BusinessDataServiceHelper.Load(this.Context, entitryMeta.BusinessInfo, null, OQLFilter.CreateHeadEntityFilter(str_result_Filter));
            this.ResultEntrity.DataValue = new List<DynamicObject>();
            Dictionary<string, Hashtable> dictionary = new Dictionary<string, Hashtable>();
            this.ResultEntrity.DataValue = new List<DynamicObject>();
            foreach (DynamicObject obj2 in objArray)
            {
                string key = obj2["ID"].ToString();
                bool flag = false;
                bool flag2 = true;
                double num = 0.0;
                bool flag3 = false;
                Hashtable hashtable = new Hashtable();
                foreach (BumpTypeField field in this.ResultEntrity.BumpFields)
                {
                    string fieldName = field.field.FieldName;
                    double num2 = Convert.ToDouble(field.matching);
                    object obj3 = this.BillModel.GetValue(fieldName);
                    string str3 = (obj3 == null) ? "" : this.GetFieldData(obj3);
                    string fieldData = "";
                    if (field.field.EntityKey == "FBillHead")
                    {
                        fieldData = this.GetFieldData(obj2[field.field.PropertyName]);
                    }
                    else
                    {
                        DynamicObject obj4 = ((DynamicObjectCollection)obj2[field.field.Entity.DynamicProperty.Name])[0];
                        fieldData = this.GetFieldData(obj4[field.field.PropertyName]);
                    }
                    double num3 = ((fieldData.Length == 0) || (fieldData.IndexOf(str3) < 0)) ? 0.0 : (((double)str3.Length) / ((double)fieldData.Length));
                    double num4 = Math.Round(num3, 4) * 100.0;
                    num += num4;
                    string str5 = num4.ToString() + "%";
                    if ((str3.Trim().Length == 0) || (fieldData.Trim().Length == 0))
                    {
                        str5 = "0%";
                    }
                    hashtable.Add(field.field.EntityKey + "_" + field.field.PropertyName, str5);
                    if (num4 == 100.0)
                    {
                        flag3 = true;
                    }
                    if ((num3 * 100.0) >= num2)
                    {
                        flag = true;
                    }
                }
                num = (this.ResultEntrity.BumpFields.Count == 0) ? 0.0 : (num / ((double)this.ResultEntrity.BumpFields.Count));
                if ((this.ResultEntrity.ParaFields["FAllMatching"] != null) && (num < Convert.ToDouble(this.ResultEntrity.ParaFields["FAllMatching"])))
                {
                    flag2 = false;
                }
                if (this.ResultEntrity.ParaFields["FoneMatching"] == null)
                {
                    flag3 = true;
                }
                if (flag && (flag2 || flag3))
                {
                    dictionary.Add(key, hashtable);
                    this.ResultEntrity.DataValue.Add(obj2);
                }
            }
            this.ResultEntrity.DicMacthDesc = dictionary;
        }

        // Properties
        protected IBillModel BillModel =>
            this.billmodel;

        protected Context Context =>
            this.m_ctx;

        public virtual bool IsAllowSave { get; set; }

        public bool isdirtybump { get; set; }

        public virtual bool IsShowResult { get; set; }

        public virtual BumpAnalysisResultEntrity ResultEntrity { get; set; }
    }
    
}
