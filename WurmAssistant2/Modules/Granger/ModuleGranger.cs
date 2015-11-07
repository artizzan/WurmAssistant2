using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Aldurcraft.Utility;
using System.IO;
using System.Data.SQLite;
using WurmAssistantDataTransfer.Dtos;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Granger
{
    public class ModuleGranger : AssistantModule
    {
        public PersistentObject<GrangerSettings> Settings;
        FormGrangerMain GrangerUI;
        GrangerContext Context;

        LogFeedManager LogFeedMan;

        public override void Initialize()
        {
            base.Initialize();
            Settings = new PersistentObject<GrangerSettings>(new GrangerSettings());
            Settings.SetFilePathAndLoad(Path.Combine(base.ModuleDataDir, "settings.xml"));

            //init database
            DBSchema.SetConnectionString(Path.Combine(this.ModuleDataDir, "grangerDB.s3db"));

            SQLiteHelper.CreateTableIfNotExists(DBSchema.HorsesSchema, DBSchema.HorsesTableName, DBSchema.ConnectionString);
            SQLiteHelper.ValidateTable(DBSchema.HorsesSchema, DBSchema.HorsesTableName, DBSchema.ConnectionString);

            SQLiteHelper.CreateTableIfNotExists(DBSchema.TraitValuesSchema, DBSchema.TraitValuesTableName, DBSchema.ConnectionString);
            SQLiteHelper.ValidateTable(DBSchema.TraitValuesSchema, DBSchema.TraitValuesTableName, DBSchema.ConnectionString);

            SQLiteHelper.CreateTableIfNotExists(DBSchema.HerdsSchema, DBSchema.HerdsTableName, DBSchema.ConnectionString);
            SQLiteHelper.ValidateTable(DBSchema.HerdsSchema, DBSchema.HerdsTableName, DBSchema.ConnectionString);

            Context = new GrangerContext(new SQLiteConnection(DBSchema.ConnectionString));

            GrangerUI = new FormGrangerMain(this, Settings, Context);

            LogFeedMan = new LogFeedManager(this, Context);
            LogFeedMan.UpdatePlayers(Settings.Value.CaptureForPlayers);
            GrangerUI.Granger_PlayerListChanged += GrangerUI_Granger_PlayerListChanged;
        }

        void GrangerUI_Granger_PlayerListChanged(object sender, EventArgs e)
        {
            LogFeedMan.UpdatePlayers(Settings.Value.CaptureForPlayers);
        }

        public override void Update(bool engineSleeping)
        {
            Settings.Update();
            LogFeedMan.Update();
        }

        public override void Stop()
        {
            if (GrangerUI != null) GrangerUI.SaveAllState();
            else Logger.LogError("Granger UI null when trying to save state on Stop", this);
            LogFeedMan.Dispose();
            Settings.Save();
        }

        public override void PopulateDataTransfer(WurmAssistantDto settingsDto)
        {
            var allCreatures = Context.Horses.Select(entity => new Creature()
            {
                BirthDate = entity.BirthDate,
                BrandedFor = entity.BrandedFor,
                Comments = entity.Comments,
                CreatureAge = entity.Age.ToString(),
                CreatureColor = entity.Color.ToString(),
                CreatureTraits = entity.Traits.Select(trait => trait.ToString()).ToArray(),
                EpicCurve = entity.EpicCurve,
                FatherName = entity.FatherName,
                GlobalId = null,
                GroomedOn = entity.GroomedOn,
                HerdGlobalId = null,
                HerdId = entity.Herd,
                IsMale = entity.IsMale,
                LocalId = entity.ID,
                MotherName = entity.MotherName,
                Name = entity.Name,
                NotInMood = entity.NotInMood,
                PairedWith = entity.PairedWith,
                PregnantUntil = entity.PregnantUntil,
                ServerName = null,
                SpecialTags = entity.SpecialTags.ToArray(),
                TakenCareOfBy = entity.TakenCareOfBy,
                TraitsInspectedAtSkill = entity.TraitsInspectedAtSkill
            });

            settingsDto.Creatures.AddRange(allCreatures);
        }

        public override void OpenUI(object sender, EventArgs e)
        {
            if (GrangerUI != null) GrangerUI.ShowThisDarnWindowDammitEx(); //GrangerUI.BringBackFromAbyss();
            else
            {
                Logger.LogCritical("GrangerUI was null", this);
            }
        }

        internal static void ShowPopup(string p)
        {
            Aldurcraft.Utility.PopupNotify.Popup.Schedule(p);
        }
    }
}
