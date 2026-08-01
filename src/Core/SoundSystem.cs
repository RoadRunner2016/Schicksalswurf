using Godot;

namespace Schicksalswurf.Core
{
    /// <summary>
    /// Procedural sound system using Godot AudioStreamGenerator for simple sound effects.
    /// Generates basic tones and noise effects without external audio files.
    /// </summary>
    public partial class SoundSystem : Node
    {
        private AudioStreamPlayer _musicPlayer;
        private AudioStreamPlayer _sfxPlayer;
        private bool _enabled = true;

        public override void _Ready()
        {
            _musicPlayer = new AudioStreamPlayer { Name = "MusicPlayer" };
            AddChild(_musicPlayer);

            _sfxPlayer = new AudioStreamPlayer { Name = "SfxPlayer" };
            AddChild(_sfxPlayer);
        }

        public void PlayFootstep()
        {
            if (!_enabled) return;
            // Simple low-pitch click for footsteps
            PlayTone(80, 0.08f, 0.15f);
        }

        public void PlayCombatHit()
        {
            if (!_enabled) return;
            PlayTone(200, 0.1f, 0.3f);
        }

        public void PlayCombatMiss()
        {
            if (!_enabled) return;
            PlayTone(400, 0.05f, 0.1f);
        }

        public void PlayChestOpen()
        {
            if (!_enabled) return;
            PlayTone(300, 0.15f, 0.2f);
        }

        public void PlayDoorOpen()
        {
            if (!_enabled) return;
            PlayTone(150, 0.2f, 0.15f);
        }

        public void PlaySpellCast()
        {
            if (!_enabled) return;
            PlayTone(600, 0.15f, 0.25f);
        }

        public void PlayLevelUp()
        {
            if (!_enabled) return;
            PlayTone(440, 0.1f, 0.3f);
        }

        public void PlayTrap()
        {
            if (!_enabled) return;
            PlayTone(120, 0.15f, 0.4f);
        }

        public void PlayStairs()
        {
            if (!_enabled) return;
            PlayTone(330, 0.2f, 0.2f);
        }

        public void Toggle()
        {
            _enabled = !_enabled;
            if (!_enabled)
                _musicPlayer.Stop();
        }

        public bool IsEnabled => _enabled;

        private void PlayTone(float frequency, float duration, float volume)
        {
            var sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            var buffer = new Vector2[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1.0f - (float)i / samples;
                float sample = Mathf.Sin(2 * Mathf.Pi * frequency * t) * envelope * volume;
                buffer[i] = new Vector2(sample, sample);
            }

            var stream = new AudioStreamWav
            {
                Format = AudioStreamWav.FormatEnum.Format16Bits,
                MixRate = sampleRate,
                Stereo = true,
                Data = FloatArrayToByteArray(buffer)
            };

            _sfxPlayer.Stream = stream;
            _sfxPlayer.Play();
        }

        private static byte[] FloatArrayToByteArray(Vector2[] samples)
        {
            var bytes = new byte[samples.Length * 4]; // 2 bytes per channel, stereo
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
