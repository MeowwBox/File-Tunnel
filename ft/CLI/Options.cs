﻿using CommandLine.Text;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ft.CLI
{
    public class Options
    {
        [Option('L', Required = false, HelpText = @"TCP forwarding. Syntax: [bind_address:]port:host:hostport. Specifies that the given port on the local host is to be forwarded to the given host and port on the remote side. Use forward slashes as separators when using IPV6.")]
        public IEnumerable<string> LocalTcpForwards { get; set; } = new List<string>();

        [Option('U', Required = false, HelpText = @"UDP forwarding. Syntax: [bind_address:]port:host:hostport. Specifies that the given port on the local host is to be forwarded to the given host and port on the remote side. Use forward slashes as separators when using IPV6.")]
        public IEnumerable<string> LocalUdpForwards { get; set; } = new List<string>();



        [Option('R', Required = false, HelpText = @"Remote TCP forwarding. Syntax: [bind_address:]port:host:hostport. Specifies that the given port on the remote host is to be forwarded to the given host and port on the local side. Use forward slashes as separators when using IPV6.")]
        public IEnumerable<string> RemoteTcpForwards { get; set; } = new List<string>();



        [Option("read-duration", Required = false, HelpText = @"The duration (in milliseconds) to read data from a TCP connection. Larger values increase throughput (by reducing the number of small writes to file), whereas smaller values improve responsiveness.")]
        public int ReadDurationMillis { get; set; } = 50;

        [Option("udp-send-from", Required = false, HelpText = "A local address which UDP data will be sent from. Example --udp-send-from 192.168.1.1:11000")]
        public string UdpSendFrom { get; set; } = "0.0.0.0";



        [Option('w', "write", Required = true, HelpText = @"Where to write data to. Example: --write ""\\nas\share\1.dat""")]
        public string WriteTo { get; set; } = "";

        [Option('r', "read", Required = true, HelpText = @"Where to read data from. Example: --read ""\\nas\share\2.dat""")]
        public string ReadFrom { get; set; } = "";



        [Option("write-file-size", Required = false, HelpText = @"The size (in bytes) the write file will be initialized to if it doesn't already exist.")]
        public long WriteFileSize { get; set; } = 10 * 1024 * 1024;

        [Option("tunnel-timeout", Required = false, HelpText = @"The duration (in milliseconds) to wait for responses from the counterpart. If this timeout is reached, the tunnel is considered offline and TCP connections will be closed at this point.")]
        public int TunnelTimeoutMilliseconds { get; set; } = 5000;
    }
}
