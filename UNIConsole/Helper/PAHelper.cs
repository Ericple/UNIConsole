using System;
using System.Media;

namespace UNIConsole.Helper
{
    internal class PAHelper
    {
        public static void PlayFASound(string soundName, int enable, string soundpackName)
        {
            if (enable != 1) return;
            SoundPlayer player = new SoundPlayer
            {
                SoundLocation = Environment.CurrentDirectory + "\\soundpack\\" + soundpackName + "\\fa\\" + soundName + ".wav"
            };
            TryPlay(player);
        }
        public static void PlaySysSound(string soundName, string soundpackName)
        {
            SoundPlayer player = new SoundPlayer
            {
                SoundLocation = Environment.CurrentDirectory + "\\soundpack\\" + soundpackName + "system\\" + soundName + ".wav"
            };
            TryPlay(player);
        }
        public static void PlayCoSound(string soundName, string soundpackName)
        {
            SoundPlayer player = new SoundPlayer
            {
                SoundLocation = Environment.CurrentDirectory + "\\soundpack\\" + soundpackName + "copilot\\" + soundName + ".wav"
            };
            TryPlay(player);
        }
        private static void TryPlay(SoundPlayer player)
        {
            try
            {
                player.Play();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
    }
}
