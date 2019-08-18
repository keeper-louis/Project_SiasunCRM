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
            List<long> proIds = new List<long>();
            List<long> salerIds = getSalerPersonids(ctx, personId);
            if (salerIds == null)
            {
                return null;
            }
            string Ids = string.Join(",", salerIds);
            string strSql = string.Format(@"/*dialect*/SELECT DISTINCT AA.F_PEJK_PRONO FROM PEJK_SALECONTRACTS AA INNER JOIN PEJK_SALECONTRACTENTRY BB ON AA.FID = BB.FID WHERE BB.F_PEJK_SALER IN ({0})",Ids);
            DynamicObjectCollection col = DBUtils.ExecuteDynamicObject(ctx, strSql);
            if (col == null || col.Count() == 0)
            {
                return null;
            }
            foreach (var item in col)
            {
                proIds.Add(Convert.ToInt64(item["F_PEJK_PRONO"]));
            }
            return proIds;
            
        }

        /// <summary>
        /// 获取数据规则下销售员id集合
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="personId"></param>
        /// <returns></returns>
        public List<long> getSalerPersonids(Context ctx, long personId)
        {
            List<long> salerPersonIds = new List<long>();
            //判断personId是否在CRM汇报关系设置表中
            string strSql_1 = string.Format(@"/*dialect*/SELECT * FROM PEJK_RPTSHIP WHERE F_PEJK_MANAGER = {0}",personId);
            DynamicObjectCollection  headCol = DBUtils.ExecuteDynamicObject(ctx, strSql_1);
            if (headCol == null || headCol.Count==0)
            {
                string strSql_2 = string.Format(@"/*dialect*/SELECT * FROM PEJK_RPTSHIPENTRY WHERE F_PEJK_TEAMMEMBER = {0}",personId);
                DynamicObjectCollection entryCol = DBUtils.ExecuteDynamicObject(ctx, strSql_2);
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
            DynamicObjectCollection personIds = DBUtils.ExecuteDynamicObject(ctx, sql);
            if (personIds != null&& personIds.Count()>0)
            {
                foreach (var item in personIds)
                {
                    salerPersonIds.Add(Convert.ToInt64(item["F_PEJK_TEAMMEMBER"]));
                }
                string Ids = string.Join(",", salerPersonIds);
                string strSql = string.Format(@"/*dialect*/SELECT V.FID
  FROM T_BD_PERSON A
 INNER JOIN T_BD_STAFF B
    ON A.FPERSONID = B.FPERSONID
 INNER JOIN V_BD_SALESMAN V
    ON B.FSTAFFID = V.FSTAFFID
 WHERE A. FPERSONID IN ({0})
",Ids);
                DynamicObjectCollection salerIds = DBUtils.ExecuteDynamicObject(ctx, strSql);
                if (salerIds != null && salerIds.Count() > 0)
                {
                    salerPersonIds.Clear();
                    foreach (var item in salerIds)
                    {
                        salerPersonIds.Add(Convert.ToInt64(item["FID"]));
                    }
                    return salerPersonIds;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
