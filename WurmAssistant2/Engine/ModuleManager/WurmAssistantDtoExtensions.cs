using System.Linq;
using Aldurcraft.Utility.SoundEngine;
using WurmAssistantDataTransfer.Dtos;

namespace Aldurcraft.WurmOnline.WurmAssistant2
{
    public static class WurmAssistantDtoExtensions
    {
        public static Sound TryMergeSoundAndGet(this WurmAssistantDto dto, string soundName)
        {
            if (string.IsNullOrEmpty(soundName)) return null;

            Sound sound = dto.Sounds.FirstOrDefault(sound1 => sound1.Name == soundName);
            if (sound == null)
            {
                var soundData = SoundBank.TryGetSoundData(soundName);
                if (soundData != null)
                {
                    sound = new Sound()
                    {
                        Name = soundData.SoundName,
                        FileData = soundData.SoundFileBytes,
                        FileNameWithExt = soundData.SoundFileName
                    };
                    dto.Sounds.Add(sound);
                }
            }
            return sound;
        }
    }
}