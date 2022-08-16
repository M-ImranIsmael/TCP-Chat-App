﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace Server_GUI
{
    public partial class Server : Form
    {
        private bool ConnectButton_clicked = false;

        public List<string> chat_history = new List<string>();

        public Server()
        {
            InitializeComponent();
            Thread thread = new Thread(StartListen)
            {
                IsBackground = true,
            };
            thread.Start();
        }

        public void StartListen()
        {
            TcpListener server = null;
            try
            {

                // Set the TcpListener on port 13000.
                Int32 port = 13000;
                IPAddress localAddr = IPAddress.Any;
                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;
                // Enter the listening loop.
                while (true)
                {
                    
                    // Perform a blocking call to accept requests.
                    // You could also use server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();

                    //If client request conecction then change the Disconnect Button
                    DisconnectButton.BeginInvoke(new Action(() =>
                    {
                        DisconnectButton.BackColor = Color.Green;
                        DisconnectButton.Text = "Connected";
                        ConnectButton_clicked = true;
                    }));
                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();
                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        chat_history.Add("[Client ]: " + data);
                        LeftLabel(data);
                    }
                    // Shutdown and end connection
                    client.Close();
                }

            }
            catch (SocketException c)   
            {
                Console.WriteLine("SocketException: {0}", c);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
        }
        private void LeftLabel(string data)
        {

            string input = data;
            Label ServerLabel = new Label();

            Invoke(new Action(() =>
            {
                this.Controls.Add(ServerLabel);
                flowLayoutPanel1.Controls.Add(ServerLabel);
            }));

            ServerLabel.BeginInvoke(new Action(() =>
            {
                ServerLabel.Text = input;
                ServerLabel.BackColor = Color.Red;
                ServerLabel.ForeColor = Color.White;
                ServerLabel.BorderStyle = BorderStyle.FixedSingle;
                ServerLabel.Margin = new Padding(0, 0, 0, 4);
                ServerLabel.AutoSize = true;
            }));

            Console.WriteLine(data);
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        static void Connect(String server, String message)
        {
            try
            {
                Int32 port = 13001;
                TcpClient client = new TcpClient(server, port);
                Console.WriteLine("(You): {0}", message);
                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();
                NetworkStream stream = client.GetStream();
                // Send the message to the connected TcpServer.
                stream.Write(data, 0, data.Length);
                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }
        private void SendButton_Click(object sender, EventArgs e)
        {
            if (ConnectButton_clicked)
            {
                string input = ServerTextBox.Text;
                if(input != "")
                {
                    chat_history.Add("[Server]: " + input);
                    RightLabel(input);
                    ServerTextBox.Clear();
                    Connect(GetLocalIPAddress(), input);
                }
                
            }
            else
            {
                MessageBox.Show("No connection, make sure your client is connected to this server.");
            }
        }
        private void RightLabel(string x)
        {
            string input = x;
            Label MyLabel = new Label();
            this.Controls.Add(MyLabel);
            flowLayoutPanel1.Controls.Add(MyLabel);
            MyLabel.Text = input;
            MyLabel.BackColor = Color.Green;
            MyLabel.ForeColor = Color.White;
            MyLabel.BorderStyle = BorderStyle.FixedSingle;
            MyLabel.Margin = new Padding(flowLayoutPanel1.Width - MyLabel.Right, 0, 0, 4);
            MyLabel.AutoSize = true;
        }
        
        private void ExportButton_Click(object sender, EventArgs e)
        {
            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "txt files (*.txt)|*.txt";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    // Code to write the stream goes here.
                    TextWriter txt = new StreamWriter(myStream);
                    foreach (string s in chat_history)
                    {
                        txt.Write(s + "\n");
                    }
                    txt.Close();
                    myStream.Close();
                }
            }
        }
    }
}
