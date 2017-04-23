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
using System.Net.Http;
using System.Collections.Generic;


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
        
        public string current_game = "";
        public const int WM_PAINT = 0x000F;
        public Hashtable insideHashtable;
        public ScreenCapture ScreenCapturePortal;
        public UdpClient client;
        public Dictionary<string, int> dictionary =new Dictionary<string, int>();
        public List<string> videoList = new List<string>();
        public int VideoIndex;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void generateHashTables()
        {
            
        }

        private void createVideoWindow()
        {
            videoWindow = new second(this);
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
                    //var watch = System.Diagnostics.Stopwatch.StartNew();
                    // the code that you want to measure comes here
                    capture();//take the screenshot
                    sendAndReceive();
                    //watch.Stop();
                    //var elapsedMs = watch.ElapsedMilliseconds;
                    //Console.WriteLine("execution time: "+elapsedMs);
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
                else if (e.KeyPressed.ToString() == "N")
                {
                    //if (this.videoWindow != null && videoWindow.IsVisible)
                    //{
                    //    if (this.VideoIndex != 0) {
                    //        this.VideoIndex--;
                    //        string label_video = this.videoList[this.VideoIndex];
                    //        int idx = label_video.LastIndexOf(' ');
                    //        string video = label_video.Substring(idx + 1);
                    //        this.videoCode = video;
                    //        this.videoWindow.ShowYouTubeVideo(this.videoCode);
                    //        this.videoWindow.Show();
                    //    }
                    //}
                    skipBack();
                }
                else if (e.KeyPressed.ToString() == "M")
                {
                    //if (this.videoWindow != null && videoWindow.IsVisible)
                    //{
                    //    if (this.VideoIndex != this.videoList.Count - 1)
                    //    {
                    //        this.VideoIndex++;
                    //        string label_video = this.videoList[this.VideoIndex];
                    //        int idx = label_video.LastIndexOf(' ');
                    //        string video = label_video.Substring(idx + 1);
                    //        this.videoCode = video;
                    //        this.videoWindow.ShowYouTubeVideo(this.videoCode);
                    //        this.videoWindow.Show();
                    //    }
                    //}
                    skipForward();
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
                

                ImageConverter converter2 = new ImageConverter();
                Bitmap resize = new Bitmap(capImage, new System.Drawing.Size(256, 256));
                this.resizeArray = (byte[])converter2.ConvertTo(resize, typeof(byte[]));
                
                GDI32.DeleteDC(hdcDest);
                GDI32.DeleteObject(hBitmap);
                User32.ReleaseDC(steamHandle, hdcSrc);
            }
        }

        // Sends 

        private async void sendAndReceive()
        {

            var watch = System.Diagnostics.Stopwatch.StartNew();

            using (var client = new HttpClient())
            {
                using (var ct = new MultipartFormDataContent())
                {
                    ct.Add(new StreamContent(new MemoryStream(resizeArray)), "file", "file.png");

                    using (
                       var message =
                           await client.PostAsync("http://128.95.31.215:5000/?gamename=" + current_game, ct))
                    {
                        var input = await message.Content.ReadAsStringAsync();
                        Console.WriteLine(input);
                        getVideoCode(input);

                        if (this.videoWindow == null)
                        {
                            createVideoWindow();
                        }
                        this.videoWindow.ShowYouTubeVideo(this.videoCode);
                        this.videoWindow.Show();
                    }
                }
            }
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("execution time: " + elapsedMs);

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
            GenerateDictionary(current_game);
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
            GenerateDictionary(current_game);
        }

        private void GenerateDictionary(String current_game){
            System.IO.StreamReader file =new System.IO.StreamReader(current_game+"_videos.txt");
            string line;
            int index = 0;
            this.dictionary.Clear();
            this.videoList.Clear();

            while ((line = file.ReadLine()) != null)
            {
                
                int idx = line.LastIndexOf(' ');
                string label = line.Substring(0, idx);
                string video = line.Substring(idx + 1);
                Console.WriteLine("line: " + line + "!END");
                Console.WriteLine("Lable: "+label + "!END"); // "My. name. is Bond"
                Console.WriteLine("video: "+video + "!END"); // "_James Bond!

                //string[] label_video = line.Split(' ');
                //Console.WriteLine(line);
                this.dictionary.Add(label, index);
                this.videoList.Add(line);
                index++;
            }
            file.Close();
        }

        // plays YOUTUBE walkthrough corresponding to given string label
        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        private void getVideoCode(String data){
            if (dictionary.ContainsKey(data))
            {
                this.VideoIndex = dictionary[data];
                Console.WriteLine("this VideoIndex is " + this.VideoIndex);//
                string label_video = this.videoList[this.VideoIndex];
                int idx = label_video.LastIndexOf(' ');
                string video = label_video.Substring(idx + 1);
                Console.WriteLine("getVideoCode label is : " + label);
                this.videoCode = video;
                Console.WriteLine("this youtube video code is : " + this.videoCode);
            }
            else {
                Console.WriteLine("Can not find this label in the dictionary : " + data);
            }
            
        }


        public void skipForward()
        {
            if (this.videoWindow != null && videoWindow.IsVisible)
            {
                if (this.VideoIndex != this.videoList.Count - 1)
                {
                    this.VideoIndex++;
                    string label_video = this.videoList[this.VideoIndex];
                    int idx = label_video.LastIndexOf(' ');
                    string video = label_video.Substring(idx + 1);
                    this.videoCode = video;
                    this.videoWindow.ShowYouTubeVideo(this.videoCode);
                    this.videoWindow.Show();
                }
            }

        }

        public void skipBack()
        {
            if (this.videoWindow != null && videoWindow.IsVisible)
            {
                if (this.VideoIndex != 0)
                {
                    this.VideoIndex--;
                    string label_video = this.videoList[this.VideoIndex];
                    int idx = label_video.LastIndexOf(' ');
                    string video = label_video.Substring(idx + 1);
                    this.videoCode = video;
                    this.videoWindow.ShowYouTubeVideo(this.videoCode);
                    this.videoWindow.Show();
                }
            }

        }




    }
}
