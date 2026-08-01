using Godot;

namespace Schicksalswurf.Core
{
    /// <summary>
    /// Day/night cycle system that adjusts ambient lighting over time.
    /// </summary>
    public partial class DayNightCycle : Node
    {
        private float _timeOfDay = 0.25f; // 0-1, 0=midnight, 0.25=dawn, 0.5=noon, 0.75=dusk
        private float _cycleSpeed = 0.005f; // how fast time progresses per second
        private bool _enabled = false; // disabled in dungeons by default

        public float TimeOfDay => _timeOfDay;
        public bool IsDaytime => _timeOfDay > 0.2f && _timeOfDay < 0.8f;
        public bool IsEnabled => _enabled;

        public override void _Process(double delta)
        {
            if (!_enabled) return;

            _timeOfDay += _cycleSpeed * (float)delta;
            if (_timeOfDay >= 1.0f)
                _timeOfDay -= 1.0f;
        }

        public void Enable()
        {
            _enabled = true;
        }

        public void Disable()
        {
            _enabled = false;
        }

        public Color GetAmbientColor()
        {
            if (!_enabled) return new Color(0.3f, 0.25f, 0.2f);

            // Smooth color transition: night -> dawn -> day -> dusk -> night
            if (_timeOfDay < 0.2f || _timeOfDay > 0.8f)
                return new Color(0.1f, 0.1f, 0.15f); // night
            else if (_timeOfDay < 0.3f)
                return new Color(0.4f, 0.3f, 0.2f).Lerp(new Color(0.5f, 0.45f, 0.4f), (_timeOfDay - 0.2f) / 0.1f); // dawn
            else if (_timeOfDay < 0.7f)
                return new Color(0.5f, 0.45f, 0.4f); // day
            else
                return new Color(0.5f, 0.45f, 0.4f).Lerp(new Color(0.4f, 0.25f, 0.15f), (_timeOfDay - 0.7f) / 0.1f); // dusk
        }

        public string GetTimeString()
        {
            int hours = (int)(_timeOfDay * 24);
            int minutes = (int)((_timeOfDay * 24 - hours) * 60);
            return $"{hours:D2}:{minutes:D2}";
        }
    }
}
