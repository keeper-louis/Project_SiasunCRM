using Kingdee.BOS.App.Data;
using Kingdee.BOS.Orm.DataEntity;
using System;
using Kingdee.BOS.WebApi.Client;
using Kingdee.BOS.WebApi.ServicesStub;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS;
using Kingdee.BOS.ServiceFacade.KDServiceClient.User;
using Kingdee.BOS.Authentication;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Web;

namespace Ken.CRM.CustomizeWebApi.ServicesStub
{
    public class WaitingTaskOrMessageSearch : AbstractWebApiBusinessService
    {
        public WaitingTaskOrMessageSearch(KDServiceContext context) : base(context)
        {
        }
        public string APPtest(string parameter)
        {
            string value = HttpContext.Current.Request.Form["Data"];
            JObject jObject = (JObject)JsonConvert.DeserializeObject(value);
            string DBID = jObject["DBID"].ToString();
            string UserName = jObject["UserName"].ToString();
            string PassWord = jObject["PassWord"].ToString();
            string SearchType = jObject["SearchType"].ToString();
            string FRECEIVERID = jObject["FRECEIVERID"].ToString();


            string sContent = "";

            JObject jsonRoot = new JObject();
            JObject basedata = new JObject();
            JArray entrys = new JArray();//单个model中存储多行分录体集合，存储mBentry
            Context ctx = getContext(UserName, PassWord, 2052, DBID, "http://localhost/K3Cloud/");
            ApiClient client = new ApiClient("http://localhost/K3Cloud/");
            bool bLogin = client.Login(DBID, UserName, PassWord, 2052);
            if (bLogin)//登录成功
            {

                if (SearchType.Equals("0"))//代办事项
                {
                    string strSql = string.Format(@"/*dialect*/SELECT t_WF_PiBiMap.FPROCINSTID,
 t_WF_PiBiMap.FKEYVALUE,t_WF_PiBiMap.FOBJECTTYPEID,
   T_SEC_USER1.FNAME,t_WF_Receiver.FTITLE,
      t_WF_ApprovalAssign.FDEALTIME,T_WF_PROCDEF_L.FDISPLAYNAME,t_WF_ApprovalItem.FDisposition   
	    FROM t_WF_PiBiMap 
		INNER JOIN t_WF_ProcInst ON (t_WF_ProcInst.FProcInstId = t_WF_PiBiMap.FProcInstId) 
		INNER JOIN t_WF_ActInst on (t_WF_ActInst.FProcInstId = t_WF_ProcInst.FProcInstId) 
		INNER JOIN t_WF_Assign on (t_WF_Assign.FActInstId = t_WF_ActInst.FActInstId) 
		INNER JOIN t_WF_Receiver on (t_WF_Receiver.FAssignId = t_WF_Assign.FAssignId) 
		INNER JOIN t_SEC_User ON (t_SEC_User.FUserId = t_WF_Receiver.FReceiverId) 
		INNER JOIN t_WF_ApprovalAssign on (t_WF_Assign.FAssignId = t_WF_ApprovalAssign.FAssignId)
		  LEFT JOIN t_WF_ApprovalItem on (t_WF_ApprovalItem.FApprovalAssignId = t_WF_ApprovalAssign.FApprovalAssignId 
		                                    AND t_WF_ApprovalItem.FReceiverId = t_WF_Receiver.FReceiverId) 
											left join T_SEC_USER T_SEC_USER1 on T_SEC_USER1.FUSERID=t_WF_ProcInst.FORIGINATORID 
											left join T_WF_PROCDEF_L on T_WF_PROCDEF_L.FPROCDEFID=t_WF_ProcInst.FPROCDEFID
 where t_WF_Assign.FSTATUS=0 and  t_WF_Receiver.FReceiverId='{0}' order by t_WF_ApprovalAssign.FDEALTIME desc", FRECEIVERID);
                    DynamicObjectCollection items = DBUtils.ExecuteDynamicObject(ctx, strSql);
                    foreach (DynamicObject item in items)
                    {
                        basedata = new JObject();
                        basedata.Add("FPROCINSTID", Convert.ToString(item["FPROCINSTID"]));
                        basedata.Add("FKEYVALUE", Convert.ToString(item["FKEYVALUE"]));
                        basedata.Add("FOBJECTTYPEID", Convert.ToString(item["FOBJECTTYPEID"]));
                        basedata.Add("FNAME", Convert.ToString(item["FNAME"]));
                        basedata.Add("FTITLE", Convert.ToString(item["FTITLE"]));
                        basedata.Add("FDEALTIME", Convert.ToString(item["FDEALTIME"]));
                        basedata.Add("FDISPLAYNAME", Convert.ToString(item["FDISPLAYNAME"]));
                        basedata.Add("FDisposition", Convert.ToString(item["FDisposition"]));
                        entrys.Add(basedata);
                    }
                    jsonRoot.Add("Result", entrys);
                }
                if (SearchType.Equals("1"))//监控消息
                {
                    string strSql = string.Format(@"/*dialect*/select T_BAS_WARNMERGEMESSAGE.FSTATUS,t_SEC_User.FNAME, 
T_BAS_WARNMERGEMESSAGE.FMERGETITLE,T_BAS_WARNMERGEMESSAGE.FCREATETIME  
from T_BAS_WARNMERGEMESSAGE  
left join t_SEC_User ON t_SEC_User.FUserId = T_BAS_WARNMERGEMESSAGE.FSENDERID 
 where FRECEIVERID='{0}' order by T_BAS_WARNMERGEMESSAGE.FCREATETIME desc", FRECEIVERID);
                    DynamicObjectCollection items = DBUtils.ExecuteDynamicObject(ctx, strSql);
                    foreach (DynamicObject item in items)
                    {
                        basedata = new JObject();
                        basedata.Add("FSTATUS", Convert.ToString(item["FSTATUS"]));
                        basedata.Add("FNAME", Convert.ToString(item["FNAME"]));
                        basedata.Add("FMERGETITLE", Convert.ToString(item["FMERGETITLE"]));
                        basedata.Add("FCREATETIME", Convert.ToString(item["FCREATETIME"]));
                        entrys.Add(basedata);
                    }
                    jsonRoot.Add("Result", entrys);
                }
                if (SearchType.Equals("2"))//流程消息
                {
                    string strSql = string.Format(@"/*dialect*/select T_WF_MESSAGE.FSTATUS,t_SEC_User.FNAME, 
T_META_OBJECTTYPE_L.FNAME OBJECTNAME,T_WF_MESSAGE.FTITLE,T_WF_MESSAGE.FCREATETIME  
from T_WF_MESSAGE  
left join t_SEC_User ON t_SEC_User.FUserId = T_WF_MESSAGE.FSENDERID  
left join T_META_OBJECTTYPE_L on T_WF_MESSAGE.FOBJECTTYPEID=T_META_OBJECTTYPE_L.FID and T_META_OBJECTTYPE_L.FLOCALEID='2052'  
where T_WF_MESSAGE.FTYPE=0 and FRECEIVERID='{0}' order by T_WF_MESSAGE.FCREATETIME desc
					", FRECEIVERID);
                    DynamicObjectCollection items = DBUtils.ExecuteDynamicObject(ctx, strSql);
                    foreach (DynamicObject item in items)
                    {
                        basedata = new JObject();
                        basedata.Add("FSTATUS", Convert.ToString(item["FSTATUS"]));
                        basedata.Add("FNAME", Convert.ToString(item["FNAME"]));
                        basedata.Add("OBJECTNAME", Convert.ToString(item["OBJECTNAME"]));
                        basedata.Add("FTITLE", Convert.ToString(item["FTITLE"]));
                        basedata.Add("FCREATETIME", Convert.ToString(item["FCREATETIME"]));
                        entrys.Add(basedata);
                    }
                    jsonRoot.Add("Result", entrys);
                }
                if (SearchType.Equals("3"))//总数
                {
                    string strSql = string.Format(@"/*dialect*/SELECT t_WF_PiBiMap.FPROCINSTID,
 t_WF_PiBiMap.FKEYVALUE,t_WF_PiBiMap.FOBJECTTYPEID,
   T_SEC_USER1.FNAME,t_WF_Receiver.FTITLE,
      t_WF_ApprovalAssign.FDEALTIME,T_WF_PROCDEF_L.FDISPLAYNAME  
	    FROM t_WF_PiBiMap 
		INNER JOIN t_WF_ProcInst ON (t_WF_ProcInst.FProcInstId = t_WF_PiBiMap.FProcInstId) 
		INNER JOIN t_WF_ActInst on (t_WF_ActInst.FProcInstId = t_WF_ProcInst.FProcInstId) 
		INNER JOIN t_WF_Assign on (t_WF_Assign.FActInstId = t_WF_ActInst.FActInstId) 
		INNER JOIN t_WF_Receiver on (t_WF_Receiver.FAssignId = t_WF_Assign.FAssignId) 
		INNER JOIN t_SEC_User ON (t_SEC_User.FUserId = t_WF_Receiver.FReceiverId) 
		INNER JOIN t_WF_ApprovalAssign on (t_WF_Assign.FAssignId = t_WF_ApprovalAssign.FAssignId)
		  LEFT JOIN t_WF_ApprovalItem on (t_WF_ApprovalItem.FApprovalAssignId = t_WF_ApprovalAssign.FApprovalAssignId 
		                                    AND t_WF_ApprovalItem.FReceiverId = t_WF_Receiver.FReceiverId) 
											left join T_SEC_USER T_SEC_USER1 on T_SEC_USER1.FUSERID=t_WF_ProcInst.FORIGINATORID 
											left join T_WF_PROCDEF_L on T_WF_PROCDEF_L.FPROCDEFID=t_WF_ProcInst.FPROCDEFID
 where t_WF_Assign.FSTATUS=0 and  t_WF_Receiver.FReceiverId='{0}' order by t_WF_ApprovalAssign.FDEALTIME desc", FRECEIVERID);
                    DynamicObjectCollection items = DBUtils.ExecuteDynamicObject(ctx, strSql);

                    strSql = string.Format(@"/*dialect*/select T_BAS_WARNMERGEMESSAGE.FSTATUS,t_SEC_User.FNAME, 
T_BAS_WARNMERGEMESSAGE.FMERGETITLE,T_BAS_WARNMERGEMESSAGE.FCREATETIME  
from T_BAS_WARNMERGEMESSAGE  
left join t_SEC_User ON t_SEC_User.FUserId = T_BAS_WARNMERGEMESSAGE.FSENDERID 
 where FRECEIVERID='{0}' order by T_BAS_WARNMERGEMESSAGE.FCREATETIME desc", FRECEIVERID);
                    DynamicObjectCollection items1 = DBUtils.ExecuteDynamicObject(ctx, strSql);

                    strSql = string.Format(@"/*dialect*/select T_WF_MESSAGE.FSTATUS,t_SEC_User.FNAME, 
T_META_OBJECTTYPE_L.FNAME OBJECTNAME,T_WF_MESSAGE.FTITLE,T_WF_MESSAGE.FCREATETIME  
from T_WF_MESSAGE  
left join t_SEC_User ON t_SEC_User.FUserId = T_WF_MESSAGE.FSENDERID  
left join T_META_OBJECTTYPE_L on T_WF_MESSAGE.FOBJECTTYPEID=T_META_OBJECTTYPE_L.FID and T_META_OBJECTTYPE_L.FLOCALEID='2052'  
where T_WF_MESSAGE.FTYPE=0 and FRECEIVERID='{0}' order by T_WF_MESSAGE.FCREATETIME desc
					", FRECEIVERID);
                    DynamicObjectCollection items2 = DBUtils.ExecuteDynamicObject(ctx, strSql);

                    jsonRoot.Add("result", new JObject
                    {

                        {
                            "count0",
                            Convert.ToString(items.Count)
                        },

                        {
                            "count1",
                            Convert.ToString(items1.Count)
                        },

                        {
                            "count2",
                            Convert.ToString(items2.Count)
                        }
                    });

                }
            }





            sContent = JsonConvert.SerializeObject(jsonRoot);



            return sContent;
        }


        public static Context getContext(string UserName, string PassWord, int ICID, string DBID, string ServerUrl)
        {


            UserServiceProxy proxy = new UserServiceProxy();// 引用Kingdee.BOS.ServiceFacade.KDServiceClient.dll
            proxy.HostURL = @"http://localhost/K3Cloud/";//k/3cloud地址

            LoginInfo logininfo = new LoginInfo();
            logininfo.Username = UserName;
            logininfo.Password = PassWord;
            logininfo.Lcid = ICID;
            logininfo.AcctID = DBID;

            Context ctx = proxy.ValidateUser("http://localhost/K3Cloud/", logininfo).Context;//guid为业务数据中心dbid，可以去管理中心数据库查询一下t_bas_datacenter_l表查找，后面需要用户名和密码

            return ctx;
        }
    }
}
