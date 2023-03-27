using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace SimpleWebServer
{
    public static class Server
    {
        private static HttpListener listener;
        /// <summary>
        /// Returns list of IPs of devices assigned to localhost network.
        /// </summary>
        /// <returns></returns>
        private static List<IPAddress> GetLocalHostIPs()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            List<IPAddress> ips = host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToList();
            return ips;
        }
        private static HttpListener InitializeListener(List<IPAddress> localHostIPs)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost/");

            localHostIPs.ForEach(ip =>
            {
                listener.Prefixes.Add($"http://{ip.ToString()}/");
                Console.WriteLine("IP: " + $"http://{ip.ToString()}/");
            });
            return listener;
        }

        public static int maxSimultaneousConnections = 20;
        private static Semaphore sem = new Semaphore(maxSimultaneousConnections, maxSimultaneousConnections);

        private static void Start(HttpListener listener)
        {
            listener.Start();
            Task.Run(() => RunServer(listener));
        }
        /// <summary>
        /// Start awaiting for connections, up to the max value.
        /// Code runs in a separate thread.
        /// </summary>
        /// <param name="listener"></param>
        private static void RunServer(HttpListener listener) 
        {
            while (true)
            {
                sem.WaitOne();
                StartConnectionListener(listener);
            }
        }
        /// <summary>
        /// Await connections.
        /// </summary>
        /// <param name="listener"></param>
        private static async void StartConnectionListener(HttpListener listener)
        {
            HttpListenerContext context = await listener.GetContextAsync();
            sem.Release();

            Console.WriteLine("tuta");

            var resp = context.Response;
            string response = "<html><head><meta http-equiv='content-type' content='text/html; charset=utf-8'/></ head > Hello Browser! </ html > ";
            byte[] encoded = Encoding.UTF8.GetBytes(response);
            resp.ContentLength64 = encoded.Length;
            using Stream output = resp.OutputStream;
            await output.WriteAsync(encoded);
            await output.FlushAsync();

            Console.WriteLine("a teper tuta");
        }

        /// <summary>
        /// Starts the web server.
        /// </summary>
        public static void Start()
        {
            List<IPAddress> localHostIPs = GetLocalHostIPs();
            HttpListener listener = InitializeListener(localHostIPs);
            Start(listener);
        }
    }
}
