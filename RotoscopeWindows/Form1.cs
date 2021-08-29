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
        private VideoPlayer player = null;

        public Form1()
        {
            InitializeComponent();

            LoadConfig();
            LoadImage();
            LoadButtons();
            //InitSerialConnection();
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

                if(distance < 0)
                {
                    distance = appConfig.TotalCM;
                }
                else if(distance > appConfig.TotalCM)
                {
                    distance = 0;
                }
                else
                {
                    distance = appConfig.TotalCM - distance;
                }

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
                    Text = "",
                    Height = 60,
                    Width = 160,
                    BackgroundImage = Image.FromFile("Hindi.png"),
                    FlatStyle = FlatStyle.Flat,
                    BackgroundImageLayout = ImageLayout.Stretch,
                    Location = new Point(touchPoint.Position.X, touchPoint.Position.Y),
                    Name = "bH_" + i
                };

                Button btnEnglish = new Button()
                {
                    Text = "",
                    Height = 60,
                    Width = 160,
                    BackgroundImage = Image.FromFile("English.png"),
                    FlatStyle = FlatStyle.Flat,
                    BackgroundImageLayout = ImageLayout.Stretch,
                    Location = new Point(touchPoint.Position.X + 180, touchPoint.Position.Y),
                    Name = "bE_" + i,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                btnHindi.Click += BtnPlay_Click;
                btnEnglish.Click += BtnPlay_Click;

                mainImage.Controls.Add(btnHindi);
                mainImage.Controls.Add(btnEnglish);

                i++;
            }
        }

        private void BtnPlay_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int index = Convert.ToInt32(button.Name.Split('_')[1]);

            PlayVideo(index, button.Name.StartsWith("bH"), button.Name); 
        }

        private void PlayVideo(int index, bool isHindi, string callerName)
        {
            if(isHindi)
            {
                player = new VideoPlayer(appConfig.TouchPoints[index].FileH, callerName);
            }
            else
            {
                player = new VideoPlayer(appConfig.TouchPoints[index].FileE, callerName);
            }
        }

        private void MoveImage(int distance)
        {
            int moveTo = -1 * ((image.Width - 1920) - (int)(distance * PixelsPerCentimeter));

            if(moveTo > 1920 - image.Width || moveTo < 1)
            {
                mainImage.Invoke((MethodInvoker)(() =>
                {
                    Transition.run(mainImage, "Left", moveTo, new TransitionType_Linear(appConfig.TransitionSpeed));
                    mainImage.SendToBack();

                    // Check if video is playing and close it if button is out of bounds of screen.
                    if(player != null)
                    {
                        Button btn = mainImage.Controls.Find(player.CallerControlName, true)[0] as Button;

                        if(moveTo + btn.Location.X < 0 || moveTo + btn.Location.X + btn.Width > 1920)
                        {
                            player.Close();
                            player = null;
                        }
                    }
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
