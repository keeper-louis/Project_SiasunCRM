using Kingdee.BOS.Core.Metadata.FieldElement;
using Kingdee.BOS.Orm.DataEntity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KEEPER.K3.CRM.Entity
{
    public class KEEPERBumpAnalysisResultEntrity
    {
        public Dictionary<string, KEEPERBumpAnalysisFields> BumpAnalysisFields { get; set; }

        public List<KEEPERBumpTypeField> BumpFields { get; set; }

        public List<Field> BusinessInfoField { get; set; }

        public List<DynamicObject> DataValue { get; set; }

        public Dictionary<string, Hashtable> DicMacthDesc { get; set; }

        public List<FieldAppearance> LayoutInfoAppearance { get; set; }

        public Hashtable ParaFields { get; set; }
    }
}
