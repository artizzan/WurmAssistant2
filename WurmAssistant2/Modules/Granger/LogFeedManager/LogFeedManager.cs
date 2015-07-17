using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using Aldurcraft.WurmOnline.WurmLogsManager;
using Aldurcraft.WurmOnline.WurmLogsManager.Searcher;
using System.Threading.Tasks;
using Aldurcraft.Utility;
using Aldurcraft.WurmOnline.WurmState;
using Aldurcraft.WurmOnline.Utility;
using System.Text.RegularExpressions;
using Aldurcraft.Utility.PopupNotify;
using System.Runtime.Serialization;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Granger
{
    public class LogFeedManager : IDisposable
    {
        [DataContract]
        public struct CachedAHSkillID
        {
            [DataMember]
            public readonly WurmState.WurmServer.ServerInfo.ServerGroup ServerGroup;
            [DataMember]
            public readonly string PlayerName;

            public CachedAHSkillID(WurmState.WurmServer.ServerInfo.ServerGroup serverGroup, string playerName)
            {
                ServerGroup = serverGroup;
                PlayerName = playerName;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is CachedAHSkillID)) return false;
                var other = (CachedAHSkillID)obj;
                return Equals(other);
            }

            public bool Equals(CachedAHSkillID other)
            {
                return ServerGroup == other.ServerGroup && PlayerName == other.PlayerName;
            }

            public override int GetHashCode()
            {
                return unchecked((
                    PlayerName == null ? string.Empty.GetHashCode() : PlayerName.GetHashCode()) * ServerGroup.GetHashCode());
            }
        }

        public class SkillObtainedEventArgs : EventArgs
        {
            public readonly string PlayerName;
            public SkillObtainedEventArgs(string playerName)
            {
                this.PlayerName = playerName;
            }
        }

        readonly GrangerContext _context;
        readonly ModuleGranger _parentModule;
        readonly Dictionary<string, PlayerManager> _playerManagers = new Dictionary<string, PlayerManager>();

        public LogFeedManager(ModuleGranger parentModule, GrangerContext context)
        {
            _parentModule = parentModule;
            _context = context;
        }

        public void RegisterPlayer(string playerName)
        {
            if (!_playerManagers.ContainsKey(playerName))
            {
                _playerManagers[playerName] = new PlayerManager(_parentModule, _context, playerName);
            }
        }


        public void UnregisterPlayer(string playerName)
        {
            PlayerManager ph;
            if (_playerManagers.TryGetValue(playerName, out ph))
            {
                ph.Dispose();
                _playerManagers.Remove(playerName);
            }
        }

        /// <summary>
        /// null if no skill available yet (ah skill search not finished or server group not established)
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns></returns>
        public float? GetSkillForPlayer(string playerName)
        {
            PlayerManager ph;
            if (_playerManagers.TryGetValue(playerName, out ph))
            {
                return ph.GetAhSkill();
            }
            else return null;
        }

        public void Dispose()
        {
            foreach (var keyval in _playerManagers)
            {
                keyval.Value.Dispose();
            }
        }

        internal void UpdatePlayers(List<string> list)
        {
            foreach (var player in list)
            {
                if (!_playerManagers.ContainsKey(player))
                {
                    RegisterPlayer(player);
                }
            }

            var managedPlayers = _playerManagers.Keys.ToArray();
            foreach (var player in managedPlayers)
            {
                if (!list.Contains(player))
                    UnregisterPlayer(player);
            }
        }

        internal void Update()
        {
            foreach (var keyval in _playerManagers)
            {
                keyval.Value.Update();
            }
        }
    }
}
