using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatClientTCP
{
    public partial class Form1 : Form
    {
        string userName;
        const string HOST = "127.0.0.1";
        const int PORT = 8888;
        TcpClient client;
        NetworkStream stream;

        public Form1()
        {
            InitializeComponent();

            buttonLogin.Enabled = true;
            buttonLogout.Enabled = false;
            buttonSend.Enabled = false;
            textBoxChat.ReadOnly = true;
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            userName = textBoxName.Text;
            textBoxName.ReadOnly = true;
            textBoxChat.Clear();

            try
            {
                client = new TcpClient();
                client.Connect(HOST, PORT);
                stream = client.GetStream();

                string message = userName;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);

                Task receiveTask = new Task(ReceiveMessage);
                receiveTask.Start();

                ShowMessage(String.Format("Добро пожаловать, {0}!", userName));
                buttonLogin.Enabled = false;
                buttonLogout.Enabled = true;
                buttonSend.Enabled = true;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                Disconnect();
            }
        }

        private void buttonLogout_Click(object sender, EventArgs e)
        {
            ExitChat();
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            try
            {
                string message = String.Format("{0}: {1}", userName, textBoxMessage.Text);
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);
                ShowMessage(textBoxMessage.Text);

                textBoxMessage.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Disconnect();
            }
        }

        void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64];
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        if (bytes == 0) throw new Exception("Вы вышли из чата");
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    ShowMessage(message);
                }
                catch(ObjectDisposedException)
                {
                    Disconnect();
                    break;
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Disconnect();
                    break;
                }
            }
        }

        void ShowMessage(string message)
        {
            this.Invoke(new MethodInvoker(() =>
            {
                string time = DateTime.Now.ToShortTimeString();
                textBoxChat.Text = textBoxChat.Text + "[" + time + "] " + message + "\r\n";
            }));
        }

        void Disconnect()
        {
            if (stream != null) stream.Close();
            if (client != null) client.Close();
            Environment.Exit(0);
        }

        void ExitChat()
        {
            //string message = userName + " покидает чат";
            //byte[] data = Encoding.Unicode.GetBytes(message);
            //stream.Write(data, 0, data.Length);
                        
            Disconnect();
            
            ShowMessage("Вы покинули чат");
            buttonLogin.Enabled = true;
            buttonLogout.Enabled = false;
            buttonSend.Enabled = false;
        }

        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ExitChat();
        }
    }
}
