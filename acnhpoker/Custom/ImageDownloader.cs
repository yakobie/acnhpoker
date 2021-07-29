using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace ACNHPoker
{
    public partial class ImageDownloader : Form
    {
        public ImageDownloader()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            yesBtn.Enabled = false;
            noBtn.Enabled = false;


            string path = Path.Combine(Application.StartupPath, "img.zip");

            if (File.Exists(path))
            {
                waitmsg.Visible = true;
                progressBar.Visible = false;

                Thread unzipThread = new Thread(delegate () { extractHere(); });
                unzipThread.Start();
            }
            else
            {
                WebClient webClient = new WebClient();
                webClient.Proxy = null;

                webClient.DownloadProgressChanged += (s, ez) =>
                {
                    progressBar.Value = ez.ProgressPercentage;
                };

                webClient.DownloadFileCompleted += (s, ez) =>
                {
                    waitmsg.Visible = true;
                    progressBar.Visible = false;

                    Thread unzipThread = new Thread(delegate () { extractHere(); });
                    unzipThread.Start();
                };

                webClient.DownloadFileAsync(new Uri("https://github.com/MyShiLingStar/ACNHPoker/releases/download/ImgPack7/img.zip"), "img.zip");
            }

        }

        private void extractHere()
        {
            ZipFile.ExtractToDirectory(@".\img.zip", @".\");
            this.Invoke((MethodInvoker)delegate
            {
                this.Close();
                Application.Restart();
            });
        }
    }
}
