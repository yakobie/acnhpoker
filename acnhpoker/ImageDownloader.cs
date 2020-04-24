using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO.Compression;
using System.IO;

namespace acnhpoker
{
    public partial class ImageDownloader : Form
    {
        public ImageDownloader()
        {
            InitializeComponent();
        }



        private void label1_Click(object sender, EventArgs e)
        {

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
                webClient.Proxy = GlobalProxySelection.GetEmptyWebProxy();

                webClient.DownloadProgressChanged += (s, ez) =>
                {
                    progressBar1.Value = ez.ProgressPercentage;
                };
                webClient.DownloadFileCompleted += (s, ez) =>
                {
                    progressBar1.Visible = false;
                    extractHere();
                };

                webClient.DownloadFileAsync(new Uri("https://github.com/KingLycosa/acnhpoker/releases/download/0.0001/img.zip"), "temp.zip");
            }

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void extractHere()
        {
            ZipFile.ExtractToDirectory(@".\temp.zip", @".\");
            this.Close();
        }
    }
}
