using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace Sayohime.Main
{
	public static class Audio
	{
		public static Dictionary<GameMusic, uint[]> MUSIC_LOOP_POINTS = new Dictionary<GameMusic, uint[]>()
		{
			// { GameMusic.Victory,  new uint[] { 3200, 28800 } }
		};

		private static GameMusic currentMusic = GameMusic.None;

		private static float soundVolume = 1.0f;
		private static float musicVolume = 1.0f;

		public static void ApplySettings()
		{
			//SoundVolume = Settings.GetProgramSetting<int>("SoundVolume") / 100.0f;
			//MusicVolume = Settings.GetProgramSetting<int>("MusicVolume") / 100.0f;

			MediaPlayer.Volume = musicVolume;
		}

		public static void PlayMusic(GameMusic musicType, bool looping = true)
		{
			if (musicType == currentMusic) return;
			if (musicType == GameMusic.None)
			{
				StopMusic();
				return;
			}

			currentMusic = musicType;

			MediaPlayer.Play(AssetCache.MUSIC[musicType]);
			MediaPlayer.IsRepeating = looping;
		}

		public static void PlayMusic(string[] scriptTokens)
		{
			bool looping = true;
			if (scriptTokens.Length > 2) looping = bool.Parse(scriptTokens[2]);
			PlayMusic((GameMusic)Enum.Parse(typeof(GameMusic), scriptTokens[1]), looping);
		}

		public static void PauseMusic(bool pause)
		{
			MediaPlayer.Pause();
		}

		public static void StopMusic()
		{
			MediaPlayer.Stop();

			currentMusic = GameMusic.None;
		}

		public static SoundEffectInstance PlaySound(GameSound soundType, float volume = 0.5f, float pitch = 0.0f, float pan = 0.0f)
		{
			SoundEffectInstance soundEffectInstance = AssetCache.SOUNDS[soundType].CreateInstance();

			soundEffectInstance.Volume = soundVolume * volume;
			soundEffectInstance.Pitch = pitch;
			soundEffectInstance.Pan = pan;
			soundEffectInstance.Play();

			return soundEffectInstance;
		}

		public static void PlaySound(string[] scriptTokens)
		{
			float volume = 0.5f;
			float pitch = 0.0f;
			float pan = 0.0f;

			if (scriptTokens.Length > 2) volume = float.Parse(scriptTokens[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
			if (scriptTokens.Length > 3) pan = float.Parse(scriptTokens[3], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
			if (scriptTokens.Length > 4) pitch = float.Parse(scriptTokens[4], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture);
			PlaySound((GameSound)Enum.Parse(typeof(GameSound), scriptTokens[1]), volume, pan, pitch);
		}

		public static float SoundVolume { set => soundVolume = value; get => soundVolume; }
		public static float MusicVolume
		{
			set
			{
				musicVolume = value;
				MediaPlayer.Volume = musicVolume;
			}

			get => musicVolume;
		}

		public static GameMusic CurrentMusic { get => currentMusic; }
	}
}
