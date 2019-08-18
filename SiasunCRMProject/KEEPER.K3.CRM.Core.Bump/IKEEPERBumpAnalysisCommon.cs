using KEEPER.K3.CRM.Entity;
using Kingdee.K3.CRM.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KEEPER.K3.CRM.Core.Bump
{
    public interface IKEEPERBumpAnalysisCommon
    {
        // Properties
        bool IsAllowSave { get; }
        bool isdirtybump { get; set; }
        bool IsShowResult { get; }
        BumpAnalysisResultEntrity ResultEntrity { get; }
    }
}
