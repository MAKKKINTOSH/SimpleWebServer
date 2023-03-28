﻿using System.Net;
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
                listener.Prefixes.Add($"http://{ip.ToString()}:8888/");
                Console.WriteLine("IP: " + $"http://{ip.ToString()}:8888/");
            });
            return listener;
        }

        public static int maxSimultaneousConnections = 20;
        private static Semaphore sem = new Semaphore(maxSimultaneousConnections, maxSimultaneousConnections);

        private static void Start(HttpListener listener)
        {
            listener.Start();
            Task.Run(RunServer(listener));
        }
        /// <summary>
        /// Start awaiting for connections, up to the max value.
        /// Code runs in a separate thread.
        /// </summary>
        /// <param name="listener"></param>
        private static Action RunServer(HttpListener listener) 
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

            Console.WriteLine("\n\n\n" + context.Request.Headers + "\n\n\n");
            string response = "<html><head><meta http-equiv='content-type' content='text/html; charset=utf-8'/></ head >XD</ html > ";
            byte[] encoded = Encoding.UTF8.GetBytes(response);
            context.Response.ContentLength64 = encoded.Length;
            context.Response.OutputStream.Write(encoded, 0, encoded.Length);
            context.Response.OutputStream.Close();
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
