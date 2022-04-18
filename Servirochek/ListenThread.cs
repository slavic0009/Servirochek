using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace reciever
{
    internal class ListenThread
    {
        Socket ListenSocket;
        string Name;
        public ListenThread(Socket Sockett, string Name_)
        {
            Name = Name_;
            ListenSocket = Sockett;
            Thread ListenThread = new Thread(Listening);
            ListenThread.IsBackground = true;
            ListenThread.Start();
        }
        void Listening()
        {
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[8196];
                    int bytesRec = ListenSocket.Receive(buffer);
                    string data = Encoding.UTF8.GetString(buffer, 0, bytesRec);
                    if (data != "#")
                    {
                        Console.WriteLine(Name + ": " + data);
                        data = Name + ": " + data;
                        Sending(Program.Sockets, data);
                    }
                    else 
                    {
                        Console.WriteLine(Name + " отключается");
                        Sending(Program.Sockets, Name + " отключается");
                    }
                }
                catch { }
            }
        }
        public static void Sending(List<Socket> Sockket, string s)
        {
            for (int i = 0; i < Sockket.Count; i++)
            {
                try
                {
                    byte[] buffer1 = Encoding.UTF8.GetBytes(s);
                    int bytesSent = Sockket[i].Send(buffer1);
                    StreamWriter SW = new StreamWriter("save.txt", true);
                    SW.WriteLine(DateTime.Now.ToString() + " " + s);
                    SW.Close();
                }
                catch { }
            }
        }
    }
}
