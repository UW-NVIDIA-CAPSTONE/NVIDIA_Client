using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WpfApplication1
{
    public class ScreenCapture
    {
        //protected static IntPtr m_HBitmap;
        //private static int i = 0;
        public IntPtr m_HBitmap;
        public int i;

        /*static void Main(string[] args)
        {
            Timer aTimer = new System.Timers.Timer();

            // Hook up the Elapsed event for the timer.
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

            // Set the Interval to 2 seconds (2000 milliseconds).
            aTimer.Interval = 2000;
            aTimer.Enabled = true;

            Console.WriteLine("Press the Enter key to exit the program.");
            Console.ReadLine();
        }*/
        public ScreenCapture()
        {
            this.i = 0;
        }

        public void Capture(int i)
        {
            try
            {
                Bitmap capture = GetDesktopImage();
                string file = Path.Combine(Environment.CurrentDirectory, "screen" + i + ".png");
                ImageFormat format = ImageFormat.Gif;
                capture.Save(file, format);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public Bitmap GetDesktopImage()
        {
            WIN32_API.SIZE size;

            IntPtr hDC = WIN32_API.GetDC(WIN32_API.GetDesktopWindow());
            IntPtr hMemDC = WIN32_API.CreateCompatibleDC(hDC);

            size.cx = WIN32_API.GetSystemMetrics(WIN32_API.SM_CXSCREEN);
            size.cy = WIN32_API.GetSystemMetrics(WIN32_API.SM_CYSCREEN);

            m_HBitmap = WIN32_API.CreateCompatibleBitmap(hDC, size.cx, size.cy);

            if (m_HBitmap != IntPtr.Zero)
            {
                IntPtr hOld = (IntPtr)WIN32_API.SelectObject(hMemDC, m_HBitmap);
                WIN32_API.BitBlt(hMemDC, 0, 0, size.cx, size.cy, hDC, 0, 0, WIN32_API.SRCCOPY);
                WIN32_API.SelectObject(hMemDC, hOld);
                WIN32_API.DeleteDC(hMemDC);
                WIN32_API.ReleaseDC(WIN32_API.GetDesktopWindow(), hDC);
                return System.Drawing.Image.FromHbitmap(m_HBitmap);
            }
            return null;
        }

        /*private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Console.WriteLine("This is the " + i + "th time.");
            Capture(i);
            i++;
        }*/
    }
}
