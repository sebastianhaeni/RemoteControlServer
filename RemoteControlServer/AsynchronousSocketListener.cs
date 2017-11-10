using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RemoteControlServer
{
    // State object for reading client data asynchronously
    public class StateObject
    {
        // Client  socket.
        public Socket WorkSocket;

        // Size of receive buffer.
        public const int BufferSize = 1024;

        // Receive buffer.
        public readonly byte[] Buffer = new byte[BufferSize];

        // Received data string.
        public readonly StringBuilder Sb = new StringBuilder();
    }

    public static class AsynchronousSocketListener
    {
        // Thread signal.
        private static readonly ManualResetEvent AllDone = new ManualResetEvent(false);

        public static void StartListening()
        {
            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com".
            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = ipHostInfo.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            if (ipAddress == null)
            {
                Environment.Exit(1);
            }

            var localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP socket.
            var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.
                    AllDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    listener.BeginAccept(AcceptCallback, listener);

                    // Wait until a connection is made before continuing.
                    AllDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            AllDone.Set();

            // Get the socket that handles the client request.
            var listener = (Socket) ar.AsyncState;
            var handler = listener.EndAccept(ar);

            // Create the state object.
            var state = new StateObject {WorkSocket = handler};
            handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReadCallback, state);
        }

        private static void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            var state = (StateObject) ar.AsyncState;
            var handler = state.WorkSocket;

            // Read data from the client socket. 
            var bytesRead = handler.EndReceive(ar);

            if (bytesRead <= 0)
            {
                return;
            }

            // There  might be more data, so store the data received so far.
            state.Sb.Append(Encoding.ASCII.GetString(
                state.Buffer, 0, bytesRead));

            // Check for end-of-file tag. If it is not there, read 
            // more data.
            var content = state.Sb.ToString();
            if (content.IndexOf("<EOF>", StringComparison.Ordinal) > -1)
            {
                // All the data has been read from the 
                // client. Display it on the console.
                content = content.Remove(content.Length - "<EOF>".Length);
                Console.WriteLine(@"Read {0} bytes from socket. 
 Data : {1}", content.Length, content);

                var response = HandleInstruction(content);
                // Echo the data back to the client.
                Send(handler, response);
            }
            else
            {
                // Not all data received. Get more.
                handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReadCallback, state);
            }
        }

        private static string HandleInstruction(string content)
        {
            var parts = content.Split(new[] {':'}, 2);
            var type = parts[0];
            var value = parts[1];
            switch (type.ToLowerInvariant())
            {
                case "password":
                    return value.Equals("1234") ? "OK" : "ERROR";
                case "volume":
                    SystemVolumeChanger.SetVolume(int.Parse(value));
                    return "OK";
                case "mouse":
                    MouseMover.Move(value);
                    return "OK";
                case "mouse_left":
                    MouseMover.Click(MouseMover.EButton.Left,
                        value.Equals("down") ? MouseMover.EEventType.Down : MouseMover.EEventType.Up);
                    return "OK";
                case "mouse_right":
                    MouseMover.Click(MouseMover.EButton.Right,
                        value.Equals("down") ? MouseMover.EEventType.Down : MouseMover.EEventType.Up);
                    return "OK";
                case "mouse_middle":
                    MouseMover.Click(MouseMover.EButton.Middle,
                        value.Equals("down") ? MouseMover.EEventType.Down : MouseMover.EEventType.Up);
                    return "OK";
                case "keyboard":
                    Keyboard.SendKey(value);
                    return "OK";
                case "keyboard_backspace":
                    for (var i = 0; i < int.Parse(value); i++)
                    {
                        Keyboard.SendKey("{BS}");
                    }
                    return "OK";
            }
            return "ERROR";
        }

        private static void Send(Socket handler, string data)
        {
            // Convert the string data to byte data using ASCII encoding.
            var byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                var handler = (Socket) ar.AsyncState;

                // Complete sending the data to the remote device.
                var bytesSent = handler.EndSend(ar);
                Console.WriteLine(@"Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}