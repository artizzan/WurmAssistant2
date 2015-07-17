using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Aldurcraft.Utility;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Granger
{
    [DataContract]
    public class GrangerSettings
    {
        [DataMember]
        public TimeSpan ShowGroomingTime;

        /// <summary>
        /// By default horses can't be updated if wrong herds are selected.
        /// This option makes update possible as long, as horse name
        /// is unique in ENTIRE database
        /// </summary>
        [DataMember]
        public bool DoNotBlockDataUpdateUnlessMultiplesInEntireDb;

        [DataMember]
        public bool UpdateHorseDataFromAnyEventLine;

        [DataMember]
        public bool DoNotShowReadFirstWindow;

        [DataMember]
        public bool TraitViewVisible;
        [DataMember]
        public bool HerdViewVisible;
        [DataMember]
        public bool LogCaptureEnabled;

        //capture for players
        [DataMember]
        public List<string> CaptureForPlayers;

        [DataMember]
        public System.Drawing.Size MainWindowSize;

        [DataMember]
        public string ValuePresetID;
        [DataMember]
        public string AdvisorID;

        [DataMember]
        public byte[] HorseListState;

        [DataMember]
        public TraitViewManager.TraitDisplayMode TraitViewDisplayMode;
        [DataMember]
        public int HerdViewSplitterPosition;
        [DataMember]
        public byte[] TraitViewState;

        [DataMember]
        private Dictionary<LogFeedManager.CachedAHSkillID, float> CachedAHSkillVals;

        public bool TryGetAHSkill(LogFeedManager.CachedAHSkillID id, out float result)
        {
            return CachedAHSkillVals.TryGetValue(id, out result);
        }

        public void SetAHSkill(LogFeedManager.CachedAHSkillID id, float AHValue)
        {
            CachedAHSkillVals[id] = AHValue;
        }

        [DataMember]
        private Dictionary<LogFeedManager.CachedAHSkillID, DateTime> LastAHSkillCheck;

        public bool TryGetAHCheckDate(LogFeedManager.CachedAHSkillID id, out DateTime result)
        {
            return LastAHSkillCheck.TryGetValue(id, out result);
        }

        public void SetAHCheckDate(LogFeedManager.CachedAHSkillID id, DateTime AHValue)
        {
            LastAHSkillCheck[id] = AHValue;
        }

        [DataMember]
        private Dictionary<string, DateTime> GenesisLog;

        internal void AddGenesisCast(DateTime castDate, string horseName)
        {
            List<string> keysToRemove = null;
            var dtTreshhold = DateTime.Now - TimeSpan.FromHours(1);
            foreach (var keyval in GenesisLog)
            {
                if (keyval.Value < dtTreshhold)
                {
                    if (keysToRemove == null) keysToRemove = new List<string>();
                    keysToRemove.Add(keyval.Key);
                }
            }
            if (keysToRemove != null)
            {
                foreach (var horsename in keysToRemove)
                {
                    GenesisLog.Remove(horsename);
                    Logger.LogInfo(string.Format("Removed cached genesis cast data for {0}", horsename), this);
                }
            }
            GenesisLog[horseName] = castDate;
        }

        internal bool HasGenesisCast(string horseName)
        {
            DateTime castTime;
            if (GenesisLog.TryGetValue(horseName, out castTime))
            {
                if (castTime > DateTime.Now - TimeSpan.FromHours(1))
                {
                    return true;
                }
            }
            return false;
        }

        internal void RemoveGenesisCast(string horseName)
        {
            GenesisLog.Remove(horseName);
        }


        [DataMember]
        private Dictionary<Type, object> _BreedingEvalOptionsDict;

        /// <summary>
        /// null if none available
        /// </summary>
        /// <param name="optionsType"></param>
        /// <returns></returns>
        public object GetBreedingEvalOptions(Type optionsType)
        {
            try
            {
                object result = null;
                _BreedingEvalOptionsDict.TryGetValue(optionsType, out result);
                return result;
            }
            catch (Exception _e)
            {
                Logger.LogError("GetBreedingEvalOptions", this, _e);
                throw;
            }
        }
        public void SetBreedingEvalOptions(Type optionsType, object options)
        {
            try
            {
                _BreedingEvalOptionsDict[optionsType] = options;
            }
            catch (Exception _e)
            {
                Logger.LogError("SetBreedingEvalOptions", this, _e);
                throw;
            }
        }

        void InitMe()
        {
            DoNotShowReadFirstWindow = false;

            TraitViewVisible = true;
            HerdViewVisible = true;
            LogCaptureEnabled = true;

            MainWindowSize = new System.Drawing.Size(784, 484);

            CaptureForPlayers = new List<string>();

            ValuePresetID = string.Empty;
            AdvisorID = string.Empty;

            HerdViewSplitterPosition = 250;

            CachedAHSkillVals = new Dictionary<LogFeedManager.CachedAHSkillID, float>();
            LastAHSkillCheck = new Dictionary<LogFeedManager.CachedAHSkillID, DateTime>();
            _BreedingEvalOptionsDict = new Dictionary<Type, object>();
            GenesisLog = new Dictionary<string, DateTime>();

            ShowGroomingTime = TimeSpan.FromMinutes(60);
            UpdateHorseDataFromAnyEventLine = true;
        }

        public GrangerSettings()
        {
            InitMe();
        }

        [OnDeserializing]
        public void OnDes(StreamingContext context)
        {
            InitMe();
        }
    }
}
