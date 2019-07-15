using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Orm;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Workflow.Assignment;
using Kingdee.BOS.Workflow.Interface;
using Kingdee.BOS.Workflow.Models.ApprovalAssignment;
using Kingdee.BOS.Workflow.Models.EnumStatus;
using Kingdee.BOS.Workflow.ServiceHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Kingdee.BOS.WebApi.Client;
using Kingdee.BOS.WebApi.ServicesStub;
using Kingdee.K3.CRM.Contracts;
using Kingdee.K3.CRM.Core;
using Kingdee.K3.CRM.Entity;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS;
using Kingdee.BOS.ServiceFacade.KDServiceClient.User;
using Kingdee.BOS.Authentication;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Web.Http;
using System.IO;
using System.Text;
using System.Collections.Specialized;

namespace Ken.K3Cloud.CRM.CustomizeWebApi.ServicesStub
{
    public class WaitingTaskOrMessageSearch : AbstractWebApiBusinessService
    {
        public WaitingTaskOrMessageSearch(KDServiceContext context) : base(context)
        {
        }



        public Context ctx
        {
            get
            {
                return this.KDContext.Session.AppContext;
            }
        }
        public string APPtest([FromBody]JObject parameter)
        {
            string s = System.Web.HttpContext.Current.Request.Form["Data"];
            JObject Jo = (JObject)JsonConvert.DeserializeObject(s);
            string DBID = Jo["DBID"].ToString();
            string UserName = Jo["UserName"].ToString();
            string PassWord = Jo["PassWord"].ToString();
            string SearchType = Jo["SearchType"].ToString();
            string FRECEIVERID = Jo["FRECEIVERID"].ToString();

            string sContent = "";

            JObject jsonRoot = new JObject();
            JObject basedata = new JObject();

            Context ctx = getContext(UserName, PassWord, 2052, DBID, "http://localhost/K3Cloud/");

            K3CloudApiClient client = new K3CloudApiClient("http://localhost/K3Cloud/");
            var ret = client.ValidateLogin(DBID, UserName, PassWord, 2052);
            var result = JObject.Parse(ret)["LoginResultType"].Value<int>();

            if (result == 1)//登录成功
            {
                if (SearchType.Equals("0"))
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
								  where t_WF_Assign.FSTATUS=0 and  t_WF_Receiver.FReceiverId='{0}'", FRECEIVERID);
                    DynamicObjectCollection items = DBUtils.ExecuteDynamicObject(ctx, strSql);
                    for (int i = 0; i < items.Count(); i++)
                    {
                        basedata = new JObject();
                        basedata.Add("FPROCINSTID", Convert.ToString(items[i]["FPROCINSTID"]));
                        basedata.Add("FKEYVALUE", Convert.ToString(items[i]["FKEYVALUE"]));
                        basedata.Add("FOBJECTTYPEID", Convert.ToString(items[i]["FOBJECTTYPEID"]));
                        basedata.Add("FNAME", Convert.ToString(items[i]["FNAME"]));
                        basedata.Add("FTITLE", Convert.ToString(items[i]["FTITLE"]));
                        basedata.Add("FDEALTIME", Convert.ToString(items[i]["FDEALTIME"]));
                        basedata.Add("FDISPLAYNAME", Convert.ToString(items[i]["FDISPLAYNAME"]));
                        jsonRoot.Add(Convert.ToString(i), basedata);
                    }
                }
                if (SearchType.Equals("1"))
                {
                    string strSql = string.Format(@"/*dialect*/select T_BAS_WARNMERGEMESSAGE.FSTATUS,t_SEC_User.FNAME,
T_BAS_WARNMERGEMESSAGE.FMERGETITLE,T_BAS_WARNMERGEMESSAGE.FCREATETIME 
from T_BAS_WARNMERGEMESSAGE 
left join t_SEC_User ON t_SEC_User.FUserId = T_BAS_WARNMERGEMESSAGE.FSENDERID
 where FRECEIVERID='{0}'", FRECEIVERID);
                    DynamicObjectCollection items = DBUtils.ExecuteDynamicObject(ctx, strSql);
                    for (int i = 0; i < items.Count(); i++)
                    {
                        basedata = new JObject();
                        basedata.Add("FSTATUS", Convert.ToString(items[i]["FSTATUS"]));
                        basedata.Add("FNAME", Convert.ToString(items[i]["FNAME"]));
                        basedata.Add("FMERGETITLE", Convert.ToString(items[i]["FMERGETITLE"]));
                        basedata.Add("FCREATETIME", Convert.ToString(items[i]["FCREATETIME"]));
                        jsonRoot.Add(Convert.ToString(i), basedata);
                    }
                }
                if (SearchType.Equals("2"))
                {
                    string strSql = string.Format(@"/*dialect*/select T_WF_MESSAGE.FSTATUS,t_SEC_User.FNAME,
T_META_OBJECTTYPE_L.FNAME OBJECTNAME,T_WF_MESSAGE.FTITLE,T_WF_MESSAGE.FCREATETIME
 from T_WF_MESSAGE
left join t_SEC_User ON t_SEC_User.FUserId = T_WF_MESSAGE.FSENDERID
left join T_META_OBJECTTYPE_L on T_WF_MESSAGE.FOBJECTTYPEID=T_META_OBJECTTYPE_L.FID and T_META_OBJECTTYPE_L.FLOCALEID='2052'
 where T_WF_MESSAGE.FTYPE=0 and FRECEIVERID='{0}'", FRECEIVERID);
                    DynamicObjectCollection items = DBUtils.ExecuteDynamicObject(ctx, strSql);

                    for(int i =0;i< items.Count();i++)
                    {
                        basedata = new JObject();
                        basedata.Add("FSTATUS", Convert.ToString(items[i]["FSTATUS"]));
                        basedata.Add("FNAME", Convert.ToString(items[i]["FNAME"]));
                        basedata.Add("OBJECTNAME", Convert.ToString(items[i]["OBJECTNAME"]));
                        basedata.Add("FTITLE", Convert.ToString(items[i]["FTITLE"]));
                        basedata.Add("FCREATETIME", Convert.ToString(items[i]["FCREATETIME"]));
                        jsonRoot.Add(Convert.ToString(i), basedata);
                    }
                }
                if (SearchType.Equals("3"))
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
								  where t_WF_Assign.FSTATUS=0 and  t_WF_Receiver.FReceiverId='{0}'", FRECEIVERID);
                    DynamicObjectCollection items0 = DBUtils.ExecuteDynamicObject(ctx, strSql);
                    strSql = string.Format(@"/*dialect*/select T_BAS_WARNMERGEMESSAGE.FSTATUS,t_SEC_User.FNAME,
T_BAS_WARNMERGEMESSAGE.FMERGETITLE,T_BAS_WARNMERGEMESSAGE.FCREATETIME 
from T_BAS_WARNMERGEMESSAGE 
left join t_SEC_User ON t_SEC_User.FUserId = T_BAS_WARNMERGEMESSAGE.FSENDERID
 where T_BAS_WARNMERGEMESSAGE.FSTATUS=0 and  FRECEIVERID='{0}'", FRECEIVERID);
                    DynamicObjectCollection items1 = DBUtils.ExecuteDynamicObject(ctx, strSql);
                    strSql = string.Format(@"/*dialect*/select T_WF_MESSAGE.FSTATUS,t_SEC_User.FNAME,
T_META_OBJECTTYPE_L.FNAME OBJECTNAME,T_WF_MESSAGE.FTITLE,T_WF_MESSAGE.FCREATETIME
 from T_WF_MESSAGE
left join t_SEC_User ON t_SEC_User.FUserId = T_WF_MESSAGE.FSENDERID
left join T_META_OBJECTTYPE_L on T_WF_MESSAGE.FOBJECTTYPEID=T_META_OBJECTTYPE_L.FID and T_META_OBJECTTYPE_L.FLOCALEID='2052'
 where T_WF_MESSAGE.FTYPE=0 and T_WF_MESSAGE.FSTATUS=0 and FRECEIVERID='{0}'", FRECEIVERID);
                    DynamicObjectCollection items2 = DBUtils.ExecuteDynamicObject(ctx, strSql);

                    basedata = new JObject();
                    basedata.Add("count0", Convert.ToString(items0.Count()));
                    basedata.Add("count1", Convert.ToString(items1.Count()));
                    basedata.Add("count2", Convert.ToString(items2.Count()));

                    jsonRoot.Add("result", basedata);
                    
                }


            }
            else
            {
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
