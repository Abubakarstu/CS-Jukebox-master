﻿using System;
using System.Windows.Forms;
using WMPLib;

namespace CS_Jukebox
{
    public class Jukebox
    {
        private WindowsMediaPlayer player;
        private SongProfile currentSong;

        private Timer fadeTimer;

        private bool isPlaying = false;
        private bool shouldStop = false;
        private int timerCount = 0;
        private int timerGoal = 0;
        private int active = 0;
        private int delayedCount = 0;
        private float fadeVolume;
        private float volumeIncrement; //Incremental change in volume when fading out song.

        public Jukebox()
        {
            player = new WindowsMediaPlayer();

            SetupTimer();
        }

        public void PlaySong(string path)
        {
            player.URL = path;
            player.controls.play();
        }

        //Play song for length or loop indefinitely
        public void PlaySong(SongProfile song, bool loop)
        {
            if (song.Path == "") return;

            float volume = ((float)Properties.MasterVolume / 100) * (float)song.Volume * active;
            currentSong = song;

            player.settings.volume = (int)volume;
            player.URL = song.Path;
            player.controls.currentPosition = song.Start;
            player.controls.play();
            player.settings.setMode("loop", loop);
        }

        //Play song with a determined amount of time in seconds
        public void PlaySong(SongProfile song, bool loop, int duration)
        {
            PlaySong(song, loop);

            timerCount = 0;
            timerGoal = duration;
            isPlaying = true;
        }

        public void UpdateVolume()
        {
            if (currentSong == null) return;
            float volume = ((float)Properties.MasterVolume / 100) * currentSong.Volume * active;
            player.settings.volume = (int)volume;
        }

        //Sets shouldStop to true so that on the next on the next timer tick,
        //StopSong() will be called. The fadeTimer only starts if it is done like this.
        public void Stop()
        {
            shouldStop = true;
        }

        private void StopSong()
        {
            int fadeTime = 2;
            float startVolume = player.settings.volume;
            fadeVolume = startVolume;
            volumeIncrement = startVolume / ((1000 / 8) * fadeTime);
            
            timerCount = 0;

            fadeTimer = new Timer();
            fadeTimer.Interval = 8;
            fadeTimer.Tick += new EventHandler(FadeTimerTick);
            fadeTimer.Start();
        }

        private void FadeTimerTick(object sender, EventArgs e)
        {
            if (fadeVolume > 0)
            {
                fadeVolume -= volumeIncrement;
                player.settings.volume = (int)fadeVolume;
                Console.WriteLine(fadeVolume);
            }
            else
            {
                player.controls.stop();
                fadeTimer.Stop();
                isPlaying = false;
            }
        }

        private void SetupTimer()
        {
            Timer songTimer = new Timer();
            songTimer.Interval = 1000;
            songTimer.Tick += new EventHandler(TimerTick);
            songTimer.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            timerCount += 1;

            if (shouldStop)
            {
                StopSong();
                shouldStop = false;
            }

            if (isPlaying && timerCount >= timerGoal)
            {
                StopSong();
            }

            //Check if csgo is focused
            if (WinAPI.GetActiveProcess() == "csgo")
            {
                active = 1;
            }
            else
            {
                active = 0;
            }

            if (delayedCount >= 2)
            {
                UpdateVolume();
            }
            else
            {
                delayedCount++;
            }
        }
    }
}
