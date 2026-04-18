using Plugin.Maui.Audio;
using Strada.Shared.Services;

namespace Strada.Services;

public class SoundService : ISoundService
{
    public async Task PlaySound(string soundFileName) =>
        AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync(soundFileName)).Play();
}
