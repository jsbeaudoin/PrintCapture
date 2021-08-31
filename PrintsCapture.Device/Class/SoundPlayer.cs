namespace PrintsCapture.Device
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading;
    using System.Windows;

    public enum DeviceSound
    {
        Beep,
        Error
    }

    /// <summary>
    /// This class is useful for devices that do not support direct sound output (I3)
    /// </summary>
    public static class DeviceSoundPlayer
    {
        private class SoundPlayerData
        {
            public SoundPlayerData(System.Media.SoundPlayer player, DeviceSound sound)
            {
                this.Player = player;
                this.Sound = sound;
                this.IsPlaying = false;
            }

            public System.Media.SoundPlayer Player { get; private set; }

            // ReSharper disable UnusedAutoPropertyAccessor.Local
            public DeviceSound Sound { get; private set; }
            // ReSharper restore UnusedAutoPropertyAccessor.Local

            public bool IsPlaying { get; set; }            
        }

        #region fields        

        private static readonly Dictionary<DeviceSound, SoundPlayerData> SoundPlayers;

        #endregion

        #region Properties
        
        public static bool IsMuted { get; set; }
        
        #endregion

        #region Constructor

        static DeviceSoundPlayer()
        {
            SoundPlayers = new Dictionary<DeviceSound, SoundPlayerData>();
            
            Reset();
        }

        #endregion

        #region Public methods        

        /// <summary>
        /// Plays a sound on the Computer Speakers
        /// </summary>
        /// <param name="sound">Kind of sound</param>
        /// <param name="nbTimes">Number of times to play the sound</param>
        /// <param name="msInterval">Interval to pause between playing time</param>
        public static void Play(DeviceSound sound, int nbTimes = 1, int msInterval = 100)
        {
            if (IsMuted || SoundPlayers[sound].IsPlaying)
            {
                return;
            }
            
            SoundPlayers[sound].IsPlaying = true;
            
            var bgWorker = new BackgroundWorker();
            bgWorker.DoWork += (sender, args) =>
            {                
                for (int i = 0; i < nbTimes; i++)
                {
                    if (!SoundPlayers[sound].IsPlaying)
                    {
                        return;
                    }

                    SoundPlayers[sound].Player.PlaySync();
                    if (i < nbTimes - 1 && msInterval > 0)
                    {
                        Thread.Sleep(msInterval);                    
                    }
                    
                }
                SoundPlayers[sound].IsPlaying = false;
            };
            bgWorker.RunWorkerAsync();
        }        

        /// <summary>
        /// Stop all sounds being played
        /// </summary>
        public static void Stop()
        {            
            foreach (var playerData in SoundPlayers)
            {
                try
                {
                    if (playerData.Value.IsPlaying)
                    {
                        playerData.Value.IsPlaying = false;
                        playerData.Value.Player.Stop();
                    }                    
                }
                catch
                {
                    // nothing
                }                
            }
        }

        /// <summary>
        /// Reset the sound player. Stop sounds, reinitialize all sounds player
        /// </summary>
        public static void Reset()
        {            

            Stop();
            foreach (var playerData in SoundPlayers)
            {
                playerData.Value.Player.Dispose();                
            }

            IsMuted = false;

            SoundPlayers.Clear();
            SoundPlayers.Add(DeviceSound.Beep, GetNewSoundPlayer(new Uri("pack://application:,,,/PrintsCapture.Device;component/Sounds/Beep.wav"), DeviceSound.Beep));
            SoundPlayers.Add(DeviceSound.Error, GetNewSoundPlayer(new Uri("pack://application:,,,/PrintsCapture.Device;component/Sounds/ErrorBeep.wav"), DeviceSound.Error));            
        }

        #endregion

        #region Private methods

        private static SoundPlayerData GetNewSoundPlayer(Uri uriPath, DeviceSound sound)
        {
            var player = new System.Media.SoundPlayer();
            var streamInfo = Application.GetResourceStream(uriPath);
            if (streamInfo == null)
            {
                return null;
            }

            player.Stream = streamInfo.Stream;
            player.LoadAsync();

            return new SoundPlayerData(player, sound);
        }

        #endregion
    }
}
