using Kingdee.BOS;
using Kingdee.BOS.Authentication;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata;
using Kingdee.BOS.Core.SqlBuilder;
using Kingdee.BOS.JSON;
using Kingdee.BOS.Log;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Metadata.DataEntity;
using Kingdee.BOS.Resource;
using Kingdee.BOS.ServiceFacade.KDServiceClient.User;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.WebApi.Client;
using Kingdee.BOS.WebApi.ServicesStub;
using Kingdee.K3.CRM.Contracts;
using Kingdee.K3.CRM.Core;
using Kingdee.K3.CRM.Entity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Ken.K3Cloud.CRM.CustomizeWebApi.ServicesStub
{
    public class WinOrLose : AbstractWebApiBusinessService
    {
        public WinOrLose(KDServiceContext context) : base(context)
        {
        }

        public string APPtest(string parameter)
        {                                                                           
            string s = System.Web.HttpContext.Current.Request.Form["Data"];
            JObject Jo = (JObject)JsonConvert.DeserializeObject(s);
            string DBID = Jo["DBID"].ToString();
            string UserName = Jo["UserName"].ToString();
            string PassWord = Jo["PassWord"].ToString();
            string BillId = Jo["BillId"].ToString();
            string WinOrLose = Jo["WinOrLose"].ToString();

            string reason = "";
            string sContent = "";
            IOperationResult operationResult = new OperationResult();

            Context ctx = getContext(UserName, PassWord, 2052, DBID, "http://localhost/K3Cloud/");

            K3CloudApiClient client = new K3CloudApiClient("http://localhost/K3Cloud/");
            var ret = client.ValidateLogin(DBID, UserName, PassWord, 2052);
            var result = JObject.Parse(ret)["LoginResultType"].Value<int>();

            if (result == 1)//登录成功
            {

                if (WinOrLose.Equals("0"))
                {
                    operationResult = WinOPP(ctx, BillId);
                    reason = operationResult.OperateResult[0].Message;
                }
                else if (WinOrLose.Equals("1"))
                {
                    operationResult = UnWinOPP(ctx, BillId);
                    reason = operationResult.OperateResult[0].Message;
                }
                else if (WinOrLose.Equals("2"))
                {
                    operationResult = LoseOPP(ctx, BillId);
                    reason = operationResult.OperateResult[0].Message;
                }
                else if (WinOrLose.Equals("3"))
                {
                    operationResult = UnLoseOPP(ctx, BillId);
                    reason = operationResult.OperateResult[0].Message;
                }
            }
            else
            {
                reason = "登录失败";
            }
            if (reason.Equals(""))
            {
                reason = "未传入正确WinOrLose参数";
            }

            JObject jsonRoot = new JObject();
            JObject Result = new JObject();
            JObject ResponseStatus = new JObject();
            JArray Errors = new JArray();
            JObject basedata = new JObject();


            basedata.Add("FieldName", "");
            Errors.Add(basedata);
            basedata = new JObject();
            basedata.Add("Message", reason);
            Errors.Add(basedata);
            basedata = new JObject();
            basedata.Add("DIndex", "0");
            Errors.Add(basedata);

            ResponseStatus.Add("IsSuccess", "true");
            ResponseStatus.Add("Errors", Errors);

            Result.Add("ResponseStatus", ResponseStatus);

            jsonRoot.Add("Result", Result);

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

        public static IOperationResult WinOPP(Context ctx, string BillId)
        {

            if (!CommonHelper.CheckPermission(ctx, "CRM_OPP_Opportunity", "005056942d56a33b11e44f8cb00eb8d2"))
            {
                return null;
            }

            IOperationResult operationResult = DoAction(ctx, new string[]
            {
                BillId
            }, Enum_OPPAction.WIN);
            return operationResult;

        }

        public static IOperationResult UnWinOPP(Context ctx, string BillId)
        {

            if (!CommonHelper.CheckPermission(ctx, "CRM_OPP_Opportunity", "005056942d56a33b11e44f8cc2ba9a1a"))
            {
                return null;
            }
            IOperationResult operationResult = DoAction(ctx, new string[]
            {
                BillId
            }, Enum_OPPAction.UNWIN);
            return operationResult;

        }

        public static IOperationResult LoseOPP(Context ctx, string BillId)
        {

            if (!CommonHelper.CheckPermission(ctx, "CRM_OPP_Opportunity", "005056942d56a33b11e44f8cd1cca926"))
            {
                return null;
            }
            IOperationResult operationResult = DoAction(ctx, new string[]
            {
                BillId
            }, Enum_OPPAction.LOSE);
            return operationResult;

        }

        public static IOperationResult UnLoseOPP(Context ctx, string BillId)
        {

            if (!CommonHelper.CheckPermission(ctx, "CRM_OPP_Opportunity", "005056942d56a33b11e44f8cdde5d646"))
            {
                return null;
            }
            IOperationResult operationResult = DoAction(ctx, new string[]
            {
                BillId
            }, Enum_OPPAction.UNLOSE);
            return operationResult;

        }
        public static IOperationResult DoAction(Context context, string[] selPKArray, Enum_OPPAction action)
        {
            IOperationResult operationResult = new OperationResult();
            OperateResultCollection operateResultCollection = new OperateResultCollection();
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            List<DynamicObject> list = new List<DynamicObject>();
            FormMetadata formMetadata = (FormMetadata)MetaDataServiceHelper.Load(context, "CRM_OPP_Opportunity", true);
            BusinessInfo businessInfo = formMetadata.BusinessInfo;
            DynamicObject[] array = BusinessDataServiceHelper.Load(context, selPKArray, businessInfo.GetDynamicObjectType());
            if (array == null || array.Length == 0)
            {
                operateResultCollection.Add(OpportunityCommon.CreateOperateResult(false, ResManager.LoadKDString("未找到对应的数据，可能已被删除！", "006008030001276", SubSystemType.CRM, new object[0])));
                operationResult.OperateResult = operateResultCollection;
                return operationResult;
            }
            DynamicObject[] array2 = array;
            for (int i = 0; i < array2.Length; i++)
            {
                DynamicObject dynamicObject = array2[i];
                string text = CheckBeforeAction(dynamicObject, action);
                if (text != "")
                {
                    operateResultCollection.Add(OpportunityCommon.CreateOperateResult(false, text));
                    operationResult.OperateResult = operateResultCollection;
                }
                else
                {
                    if (action == Enum_OPPAction.WIN)
                    {
                        dynamicObject["FDocumentStatus"] = "E";
                    }
                    else
                    {
                        if (action == Enum_OPPAction.LOSE)
                        {
                            dynamicObject["FDocumentStatus"] = "F";
                        }
                        else
                        {
                            if (action == Enum_OPPAction.UNWIN || action == Enum_OPPAction.UNLOSE)
                            {
                                dynamicObject["FDocumentStatus"] = "G";
                            }
                        }
                    }
                    if (action == Enum_OPPAction.WIN || action == Enum_OPPAction.LOSE)
                    {
                        DynamicObjectCollection dynamicObjectCollection = dynamicObject["T_CRM_OppPhase"] as DynamicObjectCollection;
                        foreach (DynamicObject current in dynamicObjectCollection)
                        {
                            if (current["FIsCurrent"].ToString() == "True")
                            {
                                current["FEndtimeReal"] = DateTime.Now;
                                break;
                            }
                        }
                    }
                    dictionary.Add(dynamicObject["Id"].ToString(), dynamicObject["FOPPName"].ToString());
                    list.Add(dynamicObject);
                }
            }
            if (list.Count > 0)
            {
                IOperationResult operationResult2 = BusinessDataServiceHelper.Save(context, businessInfo, list.ToArray(), null, "");
                if (action == Enum_OPPAction.WIN || action == Enum_OPPAction.LOSE)
                {
                    Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
                    string format = ResManager.LoadKDString("赢单商机【{0}】", "006008030001476", SubSystemType.CRM, new object[0]);
                    string text2 = "Win";
                    if (action == Enum_OPPAction.LOSE)
                    {
                        format = ResManager.LoadKDString("输单商机【{0}】", "006008030001477", SubSystemType.CRM, new object[0]);
                        text2 = "Lose";
                    }
                    foreach (OperateResult current2 in operationResult2.OperateResult)
                    {
                        if (current2.SuccessStatus)
                        {
                            dictionary2.Add(current2.PKValue.ToString(), string.Format(format, dictionary[current2.PKValue.ToString()]));
                        }
                    }
                    if (dictionary2.Count > 0)
                    {
                        ILatestInfoService latestInfoService = ServiceFactory.GetLatestInfoService(context);
                        latestInfoService.AddActionInfo(context, "CRM_OPP_Opportunity", text2, dictionary2);
                    }
                }
                if (operationResult.OperateResult.Count > 0)
                {
                    using (IEnumerator<OperateResult> enumerator3 = operationResult2.OperateResult.GetEnumerator())
                    {
                        while (enumerator3.MoveNext())
                        {
                            OperateResult current3 = enumerator3.Current;
                            operationResult.OperateResult.Add(current3);
                        }
                        return operationResult;
                    }
                }
                operationResult.OperateResult = operationResult2.OperateResult;
            }
            return operationResult;
        }

        public static string CheckBeforeAction(DynamicObject doOPP, Enum_OPPAction action)
        {
            string result = "";
            switch (action)
            {
                case Enum_OPPAction.WIN:
                    result = CheckBeforeWin(doOPP);
                    break;
                case Enum_OPPAction.UNWIN:
                    result = CheckBeforeUnWin(doOPP);
                    break;
                case Enum_OPPAction.LOSE:
                    result = CheckBeforeLose(doOPP);
                    break;
                case Enum_OPPAction.UNLOSE:
                    result = CheckBeforeUnLose(doOPP);
                    break;
            }
            return result;
        }

        public static string CheckBeforeWin(DynamicObject doOPP)
        {
            string a = doOPP["FCloseStatus"].ToString();
            string a2 = doOPP["FDocumentStatus"].ToString();
            string arg = doOPP["FBillNo"].ToString();
            if (a == Convert.ToString(1))
            {
                return string.Format(ResManager.LoadKDString("商机{0}已经关闭！", "006008030001261", SubSystemType.CRM, new object[0]), arg);
            }
            if (a2 == "E" || a2 == "F")
            {
                return string.Format(ResManager.LoadKDString("商机{0}已经完成输赢单操作！", "006008030001262", SubSystemType.CRM, new object[0]), arg);
            }
            if (a2 != "G")
            {
                return string.Format(ResManager.LoadKDString("商机{0}只能对执行中的商机执行赢单操作！", "006008030001263", SubSystemType.CRM, new object[0]), arg);
            }
            if (doOPP["FWinDate"] == null || string.IsNullOrEmpty(doOPP["FWinDate"].ToString()))
            {
                return string.Format(ResManager.LoadKDString("商机{0}输赢日期不能为空！", "006008030001264", SubSystemType.CRM, new object[0]), arg);
            }
            if (doOPP["FWinReason"] == null || string.IsNullOrEmpty(doOPP["FWinReason"].ToString()))
            {
                return string.Format(ResManager.LoadKDString("商机{0}输赢原因不能为空！", "006008030001265", SubSystemType.CRM, new object[0]), arg);
            }
            return "";
        }

        public static string CheckBeforeUnWin(DynamicObject doOPP)
        {
            string a = doOPP["FCloseStatus"].ToString();
            string a2 = doOPP["FDocumentStatus"].ToString();
            string arg = doOPP["FBillNo"].ToString();
            if (a == Convert.ToString(1))
            {
                return string.Format(ResManager.LoadKDString("商机{0}已经关闭！", "006008030001261", SubSystemType.CRM, new object[0]), arg);
            }
            if (a2 != "E")
            {
                return string.Format(ResManager.LoadKDString("商机{0}只能对赢单的商机执行反赢单操作！", "006008030001267", SubSystemType.CRM, new object[0]), arg);
            }
            return "";
        }

        public static string CheckBeforeLose(DynamicObject doOPP)
        {
            string a = doOPP["FCloseStatus"].ToString();
            string a2 = doOPP["FDocumentStatus"].ToString();
            string arg = doOPP["FBillNo"].ToString();
            if (a == Convert.ToString(1))
            {
                return string.Format(ResManager.LoadKDString("商机{0}已经关闭！", "006008030001261", SubSystemType.CRM, new object[0]), arg);
            }
            if (a2 == "E")
            {
                return string.Format(ResManager.LoadKDString("商机{0}已经进行赢单操作！", "006008030001269", SubSystemType.CRM, new object[0]), arg);
            }
            if (a2 == "F")
            {
                return string.Format(ResManager.LoadKDString("商机{0}已经进行输单操作！", "006008030001270", SubSystemType.CRM, new object[0]), arg);
            }
            if (a2 != "G")
            {
                return string.Format(ResManager.LoadKDString("商机{0}只能对执行中的商机执行输单操作！", "006008030001271", SubSystemType.CRM, new object[0]), arg);
            }
            return "";
        }

        public static string CheckBeforeUnLose(DynamicObject doOPP)
        {
            string a = doOPP["FCloseStatus"].ToString();
            string a2 = doOPP["FDocumentStatus"].ToString();
            string arg = doOPP["FBillNo"].ToString();
            if (a == Convert.ToString(1))
            {
                return string.Format(ResManager.LoadKDString("商机{0}已经关闭！", "006008030001261", SubSystemType.CRM, new object[0]), arg);
            }
            if (a2 != "F")
            {
                return string.Format(ResManager.LoadKDString("商机{0}只能对输单的商机执行反输单操作！", "006008030001273", SubSystemType.CRM, new object[0]), arg);
            }
            return "";
        }

    }

}

