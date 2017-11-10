using System.Windows.Forms;

namespace RemoteControlServer
{
    internal static class Keyboard
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
            }
            SendKeys.SendWait(value);
        }
    }
}