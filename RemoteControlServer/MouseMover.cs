using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RemoteControlServer
{
    internal static class MouseMover
    {
        public enum EButton
        {
            Left,
            Right,
            Middle
        }

        public enum EEventType
        {
            Down,
            Up
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        private const uint MouseEventLeftDown = 0x02;
        private const uint MouseEventLeftUp = 0x04;
        private const uint MouseEventRightDown = 0x08;
        private const uint MouseEventRightUp = 0x10;
        private const uint MouseEventMiddleDown = 0x20;
        private const uint MouseEventMiddleUp = 0x40;

        internal static void Move(string value)
        {
            //Cursor cursor = new Cursor(Cursor.Current.Handle);
            var parts = value.Split(new[] {','}, 2);
            var x = int.Parse(parts[0]);
            var y = int.Parse(parts[1]);
            Cursor.Position = new Point(Cursor.Position.X + x, Cursor.Position.Y + y);
        }

        internal static void Click(EButton button, EEventType type)
        {
            //Call the imported function with the cursor's current position
            var x = (uint) Cursor.Position.X;
            var y = (uint) Cursor.Position.Y;
            uint eventType = 0;
            switch (button)
            {
                case EButton.Left:
                    eventType = type == EEventType.Down ? MouseEventLeftDown : MouseEventLeftUp;
                    break;
                case EButton.Right:
                    eventType = type == EEventType.Down ? MouseEventRightDown : MouseEventRightUp;
                    break;
                case EButton.Middle:
                    eventType = type == EEventType.Down ? MouseEventMiddleDown : MouseEventMiddleUp;
                    break;
            }

            mouse_event(eventType, x, y, 0, 0);
        }
    }
}