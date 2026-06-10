using System;
using System.Diagnostics;
using System.IO;

namespace LocalAIChat
{
    class Program
    {
        static void Main(string[] args)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string batFile = Path.Combine(baseDir, "LocalAIChat.bat");

            if (!File.Exists(batFile))
            {
                Console.WriteLine("ERROR: LocalAIChat.bat not found in " + baseDir);
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = "/c \"" + batFile + "\"";
            psi.UseShellExecute = true;
            Process.Start(psi);
        }
    }
}
