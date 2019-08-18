using Kingdee.BOS.Core.Metadata.FieldElement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KEEPER.K3.CRM.Entity
{
    public class KEEPERBumpAnalysisFields
    {
        // Fields
        public string ID;
        public bool IsBumpField;
        public bool IsShowField;

        // Methods
        public KEEPERBumpAnalysisFields(string ID, KEEPERBumpTypeField BumpFields, FieldAppearance LayoutInfoAppearance, bool IsShowField, bool IsBumpField)
        {
            this.ID = ID;
            this.LayoutInfoAppearance = LayoutInfoAppearance;
            this.IsShowField = IsShowField;
            this.IsBumpField = IsBumpField;
            this.BumpFields = BumpFields;
        }

        // Properties
        public KEEPERBumpTypeField BumpFields { get; set; }

        public FieldAppearance LayoutInfoAppearance { get; set; }
    }
}
