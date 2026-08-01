using Godot;

namespace Schicksalswurf.Dungeon
{
    /// <summary>
    /// First-person camera that moves in grid steps with smooth interpolation.
    /// Handles movement animation, rotation animation, and head-bob effect.
    /// </summary>
    public partial class FirstPersonCamera : Camera3D
    {
        private Vector3 _targetPosition;
        private float _targetRotationY;
        private Vector3 _startPosition;
        private float _startRotationY;

        private float _moveProgress = 1.0f;
        private float _rotationProgress = 1.0f;

        private const float MoveDuration = 0.35f;
        private const float RotationDuration = 0.3f;

        // Head bob
        private float _bobTime = 0;
        private bool _isMoving = false;
        private const float BobFrequency = 8.0f;
        private const float BobAmplitude = 0.04f;

        public bool IsAnimating => _moveProgress < 1.0f || _rotationProgress < 1.0f;

        public override void _Ready()
        {
            Fov = 70.0f;
            Near = 0.05f;
            Far = 100.0f;

            // Enable environment fog for atmosphere
            var env = new Godot.Environment
            {
                BackgroundMode = Godot.Environment.BGMode.Color,
                BackgroundColor = new Color(0.02f, 0.02f, 0.03f),
                FogEnabled = true,
                FogLightColor = new Color(0.1f, 0.08f, 0.06f),
                FogLightEnergy = 0.3f,
                FogDensity = 0.08f,
                VolumetricFogEnabled = true,
                VolumetricFogDensity = 0.02f,
                VolumetricFogEmission = new Color(0.15f, 0.1f, 0.05f),
                VolumetricFogEmissionEnergy = 0.5f
            };

            var worldEnv = new WorldEnvironment { Environment = env };
            GetParent().AddChild(worldEnv);
        }

        public void SnapToPosition(Vector3 position, float rotationY)
        {
            _targetPosition = position;
            _targetRotationY = rotationY;
            _startPosition = position;
            _startRotationY = rotationY;
            _moveProgress = 1.0f;
            _rotationProgress = 1.0f;
            ApplyTransform();
        }

        public void MoveTo(Vector3 newPosition)
        {
            _startPosition = GlobalPosition;
            _targetPosition = newPosition;
            _moveProgress = 0.0f;
            _isMoving = true;
        }

        public void RotateTo(float newRotationY)
        {
            _startRotationY = Rotation.Y;
            // Normalize angle difference to shortest path
            float diff = newRotationY - _startRotationY;
            while (diff > Mathf.Pi) diff -= Mathf.Pi * 2;
            while (diff < -Mathf.Pi) diff += Mathf.Pi * 2;
            _targetRotationY = _startRotationY + diff;
            _rotationProgress = 0.0f;
        }

        public override void _Process(double delta)
        {
            float dt = (float)delta;

            // Animate movement
            if (_moveProgress < 1.0f)
            {
                _moveProgress += dt / MoveDuration;
                _moveProgress = Mathf.Min(_moveProgress, 1.0f);

                float t = EaseInOutQuad(_moveProgress);
                var pos = _startPosition.Lerp(_targetPosition, t);

                // Head bob during movement
                _bobTime += dt * BobFrequency;
                float bob = Mathf.Sin(_bobTime) * BobAmplitude;
                pos.Y += bob;

                GlobalPosition = pos;

                if (_moveProgress >= 1.0f)
                {
                    _isMoving = false;
                    GlobalPosition = _targetPosition;
                }
            }

            // Animate rotation
            if (_rotationProgress < 1.0f)
            {
                _rotationProgress += dt / RotationDuration;
                _rotationProgress = Mathf.Min(_rotationProgress, 1.0f);

                float t = EaseInOutQuad(_rotationProgress);
                float rotY = Mathf.Lerp(_startRotationY, _targetRotationY, t);

                Rotation = new Vector3(0, rotY, 0);
            }
        }

        private static float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
        }

        private void ApplyTransform()
        {
            GlobalPosition = _targetPosition;
            Rotation = new Vector3(0, _targetRotationY, 0);
        }

        public Vector3 CurrentTarget => _targetPosition;
        public float CurrentTargetRotation => _targetRotationY;
    }
}
