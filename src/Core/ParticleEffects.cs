using Godot;

namespace Schicksalswurf.Core
{
    /// <summary>
    /// Particle effects for torches, spell casts, and combat hits.
    /// Uses Godot GpuParticles3D for simple procedural effects.
    /// </summary>
    public partial class ParticleEffects : Node3D
    {
        public static ParticleEffects Instance { get; private set; }

        public override void _Ready()
        {
            Instance = this;
        }

        /// <summary>
        /// Creates a torch flame particle effect at the given position.
        /// </summary>
        public GpuParticles3D CreateTorchFlame(Vector3 position)
        {
            var particles = new GpuParticles3D
            {
                Position = position,
                Amount = 20,
                Lifetime = 0.5f,
                Emitting = true
            };

            var mat = new ParticleProcessMaterial();
            mat.Direction = new Vector3(0, 1, 0);
            mat.Spread = 15f;
            mat.InitialVelocityMin = 0.5f;
            mat.InitialVelocityMax = 1.5f;
            mat.Gravity = new Vector3(0, -0.5f, 0);
            mat.Color = new Color(1.0f, 0.6f, 0.2f, 0.8f);
            mat.ScaleMin = 0.3f;
            mat.ScaleMax = 0.8f;
            particles.ProcessMaterial = mat;

            var mesh = new SphereMesh { Radius = 0.05f, Height = 0.1f };
            var meshMat = new StandardMaterial3D
            {
                AlbedoColor = new Color(1.0f, 0.7f, 0.3f),
                Emission = new Color(1.0f, 0.5f, 0.1f),
                EmissionEnergyMultiplier = 2.0f
            };
            mesh.Material = meshMat;
            particles.DrawPass1 = mesh;

            AddChild(particles);
            return particles;
        }

        /// <summary>
        /// Creates a spell cast effect at the given position.
        /// </summary>
        public GpuParticles3D CreateSpellEffect(Vector3 position, Color color)
        {
            var particles = new GpuParticles3D
            {
                Position = position,
                Amount = 30,
                Lifetime = 0.6f,
                Emitting = true,
                OneShot = true
            };

            var mat = new ParticleProcessMaterial();
            mat.Direction = new Vector3(0, 0, 0);
            mat.Spread = 180f;
            mat.InitialVelocityMin = 1.0f;
            mat.InitialVelocityMax = 3.0f;
            mat.Gravity = Vector3.Zero;
            mat.Color = color;
            mat.ScaleMin = 0.2f;
            mat.ScaleMax = 0.5f;
            particles.ProcessMaterial = mat;

            var mesh = new SphereMesh { Radius = 0.08f, Height = 0.16f };
            var meshMat = new StandardMaterial3D
            {
                AlbedoColor = color,
                Emission = color,
                EmissionEnergyMultiplier = 3.0f
            };
            mesh.Material = meshMat;
            particles.DrawPass1 = mesh;

            AddChild(particles);

            // Auto-remove after lifetime
            var timer = new Timer { WaitTime = 1.0f, OneShot = true };
            AddChild(timer);
            timer.Timeout += () => { particles.QueueFree(); timer.QueueFree(); };
            timer.Start();

            return particles;
        }

        /// <summary>
        /// Creates a hit effect at the given position.
        /// </summary>
        public GpuParticles3D CreateHitEffect(Vector3 position)
        {
            return CreateSpellEffect(position, new Color(0.9f, 0.2f, 0.1f));
        }

        /// <summary>
        /// Creates a healing effect at the given position.
        /// </summary>
        public GpuParticles3D CreateHealEffect(Vector3 position)
        {
            return CreateSpellEffect(position, new Color(0.2f, 0.9f, 0.3f));
        }
    }
}
