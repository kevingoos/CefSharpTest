using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CefSharpTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += Resolver;

                var tasks = new List<Task>();
                for (var i = 0; i < 10; i++)
                {
                    tasks.Add(Task.Run(() => new ScreenshotService($"Task{i}").Initialize()));
                }

                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static Assembly Resolver(object sender, ResolveEventArgs args)
        {
            var archSpecificPath = Environment.Is64BitProcess ? "x64" : "x86";
            return File.Exists(archSpecificPath)
                ? Assembly.LoadFile(archSpecificPath)
                : null;
        }
    }
}
