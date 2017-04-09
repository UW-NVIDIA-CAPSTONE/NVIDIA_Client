using DesktopWPFAppLowLevelKeyboardHook;
using System;
using System.Windows;
using System.Drawing;
using System.Net.Sockets;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Net.WebSockets;
using System.Text;
using System.IO;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    //http://www.subodh.com/Blog/PostID/31/Capture-Any-Screenshot-on-your-desktop-window-or-application

    public partial class MainWindow : Window
    {
        public second videoWindow;
        bool startPressed = false;
        private LowLevelKeyboardListener _listener;
        byte[] resizeArray;
        public String videoCode;
        public IntPtr steamHandle;
        private int MAX_RESIZE = 255;
        private int MAX_PACKET_SIZE = 64000;
        byte[] gameNumber;
        
        public string current_game = "";
        public const int WM_PAINT = 0x000F;
        public Hashtable insideHashtable;
        public ScreenCapture ScreenCapturePortal;
        public UdpClient client;

        public MainWindow()
        {
            InitializeComponent();
            //TcpClient client = new TcpClient();
        }

        private void generateHashTables()
        {
            
        }

        private void createVideoWindow()
        {
            videoWindow = new second();
            videoWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            videoWindow.Top = 10;
            videoWindow.Left = 10;
            videoWindow.Topmost = true;
            videoWindow.Closing += new CancelEventHandler(this.OnWindowClosing);
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            this.videoWindow = null;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {   
            _listener = new LowLevelKeyboardListener();
            _listener.OnKeyPressed += listener_OnKeyPressed;
            _listener.HookKeyboard();
            this.textBox_DisplayKeyboardInput.Text = "";  
        }

        private void listener_OnKeyPressed(object sender, KeyPressedArgs e)
        {

            if (startPressed)
            {
                this.textBox_DisplayKeyboardInput.Text += e.KeyPressed.ToString();

                if (e.KeyPressed.ToString() == "H")
                {
                    //Find the Window handle for INSIDE and Portal in order for us to only take screen
                    //captures of those windows and not just the screen.
                    /////////////////////////////////////////////////////////////////////////////////////
                    //current game string gets assigned when the user launches a game from the gui.
                    if (current_game == "INSIDE")
                    {
                        Process[] processes = Process.GetProcessesByName("INSIDE");

                        foreach (Process p in processes)
                        {
                            steamHandle = p.MainWindowHandle;
                            // do something with windowHandle
                        }
                    }
                    else if (current_game == "Portal")
                    {
                        Process[] processes = Process.GetProcessesByName("hl2");

                        foreach (Process p in processes)
                        {
                            steamHandle = p.MainWindowHandle;
                            // do something with windowHandle
                        }
                        System.Console.WriteLine("find portal" + processes.Length);
                    }
                    
                    System.Console.WriteLine(current_game);
                    ////TODO if Process doesnt see INSIDE or Portal then the games are not
                    ////running therefore prompt the user to run the games.

                    //////////////////////////////////////////////////////////////////////////////////////
                    
                    capture();//take the screenshot
                    sendAndReceive();

                    if (this.videoWindow == null)
                    {
                        createVideoWindow();
                    }
                    this.videoWindow.ShowYouTubeVideo(this.videoCode);
                    this.videoWindow.Show();

                    
                }
                else if (e.KeyPressed.ToString() == "C")
                {
                    if (videoWindow != null && videoWindow.IsVisible)
                    {
                        System.Console.WriteLine("close video window!");
                        //videoWindow.Close();
                        videoWindow.Hide();
                    }
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _listener.UnHookKeyboard();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            startPressed = true;
            this.textBox_DisplayKeyboardInput.Text = "";
            Process[] processList = Process.GetProcesses();
            foreach (Process p in processList)
            {
                //this.textBox_DisplayKeyboardInput.Text += p.ProcessName + "NEW ";
            }
        }

        private void capture()
        {
            double screenLeft = SystemParameters.VirtualScreenLeft;
            double screenTop = SystemParameters.VirtualScreenTop;
            double screenWidth = SystemParameters.VirtualScreenWidth;
            double screenHeight = SystemParameters.VirtualScreenHeight;
            Bitmap bmp = new Bitmap((int)screenWidth, (int)screenHeight);



            using (Graphics g = Graphics.FromImage(bmp))
            {
                // screenshot window
                IntPtr hdcSrc = User32.GetWindowDC(steamHandle);

                // get the size
                User32.RECT windowRect = new User32.RECT();
                User32.GetWindowRect(steamHandle, ref windowRect);
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
                        
                // get a .NET image object for it
                System.Drawing.Image img = System.Drawing.Image.FromHbitmap(hBitmap);

                Bitmap capImage = (Bitmap)img;
                capImage.Save("capturefile.png");

                string gameString = "";
                if (current_game == "INSIDE")
                {
                    gameString = "0";
                }
                else if (current_game == "Portal")
                {
                    gameString = "1";
                }
                gameNumber = System.Text.Encoding.ASCII.GetBytes(gameString);///

                int size = MAX_RESIZE;
                int decrement_size = 5;

                Bitmap resize = new Bitmap(capImage, new System.Drawing.Size(size, size));
                resize.Save("resized_frame.png");
                ImageConverter converter2 = new ImageConverter();
                byte[] byteArray = (byte[])converter2.ConvertTo(bmp, typeof(byte[]));
                this.resizeArray = (byte[])converter2.ConvertTo(resize, typeof(byte[]));

                // will downsize image until it is small enough to be sent through UDP socket
                while (this.resizeArray.Length + gameNumber.Length > MAX_PACKET_SIZE)
                {
                    resize = new Bitmap(capImage, new System.Drawing.Size(size, size));
                    //resize.Save("resized_frame.png");
                    byteArray = (byte[])converter2.ConvertTo(bmp, typeof(byte[]));
                    this.resizeArray = (byte[])converter2.ConvertTo(resize, typeof(byte[]));
                    size -= decrement_size;
                }
                // clean up 
                GDI32.DeleteDC(hdcDest);
                GDI32.DeleteObject(hBitmap);
                User32.ReleaseDC(steamHandle, hdcSrc);
            }
        }

        // Sends 
        
        private void sendAndReceive()
        {
            // establish connection with web server
            ////////128.95.31.215
            //IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 5000); // endpoint where server is listening



            //COMMENTED FOR NOW
            //TcpClient client = new TcpClient();
            //client.Connect("128.95.31.215", 5000);
            //String text = "Success";
            //Stream stm = client.GetStream();
            //ASCIIEncoding asen = new ASCIIEncoding();
            //byte[] ba = asen.GetBytes(text);
            //stm.Write(ba, 0, ba.Length);




            //client.Send(this.resizeArray,this.resizeArray.Length);
            /*
            // send data
            // prepare data packet (game number + image info)
            byte[] dataPacket = new byte[this.resizeArray.Length + gameNumber.Length];
            System.Console.WriteLine(gameNumber.Length);
            System.Buffer.BlockCopy(gameNumber, 0, dataPacket, 0, gameNumber.Length);
            System.Buffer.BlockCopy(this.resizeArray, 0, dataPacket, gameNumber.Length, this.resizeArray.Length);
            ImageConverter converter = new ImageConverter();
            Console.WriteLine("byte array size " + dataPacket.Length);
            client.Send(dataPacket, dataPacket.Length);

            // then receive data
            System.Console.WriteLine("start receive data!");
            Byte[] receiveBytes = client.Receive(ref ep);
            string returnData = Encoding.ASCII.GetString(receiveBytes);
            Console.WriteLine("This is the message you received " + returnData.ToString());

            // play corresponding walkthrough video
            getVideoCode(returnData);*/
        }

        private object Connect(string v)
        {
            throw new NotImplementedException();
        }

        private void launch_INSIDE_btn_Click(object sender, RoutedEventArgs e)
        {
            current_game = "INSIDE";
            textBox_current_game.Text = "";
            textBox_current_game.Text = current_game;
            Process[] processes = Process.GetProcessesByName("INSIDE");
            System.Console.WriteLine(processes);
            if (processes.Length == 0)
            {
                Process.Start("steam://rungameid/304430");
            }
            startPressed = true;
        }

        private void launch_Portal_btn_Click(object sender, RoutedEventArgs e)
        {
            current_game = "Portal";
            textBox_current_game.Text = "";
            textBox_current_game.Text = current_game;
            Process[] processes = Process.GetProcessesByName("hl2");
            if (processes.Length == 0)
            {
                Process.Start("steam://rungameid/400");
            }
            startPressed = true;
        }

        // plays YOUTUBE walkthrough corresponding to given string label
        private void getVideoCode(String data) {

            if (current_game == "INSIDE")
            {
                if (data == "1Forest CP1")
                {
                    this.videoCode = "YLUywUKTwx4&feature=youtu.be";
                }
                else if (data == "1Forest CP2")
                {
                    this.videoCode = "8Haulmquarc";
                }
                else if (data == "1Forest CP3")
                {
                    this.videoCode = "s7yoafff8HI";
                }
                else if (data == "1Forest CP4")
                {
                    this.videoCode = "awfpwZ6mYmM";
                }
                else if (data == "1Forest CP5")
                {
                    this.videoCode = "aR7p_1EJxv4";
                }
                else if (data == "1Forest CP6")
                {
                    this.videoCode = "UQ2rP46-EOY";
                }
                else
                {
                    this.videoCode = "UQ2rP46-EOY";
                }
            }
            else if (current_game == "Portal")
            {
                if (data == "TC00.0")
                {
                    this.videoCode = "YBocqyxiA74";
                }
                else if (data == "TC00.1")
                {
                    this.videoCode = "X5JsBpuEM7w";
                }
                else if (data == "TC01.0")
                {
                    this.videoCode = "gNXSn0eaqpw";
                }
                else if (data == "TC02.0")
                {
                    this.videoCode = "h8F9U3_3jHU";
                }
                else if (data == "TC02.1")
                {
                    this.videoCode = "YYrzHyGfmdo";
                }
                else if (data == "TC03.0")
                {
                    this.videoCode = "P3Z63XvzLbo";
                }
                else if (data == "TC03.1")
                {
                    this.videoCode = "5OCEWFlr9Sc";
                }
                else if (data == "TC04.0")
                {
                    this.videoCode = "u6fnmluMMUU";
                }
                else if (data == "TC05.0")
                {
                    this.videoCode = "YpFt6whzRXw";
                }
                else if (data == "TC05.1")
                {
                    this.videoCode = "6l3hvqr7wRM";
                }
                else if (data == "TC06.0")
                {
                    this.videoCode = "my3tBeJUBrU";
                }
                else if (data == "TC07.0")
                {
                    this.videoCode = "ajeznZxozcA";
                }
                else if (data == "TC08.0")
                {
                    this.videoCode = "AVTGQJPnYkE";
                }
                else if (data == "TC09.0")
                {
                    this.videoCode = "__cItdH3nrY";
                }
                else if (data == "TC10.0")
                {
                    this.videoCode = "4B4tb_oXFGo";
                }
                else if (data == "TC10.1")
                {
                    this.videoCode = "Io09vP3J6P4";
                }
                else if (data == "TC10.2")
                {
                    this.videoCode = "VhuPwwLbq7U";
                }
                else if (data == "TC11.0")
                {
                    this.videoCode = "Vf8afH9jdy8";
                }
                else if (data == "TC11.1")
                {
                    this.videoCode = "wsNs0xbcfvY";
                }
                else if (data == "TC12.0")
                {
                    this.videoCode = "IR7Qpq9XFjc";
                }
                else if (data == "TC13.0")
                {
                    this.videoCode = "X_m_Nik7YKQ";
                }
                else if (data == "TC14.0")
                {
                    this.videoCode = "ALJTAY70X_g";
                }
                else if (data == "TC15.0")
                {
                    this.videoCode = "x81_UM2ph5I";
                }
                else if (data == "TC15.1")
                {
                    this.videoCode = "UzEiAFiGc7I";
                }
                else if (data == "TC15.2")
                {
                    this.videoCode = "5jbPOFpolp4";
                }
                else if (data == "TC15.3")
                {
                    this.videoCode = "rrI0gYbpo9c";
                }
                else if (data == "TC15.4")
                {
                    this.videoCode = "kqqMDiZO5ms";
                }
                else if (data == "TC15.5")
                {
                    this.videoCode = "HswIdXyP_6k";
                }
                else if (data == "TC16.0")
                {
                    this.videoCode = "33NzyXSoaac";
                }
                else if (data == "TC16.1")
                {
                    this.videoCode = "XrRnZ22pbR4";
                }
                else if (data == "TC16.2")
                {
                    this.videoCode = "oID4Kuux5FA";
                }
                else if (data == "TC16.3")
                {
                    this.videoCode = "8hFHV_Ara_c";
                }
                else if (data == "TC16.4")
                {
                    this.videoCode = "ASfFFiD__7s";
                }
                else if (data == "TC17.0")
                {
                    this.videoCode = "0nkvqV4XY9Q";
                }
                else if (data == "TC17.1")
                {
                    this.videoCode = "eKt48jSo7FY";
                }
                else if (data == "TC17.2")
                {
                    this.videoCode = "sgscsA5zT_A";
                }
                else if (data == "TC18.0")
                {
                    this.videoCode = "s0m0TllElGY";
                }
                else if (data == "TC18.1")
                {
                    this.videoCode = "dRjbmS9Sqx4";
                }
                else if (data == "TC18.2")
                {
                    this.videoCode = "jVu4we7IzJ4";
                }
                else if (data == "TC19.0")
                {
                    this.videoCode = "GGWw3DrM6OQ";
                }
                else if (data == "TC19.1")
                {
                    this.videoCode = "99jdULYo1Bk";
                }
                else if (data == "TC19.2")
                {
                    this.videoCode = "VE3e91OT6pU";
                }
                else if (data == "TC19.3")
                {
                    this.videoCode = "gXVQIvunEFc";
                }
                else if (data == "TC19.4")
                {
                    this.videoCode = "WaoOkejxykU";
                }
                else if (data == "TC19.5")
                {
                    this.videoCode = "tJBRnAFxKnQ";
                }
                else if (data == "TC19.6")
                {
                    this.videoCode = "S-6-BuLmFIU";
                }
                else if (data == "TC19.7")
                {
                    this.videoCode = "3AVNFhJG0q0";
                }
                else if (data == "TC19.8")
                {
                    this.videoCode = "kf70usiJLfc";
                }
                else if (data == "TC19.9")
                {
                    this.videoCode = "m9grbnePQL0";
                }
                else if (data == "TC19.10")
                {
                    this.videoCode = "7ev3-siNRgc";
                }
                else if (data == "TC19.11")
                {
                    this.videoCode = "sWT5IOLjOKo";
                }
                else if (data == "TC19.12")
                {
                    this.videoCode = "weZFmomeCFI";
                }
                else if (data == "TC19.13")
                {
                    this.videoCode = "6HY0pUsZpi8";
                }
                else if (data == "TC19.14")
                {
                    this.videoCode = "sBsbg97vY6M";
                }
                else if (data == "TC19.15")
                {
                    this.videoCode = "ID5vbUyH0Tw";
                }
                else if (data == "TC19.16")
                {
                    this.videoCode = "mzNmceJf8sM";
                }
                else if (data == "TC19.17")
                {
                    this.videoCode = "7E8dkvoM_OY";
                }
                else if (data == "TC19.18")
                {
                    this.videoCode = "Nd-sm-JSO9w";
                }
                else if (data == "TC19.19")
                {
                    this.videoCode = "WgP4viH5aNo";
                }
            }
        }

    }
}
