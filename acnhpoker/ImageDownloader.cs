using System;
using System.IO;
using System.IO.Compression;
using System.Net;
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
            button1.Enabled = false;
            button2.Enabled = false;


            string path = Path.Combine(Application.StartupPath, "temp.zip");

            if (File.Exists(path))
            {
                extractHere();
            }
            else
            {
                WebClient webClient = new WebClient();
                webClient.Proxy = null;

                webClient.DownloadProgressChanged += (s, ez) =>
                {
                    progressBar1.Value = ez.ProgressPercentage;
                };
                webClient.DownloadFileCompleted += (s, ez) =>
                {
                    progressBar1.Visible = false;
                    extractHere();
                };

                webClient.DownloadFileAsync(new Uri("https://github.com/MyShiLingStar/ACNHPoker/releases/download/ImgPack4/img.zip"), "temp.zip");
            }

        }

        private void extractHere()
        {
            ZipFile.ExtractToDirectory(@".\temp.zip", @".\");
            this.Close();
        }
    }
}
