using Godot;

namespace Schicksalswurf.Core
{
    /// <summary>
    /// Procedural animation system for doors, chests, and combat effects.
    /// Uses Tween animations for smooth transitions.
    /// </summary>
    public static class AnimationSystem
    {
        public static void AnimateDoorOpen(Node3D doorNode)
        {
            if (doorNode == null) return;
            var tween = doorNode.CreateTween();
            tween.TweenProperty(doorNode, "rotation:y", Mathf.Pi / 2, 0.5f)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.Out);
        }

        public static void AnimateChestOpen(Node3D chestLid)
        {
            if (chestLid == null) return;
            var tween = chestLid.CreateTween();
            tween.TweenProperty(chestLid, "rotation:x", -Mathf.Pi / 3, 0.4f)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);
        }

        public static void AnimateCombatHit(Node3D targetNode)
        {
            if (targetNode == null) return;
            var originalPos = targetNode.Position;
            var tween = targetNode.CreateTween();
            tween.TweenProperty(targetNode, "position", originalPos + new Vector3(0, 0, -0.3f), 0.1f)
                .SetTrans(Tween.TransitionType.Sine);
            tween.TweenProperty(targetNode, "position", originalPos, 0.15f)
                .SetTrans(Tween.TransitionType.Elastic)
                .SetEase(Tween.EaseType.Out);
        }

        public static void AnimateSpellCast(Node3D casterNode)
        {
            if (casterNode == null) return;
            var originalScale = casterNode.Scale;
            var tween = casterNode.CreateTween();
            tween.TweenProperty(casterNode, "scale", originalScale * 1.2f, 0.2f)
                .SetTrans(Tween.TransitionType.Sine);
            tween.TweenProperty(casterNode, "scale", originalScale, 0.3f)
                .SetTrans(Tween.TransitionType.Elastic)
                .SetEase(Tween.EaseType.Out);
        }

        public static void AnimateFadeIn(Control control, float duration = 0.3f)
        {
            if (control == null) return;
            control.Modulate = new Color(1, 1, 1, 0);
            var tween = control.CreateTween();
            tween.TweenProperty(control, "modulate:a", 1.0f, duration)
                .SetTrans(Tween.TransitionType.Sine);
        }

        public static void AnimateFadeOut(Control control, float duration = 0.3f)
        {
            if (control == null) return;
            var tween = control.CreateTween();
            tween.TweenProperty(control, "modulate:a", 0.0f, duration)
                .SetTrans(Tween.TransitionType.Sine);
        }

        public static void AnimateShake(Node3D node, float intensity = 0.1f, float duration = 0.3f)
        {
            if (node == null) return;
            var originalPos = node.Position;
            var tween = node.CreateTween();
            int steps = 6;
            for (int i = 0; i < steps; i++)
            {
                var offset = new Vector3(
                    (float)GD.RandRange(-intensity, intensity),
                    (float)GD.RandRange(-intensity, intensity),
                    (float)GD.RandRange(-intensity, intensity)
                );
                tween.TweenProperty(node, "position", originalPos + offset, duration / steps);
            }
            tween.TweenProperty(node, "position", originalPos, duration / steps);
        }
    }
}
