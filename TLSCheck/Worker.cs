using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Console = Colorful.Console;

namespace TLSCheck
{
    public class Worker : MarshalByRefObject
    {
        public void Run(Options options)
        {
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.ServerCertificateValidationCallback = ServerCertificateValidationCallback;

            WriteVersionInfo();
            WriteSecurityProtocol();
            var protocols = options.SecurityProtocols?.ToList();
            if (protocols?.Any() == true)
            {
                var proto = options.SecurityProtocols.Aggregate((SecurityProtocolType) 0, (type, protocolType) => type | protocolType);
                Console.WriteLineFormatted("Setting protocol to: {0}", proto, Color.Goldenrod, Color.White);
                ServicePointManager.SecurityProtocol = proto;
                WriteSecurityProtocol();
            }

            if (options.CheckTls) WriteTlsSupport();

            if (!string.IsNullOrEmpty(options.Url)) CheckSslProtocol(options.Url);
        }

        private bool ServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }

        private static void WriteSecurityProtocol()
        {
            Console.WriteLineFormatted("SecurityProtocol: {0}", ServicePointManager.SecurityProtocol, Color.Goldenrod, Color.White);
        }

        private static void WriteTlsSupport()
        {
            var supportedProtocols = string.Join(", ", GetTlsSupport());
            Console.WriteLineFormatted("Supported Protocols: {0}", supportedProtocols, Color.Goldenrod, Color.White);
        }

        private static void WriteVersionInfo()
        {
            Console.WriteLineFormatted("Current .NET Target Framework: {0}", AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName, Color.Goldenrod, Color.White);
        }

        private static IEnumerable<string> GetTlsSupport()
        {
            var testServers = new Dictionary<string, string>
            {
                {"SSL 2", "https://www.ssllabs.com:10200"},
                {"SSL 3", "https://www.ssllabs.com:10300"},
                {"TLS 1.0", "https://www.ssllabs.com:10301"},
                {"TLS 1.1", "https://www.ssllabs.com:10302"},
                {"TLS 1.2", "https://www.ssllabs.com:10303"}
            };

            var client = new WebClient();
            bool IsSupported(string url)
            {
                try { return !string.IsNullOrEmpty(client.DownloadString(url)); }
                catch { return false; }
            }

            return testServers.Where(server => IsSupported(server.Value)).Select(x => x.Key);
        }

        private static void CheckSslProtocol(string url)
        {
            try
            {
                var uri = new Uri(url);
                using (var tcpClient = new TcpClient(uri.Host, uri.Port))
                {
                    var sslStream = new SslStream(tcpClient.GetStream(), false);
                    sslStream.AuthenticateAsClient(uri.Host, null, (SslProtocols) ServicePointManager.SecurityProtocol, false);
                    Console.WriteLineFormatted("Connection to {0} is using {1} protocol", url, sslStream.SslProtocol, Color.Goldenrod, Color.White);
                }
            }
            catch
            {
                Console.WriteLineFormatted("Error connecting to: {0}", url, Color.Red, Color.White);
            }
        }
    }
}
