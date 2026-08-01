using Godot;

namespace Schicksalswurf.Core
{
    /// <summary>
    /// Ambient sound system: plays looping background sounds for dungeons and music.
    /// </summary>
    public partial class AmbientSoundSystem : Node
    {
        private AudioStreamPlayer _ambientPlayer;
        private AudioStreamPlayer _musicPlayer;
        private bool _enabled = true;

        public override void _Ready()
        {
            _ambientPlayer = new AudioStreamPlayer { Name = "AmbientPlayer" };
            AddChild(_ambientPlayer);

            _musicPlayer = new AudioStreamPlayer { Name = "MusicPlayer" };
            AddChild(_musicPlayer);
        }

        public void PlayDungeonAmbient()
        {
            if (!_enabled) return;
            // Generate a low rumbling ambient tone
            PlayAmbientTone(40, 2.0f, 0.05f, true);
        }

        public void PlayCombatMusic()
        {
            if (!_enabled) return;
            // Generate a tense combat tone
            PlayMusicTone(220, 0.5f, 0.08f, true);
        }

        public void PlayTownMusic()
        {
            if (!_enabled) return;
            // Generate a calm town tone
            PlayMusicTone(330, 1.0f, 0.06f, true);
        }

        public void StopAmbient()
        {
            _ambientPlayer.Stop();
        }

        public void StopMusic()
        {
            _musicPlayer.Stop();
        }

        public void StopAll()
        {
            _ambientPlayer.Stop();
            _musicPlayer.Stop();
        }

        public void SetEnabled(bool enabled)
        {
            _enabled = enabled;
            if (!enabled) StopAll();
        }

        private void PlayAmbientTone(float frequency, float duration, float volume, bool loop)
        {
            var sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            var buffer = new Vector2[samples];

            var rng = new RandomNumberGenerator();
            rng.Randomize();

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                // Low rumble with slight noise
                float wave = Mathf.Sin(2 * Mathf.Pi * frequency * t) * 0.5f;
                wave += Mathf.Sin(2 * Mathf.Pi * frequency * 1.5f * t) * 0.3f;
                wave += (rng.Randf() - 0.5f) * 0.2f; // noise
                buffer[i] = new Vector2(wave * volume, wave * volume);
            }

            var stream = new AudioStreamWav
            {
                Format = AudioStreamWav.FormatEnum.Format16Bits,
                MixRate = sampleRate,
                Stereo = true,
                Data = FloatToByteArray(buffer),
                LoopMode = loop ? AudioStreamWav.LoopModeEnum.Forward : AudioStreamWav.LoopModeEnum.Disabled,
                LoopBegin = 0,
                LoopEnd = samples
            };

            _ambientPlayer.Stream = stream;
            _ambientPlayer.Play();
        }

        private void PlayMusicTone(float frequency, float duration, float volume, bool loop)
        {
            var sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            var buffer = new Vector2[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float wave = Mathf.Sin(2 * Mathf.Pi * frequency * t) * 0.6f;
                wave += Mathf.Sin(2 * Mathf.Pi * frequency * 1.33f * t) * 0.3f;
                wave += Mathf.Sin(2 * Mathf.Pi * frequency * 1.5f * t) * 0.2f;
                buffer[i] = new Vector2(wave * volume, wave * volume);
            }

            var stream = new AudioStreamWav
            {
                Format = AudioStreamWav.FormatEnum.Format16Bits,
                MixRate = sampleRate,
                Stereo = true,
                Data = FloatToByteArray(buffer),
                LoopMode = loop ? AudioStreamWav.LoopModeEnum.Forward : AudioStreamWav.LoopModeEnum.Disabled,
                LoopBegin = 0,
                LoopEnd = samples
            };

            _musicPlayer.Stream = stream;
            _musicPlayer.Play();
        }

        private static byte[] FloatToByteArray(Vector2[] samples)
        {
            var bytes = new byte[samples.Length * 4];
            for (int i = 0; i < samples.Length; i++)
            {
                short left = (short)(Mathf.Clamp(samples[i].X, -1f, 1f) * 32767);
                short right = (short)(Mathf.Clamp(samples[i].Y, -1f, 1f) * 32767);
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(left), 0, bytes, i * 4, 2);
                System.Buffer.BlockCopy(System.BitConverter.GetBytes(right), 0, bytes, i * 4 + 2, 2);
            }
            return bytes;
        }
    }
}
