using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Net;

namespace TanksTheGame
{
    class ComLink
    {
        public static DateTime lastSend;
        public static int MIN_COMM_GAP = 1200;  //minimum time between 2 sends

        public static String inIP = "10.10.5.166";
        public static String outIP = "10.8.106.49";
        public static int inPort = 7000;
        public static int outPort = 6000;

        //to server
        public static void send(String message)
        {
            try
            {
                TcpClient client = new TcpClient();
                Console.WriteLine("Connecting ...");
                client.Connect(outIP, outPort); //server IP and port

                Console.WriteLine("Connected");

                Stream stream = client.GetStream();
                byte[] buffer = Encoding.ASCII.GetBytes(message);

                stream.Write(buffer, 0, buffer.Length);        //writing bytes to stream
                stream.Flush();
                client.Close();

                //remember last sent time
                lastSend = DateTime.UtcNow;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error:" + e);
            }
        }

        //from server
        public static String receive()
        {
            String message = null;
            try
            {
                IPAddress ipAd = IPAddress.Parse(inIP);
                TcpListener listener = new TcpListener(ipAd, inPort);   //listen on port 7000
                listener.Start();

                Socket socket = listener.AcceptSocket();

                byte[] buffer = new byte[1500];
                int size = socket.Receive(buffer);

                message = Encoding.ASCII.GetString(buffer, 0, size - 1);    //remove # mark
                Console.WriteLine(message);

                socket.Close();
                listener.Stop();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error:" + e);
            }
            return message;
        }
    }
}
