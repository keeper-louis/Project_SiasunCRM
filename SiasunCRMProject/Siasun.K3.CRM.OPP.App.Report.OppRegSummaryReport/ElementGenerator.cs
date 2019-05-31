using System;
using Kingdee.BOS.Core.Report;
using System.Text;
using Kingdee.BOS;

namespace Siasun.K3.CRM.OPP.App.Report.OppRegSummaryReport
{
    internal class ElementGenerator
    {
        internal static string GetSqlByDimension(string dimension, string year, string billStatus, string tableName)
        {
            switch (dimension)
            {
                case "1": //按产业
                    return GetSqlByIndustry(year, billStatus, tableName);

                case "2": //按月份
                    return GetSqlByMonth(year, billStatus, tableName);

                case "3": //按省份
                    return GetSqlByProvince(year, billStatus, tableName);

                case "4": //按区域
                    return GetSqlByRegion(year, billStatus, tableName);

                default: //按产业
                    return GetSqlByIndustry(year, billStatus, tableName);
            }
        }

        private static string GetSqlByIndustry(string year, string billStatus, string tableName)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" select ROW_NUMBER() OVER(ORDER BY industryNO) FIDENTITYID,industryName,oppCount ");
            sql.Append(" into " + tableName);
            sql.Append(" from (");
            sql.Append("    select custInd.fnumber industryNO,custInd_l.FNAME industryName,isnull(custIndData.oppCount,0) oppCount ");
            sql.Append("    from PEJK_CUSTINDUSTRY custInd ");
            sql.Append("    inner join PEJK_CUSTINDUSTRY_L custInd_l on custInd.FID=custInd_l.FID and custInd.FDOCUMENTSTATUS='C' ");
            sql.Append("    left join ( ");
            sql.Append("    	select cust.F_PEJK_CUSTINDUSTRY,count(1) oppCount  ");
            sql.Append("    	from T_CRM_OPPORTUNITY opp ");
            sql.Append("    	inner join T_CRM_CLUE clue on opp.FSOURCEBILLNO=clue.FBILLNO and CHARINDEX(opp.FDOCUMENTSTATUS,'" + billStatus + "') > 0 ");
            sql.Append("       and YEAR(opp.F_PEJK_AUDITDATE)='" + year + "' ");
            sql.Append("    	inner join T_CRM_CLUE_CUST cust on cust.FID=clue.FID ");
            sql.Append("    	group by cust.F_PEJK_CUSTINDUSTRY) custIndData ");
            sql.Append("    on custInd.FID=custIndData.F_PEJK_CUSTINDUSTRY ");
            sql.Append(" ) tt ");
            return sql.ToString();
        }

        private static string GetSqlByMonth(string year, string billStatus, string tableName)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("/*dialect*/");
            sql.Append(" select  ROW_NUMBER() OVER(ORDER BY curYear) FIDENTITYID, ");
            sql.Append("         m1,m2,m3,m4,m5,m6,m7,m8,m9,m10,m11,m12, ");
            sql.Append(" 	     m1+m2+m3+m4+m5+m6+m7+m8+m9+m10+m11+m12 yearTotal  ");
            sql.Append(" into " + tableName);
            sql.Append(" from ( ");
            sql.Append(" 	select N'" + year + "' curYear, ");
            sql.Append("           ISNULL([1],0) m1,ISNULL([2],0) m2,ISNULL([3],0) m3,ISNULL([4],0) m4,ISNULL([5],0) m5,ISNULL([6],0) m6, ");
            sql.Append(" 		   ISNULL([7],0) m7,ISNULL([8],0) m8,ISNULL([9],0) m9,ISNULL([10],0) m10,ISNULL([11],0) m11,ISNULL([12],0) m12  ");
            sql.Append(" 	from ( ");
            sql.Append(" 		select MONTH(opp.F_PEJK_AUDITDATE) curmonth,count(1) opp_count from T_CRM_OPPORTUNITY opp ");
            sql.Append(" 		where YEAR(opp.F_PEJK_AUDITDATE)='"+ year + "' and CHARINDEX(opp.FDOCUMENTSTATUS,'" + billStatus + "') > 0 ");
            sql.Append(" 		group by MONTH(opp.F_PEJK_AUDITDATE) ");
            sql.Append(" 	) a ");
            sql.Append(" 	pivot ( ");
            sql.Append(" 		sum(opp_count) for a.curmonth in ([1],[2],[3],[4],[5],[6],[7],[8],[9],[10],[11],[12]) ");
            sql.Append(" 	) b ");
            sql.Append(" ) tt ");
            return sql.ToString();
        }

        private static string GetSqlByProvince(string year, string billStatus, string tableName)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" select ROW_NUMBER() OVER(ORDER BY provinceNO) FIDENTITYID,provinceName,oppCount");
            sql.Append(" into " + tableName);
            sql.Append(" from (");
            sql.Append("    select a.fnumber provinceNO,c.FDATAVALUE provinceName,isnull(realData.oppCount,0) oppCount ");
            sql.Append("    from T_BAS_ASSISTANTDATAENTRY a ");
            sql.Append("    inner join T_BAS_ASSISTANTDATA b on a.fid=b.fid and b.FNUMBER='provinces' and a.FDOCUMENTSTATUS='C' ");
            sql.Append("    inner join T_BAS_ASSISTANTDATAENTRY_L c on a.FENTRYID=c.FENTRYID and c.FLOCALEID='2052' ");
            sql.Append("    left join ( ");
            sql.Append("    	select sf.FDATAVALUE sf,count(1) oppCount  ");
            sql.Append("    	from T_CRM_OPPORTUNITY opp ");
            sql.Append("    	inner join T_CRM_CLUE clue on opp.FSOURCEBILLNO=clue.FBILLNO and CHARINDEX(opp.FDOCUMENTSTATUS,'" + billStatus + "') > 0 ");
            sql.Append("       and YEAR(opp.F_PEJK_AUDITDATE)='" + year + "' ");
            sql.Append("    	inner join T_BAS_ASSISTANTDATAENTRY_L sf on sf.FLOCALEID='2052' and sf.FENTRYID=clue.F_PEJK_PROVINCE ");
            sql.Append("    	group by sf.FDATAVALUE) realData  ");
            sql.Append("    on c.FDATAVALUE=realData.sf ");
            sql.Append(" ) tt ");
            return sql.ToString();
        }

        private static string GetSqlByRegion(string year, string billStatus, string tableName)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" select ROW_NUMBER() OVER(ORDER BY regionNO) FIDENTITYID,regionName,oppCount");
            sql.Append(" into " + tableName);
            sql.Append(" from (");
            sql.Append("    select a.fnumber regionNO,c.FDATAVALUE regionName,isnull(realData.oppCount,0) oppCount ");
            sql.Append("    from ");
            sql.Append("    T_BAS_ASSISTANTDATAENTRY a ");
            sql.Append("    inner join T_BAS_ASSISTANTDATA b on a.fid=b.fid and b.FNUMBER='Provincial' and a.FDOCUMENTSTATUS='C' ");
            sql.Append("    inner join T_BAS_ASSISTANTDATAENTRY_L c on a.FENTRYID=c.FENTRYID and c.FLOCALEID='2052' ");
            sql.Append("    left join ( ");
            sql.Append("    	select sf.FDATAVALUE sf,count(1) oppCount  ");
            sql.Append("    	from T_CRM_OPPORTUNITY opp ");
            sql.Append("    	inner join T_CRM_CLUE clue on opp.FSOURCEBILLNO=clue.FBILLNO and CHARINDEX(opp.FDOCUMENTSTATUS,'" + billStatus + "') > 0 ");
            sql.Append("    	and YEAR(opp.F_PEJK_AUDITDATE)='" + year + "' ");
            sql.Append("    	inner join T_BAS_ASSISTANTDATAENTRY_L sf on sf.FLOCALEID='2052' and sf.FENTRYID=clue.F_PEJK_REGION ");
            sql.Append("    	group by sf.FDATAVALUE) realData  ");
            sql.Append("    on c.FDATAVALUE=realData.sf  ");
            sql.Append(" ) tt ");
            return sql.ToString();
        }

        internal static ReportHeader GetHeaderByDimension(string dimension)
        {
            switch (dimension)
            {
                case "1": //按产业
                    return GetHeaderByIndustry();

                case "2": //按月份
                    return GetHeaderByMonth();

                case "3": //按省份
                    return GetHeaderByProvince();

                case "4": //按区域
                    return GetHeaderByRegion();

                default: //按产业
                    return GetHeaderByIndustry();
            }
        }

        private static ReportHeader GetHeaderByIndustry()
        {
            ReportHeader header = new ReportHeader();
            header.AddChild("industryName", new LocaleValue("产业"));
            header.AddChild("oppCount", new LocaleValue("数量"), SqlStorageType.SqlInt);
            return header;
        }

        private static ReportHeader GetHeaderByMonth()
        {
            ReportHeader header = new ReportHeader();
            header.AddChild("m1", new LocaleValue("1月"), SqlStorageType.SqlInt);
            header.AddChild("m2", new LocaleValue("2月"), SqlStorageType.SqlInt);
            header.AddChild("m3", new LocaleValue("3月"), SqlStorageType.SqlInt);
            header.AddChild("m4", new LocaleValue("4月"), SqlStorageType.SqlInt);
            header.AddChild("m5", new LocaleValue("5月"), SqlStorageType.SqlInt);
            header.AddChild("m6", new LocaleValue("6月"), SqlStorageType.SqlInt);
            header.AddChild("m7", new LocaleValue("7月"), SqlStorageType.SqlInt);
            header.AddChild("m8", new LocaleValue("8月"), SqlStorageType.SqlInt);
            header.AddChild("m9", new LocaleValue("9月"), SqlStorageType.SqlInt);
            header.AddChild("m10", new LocaleValue("10月"), SqlStorageType.SqlInt);
            header.AddChild("m11", new LocaleValue("11月"), SqlStorageType.SqlInt);
            header.AddChild("m12", new LocaleValue("12月"), SqlStorageType.SqlInt);
            header.AddChild("yearTotal", new LocaleValue("总计"), SqlStorageType.SqlInt);
            return header;
        }

        private static ReportHeader GetHeaderByProvince()
        {
            ReportHeader header = new ReportHeader();
            header.AddChild("provinceName", new LocaleValue("省份"));
            header.AddChild("oppCount", new LocaleValue("数量"), SqlStorageType.SqlInt);
            return header;
        }

        private static ReportHeader GetHeaderByRegion()
        {
            ReportHeader header = new ReportHeader();
            header.AddChild("regionName", new LocaleValue("区域"));
            header.AddChild("oppCount", new LocaleValue("数量"), SqlStorageType.SqlInt);
            return header;
        }

        internal static string GetTitleByDimension(string dimension)
        {
            switch (dimension)
            {
                case "1": //按产业
                    return " （按产业）";

                case "2": //按月份
                    return " （按月份）";

                case "3": //按省份
                    return " （按省份分布）";

                case "4": //按区域
                    return " （按区域分布）";

                default: //按产业
                    return "";
            }
        }
    }
}