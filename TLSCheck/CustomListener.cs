using System.Diagnostics;
using System.Net;

namespace TLSCheck
{
    public class CustomListener : ConsoleTraceListener
    {
        public override void Write(string message)
        {
            if (message == null || !message.Contains("ProcessAuthentication")) return;
            base.Write(message);
        }

        public override void WriteLine(string message)
        {
            if (message == null || !message.Contains("ProcessAuthentication")) return;
            base.WriteLine(message);
        }
    }
}