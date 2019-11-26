using Kingdee.BOS;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Msg;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Orm.Drivers;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KEEPER.K3.SIASUN.CRM.OppScheduleServicePlugIn
{
    [Description("销售员与HR同步后异动自动消息提示")]
    public class SalePersonAutoSMS : IScheduleService
    {
        public void Run(Context ctx, Schedule schedule)
        {
            DateTime dateTime = DateTime.Now;
            string nowdate = string.Format("{0:yyyy-MM-dd}", dateTime);
            //获得 销售岗位被禁用的 人员列表
            string sql = string.Format(@" SELECT THR.FNUMBER     PERSONNUMBER,
       THRL.FNAME      PERSONNAME,
       TOP2.FNUMBER     POSITIONNUMBER,
       TOPL.FNAME      POSITIONNAME,
       STA.FFORBIDDATE,
    saleman.fnumber
  FROM 
  T_BD_STAFF STA
  LEFT JOIN T_ORG_POST TOP2
    ON STA.FPOSTID = TOP2.FPOSTID
  LEFT JOIN T_ORG_POST_L topl
    ON TOP2.FPOSTID = topl.FPOSTID
  LEFT JOIN T_HR_EMPINFO THR
    ON STA.FEMPINFOID = THR.FID
  LEFT JOIN T_HR_EMPINFO_L THRL
    ON THR.FID = THRL.FID

inner join   V_BD_SALESMAN saleman 
 on saleman.fnumber=STA.FNUMBER
 

 WHERE STA.FFORBIDSTATUS = 'B'  and  STA.FFORBIDDATE is not null and to_char(STA.FFORBIDDATE,'yyyy-MM-dd') >='{0}'
 order by FFORBIDDATE desc ", nowdate);

            DynamicObjectCollection col = DBUtils.ExecuteDynamicObject(ctx, sql);

            StringBuilder message = new StringBuilder();
            message.AppendLine(" 如下销售员岗位被禁用：");
            foreach (DynamicObject item in col)
            {
                string PERSONNUMBER = Convert.ToString(item["PERSONNUMBER"]);
                string PERSONNAME = Convert.ToString(item["PERSONNAME"]);
                string POSITIONNUMBER = Convert.ToString(item["POSITIONNUMBER"]);
                string POSITIONNAME = Convert.ToString(item["POSITIONNAME"]);


                message.AppendLine(" 员工编码：" + PERSONNUMBER + " 员工名称:" + PERSONNAME + " 岗位编码：" + POSITIONNUMBER + " 岗位名称：" + POSITIONNAME);

            }
            //如果大与零给管理员发送系统消息
            if (col.Count > 0)
            {
                string rolesql = string.Format(@"/*dialect*/ 
select distinct a.FUSERID,a.FNAME,
d.FNUMBER,dl.FNAME 
from T_SEC_USER a
inner join t_sec_userorg b on a.FUSERID = b.FUSERID
inner join t_sec_userrolemap c on c.FENTITYID = b.FENTITYID
inner join t_SEC_role d on d.FROLEID = c.FROLEID
inner join t_SEC_role_l dl on dl.FROLEID = d.FROLEID
where 
( d.FNUMBER in ('administrator') or dl.FNAME like '%administrator%'  or dl.FDESCRIPTION like '%ADMIN%') 
and a.fname not in ('user') 
");
                DynamicObjectCollection col2 = DBUtils.ExecuteDynamicObject(ctx, rolesql);
                foreach (DynamicObject one in col2 )
                {
                    //发送消息给管理员
                    SendMessage(ctx, "0", "0", " 有销售员岗位被禁用，请CRM系统管理员进行核实一下！", message.ToString(), DateTime.Now, Convert.ToInt64(one["FUSERID"]), 100010);

                }
            }
        }

        //发送消息
        private Message SendMessage(Context ctx, string formId, string billId, string title, string content, DateTime now, long receiverId, long senderid)

        {
            string messageId = SequentialGuid.NewGuid().ToString();
            Message msg = new DynamicObject(Message.MessageDynamicObjectType);
            msg.MessageId = messageId;
            msg.Title = title;
            msg.Content = content;
            msg.CreateTime = now;
            msg.SenderId = senderid;
            msg.ObjectTypeId = formId;
            msg.KeyValue = billId;
            msg.ReceiverId = receiverId;
            msg.MsgType = Kingdee.BOS.Msg.MsgType.CommonMessage;

            IDbDriver driver = new OLEDbDriver(ctx);
            var dataManager = Kingdee.BOS.Orm.DataManagerUtils.GetDataManager(Message.MessageDynamicObjectType, driver);
            dataManager.Save(msg.DataEntity);

            return msg;
        }
    }
}
