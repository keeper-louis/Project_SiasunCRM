using KEEPER.K3.CRM.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Orm.DataEntity;

namespace KEEPER.K3.App
{
    public class CRMService : ICRMService
    {
        /// <summary>
        /// 获取数据规则下项目信息id集合
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="personId"></param>
        /// <returns></returns>
        public List<long> getProjectIds(Context ctx, long personId)
        {

            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取数据规则下销售员id集合
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="personId"></param>
        /// <returns></returns>
        public List<long> getSalerPersonids(Context ctx, long personId)
        {
            List<long> salerPersonids = new List<long>();
            //判断personId是否在CRM汇报关系设置表中
            string strSql_1 = string.Format(@"/*dialect*/SELECT * FROM PEJK_RPTSHIP WHERE F_PEJK_MANAGER = {0}");
            DynamicObjectCollection  headCol = DBUtils.ExecuteDynamicObject(ctx, strSql_1);
            if (headCol == null || headCol.Count==0)
            {
                string strSql_2 = string.Format(@"/*dialect*/SELECT * FROM PEJK_RPTSHIPENTRY WHERE F_PEJK_TEAMMEMBER = {0}");
                DynamicObjectCollection entryCol = DBUtils.ExecuteDynamicObject(ctx, strSql_1);
                if (entryCol == null || entryCol.Count ==0)
                {
                    return null;
                }
            }
            string sql = string.Format(@"/*dialect*/SELECT B.F_PEJK_TEAMMEMBER
  FROM PEJK_RPTSHIP A
 INNER JOIN PEJK_RPTSHIPENTRY B
    ON A.FID = B.FID
 WHERE A.F_PEJK_MANAGER = {0}

UNION ALL

SELECT BB.F_PEJK_TEAMMEMBER
  FROM PEJK_RPTSHIP A
 INNER JOIN PEJK_RPTSHIPENTRY B
    ON A.FID = B.FID
 INNER JOIN PEJK_RPTSHIP AA
    ON B.F_PEJK_TEAMMEMBER = AA.F_PEJK_MANAGER
 INNER JOIN PEJK_RPTSHIPENTRY BB
    ON AA.FID = BB.FID
 WHERE A.F_PEJK_MANAGER = {0}

UNION ALL

SELECT BBB.F_PEJK_TEAMMEMBER
  FROM PEJK_RPTSHIP A
 INNER JOIN PEJK_RPTSHIPENTRY B
    ON A.FID = B.FID
 INNER JOIN PEJK_RPTSHIP AA
    ON B.F_PEJK_TEAMMEMBER = AA.F_PEJK_MANAGER
 INNER JOIN PEJK_RPTSHIPENTRY BB
    ON AA.FID = BB.FID
 INNER JOIN PEJK_RPTSHIP AAA
    ON BB.F_PEJK_TEAMMEMBER = AAA.F_PEJK_MANAGER
 INNER JOIN PEJK_RPTSHIPENTRY BBB
    ON AAA.FID = BBB.FID
 WHERE A.F_PEJK_MANAGER = {0}

UNION ALL

SELECT BBBB.F_PEJK_TEAMMEMBER
  FROM PEJK_RPTSHIP A
 INNER JOIN PEJK_RPTSHIPENTRY B
    ON A.FID = B.FID
 INNER JOIN PEJK_RPTSHIP AA
    ON B.F_PEJK_TEAMMEMBER = AA.F_PEJK_MANAGER
 INNER JOIN PEJK_RPTSHIPENTRY BB
    ON AA.FID = BB.FID
 INNER JOIN PEJK_RPTSHIP AAA
    ON BB.F_PEJK_TEAMMEMBER = AAA.F_PEJK_MANAGER
 INNER JOIN PEJK_RPTSHIPENTRY BBB
    ON AAA.FID = BBB.FID
 INNER JOIN PEJK_RPTSHIP AAAA
    ON BBB.F_PEJK_TEAMMEMBER = AAAA.F_PEJK_MANAGER
 INNER JOIN PEJK_RPTSHIPENTRY BBBB
    ON AAAA.FID = BBBB.FID
 WHERE A.F_PEJK_MANAGER = {0}
UNION ALL
SELECT {0}", personId);
            DynamicObjectCollection salerIds = DBUtils.ExecuteDynamicObject(ctx, sql);
            if (salerIds!=null&&salerIds.Count()>0)
            {
                foreach (var item in salerIds)
                {
                    salerPersonids.Add(Convert.ToInt64(item["F_PEJK_TEAMMEMBER"]));
                }
                return salerPersonids;
            }
            else
            {
                return null;
            }
        }
    }
}
