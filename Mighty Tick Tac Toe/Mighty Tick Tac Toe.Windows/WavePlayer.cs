using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace Mighty_Tick_Tac_Toe
{
    public class WavePlayer : IDisposable
    {
        public bool IsSoundOn { get; set; }

        private readonly MediaElement _media;
        readonly Dictionary<string, Windows.Storage.StorageFile> sounds;

        public WavePlayer()
        {
            _media = new MediaElement();
            sounds = new Dictionary<string, Windows.Storage.StorageFile>();
        }

        // Reads a sound and puts it in the dictionary
        public async void AddWave(string key, string filepath)
        {
            var folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
            var file = await folder.GetFileAsync(filepath);

            sounds.Add(key, file);
        }

        // Plays the sound
        public async void PlayWave(string key)
        {
            if (!sounds.ContainsKey(key)) return;
            var soundFile = sounds[key];

            var stream = await soundFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
            _media.SetSource(stream, soundFile.ContentType);
            _media.Play();
        }

        public void Dispose()
        {
        }
    }
}