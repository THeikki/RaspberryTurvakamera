using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Unosquare.WiringPi;
using Unosquare.RaspberryIO;
using System.IO;
using System.Net;
using Unosquare.RaspberryIO.Abstractions;

namespace turvakamera
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        static void takePicture()
        {
            DateTime date = DateTime.Now;

            var pictureBytes = Pi.Camera.CaptureImageJpeg(1024, 768);
            var targetPath = "/home/pi/Pictures/asunto.jpg" + date.ToString(" dd-MM-yyyy HH:mm:ss");

            File.WriteAllBytes(targetPath, pictureBytes);

        }
        void sendAlert()
        {
            DateTime date = DateTime.Now;

            var pictureBytes = Pi.Camera.CaptureImageJpeg(1024, 768);
            var targetPath = "/home/pi/Pictures/asunto.jpg" + date.ToString(" dd-MM-yyyy HH:mm:ss");

            File.WriteAllBytes(targetPath, pictureBytes);

            string base64 = Convert.ToBase64String(pictureBytes);

            string json = "{\"date\":\"" + date.ToString("dd-MM-yyyy HH:mm:ss") + "\",\"picture\":\"" + base64 + "\"}";

            string url = String.Format("https://raspberry-api.herokuapp.com/api/alerts");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/json";
            request.Method = "POST";

            using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                //string json = "{\"picture\":\"" + base64 + "\"}";
                //string json = "{\"date\":\"" + date.ToString() + "\",\"picture\":\"testi.jpg\"}";
                //string postData = "{\"date\":\"2.3.1111\",\"picture\":\"testi.jpg\"}";

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string result;
                using (StreamReader rdr = new StreamReader(response.GetResponseStream()))
                {
                    result = rdr.ReadToEnd();
                    Console.WriteLine(result);
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
                //MessageBox.Show(err.Message, "Virhe");       
                return;
            }
        }

        async void checkMovement()
        {
            var pirPin = Pi.Gpio[7];
            pirPin.PinMode = GpioPinDriveMode.Input;

            try
            {
                while (true)
                {
                    await Task.Delay(200);
                    if (pirPin.Read())
                    {
                        sendAlert();
                        await Task.Delay(200);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return;
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            checkMovement();
            Pi.Init<BootstrapWiringPi>();
        }

        private void buttonClose_Click_1(object sender, EventArgs e)
        {
            Pi.Camera.CloseVideoStream();
            Application.Exit();

        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
                
                takePicture();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Virhe");

            }

        }
    }

}

