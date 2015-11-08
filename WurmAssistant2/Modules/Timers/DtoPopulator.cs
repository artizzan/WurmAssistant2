using System;
using System.Collections.Generic;
using System.Linq;
using WurmAssistantDataTransfer.Dtos;

namespace Aldurcraft.WurmOnline.WurmAssistant2.ModuleNS.Timers
{
    class DtoPopulator
    {
        private readonly IEnumerable<PlayerTimersGroup> timersGroups;

        public DtoPopulator(IEnumerable<PlayerTimersGroup> timersGroups)
        {
            if (timersGroups == null) throw new ArgumentNullException("timersGroups");
            this.timersGroups = timersGroups;
        }

        public void Populate(WurmAssistantDto dto)
        {
            PopulateCustomTimerDefinitions(dto);
            PopulateTimers(dto);
        }

        private void PopulateCustomTimerDefinitions(WurmAssistantDto dto)
        {
            var customTimerDescriptors =
                WurmTimerDescriptors.Descriptors.Where(type => type.UnderlyingTimerType == typeof (CustomTimer)).ToArray();
            foreach (var customTimerDescriptor in customTimerDescriptors)
            {
                dto.LegacyCustomTimerDefinitions.Add(new LegacyCustomTimerDefinition()
                {
                    Name = customTimerDescriptor.NameID,
                    Duration = customTimerDescriptor.Options.Duration,
                    Id = null,
                    IsRegex = customTimerDescriptor.Options.IsRegex,
                    ResetConditions =
                        customTimerDescriptor.Options.ResetConditions != null
                            ? customTimerDescriptor.Options.ResetConditions.Select(
                                condition => new LegacyCustomTimerDefinition.Condition()
                                {
                                    LogType = condition.LogType.ToString().Replace("_", ""),
                                    Pattern = condition.RegexPattern
                                }).ToArray()
                            : new LegacyCustomTimerDefinition.Condition[0],
                    ResetOnUptime = customTimerDescriptor.Options.ResetOnUptime,
                    TriggerConditions =
                        customTimerDescriptor.Options.TriggerConditions != null
                            ? customTimerDescriptor.Options.TriggerConditions.Select(
                                condition => new LegacyCustomTimerDefinition.Condition()
                                {
                                    LogType = condition.LogType.ToString().Replace("_", ""),
                                    Pattern = condition.RegexPattern
                                }).ToArray()
                            : new LegacyCustomTimerDefinition.Condition[0],
                    LegacyServerGroupId = customTimerDescriptor.Group.ToString()
                });
            }
        }

        private void PopulateTimers(WurmAssistantDto dto)
        {
            foreach (var ptg in timersGroups)
            {
                var characterName = ptg.Player;

                var timersGroupedByServerGroup = ptg.WurmTimers.GroupBy(timer => timer.TargetServerGroup).ToArray();
                foreach (var grouped in timersGroupedByServerGroup)
                {
                    var serverGroup = grouped.First().TargetServerGroup;
                    foreach (var timer in grouped)
                    {
                        if (timer is AlignmentTimer)
                        {
                            var t = timer as AlignmentTimer;
                            var timerDto = new WurmAssistantDataTransfer.Dtos.AlignmentTimer
                            {
                                DefinitionId = new Guid("6e989db6-d49a-4f1d-956a-a83f4192b058"),
                                Sound = dto.TryMergeSoundAndGet(t.SoundName),
                                Name = t.NameID,
                                PopupDurationMillis = t.PopupDuration,
                                CharacterName = characterName,
                                Id = null,
                                IsWhiteLighter = t.IsWhiteLighter,
                                PersistentPopup = t.PersistentPopup,
                                PopupNotify = t.PopupNotify,
                                PopupOnWaLaunch = t.PopupOnWALaunch,
                                RuntimeTypeIdEnum = "Alignment",
                                SoundNotify = t.SoundNotify,
                                WurmReligion = t.PlayerReligion.ToString(),
                                ServerGroupId = serverGroup.ToString().ToUpperInvariant(),
                                LegacyDefinitionName = null
                            };
                            dto.Timers.Add(timerDto);
                        }

                        if (timer is CustomTimer)
                        {
                            var t = timer as CustomTimer;
                            var timerDto = new WurmAssistantDataTransfer.Dtos.CustomTimer()
                            {
                                DefinitionId = null,
                                LegacyDefinitionName = t.TimerType.NameID,
                                Name = t.NameID,
                                Sound = dto.TryMergeSoundAndGet(t.SoundName),
                                PopupDurationMillis = t.PopupDuration,
                                Id = null,
                                PopupNotify = t.PopupNotify,
                                PersistentPopup = t.PersistentPopup,
                                SoundNotify = t.SoundNotify,
                                CharacterName = characterName,
                                RuntimeTypeIdEnum = "LegacyCustom",
                                PopupOnWaLaunch = t.PopupOnWALaunch,
                                ServerGroupId = serverGroup.ToString().ToUpperInvariant()
                            };
                            dto.Timers.Add(timerDto);
                        }

                        if (timer is JunkSaleTimer)
                        {
                            var t = timer as JunkSaleTimer;
                            var timerDto = new WurmAssistantDataTransfer.Dtos.JunkSaleTimer()
                            {
                                DefinitionId = new Guid("f1238677-3139-41ba-bbee-3b61186a11ab"),
                                Name = t.NameID,
                                Sound = dto.TryMergeSoundAndGet(t.SoundName),
                                PopupDurationMillis = t.PopupDuration,
                                Id = null,
                                PopupNotify = t.PopupNotify,
                                PersistentPopup = t.PersistentPopup,
                                SoundNotify = t.SoundNotify,
                                CharacterName = characterName,
                                RuntimeTypeIdEnum = "JunkSale",
                                PopupOnWaLaunch = t.PopupOnWALaunch,
                                ServerGroupId = serverGroup.ToString().ToUpperInvariant(), 
                                LegacyDefinitionName = null
                            };
                            dto.Timers.Add(timerDto);
                        }

                        if (timer is MeditationTimer)
                        {
                            var t = timer as MeditationTimer;
                            var timerDto = new WurmAssistantDataTransfer.Dtos.MeditationTimer()
                            {
                                LegacyDefinitionName = null,
                                Name = t.NameID,
                                Sound = dto.TryMergeSoundAndGet(t.SoundName),
                                PopupDurationMillis = t.PopupDuration,
                                Id = null,
                                DefinitionId = new Guid("a420f74d-4c3c-474a-89ab-477caf501d8c"),
                                PopupNotify = t.PopupNotify,
                                CharacterName = characterName,
                                RuntimeTypeIdEnum = "Meditation",
                                PersistentPopup = t.PersistentPopup,
                                PopupOnWaLaunch = t.PopupOnWALaunch,
                                ServerGroupId = serverGroup.ToString().ToUpperInvariant(),
                                SoundNotify = t.SoundNotify,
                                ShowMeditCount = t.ShowMeditCount,
                                ShowMeditSkill = t.ShowMeditSkill,
                                SleepBonusPopupDurationMillis = t.SleepBonusPopupDuration,
                                SleepBonusReminder = t.SleepBonusReminder
                            };
                            dto.Timers.Add(timerDto);
                        }

                        if (timer is MeditPathTimer)
                        {
                            var t = timer as MeditPathTimer;
                            var timerDto = new WurmAssistantDataTransfer.Dtos.MeditPathTimer()
                            {
                                LegacyDefinitionName = null,
                                Name = t.NameID,
                                Sound = dto.TryMergeSoundAndGet(t.SoundName),
                                PopupDurationMillis = t.PopupDuration,
                                Id = null,
                                DefinitionId = new Guid("eb0df9dd-f91c-4dad-8aa0-44bf2243d2a2"),
                                PopupNotify = t.PopupNotify,
                                CharacterName = characterName,
                                RuntimeTypeIdEnum = "MeditPath",
                                PersistentPopup = t.PersistentPopup,
                                PopupOnWaLaunch = t.PopupOnWALaunch,
                                ServerGroupId = serverGroup.ToString().ToUpperInvariant(),
                                SoundNotify = t.SoundNotify
                            };
                            dto.Timers.Add(timerDto);
                        }

                        if (timer is PrayerTimer)
                        {
                            var t = timer as PrayerTimer;
                            var timerDto = new WurmAssistantDataTransfer.Dtos.PrayerTimer()
                            {
                                LegacyDefinitionName = null,
                                ServerGroupId = serverGroup.ToString().ToUpperInvariant(),
                                SoundNotify = t.SoundNotify,
                                Name = t.NameID,
                                Sound = dto.TryMergeSoundAndGet(t.SoundName),
                                PopupDurationMillis = t.PopupDuration,
                                Id = null,
                                DefinitionId = new Guid("52caa2c2-fa72-4d57-8887-d06b8b1db898"),
                                PopupNotify = t.PopupNotify,
                                CharacterName = characterName,
                                RuntimeTypeIdEnum = "Prayer",
                                PersistentPopup = t.PersistentPopup,
                                PopupOnWaLaunch = t.PopupOnWALaunch,
                                FavorNotifyOnLevel = t.Settings.Value.FavorSettings.FavorNotifyOnLevel,
                                FavorNotifyPopupEnabled = t.Settings.Value.FavorSettings.FavorNotifyPopup,
                                FavorNotifyPopupPersist = t.Settings.Value.FavorSettings.FavorNotifyPopupPersist,
                                FavorNotifySound = dto.TryMergeSoundAndGet(t.Settings.Value.FavorSettings.FavorNotifySoundName),
                                FavorNotifySoundEnabled = t.Settings.Value.FavorSettings.FavorNotifySound,
                                FavorNotifyWhenMax = t.Settings.Value.FavorSettings.FavorNotifyWhenMAX
                            };
                            dto.Timers.Add(timerDto);
                        }

                        if (timer is SermonTimer)
                        {
                            var t = timer as SermonTimer;
                            var timerDto = new WurmAssistantDataTransfer.Dtos.SermonTimer()
                            {
                                LegacyDefinitionName = null,
                                Name = t.NameID,
                                Sound = dto.TryMergeSoundAndGet(t.SoundName),
                                PopupDurationMillis = t.PopupDuration,
                                ServerGroupId = serverGroup.ToString().ToUpperInvariant(),
                                Id = null,
                                DefinitionId = new Guid("d080c694-f1d1-43d9-b1b2-6edf0147fbf3"),
                                PopupNotify = t.PopupNotify,
                                CharacterName = characterName,
                                RuntimeTypeIdEnum = "Sermon",
                                PersistentPopup = t.PersistentPopup,
                                PopupOnWaLaunch = t.PopupOnWALaunch,
                                SoundNotify = t.SoundNotify
                            };
                            dto.Timers.Add(timerDto);
                        }
                    }
                }
            }
        }
    }
}