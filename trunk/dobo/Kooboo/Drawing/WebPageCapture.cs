using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Diagnostics;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Kooboo.Drawing
{
    public class WebPageCapture
    {
        private static bool _workerRunning = false;
        private static object lockObj = new object();

        public static void CaptureInThread(string url, EventHandler<WebPageCapturedEventArgs> captured,
            HttpCookieCollection cookies = null, Action<Exception> onError = null)
        {
            WebPageCaptureWorker.Tasks.Enqueue(new WebPageCaptureTask() { Url = url, Captured = captured, Cookies = cookies });
            if (!_workerRunning)
            {
                lock (lockObj)
                {
                    if (!_workerRunning)
                    {
                        StartWorker(onError);
                    }
                }
            }
        }

        private static void StartWorker(Action<Exception> onError)
        {
            _workerRunning = true;
            var thread = new Thread(() =>
            {
                try
                {
                    var worker = new WebPageCaptureWorker();
                    worker.Run();
                }
                catch (Exception ex)
                {
                    if (onError != null)
                    {
                        onError(ex);
                    }
                }
                finally
                {
                    lock (lockObj)
                    {
                        _workerRunning = false;
                    }
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
    }

    public class WebPageCaptureWorker
    {
        internal static readonly ConcurrentQueue<WebPageCaptureTask> Tasks = new System.Collections.Concurrent.ConcurrentQueue<WebPageCaptureTask>();

        private WebBrowser _browser = new WebBrowser();

        public EventHandler<WebPageCapturedEventArgs> _captured;


        public WebPageCaptureWorker()
        {
            _browser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(Browser_DocumentCompleted);
            _browser.ScrollBarsEnabled = false;
        }

        public void Run()
        {
            WebPageCaptureTask task;
            while (Tasks.TryDequeue(out task))
            {
                _captured = task.Captured;

                if (task.Cookies != null)
                {
                    SetCookies(task.Cookies, task.Url);
                }

                _browser.Navigate(task.Url);

                while (_browser.ReadyState != WebBrowserReadyState.Complete)
                {
                    Application.DoEvents();
                }
            }
        }

        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool InternetSetCookie(string url, string name, string data);

        public static bool SetWinINETCookieString(string sURL, string sName, string sData)
        {
            return InternetSetCookie(sURL, sName, sData);
        }

        private static object SetCookiesLocker = new object();

        public static void SetCookies(HttpCookieCollection cookies, string url = null)
        {
            lock (SetCookiesLocker)
            {
                foreach (var key in cookies.AllKeys)
                {
                    var cookie = cookies[key];
                    SetWinINETCookieString(url ?? cookie.Path, cookie.Name, cookie.Value);
                }
            }
        }

        private void Browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            Rectangle body = _browser.Document.Body.ScrollRectangle;

            _browser.Width = body.Width;
            _browser.Height = body.Height;

            using (var bitmap = new Bitmap(body.Width, body.Height))
            {
                IViewObject ivo = _browser.Document.DomDocument as IViewObject;

                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    //get the handle to the device context and draw
                    IntPtr hdc = g.GetHdc();
                    ivo.Draw(1, -1, IntPtr.Zero, IntPtr.Zero,
                                IntPtr.Zero, hdc, ref body,
                                ref body, IntPtr.Zero, 0);
                    g.ReleaseHdc(hdc);
                }

                if (_captured != null)
                {
                    WebPageCapturedEventArgs args = new WebPageCapturedEventArgs()
                    {
                        Image = bitmap
                    };
                    _captured(this, args);
                }
            }
        }

        [ComVisible(true), ComImport()]
        [GuidAttribute("0000010d-0000-0000-C000-000000000046")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IViewObject
        {
            [return: MarshalAs(UnmanagedType.I4)]
            [PreserveSig]
            int Draw(
                [MarshalAs(UnmanagedType.U4)] UInt32 dwDrawAspect,
                int lindex,
                IntPtr pvAspect,
                [In] IntPtr ptd,
                IntPtr hdcTargetDev,
                IntPtr hdcDraw,
                [MarshalAs(UnmanagedType.Struct)] ref Rectangle lprcBounds,
                [MarshalAs(UnmanagedType.Struct)] ref Rectangle lprcWBounds,
                IntPtr pfnContinue,
                [MarshalAs(UnmanagedType.U4)] UInt32 dwContinue);
            [PreserveSig]
            int GetColorSet([In, MarshalAs(UnmanagedType.U4)] int dwDrawAspect,
               int lindex, IntPtr pvAspect, [In] IntPtr ptd,
                IntPtr hicTargetDev, [Out] IntPtr ppColorSet);
            [PreserveSig]
            int Freeze([In, MarshalAs(UnmanagedType.U4)] int dwDrawAspect,
                            int lindex, IntPtr pvAspect, [Out] IntPtr pdwFreeze);
            [PreserveSig]
            int Unfreeze([In, MarshalAs(UnmanagedType.U4)] int dwFreeze);
            void SetAdvise([In, MarshalAs(UnmanagedType.U4)] int aspects,
              [In, MarshalAs(UnmanagedType.U4)] int advf,
              [In, MarshalAs(UnmanagedType.Interface)] IAdviseSink pAdvSink);
            void GetAdvise([In, Out, MarshalAs(UnmanagedType.LPArray)] int[] paspects,
              [In, Out, MarshalAs(UnmanagedType.LPArray)] int[] advf,
              [In, Out, MarshalAs(UnmanagedType.LPArray)] IAdviseSink[] pAdvSink);
        }
    }

    internal class WebPageCaptureTask
    {
        public string Url { get; set; }

        public HttpCookieCollection Cookies { get; set; }

        public EventHandler<WebPageCapturedEventArgs> Captured { get; set; }
    }

    public class WebPageCapturedEventArgs : EventArgs
    {
        public Image Image { get; set; }
    }
}
