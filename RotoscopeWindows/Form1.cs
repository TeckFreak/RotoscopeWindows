using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Transitions;

namespace RotoscopeWindows
{
    public partial class Form1 : Form
    {
        private SerialPort serialPort;
        private AppConfig appConfig;
        private Image image;
        private int lastDistance = 0;

        public Form1()
        {
            InitializeComponent();

            LoadConfig();
            LoadImage();
            LoadButtons();
            InitSerialConnection();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_EX_COMPOSITED = 0x02000000;
                var cp = base.CreateParams;
                cp.ExStyle |= WS_EX_COMPOSITED;
                return cp;
            }
        }

        private void LoadConfig()
        {
            using (StreamReader reader = new StreamReader("config.json"))
            {
                string json = reader.ReadToEnd();
                appConfig = JsonConvert.DeserializeObject<AppConfig>(json);
            }
        }

        private void InitSerialConnection()
        {
            serialPort = new SerialPort(appConfig.Port)
            {
                BaudRate = 9600
            };
            serialPort.DataReceived += SerialPort_DataReceived;
            serialPort.Open();
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = (sender as SerialPort).ReadLine();
                int distance = Convert.ToInt32(data);

                if((distance > lastDistance + appConfig.ErrorCM || distance < lastDistance - appConfig.ErrorCM) && distance <= appConfig.TotalCM)
                {
                    MoveImage(distance);
                    lastDistance = distance;
                }
            }
            catch
            {

            }
        }

        private void LoadImage()
        {
            image = Image.FromFile(appConfig.MainImage);
            
            mainImage.Width = image.Width;
            mainImage.Image = image;

            this.Width = image.Width;
        }

        private void LoadButtons()
        {
            int i = 0;
            foreach (TouchPoint touchPoint in appConfig.TouchPoints)
            {
                Button btnHindi = new Button()
                {
                    Text = "Hindi",
                    Height = 40,
                    Width = 80,
                    BackColor = Color.Gray,
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(touchPoint.Position.X, touchPoint.Position.Y),
                    Name = "bH_" + i
                };

                Button btnEnglish = new Button()
                {
                    Text = "English",
                    Height = 40,
                    Width = 80,
                    BackColor = Color.Gray,
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(touchPoint.Position.X + 90, touchPoint.Position.Y),
                    Name = "bE_" + i,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                btnHindi.Click += BtnPlay_Click;
                btnEnglish.Click += BtnPlay_Click;

                mainImage.Controls.Add(btnHindi);
                mainImage.Controls.Add(btnEnglish);
            }
        }

        private void BtnPlay_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int index = Convert.ToInt32(button.Name.Split('_')[1]);

            PlayVideo(index, button.Name.StartsWith("bH")); 
        }

        private void PlayVideo(int index, bool isHindi)
        {
            if(isHindi)
            {
                new VideoPlayer(appConfig.TouchPoints[index].FileH);
            }
            else
            {
                new VideoPlayer(appConfig.TouchPoints[index].FileE);
            }
        }

        private void MoveImage(int distance)
        {
            int moveTo = -1 * ((image.Width - 1920) - (int)(distance * PixelsPerCentimeter));

            if(moveTo > 1920 - image.Width || moveTo < 1)
            {
                mainImage.Invoke((MethodInvoker)(() =>
                {
                    Transition.run(mainImage, "Left", moveTo, new TransitionType_Linear(100));
                    mainImage.SendToBack();
                }));
            }
        }

        private float PixelsPerCentimeter
        {
            get
            {
                return (image.Width - 1920) / appConfig.TotalCM;
            }
        }
    }
}
