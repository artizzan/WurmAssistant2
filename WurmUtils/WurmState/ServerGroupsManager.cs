using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Aldurcraft.WurmOnline.WurmState
{
    //plug this into state info
    [DataContract]
    internal class ServerGroupsManager
    {
        [DataContract]
        struct SgInfo
        {
            public DateTime Stamp;
            public string ServerName;
        }

        [DataMember]
        Dictionary<WurmServer.ServerInfo.ServerGroup, SgInfo> _groupToServerMap;

        public ServerGroupsManager()
        {
            Init();
        }

        [OnDeserializing]
        private void OnDes(StreamingContext context)
        {
            Init();
        }

        private void Init()
        {
            _locker = new object();
            _groupToServerMap = new Dictionary<WurmServer.ServerInfo.ServerGroup, SgInfo>();
        }

        private object _locker;
        public void Update(WurmServer.ServerInfo.ServerGroup serverGroup, DateTime stamp, string serverName)
        {
            lock (_locker)
            {
                SgInfo info;
                if (_groupToServerMap.TryGetValue(serverGroup, out info))
                {
                    if (stamp < info.Stamp) return;
                }
                _groupToServerMap[serverGroup] = new SgInfo {Stamp = stamp, ServerName = serverName};
            }
        }

        public string GetCurrentServerForGroup(WurmServer.ServerInfo.ServerGroup sg)
        {
            lock (_locker)
            {
                SgInfo info;
                if (_groupToServerMap.TryGetValue(sg, out info))
                {
                    return info.ServerName;
                }
                else return null;
            }
        }
    }
}
