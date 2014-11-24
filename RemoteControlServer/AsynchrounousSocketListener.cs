

using System;
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
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    public class AsynchronousSocketListener
    {
        // Thread signal.
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public AsynchronousSocketListener()
        {
        }

        public static void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com".
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = null;
            foreach (var ip in ipHostInfo.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress = ip;
                    break;
                }
            }

            if (ipAddress == null)
            {
                Environment.Exit(1);
            }

            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }


        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    // All the data has been read from the 
                    // client. Display it on the console.
                    content = content.Remove(content.Length - "<EOF>".Length);
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                        content.Length, content);

                    var response = HandleInstruction(content);
                    // Echo the data back to the client.
                    Send(handler, response);
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
                }
            }
        }

        private static string HandleInstruction(string content)
        {
            var parts = content.Split(new[] { ':' }, 2);
            var type = parts[0];
            var value = parts[1];
            switch (type.ToLowerInvariant())
            {
                case "password":
                    return value.Equals("1234") ? "OK" : "ERROR";
                case "volume":
                    SystemVolumeChanger.SetVolume(Int32.Parse(value));
                    return "OK";
                case "mouse":
                    MouseMover.move(value);
                    return "OK";
                case "mouse_left":
                    MouseMover.click(MouseMover.EButton.Left, value.Equals("down") ? MouseMover.EEventType.Down : MouseMover.EEventType.Up);
                    return "OK";
                case "mouse_right":
                    MouseMover.click(MouseMover.EButton.Right, value.Equals("down") ? MouseMover.EEventType.Down : MouseMover.EEventType.Up);
                    return "OK";
                case "mouse_middle":
                    MouseMover.click(MouseMover.EButton.Middle, value.Equals("down") ? MouseMover.EEventType.Down : MouseMover.EEventType.Up);
                    return "OK";
                case "keyboard":
                    Keyboard.SendKey(value);
                    return "OK";
                case "keyboard_backspace":
                    for (var i = 0; i < Int32.Parse(value); i++)
                    {
                        Keyboard.SendKey("{BS}");
                    }
                    return "OK";
            }
            return "ERROR";
        }

        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

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