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
        public VideoPlayer(string videoPath)
        {
            InitializeComponent();

            PlayVideo(videoPath);

            this.ShowDialog();
        }

        private void PlayVideo(string videoPath)
        {
            Core.Initialize();

            var libvlc = new LibVLC(enableDebugLogs: true);
            var media = new Media(libvlc, new Uri(Path.Combine(Environment.CurrentDirectory, videoPath)));
            var mediaplayer = new MediaPlayer(media);

            videoView1.MediaPlayer = mediaplayer;
            mediaplayer.Play();

            //Process.Start(Path.Combine(AppContext.BaseDirectory, videoPath));
        }
    }
}
