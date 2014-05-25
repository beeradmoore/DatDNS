using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DatDNS
{
    public static class DNSUtils
    {
        [DllImport("dnsapi.dll", EntryPoint = "DnsFlushResolverCache")]
        private static extern UInt32 DnsFlushResolverCache();
        public static UInt32 FlushDNSCache()
        {
            return DnsFlushResolverCache();
        }
    }
}
