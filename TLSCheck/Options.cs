using System;
using System.Collections.Generic;
using System.Net;
using CommandLine;

namespace TLSCheck
{
    public class Options : MarshalByRefObject
    {
        [Option('s', "set", HelpText = "Set the ServicePointManager.SecurityProtocol to the specified values. (ex: Ssl3, Tls, Tls11, Tls12, SystemDefault)")]
        public IEnumerable<SecurityProtocolType> SecurityProtocols { get; set; }

        [Option('f', "frameworks", Default = new[] { "All" }, HelpText = "Set the Target Frameworks to test with. (ex: All, 4.0, 4.5, 4.6.1, 4.7)")]
        public IEnumerable<string> TargetFrameworks { get; set; }

        [Option('t', "test", Default = true, HelpText = "Verify TLS checks against ssllabs endpoints.")]
        public bool CheckTls { get; set; }

        [Option('u', "url", Default = "", HelpText = "A URL to check the SslProtocol with")]
        public string Url { get; set; }
    }
}
