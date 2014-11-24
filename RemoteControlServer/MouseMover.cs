using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace RemoteControlServer
{
    class MouseMover
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
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        private const uint MOUSEEVENTF_LEFTDOWN     = 0x02;
        private const uint MOUSEEVENTF_LEFTUP       = 0x04;
        private const uint MOUSEEVENTF_RIGHTDOWN    = 0x08;
        private const uint MOUSEEVENTF_RIGHTUP      = 0x10;
        private const uint MOUSEEVENTF_MIDDLEDOWN   = 0x20;
        private const uint MOUSEEVENTF_MIDDLEUP     = 0x40;

        internal static void move(string value)
        {
            //Cursor cursor = new Cursor(Cursor.Current.Handle);
            var parts = value.Split(new[] { ',' }, 2);
            var x = Int32.Parse(parts[0]);
            var y = Int32.Parse(parts[1]);
            Cursor.Position = new Point(Cursor.Position.X + x, Cursor.Position.Y + y);
        }

        internal static void click(EButton button, EEventType type)
        {
            //Call the imported function with the cursor's current position
            uint X = (uint)Cursor.Position.X;
            uint Y = (uint)Cursor.Position.Y;
            uint eventType = 0;
            switch (button)
            {
                case EButton.Left:
                    eventType = type == EEventType.Down ? MOUSEEVENTF_LEFTDOWN : MOUSEEVENTF_LEFTUP;
                    break;
                case EButton.Right:
                    eventType = type == EEventType.Down ? MOUSEEVENTF_RIGHTDOWN : MOUSEEVENTF_RIGHTUP;
                    break;
                case EButton.Middle:
                    eventType = type == EEventType.Down ? MOUSEEVENTF_MIDDLEDOWN : MOUSEEVENTF_MIDDLEUP;
                    break;
            }

            mouse_event(eventType, X, Y, 0, 0);
        }
    }
}
