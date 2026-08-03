using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;

namespace Golfbutsmaller
{
    /**
* Manages game audio and music playback
*/
    public static class SoundManager
    {
        // Audio collections
        private static Dictionary<string, SoundEffect> _soundEffects = new();
        private static Dictionary<string, SoundEffectInstance> _musicInstances = new();

        // Volume controls
        private static float _masterVolume = 1.0f;
        private static float _sfxVolume = 1.0f;

        // Volume property handlers
        public static float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Math.Clamp(value, 0f, 1f);
                UpdateVolumes();
            }
        }

        public static float SFXVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Math.Clamp(value, 0f, 1f);
                UpdateVolumes();
            }
        }

        /**
         * Loads all game audio assets
         */
        public static void LoadContent(ContentManager content)
        {
            try
            {
                _soundEffects.Clear();

                // Load sound effects
                LoadSoundEffect(content, "coin-flip");
                LoadSoundEffect(content, "coin-land");
                LoadSoundEffect(content, "mario-boing", "wall_hit");
                LoadSoundEffect(content, "mario-boing", "obstacle_hit");
                LoadSoundEffect(content, "nice-shot", "score");
                LoadSoundEffect(content, "win-theme", "win");

                // Load music tracks
                LoadSoundEffect(content, "NFL");
                LoadSoundEffect(content, "background-music");

                Console.WriteLine("SoundManager: All sound effects loaded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SoundManager LoadContent error: {ex.Message}");
            }
        }

        /**
         * Plays background music track
         */
        public static void PlayMusic(string soundName, bool looped = true)
        {
            try
            {
                if (GameSettings.SoundEnabled && _soundEffects.ContainsKey(soundName))
                {
                    StopMusic(); // Stop current music
                    var instance = _soundEffects[soundName].CreateInstance();
                    instance.IsLooped = looped;
                    instance.Volume = _masterVolume * _sfxVolume;
                    instance.Play();
                    _musicInstances[soundName] = instance;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing music {soundName}: {ex.Message}");
            }
        }

        /**
         * Stops all playing music
         */
        public static void StopMusic()
        {
            try
            {
                foreach (var instance in _musicInstances.Values)
                {
                    instance.Stop();
                    instance.Dispose();
                }
                _musicInstances.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping music: {ex.Message}");
            }
        }

        /**
         * Loads individual sound effect
         */
        private static void LoadSoundEffect(ContentManager content, string assetName, string soundName = null)
        {
            try
            {
                soundName = soundName ?? assetName;
                _soundEffects[soundName] = content.Load<SoundEffect>(assetName);
                Console.WriteLine($"Loaded sound effect: {soundName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading sound effect {assetName}: {ex.Message}");
            }
        }

        /**
         * Plays one-shot sound effect
         */
        public static void PlaySound(string soundName)
        {
            try
            {
                if (GameSettings.SoundEnabled && _soundEffects.ContainsKey(soundName))
                {
                    var instance = _soundEffects[soundName].CreateInstance();
                    instance.Volume = _masterVolume * _sfxVolume;
                    instance.Play();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing sound {soundName}: {ex.Message}");
            }
        }

        /**
         * Updates volume levels for all audio
         */
        private static void UpdateVolumes()
        {
            SoundEffect.MasterVolume = _masterVolume;
            foreach (var instance in _musicInstances.Values)
            {
                instance.Volume = _masterVolume * _sfxVolume;
            }
        }

        /**
         * Cleans up audio resources
         */
        public static void UnloadContent()
        {
            try
            {
                StopMusic();
                foreach (var effect in _soundEffects.Values)
                {
                    effect.Dispose();
                }
                _soundEffects.Clear();
                Console.WriteLine("SoundManager: All sound effects unloaded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unloading sound content: {ex.Message}");
            }
        }
    }
}
