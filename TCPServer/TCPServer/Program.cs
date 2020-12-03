using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.IO;

namespace TCPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            TCPServer server;
            try
            {
                server = new TCPServer(true);
                server.OnDataReceive += Server_OnDataReceive;
                server.Start();
            }
            catch (Exception ie)
            {

            }
        }

        private static void Server_OnDataReceive(object sender, EventArgs e)
        {
            var server = (sender as TCPServer);
            if (server is null)
                return;
            Message(server.Message);
        }

        public static void Message(string message)
        {
            System.Windows.Forms.MessageBox.Show(message);
        }

        public class TCPServer
        {
            #region
            private TcpListener serverSocket;
            private TcpClient clientSocket;
            private int GlobalPort = 5556;

            public string Message = string.Empty;
            public delegate void GotMessageEvent(object sender, EventArgs e);
            public event GotMessageEvent OnDataReceive;
            #endregion

            #region Member function
            /// <summary>
            /// 物件建構子
            /// </summary>
            public TCPServer(bool useDefaultSetting)
            {
                Console.WriteLine("Port(default 5555)");
                if (!useDefaultSetting)
                {
                    var port = Console.ReadLine();
                    if (port != "")
                        GlobalPort = int.Parse(port);
                }

                serverSocket = new TcpListener(GlobalPort);

            }

            /// <summary>
            /// 開始啟動服務
            /// </summary>
            public void Start()
            {
                serverSocket.Start();
                Console.WriteLine($"localhost:{GlobalPort}    Waiting for a client...");
                clientSocket = default(TcpClient);

                while (true)
                {
                    clientSocket = serverSocket.AcceptTcpClient();

                    NetworkStream networkStream = clientSocket.GetStream();
                    byte[] bytesFrom = new byte[65536];
                    networkStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                    networkStream.Write(bytesFrom, 0, bytesFrom.Length);
                    
                    networkStream.Flush();
                    char[] cArray = System.Text.Encoding.ASCII.GetString(bytesFrom).ToCharArray();
                    string tempData = string.Empty;
                    foreach (var item in cArray)
                    {
                        if (item != '\0')
                            tempData += item;
                    }
                    this.Message = tempData;

                    

                    if (this.Message == "img")
                    {
                        //ScreenShot();
                        //SendBack(clientSocket.Client.RemoteEndPoint.ToString());
                        //networkStream = clientSocket.GetStream();
                        //byte[] messages = Encoding.Default.GetBytes("received");
                        //networkStream.Write(messages, 0, messages.Length);
                    }
                    else
                    {
                        OnDataReceive?.Invoke(this, new EventArgs());
                    }
                        
                }
            }

            private void SendBack(string remoteIP)
            {
                //利用TcpClient对象GetStream方法得到网络流
                NetworkStream clientStream = clientSocket.GetStream();
                var bw = new BinaryWriter(clientStream);
                //写入
                bw.Write("123");
                
            }

            public void ScreenShot()
            {
                PrintScreen ps = new PrintScreen();
                ps.CaptureScreenToFile("screen.png", System.Drawing.Imaging.ImageFormat.Png);
            }
            #endregion
        }
        public class PrintScreen
        {
            /// <summary>
            /// Creates an Image object containing a screen shot of the entire desktop
            /// </summary>
            /// <returns></returns>
            public Image CaptureScreen()
            {
                return CaptureWindow(User32.GetDesktopWindow());
            }

            /// <summary>
            /// Creates an Image object containing a screen shot of a specific window
            /// </summary>
            /// <param name="handle">The handle to the window. (In windows forms, this is obtained by the Handle property)</param>
            /// <returns></returns>
            public Image CaptureWindow(IntPtr handle)
            {
                // get te hDC of the target window
                IntPtr hdcSrc = User32.GetWindowDC(handle);
                // get the size
                User32.RECT windowRect = new User32.RECT();
                User32.GetWindowRect(handle, ref windowRect);
                int width = windowRect.right - windowRect.left;
                int height = windowRect.bottom - windowRect.top;
                // create a device context we can copy to
                IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
                // create a bitmap we can copy it to,
                // using GetDeviceCaps to get the width/height
                IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
                // select the bitmap object
                IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
                // bitblt over
                GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32.SRCCOPY);
                // restore selection
                GDI32.SelectObject(hdcDest, hOld);
                // clean up
                GDI32.DeleteDC(hdcDest);
                User32.ReleaseDC(handle, hdcSrc);

                // get a .NET image object for it
                Image img = Image.FromHbitmap(hBitmap);
                // free up the Bitmap object
                GDI32.DeleteObject(hBitmap);

                return img;
            }

            /// <summary>
            /// Captures a screen shot of a specific window, and saves it to a file
            /// </summary>
            /// <param name="handle"></param>
            /// <param name="filename"></param>
            /// <param name="format"></param>
            public void CaptureWindowToFile(IntPtr handle, string filename, ImageFormat format)
            {
                Image img = CaptureWindow(handle);
                img.Save(filename, format);
            }

            /// <summary>
            /// Captures a screen shot of the entire desktop, and saves it to a file
            /// </summary>
            /// <param name="filename"></param>
            /// <param name="format"></param>
            public void CaptureScreenToFile(string filename, ImageFormat format)
            {
                CaptureScreen().Save(filename, format);
            }

            /// <summary>
            /// Helper class containing Gdi32 API functions
            /// </summary>
            private class GDI32
            {

                public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter

                [DllImport("gdi32.dll")]
                public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
                    int nWidth, int nHeight, IntPtr hObjectSource,
                    int nXSrc, int nYSrc, int dwRop);

                [DllImport("gdi32.dll")]
                public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
                    int nHeight);

                [DllImport("gdi32.dll")]
                public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

                [DllImport("gdi32.dll")]
                public static extern bool DeleteDC(IntPtr hDC);

                [DllImport("gdi32.dll")]
                public static extern bool DeleteObject(IntPtr hObject);

                [DllImport("gdi32.dll")]
                public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
            }

            /// <summary>
            /// Helper class containing User32 API functions
            /// </summary>
            private class User32
            {
                [StructLayout(LayoutKind.Sequential)]
                public struct RECT
                {
                    public int left;
                    public int top;
                    public int right;
                    public int bottom;
                }

                [DllImport("user32.dll")]
                public static extern IntPtr GetDesktopWindow();

                [DllImport("user32.dll")]
                public static extern IntPtr GetWindowDC(IntPtr hWnd);

                [DllImport("user32.dll")]
                public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

                [DllImport("user32.dll")]
                public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

            }
        }
    }
}
