using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace SignalChat
{
    public class ZoomChatHub : Hub
    {
        private static Dictionary<Guid, string> ZoomDic = new Dictionary<Guid, string>();
        private static LinkedList<UserEntity> UserDic = new LinkedList<UserEntity>();
        private static object LockObject = new object();

        public void Login(string uname)
        {
            if (ZoomDic.Count == 0)
            {
                ZoomDic.Add(Guid.NewGuid(), "九层妖塔");
                ZoomDic.Add(Guid.NewGuid(), "黄皮子坟");
                ZoomDic.Add(Guid.NewGuid(), "云南古虫");
            }
            if (UserDic.Where(x => x.NickName.Equals(uname)).FirstOrDefault() == null)
            {
                UserDic.AddLast(new UserEntity() { ConnectionID = Context.ConnectionId, NickName = uname, ZoomKey = string.Empty });
            }
            else
            {
                var entity = UserDic.Where(x => x.NickName.Equals(uname)).FirstOrDefault();
                UserDic.Remove(entity);
                entity.ConnectionID = Context.ConnectionId;
                UserDic.AddLast(entity);
            }
            UpdateData();
        }

        public void JoinZoom(string zoomID)
        {
            if (ZoomDic[Guid.Parse(zoomID)] == null)
            {
                Clients.Client(Context.ConnectionId).sendMsg(false, "Please choose valid zoom.", null);
            }
            else
            {
                /*
                 * 1/ 根据当前ConnectionID 找到对应的 Entity
                 * 2/ 根据Entity 刷 ZoomID
                 * */
                var entity = UserDic.Where(x => x.ConnectionID.Equals(Context.ConnectionId)).FirstOrDefault();
                UserDic.Remove(entity);
                entity.ZoomKey = zoomID;
                UserDic.AddLast(entity);
                
                Clients.Client(Context.ConnectionId).syncUser(true, "Join successful.", UserDic);
            }
        }

        protected void UpdateData()
        {
            Clients.All.updateZoom(ZoomDic.ToArray(), UserDic.ToArray());
        }

        protected void SystemMsg(string msg)
        {
            Clients.All.SystemMsg(msg);
        }


        public void SendMsg(string uname, string zoomID, string msg)
        {
            //Clients.Group(zoomID, new string[0]).sendMessage(zoomID, $"{msg}  {DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}");
            Clients.Clients(UserDic.Where(x => x.ZoomKey.Equals(zoomID)).Select(x => x.ConnectionID).ToList()).sendMessage(zoomID, $"<span style='font-weight:bolder;'>{uname}</span>:<br/> {msg}<br/>{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}<br/>");
        }

        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled)
        {
            string r = string.Empty;
            if (stopCalled)
            {
                r = String.Format("Client {0} explicitly closed the connection.", Context.ConnectionId);
            }
            else
            {
                r = String.Format("Client {0} timed out .", Context.ConnectionId);
            }
            UserDic.Remove(UserDic.Where(x => x.ConnectionID.Equals(Context.ConnectionId)).FirstOrDefault());
            SystemMsg(r);
            UpdateData();
            return base.OnDisconnected(stopCalled);
        }
    }
    public class UserEntity
    {
        public string ZoomKey { get; set; }
        public string NickName { get; set; }
        public string ConnectionID { get; set; }
    }
}