using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace Server1704
{
    public class MyServer
    {
        private TcpListener myServer;
        private bool isRunning;
        private List<client> clients;
        private client currentUser;
        private Stopwatch stopwatch;
        private int pocetPrihlasenych;
        private int pocetChybnychPrihlaseni;
        private int pocetPrikazu;


        public MyServer(int port)
        {
            myServer = new TcpListener(System.Net.IPAddress.Any, port);
            myServer.Start();
            isRunning = true;
            clients = new List<client>();
            stopwatch = new Stopwatch();
            stopwatch.Start();
            pocetPrikazu = 0;
            pocetChybnychPrihlaseni = 0;
            ServerLoop(clients);
        }

        private void ServerLoop(List<client> clients)
        {
            Console.WriteLine("Server byl spusten");
            while (isRunning)
            {
                TcpClient client = myServer.AcceptTcpClient();
                ClientLoop(client, clients);
            }
        }

        private void ClientLoop(TcpClient client, List<client> clients)
        {
            StreamReader reader = new StreamReader(client.GetStream(), Encoding.UTF8);
            StreamWriter writer = new StreamWriter(client.GetStream(), Encoding.UTF8);

            writer.WriteLine("Byl jsi pripojen");
            writer.Flush();
            bool clientConnect = true;
            string? data = null;
            string? dataRecive = null;

            string vyber = "";

            while (!vyber.Equals("1") && !vyber.Equals("2"))
            {
                writer.Write("\"1\" - registrace\n");
                writer.Write("\"2\" - prihlaseni\n");
                writer.Write("vyber:");
                writer.Flush();
                vyber = reader.ReadLine();
            }
            if (vyber.Equals("1"))
            {
                bool volneJmeno = false;
                while (!volneJmeno)
                {
                    writer.Write("Zadejte jmeno(r):");
                    writer.Flush();
                    string jmenoTmp = reader.ReadLine();
                    writer.Write("Zadejte heslo(r):");
                    writer.Flush();
                    string hesloTmp = reader.ReadLine();
                    if (!ExistName(jmenoTmp))
                    {
                        WriteToFile(jmenoTmp, hesloTmp);
                        volneJmeno = true;
                    } else
                    {
                        writer.Write("Jmeno uz je zabrane!\n");
                    }
                }
            }
            client clientPrihlasovani = new client("", "");
            string jmeno;
            string heslo;
            bool dobryJmeno = false;
            bool dal = false;
            if (vyber.Equals("2") || vyber.Equals("1"))
            {
                ReadFile(clients);
                while (!dobryJmeno)
                {
                    writer.Write("Zadejte jmeno(p):");
                    writer.Flush();
                    jmeno = reader.ReadLine();
                    foreach (client c in clients)
                    {
                        if (c.Jmeno.Equals(jmeno))
                        {
                            dal = true;
                            dobryJmeno = true;
                            clientPrihlasovani = c;
                            break;
                        }
                        pocetChybnychPrihlaseni++;
                    }
                    if (!dobryJmeno)
                    {
                        writer.Write("Zadane jmeno zadnemu neodpovida!\n");
                    }
                }
            }
            
            if (dal)
            {
                int x = 0;
                for (int i = 0; i < 3; i++)
                {
                    writer.Write("Zadejte heslo(p):");
                    writer.Flush();
                    heslo = reader.ReadLine();
                    if (clientPrihlasovani.Heslo.Equals(heslo))
                    {
                        writer.Write("Uspesne prihlasen!\n");
                        writer.Flush();
                        pocetPrihlasenych++;
                        break;
                    }
                    x = i;
                }
                if (x >= 2)
                {
                    writer.Write("Moc pokusu!");
                    writer.Flush();
                    pocetChybnychPrihlaseni++;
                    clientConnect = false;
                }
                else
                {
                    clientConnect = true;
                    currentUser = clientPrihlasovani;
                }
            }

            while (clientConnect)
            {

                data = reader.ReadLine();
                data = data.ToLower();
                if (data == "who")
                {
                    writer.WriteLine(currentUser.ToString());
                    writer.Flush();
                    pocetPrikazu++;
                } else
                {
                    if (data == "uptime")
                    {
                        stopwatch.Stop();
                        TimeSpan stopwatchElapsed = stopwatch.Elapsed;
                        stopwatch.Start();
                        writer.WriteLine("Server uz bezi: " + (Convert.ToInt32(stopwatchElapsed.TotalSeconds)).ToString() + "s");
                        writer.Flush();
                        pocetPrikazu++;
                    } else
                    {
                        if (data == "stats")
                        {
                            pocetPrikazu++;
                            writer.WriteLine("Pocet prihlasenych uzivatelu: " + pocetPrihlasenych);
                            writer.WriteLine("Pocet chybnych prihlaseni: " + pocetChybnychPrihlaseni);
                            writer.WriteLine("Pocet zadanych prikazu: " + pocetPrikazu);
                            writer.Flush();
                            pocetPrikazu++;
                        } else
                        {
                            if (data == "last")
                            {
                                clientConnect = false;
                            } else
                            {
                                if (data == "exit")
                                {
                                    clientConnect = false;
                                    break;
                                } else
                                {
                                    if(data != "")
                                    {
                                        writer.WriteLine("Neznamy prikaz!");
                                        writer.Flush();
                                    }
                                    
                                }
                            }
                           
                        }
                        
                    }
                    
                }
               

                /*
                dataRecive = data + " prijato";
                writer.WriteLine(dataRecive);
                writer.Flush();
                */
            }
            writer.WriteLine("Byl jsi odpojen");
            writer.Flush();
        }

        private bool ExistName(string jmeno)
        {
            using (StreamReader reader = new StreamReader("uzivatele.txt"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] lineField = line.Split(",");
                    if (lineField[0].Equals(jmeno))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void ReadFile(List<client> clients)
        {
            using (StreamReader reader = new StreamReader("uzivatele.txt"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] lineField = line.Split(",");
                    clients.Add(new client(lineField[0], lineField[1]));
                }
            }
        }

        private void WriteToFile(string jmeno, string heslo)
        {
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader("uzivatele.txt"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }
            using (StreamWriter writer = new StreamWriter("uzivatele.txt"))
            {
                string line = jmeno + "," + heslo;
                writer.WriteLine(line);
                foreach (string l in lines)
                {
                    writer.WriteLine(l);
                }
            }
        }
    }
}

