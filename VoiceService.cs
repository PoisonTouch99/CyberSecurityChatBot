using System;
using System.IO;
using System.Media;
using System.Windows;

namespace CyberSecurityChatBot
{
    /// <summary>
    /// Handles audio playback for the voice greeting feature (Part 1 / Part 2 requirement).
    /// Plays a WAV file using System.Media.SoundPlayer.
    /// </summary>
    public class VoiceService
    // Note: System.Media.SoundPlayer only supports WAV files, so ensure the greeting audio is in WAV format.
    {
        private readonly string _wavFilePath;

        public VoiceService()
        {
            // FIX: folder is "Resource" (not "Resources") — matches the actual folder name in project
            _wavFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource", "greeting.wav");
        }

        /// <summary>
        /// Plays the voice greeting if the WAV file exists.
        /// Fails silently to avoid crashing the app if the file is missing.
        /// </summary>
        public void PlayGreeting()
        {
            try
            {
                if (File.Exists(_wavFilePath))
                {
                    var player = new SoundPlayer(_wavFilePath);
                    player.Play(); // async, non-blocking
                }
                // If file missing, skip silently — no crash
            }
            catch
            {
                // Fail silently
            }
        }
    }
}
