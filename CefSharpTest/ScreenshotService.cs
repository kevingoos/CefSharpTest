using System;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using CefSharp;
using CefSharp.OffScreen;
using Size = System.Drawing.Size;

namespace CefSharpTest
{
    public class ScreenshotService
    {
        private readonly string _taskName;
        private ChromiumWebBrowser _browser;
        private int _count;
        private System.Timers.Timer _timer;

        public ScreenshotService(string taskName)
        {
            _taskName = taskName;
        }

        public void Initialize()
        {
            //Setup browser
            if (!Cef.IsInitialized)
            {
                var path = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, Environment.Is64BitProcess ? "x64" : "x86",
                    "CefSharp.BrowserSubprocess.exe");
                var cachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"CefSharp\Cache");
                try
                {
                    var settings = new CefSettings
                    {
                        BrowserSubprocessPath = path,
                        CachePath = cachePath
                    };
                    Cef.Initialize(settings, true, browserProcessHandler: null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to initialize on dll path: {path} and cache path: {cachePath}. {ex}");
                }
            }

            //Reduce rendering speed to 1fps because more is not needed
            var browserSettings = new BrowserSettings {WindowlessFrameRate = 1};

            try
            {
                Console.WriteLine("Make browser connection");
                _browser = new ChromiumWebBrowser("https://www.youtube.com/watch?v=KyZArQMFhQ4", browserSettings) {Size = new Size(1400, 850)};

                async void Handler(object o, LoadingStateChangedEventArgs args)
                {
                    try
                    {
                        if (args.IsLoading) return;
                        StartupBrowserAndTimer();
                        _browser.LoadingStateChanged -= Handler;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error while handling IsLoadedCallback {ex}");
                    }
                }

                _browser.LoadingStateChanged += Handler;

                Console.WriteLine("Browser initialized, now job will keep running to detect pixel updates.");
                while (true)
                {
                    Application.DoEvents();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Cef.Shutdown();
            }
        }

        private void StartupBrowserAndTimer()
        {
            if (!Directory.Exists(_taskName))
            {
                Directory.CreateDirectory(_taskName);
            }
            
            _timer = new System.Timers.Timer { Interval = 100 };
            _timer.Elapsed += TimerOnElapsed;
            _timer.Enabled = true;
        }

        private async void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            var fullBrowserImage = await _browser.ScreenshotAsync();
            if (fullBrowserImage == null)
            {
                Console.WriteLine("No image yet...");
                return;
            }
            fullBrowserImage.Save($@"{_taskName}/{_taskName}{_count++} {DateTime.UtcNow.Ticks}.jpg");
            Console.WriteLine("Browser opened on URL and loading page...");
        }
    }
}
