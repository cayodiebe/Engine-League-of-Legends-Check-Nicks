using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using GetSummonerNames.Model;
using GetSummonerNames.Properties;
using Newtonsoft.Json;

namespace GetSummonerNames
{
    public partial class Form1 : Form
    {
       
        public Form1()
        {
            InitializeComponent();
        }

        private const string linkBrOp = "https://www.op.gg/summoners/br/";
        public static Dictionary<int, string> listaAliados { get; set; } = new Dictionary<int, string>();
        public static Dictionary<int, string> listaLinks { get; set; } = new Dictionary<int, string>();
        public static Dictionary<string, string> Riot { get; set; } = new Dictionary<string, string>();
        public static Dictionary<string, string> Client { get; set; } = new Dictionary<string, string>();

        private string _regiao = "BR";
        private string _players;
        public const int WmNclbuttondown = 0xA1;
        public const int HtCaption = 0x2;





        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WmNclbuttondown, HtCaption, 0);
            }
        }



        private static string Cmd(string gamename)
        {
            var commandline = "";
            var mngmtClass = new ManagementClass("Win32_Process");
            foreach (var managementBaseObject in mngmtClass.GetInstances())
            {
                var o = (ManagementObject)managementBaseObject;
                if (o["Name"].Equals(gamename))
                {
                    commandline = "[" + o["CommandLine"] + "]";
                }
            }

            return commandline;
        }

        private static string Findstring(string text, string from, string to)
        {
            var pFrom = text.IndexOf(from, StringComparison.Ordinal) + from.Length;
            var pTo = text.LastIndexOf(to, StringComparison.Ordinal);

            return text.Substring(pFrom, pTo - pFrom);
        }

        private static string Getregion(string requeqst)
        {
            return JsonConvert.DeserializeObject<Regiao>(requeqst).region;
        }

        private static void get_lcu()
        {
            Riot.Clear();
            Client.Clear();

            var commandline = Cmd("LeagueClientUx.exe");

            Riot.Add("port", Findstring(commandline, "--riotclient-app-port=", "\" \"--no-rads"));
            Riot.Add("token", Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes("riot:" + Findstring(commandline, "--riotclient-auth-token=", "\" \"--riotclient"))));

            Client.Add("port", Findstring(commandline, "--app-port=", "\" \"--install"));
            Client.Add("token", Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes("riot:" + Findstring(commandline, "--remoting-auth-token=", "\" \"--respawn-command=LeagueClient.exe"))));
        }

        
        private static string MakeRequest(string type, string url, bool client)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback += delegate
                {
                    const bool validationResult = true;
                    return validationResult;
                };

                int port;
                string token;

                if (client)
                {
                    port = Convert.ToInt32(Client["port"]);
                    token = Client["token"];
                }
                else
                {
                    port = Convert.ToInt32(Riot["port"]);
                    token = Riot["token"];
                }

                var request = (HttpWebRequest)WebRequest.Create("https://127.0.0.1:" + port + url);
                request.PreAuthenticate = true;
                request.ContentType = "application/json";
                request.Method = type;
                request.Headers.Add("Authorization", "Basic " + token);

                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    return streamReader.ReadToEnd();
                }
            }
            catch
            {
                MessageBox.Show("Request failed - No Connection...", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }
        }

        private void Getplayers(string req)
        {
            listaLinks.Clear();
            listaAliados.Clear();
            _players = "";
            var deserialized = JsonConvert.DeserializeObject<Aliados>(req);
            var count = 0;

            foreach (var player in deserialized.Participants)
            {
                count++;
                listaAliados.Add(count, player.Name);
                listaLinks.Add(count, linkBrOp + "/" + player.Name);
                _players += player.Name + ", ";
            }
            
            linkLabel1.Text = listaAliados[1];
            linkLabel2.Text = listaAliados[2];
            linkLabel3.Text = listaAliados[3];
            linkLabel4.Text = listaAliados[4];
            linkLabel5.Text = listaAliados[5];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            get_lcu();
            var test = MakeRequest("GET", "/chat/v5/participants/champ-select" /*Found Request in various Logs C:\Riot Games\League of Legends\Logs\LeagueClient*/, false);
            Getplayers(test);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.op.gg/multisearch/br?summoners=" + _players);
        }

        private static readonly Random Random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Text = RandomString(16);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(listaLinks[1]);
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(listaLinks[2]);
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(listaLinks[3]);
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(listaLinks[4]);
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(listaLinks[5]);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }

}
