using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Web;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace SignalChat
{
    public class ChatHub : Hub
    {
        private static Timer timer;
        private static object lockDailyMsg = new object();
        private static bool isStart = true;
        public void Send(string name, string description, DateTime dateTime)
        {
            Clients.All.broadcastMessage(name, description, dateTime.ToString("yyyy-MM-dd hh:mm:ss"));
        }

        public void UpdateModel(ShapeModel clientModel)
        {
            clientModel.LastUpdatedBy = Context.ConnectionId;

            Clients.AllExcept(clientModel.LastUpdatedBy).updateShape(clientModel);
        }



        public void DailyMsg()
        {
            lock (lockDailyMsg)
            {
                if (isStart)
                {
                    timer = new Timer();
                    timer.Interval = 2000;
                    timer.Elapsed += delegate
                    {
                        Send("Admin", "Welcome SignalR", DateTime.Now);
                    };
                    timer.Start();
                    isStart = false;
                }
            }
           
        }



    }
    public class ShapeModel
    {
        // We declare Left and Top as lowercase with 
        // JsonProperty to sync the client and server models
        [JsonProperty("left")]
        public double Left { get; set; }
        [JsonProperty("top")]
        public double Top { get; set; }
        // We don't want the client to get the "LastUpdatedBy" property
        [JsonIgnore]
        public string LastUpdatedBy { get; set; }
    }
}