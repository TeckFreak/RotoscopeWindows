using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RotoscopeWindows
{
    public partial class VideoPlayer : Form
    {
        private MediaPlayer mediaPlayer;

        public string CallerControlName
        {
            get; set;
        }

        public VideoPlayer(string videoPath, string callerName)
        {
            InitializeComponent();

            PlayVideo(videoPath);

            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;

            this.CallerControlName = callerName;
            this.TopMost = true;
            this.Show();
        }

        public void StopAndClose()
        {
            try
            {
                this.Hide();
                mediaPlayer.Stop();
                this.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void PlayVideo(string videoPath)
        {
            Core.Initialize();

            LibVLC libvlc = new LibVLC(enableDebugLogs: true);
            Media media = new Media(libvlc, new Uri(Path.Combine(Environment.CurrentDirectory, videoPath)));
            this.mediaPlayer = new MediaPlayer(media);

            videoView1.MediaPlayer = this.mediaPlayer;
            mediaPlayer.Play();
        }
    }
}
