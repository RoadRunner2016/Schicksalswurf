using Godot;

namespace Schicksalswurf.Core
{
    /// <summary>
    /// Controller/gamepad support and accessibility options.
    /// </summary>
    public partial class ControllerSystem : Node
    {
        public static ControllerSystem Instance { get; private set; }

        public bool ControllerConnected { get; private set; } = false;
        public bool UseControllerNavigation { get; set; } = false;
        public float Deadzone { get; set; } = 0.25f;

        // Controller mapping
        public JoyButton ActionButton { get; set; } = JoyButton.A;
        public JoyButton CancelButton { get; set; } = JoyButton.B;
        public JoyButton InventoryButton { get; set; } = JoyButton.X;
        public JoyButton MapButton { get; set; } = JoyButton.Y;
        public JoyButton StartButton { get; set; } = JoyButton.Start;

        public override void _Ready()
        {
            Instance = this;
            Input.JoyConnectionChanged += OnJoyConnectionChanged;

            // Check currently connected pads
            var pads = Input.GetConnectedJoypads();
            ControllerConnected = pads.Count > 0;
        }

        private void OnJoyConnectionChanged(long device, bool connected)
        {
            ControllerConnected = connected;
            if (connected)
                GD.Print($"[Controller] Gamepad {device} verbunden.");
            else
                GD.Print($"[Controller] Gamepad {device} getrennt.");
        }

        public Vector2 GetMovementVector()
        {
            if (!ControllerConnected) return Vector2.Zero;
            var x = Input.GetJoyAxis(0, JoyAxis.LeftX);
            var y = Input.GetJoyAxis(0, JoyAxis.LeftY);
            if (Mathf.Abs(x) < Deadzone) x = 0;
            if (Mathf.Abs(y) < Deadzone) y = 0;
            return new Vector2(x, y);
        }

        public bool IsActionButtonPressed() => Input.IsJoyButtonPressed(0, ActionButton);
        public bool IsCancelButtonPressed() => Input.IsJoyButtonPressed(0, CancelButton);
        public bool IsInventoryButtonPressed() => Input.IsJoyButtonPressed(0, InventoryButton);
        public bool IsMapButtonPressed() => Input.IsJoyButtonPressed(0, MapButton);

        public bool IsDPadUpPressed()
        {
            var y = Input.GetJoyAxis(0, JoyAxis.LeftY);
            return y < -Deadzone;
        }

        public bool IsDPadDownPressed()
        {
            var y = Input.GetJoyAxis(0, JoyAxis.LeftY);
            return y > Deadzone;
        }

        public bool IsDPadLeftPressed()
        {
            var x = Input.GetJoyAxis(0, JoyAxis.LeftX);
            return x < -Deadzone;
        }

        public bool IsDPadRightPressed()
        {
            var x = Input.GetJoyAxis(0, JoyAxis.LeftX);
            return x > Deadzone;
        }

        public bool IsStartPressed() => Input.IsJoyButtonPressed(0, StartButton);

        // Accessibility helpers
        public static Color GetColorblindColor(Color original, bool colorblindMode)
        {
            if (!colorblindMode) return original;
            // Adjust colors for protanopia/deuteranopia by increasing blue channel
            return new Color(
                original.R * 0.8f,
                original.G * 0.95f,
                Mathf.Min(1.0f, original.B * 1.2f),
                original.A
            );
        }

        public static int GetAccessibleFontSize(int baseSize, bool largerFont)
        {
            return largerFont ? baseSize + 4 : baseSize;
        }
    }
}
