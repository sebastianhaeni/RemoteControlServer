using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace RemoteControlServer
{
    class Keyboard
    {
        internal static void SendKey(string value)
        {
            switch (value)
            {
                case "+":
                    value = "{+}";
                    break;
                case "^":
                    value = "{^}";
                    break;
                case "%":
                    value = "{%}";
                    break;
                case "~":
                    value = "{~}";
                    break;
                case "(":
                    value = "{(}";
                    break;
                case ")":
                    value = "{)}";
                    break;
                case "{":
                    value = "{{}";
                    break;
                case "}":
                    value = "{}}";
                    break;
                case "[":
                    value = "{[}";
                    break;
                case "]":
                    value = "{]}";
                    break;
                default:

                    break;
            }
            SendKeys.SendWait(value);
        }
    }
}
