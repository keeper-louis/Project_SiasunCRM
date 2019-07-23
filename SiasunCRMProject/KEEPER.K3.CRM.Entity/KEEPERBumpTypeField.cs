using Kingdee.BOS.Core.Metadata.FieldElement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KEEPER.K3.CRM.Entity
{
    public class KEEPERBumpTypeField
    {
        public KEEPERBumpTypeField(Field field)
        {
            this.field = field;
            this.matching = "100";
        }

        public KEEPERBumpTypeField(Field field, string matching)
        {
            this.field = field;
            this.matching = matching;
        }

        // Properties
        public Field field { get; set; }

        public string matching { get; set; }
    }
}
