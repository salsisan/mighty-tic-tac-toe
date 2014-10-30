using System;
using System.Collections.Generic;
using SharpDX.IO;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace Mighty_Tick_Tac_Toe
{
    public class WavePlayer : IDisposable
    {
        public bool IsSoundOn { get; set; }

        private readonly XAudio2 xAudio;
        readonly Dictionary<string, MyWave> sounds;

        public WavePlayer()
        {
            xAudio = new XAudio2();
            xAudio.StartEngine();
            var masteringVoice = new MasteringVoice(xAudio);
            masteringVoice.SetVolume(1);
            sounds = new Dictionary<string, MyWave>();
        }

        // Reads a sound and puts it in the dictionary
        public void AddWave(string key, string filepath)
        {
            var wave = new MyWave();

            var nativeFileStream = new NativeFileStream(filepath, NativeFileMode.Open, NativeFileAccess.Read);
            var soundStream = new SoundStream(nativeFileStream);
            var buffer = new AudioBuffer { Stream = soundStream, AudioBytes = (int)soundStream.Length, Flags = BufferFlags.EndOfStream };

            wave.Buffer = buffer;
            wave.DecodedPacketsInfo = soundStream.DecodedPacketsInfo;
            wave.WaveFormat = soundStream.Format;

            sounds.Add(key, wave);
        }

        // Plays the sound
        public void PlayWave(string key)
        {
            if (!sounds.ContainsKey(key)) return;
            var w = sounds[key];

            var sourceVoice = new SourceVoice(xAudio, w.WaveFormat);
            sourceVoice.SubmitSourceBuffer(w.Buffer, w.DecodedPacketsInfo);
            sourceVoice.Start();
        }

        public void Dispose()
        {
            xAudio.Dispose();
        }
    }

    public class MyWave
    {
        public AudioBuffer Buffer { get; set; }
        public uint[] DecodedPacketsInfo { get; set; }
        public WaveFormat WaveFormat { get; set; }
    }
}