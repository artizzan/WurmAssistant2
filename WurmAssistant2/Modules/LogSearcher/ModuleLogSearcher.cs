using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aldurcraft.WurmOnline.WurmLogsManager.Searcher;
using Aldurcraft.Utility;
using WurmAssistantDataTransfer.Dtos;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.LogSearcher
{
    class ModuleLogSearcher : AssistantModule
    {
        public override void Initialize()
        {
            base.Initialize();
        }

        public override void OpenUI(object sender, EventArgs e)
        {
            try
            {
                WurmLogSearcherAPI.ToggleUI();
            }
            catch (Exception _e)
            {
                Logger.LogError("Could not open LogSearcher", this, _e);
            }
        }

        public override void PopulateDataTransfer(WurmAssistantDto settingsDto)
        {
            base.PopulateDataTransfer(settingsDto);
        }
    }
}
