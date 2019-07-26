using System;
using Kingdee.BOS.WebApi.Client;
using Kingdee.BOS.WebApi.ServicesStub;
using Kingdee.BOS.ServiceFacade.KDServiceFx;
using Kingdee.BOS;
using Kingdee.BOS.ServiceFacade.KDServiceClient.User;
using Kingdee.BOS.Authentication;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Web;
using System.Text;
using System.Linq;
using Kingdee.BOS.App.Data;
using Kingdee.K3.MFG.App;
using Kingdee.BOS.Orm.DataEntity;
using System.IO;

namespace CustomizeWebApi
{
    public class PhotoUpload : AbstractWebApiBusinessService
    {
        public PhotoUpload(KDServiceContext context) : base(context)
        {
        }
        public string APPtest(string parameter)
        {
            string value = HttpContext.Current.Request.Form["Data"];
            JObject jObject = (JObject)JsonConvert.DeserializeObject(value);
            string DBID = jObject["DBID"].ToString();
            string UserName = jObject["UserName"].ToString();
            string PassWord = jObject["PassWord"].ToString();
            string FileContent = jObject["FileContent"].ToString();
            string FileName = jObject["FileName"].ToString();

            string sContent = "";
            string reason = "";

            JObject jsonRoot = new JObject();


            Context ctx = getContext(UserName, PassWord, 2052, DBID, "http://localhost/K3Cloud/");
            ApiClient client = new ApiClient("http://localhost/K3Cloud/");
            bool bLogin = client.Login(DBID, UserName, PassWord, 2052);
            if (bLogin)//登录成功
            {
                byte[] buff = File2Bytes(FileContent);

                System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
                long timeStamp = (long)(DateTime.Now - startTime).TotalMilliseconds;
                string UploadName = FileName + timeStamp + ".dat";

                Bytes2File(buff, "D:\\" + UploadName);

                reason = "上传成功";
            }
            else
            {
                reason = "登录失败";
            }

            jsonRoot.Add("Message", reason);

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

        public static byte[] File2Bytes(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                return new byte[0];
            }


            FileInfo fi = new FileInfo(path);
            byte[] buff = new byte[fi.Length];


            FileStream fs = fi.OpenRead();
            fs.Read(buff, 0, Convert.ToInt32(fs.Length));
            fs.Close();


            return buff;
        }
        /// <summary>
        /// 将byte数组转换为文件并保存到指定地址
        /// </summary>
        /// <param name="buff">byte数组</param>
        /// <param name="savepath">保存地址</param>
        public static void Bytes2File(byte[] buff, string savepath)
        {
            if (System.IO.File.Exists(savepath))
            {
                System.IO.File.Delete(savepath);
            }


            FileStream fs = new FileStream(savepath, FileMode.CreateNew);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(buff, 0, buff.Length);
            bw.Close();
            fs.Close();
        }


    }
}
