using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Data.OleDb;
using System.Data;

namespace reciever
{
    class Program
    {
        public static List<ListenThread> ListenThreads = new List<ListenThread>();
        public static List<Socket> Sockets = new List<Socket>();
        //static int i = 0;
        static DataTable dt = new DataTable();

        static void Main(string[] args)
        {

            OleDbConnection con = new OleDbConnection(@"Provider = Microsoft.ACE.OLEDB.12.0; Data Source = Пароли.accdb");
            OleDbDataAdapter adapter = new OleDbDataAdapter("Select logs, passs, namess From Logins1", con);
            adapter.Fill(dt);

            Thread StartThread = new Thread(Searching);
            StartThread.IsBackground = true;
            StartThread.Start();
            while (true) //Рассылка всем клиентам
            {
                string message;
                message = Console.ReadLine();

                if (message == "#allmessages")
                {
                    foreach (string line in System.IO.File.ReadLines("save.txt"))
                    {
                        Console.WriteLine(line);
                    }
                }
                else
                {
                    message = "Сервер: " + message;
                    ListenThread.Sending(Sockets, message);
                }

            }

        }
        static void Searching()
        {
            Socket Sockett = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 667);
            Sockett.Bind(ipPoint);
            Sockett.Listen(100);
            while (true)
            {
                try
                {
                    int id = -1;

                    var NewSocket = Sockett.Accept();

                    byte[] buffer = new byte[8196];
                    int bytesRec = NewSocket.Receive(buffer);
                    string Login = Encoding.UTF8.GetString(buffer, 0, bytesRec);

                    Thread.Sleep(600);

                    byte[] buffer2 = new byte[8196];
                    int bytesRec1 = NewSocket.Receive(buffer2);
                    string Password = Encoding.UTF8.GetString(buffer2, 0, bytesRec1);

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Login == dt.Rows[i]["logs"].ToString() && Password == dt.Rows[i]["passs"].ToString())
                        {
                            id = i;

                            Sockets.Add(NewSocket);

                            ListenThread LT = new ListenThread(NewSocket, dt.Rows[i]["namess"].ToString());
                            ListenThreads.Add(LT);

                            Console.WriteLine("Новый подключение: " + dt.Rows[i]["namess"].ToString());

                            byte[] buffer1 = Encoding.UTF8.GetBytes("123");
                            int bytesSent = NewSocket.Send(buffer1);

                            Thread.Sleep(600);
                            ListenThread.Sending(Sockets, "Новое подключение: " + dt.Rows[i]["namess"].ToString());
                        }
                    }
                    if (id == -1)
                    {
                        byte[] buffer1 = Encoding.UTF8.GetBytes("000");
                        int bytesSent = NewSocket.Send(buffer1);
                        Console.WriteLine("Неудачная попытка подключения");
                        StreamWriter SW = new StreamWriter("save.txt", true);
                        SW.WriteLine(DateTime.Now.ToString("hh:mm") + " Неудачная попытка подключения");
                        SW.Close();
                    }
                }
                catch { }
            }
        }
    }
}