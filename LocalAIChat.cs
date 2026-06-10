using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace LocalAIChat
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LauncherForm());
        }
    }

    public class LauncherForm : Form
    {
        private ProgressBar progressBar;
        private Label statusLabel;
        private string installerUrl = "https://ollama.com/download/OllamaSetup.exe";
        private string htmlPath = "index.html";
        private bool isStarting = false;

        public LauncherForm()
        {
            this.Text = "Local AI Chat Launcher";
            this.Size = new Size(400, 150);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            statusLabel = new Label
            {
                Text = "Checking system requirements...",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 40
            };

            progressBar = new ProgressBar
            {
                Style = ProgressBarStyle.Continuous,
                Dock = DockStyle.Bottom,
                Height = 30
            };

            this.Controls.Add(statusLabel);
            this.Controls.Add(progressBar);
        }

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (!isStarting)
            {
                isStarting = true;
                await StartSetup();
            }
        }

        private bool IsOllamaInstalled()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return File.Exists(Path.Combine(localAppData, "Programs", "Ollama", "ollama.exe"));
        }

        private async Task StartSetup()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string installerPath = Path.Combine(baseDir, "OllamaSetup.exe");
                string uiPath = Path.Combine(baseDir, htmlPath);

                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string ollamaExe = Path.Combine(localAppData, "Programs", "Ollama", "ollama.exe");

                if (!IsOllamaInstalled())
                {
                    var result = MessageBox.Show(this, "Ollama is not installed on this system. We need to download and install the official Ollama Windows App to run the AI engine.\n\nContinue?", "Setup Required", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (result == DialogResult.No)
                    {
                        Application.Exit();
                        return;
                    }

                    if (!File.Exists(installerPath))
                    {
                        statusLabel.Text = "Downloading official Installer... Please wait.";
                        using (WebClient client = new WebClient())
                        {
                            client.DownloadProgressChanged += (s, ev) =>
                            {
                                progressBar.Value = ev.ProgressPercentage;
                                statusLabel.Text = string.Format("Downloading... {0}% ({1} MB / {2} MB)", ev.ProgressPercentage, ev.BytesReceived / 1024 / 1024, ev.TotalBytesToReceive / 1024 / 1024);
                            };

                            await client.DownloadFileTaskAsync(new Uri(installerUrl), installerPath);
                        }
                    }

                    progressBar.Value = 100;
                    statusLabel.Text = "Running Installer (Please accept any prompts)...";
                    
                    var installProcess = Process.Start(new ProcessStartInfo
                    {
                        FileName = installerPath,
                        Arguments = "/S", // Silent install if supported
                        UseShellExecute = true,
                        Verb = "runas" // Request admin
                    });
                    
                    installProcess.WaitForExit();
                    
                    if (!IsOllamaInstalled())
                    {
                        MessageBox.Show("Installation failed or was cancelled.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Application.Exit();
                        return;
                    }
                }

                statusLabel.Text = "Starting AI Engine...";
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = ollamaExe,
                    Arguments = "serve",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                startInfo.EnvironmentVariables["OLLAMA_ORIGINS"] = "*";
                
                try { Process.Start(startInfo); } catch { }

                await Task.Delay(3000); // give server time to start

                statusLabel.Text = "Verifying/Downloading Llama 3 model...";
                
                Process.Start(new ProcessStartInfo
                {
                    FileName = ollamaExe,
                    Arguments = "pull llama3",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                statusLabel.Text = "Opening UI...";
                Process.Start(new ProcessStartInfo
                {
                    FileName = "file:///" + uiPath.Replace("\\", "/"),
                    UseShellExecute = true
                });

                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error: " + ex.Message, "Setup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }
    }
}
