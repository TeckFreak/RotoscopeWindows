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
        private List<Button> buttons = new List<Button>();

        public Form1()
        {
            InitializeComponent();

            LoadConfig();
            LoadImage();
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

                if(distance > lastDistance + appConfig.ErrorCM || distance < lastDistance - appConfig.ErrorCM)
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
        }

        private void MoveImage(int distance)
        {
            int moveTo = -1 * ((image.Width - 1920) - (int)(distance * PixelsPerCentimeter));

            if(moveTo > 1920 - image.Width || moveTo < 1)
            {
                mainImage.Invoke((MethodInvoker)(() =>
                {
                    Transition.run(mainImage, "Left", moveTo, new TransitionType_Linear(20));
                    // mainImage.Location = new Point(moveTo, 0);
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
