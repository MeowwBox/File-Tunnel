﻿using ft.Commands;
using ft.Listeners;
using ft.Streams;
using ft.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ft.Tunnels
{
    public class RemoteToLocalTunnel
    {
        public RemoteToLocalTunnel(List<string> remoteTcpForwards, SharedFileManager sharedFileManager, LocalToRemoteTunnel localToRemoteTunnel, long writeFileSize, int readDurationMillis, string udpSendFrom)
        {
            RemoteTcpForwards = remoteTcpForwards;
            SharedFileManager = sharedFileManager;

            sharedFileManager.OnlineStatusChanged += (sender, args) =>
            {
                if (args.IsOnline)
                {
                    ProvideRemoteForwardsToCounterpart();
                }
                else
                {
                    sharedFileManager.TearDownAllConnections();
                }
            };

            sharedFileManager.SessionChanged += (sender, args) =>
            {
                ProvideRemoteForwardsToCounterpart();
            };

            sharedFileManager.ConnectionAccepted += (sender, connectionDetails) =>
            {
                var connectToTokens = connectionDetails.DestinationEndpointString.Split(["://"], StringSplitOptions.None);
                var protocol = connectToTokens[0];
                var destinationEndpointStr = connectToTokens[1];

                var destinationEndpoint = destinationEndpointStr.AsEndpoint();

                if (protocol.Equals("tcp"))
                {
                    try
                    {
                        var tcpClient = new TcpClient();
                        tcpClient.Connect(destinationEndpoint);

                        if (tcpClient.Connected)
                        {
                            Program.Log($"Connected to {destinationEndpointStr}");

                            var relay1 = new Relay(tcpClient.GetStream(), connectionDetails.Stream, writeFileSize, readDurationMillis);
                            var relay2 = new Relay(connectionDetails.Stream, tcpClient.GetStream(), writeFileSize, readDurationMillis);

                            void TearDown()
                            {
                                relay1.Stop();
                                relay2.Stop();
                            }

                            relay1.RelayFinished += (s, a) => TearDown();
                            relay2.RelayFinished += (s, a) => TearDown();
                        }
                        else
                        {
                            Program.Log($"Could not connect to: {destinationEndpointStr}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Program.Log($"Error during connection to {destinationEndpointStr}. {ex.Message}");
                    }
                }

                if (protocol.Equals("udp"))
                {
                    var sendFromEndpoint = udpSendFrom.AsEndpoint();

                    var udpClient = new UdpClient();
                    udpClient.Client.Bind(sendFromEndpoint);

                    var udpStream = new UdpStream(udpClient, destinationEndpoint);

                    var relay1 = new Relay(udpStream, connectionDetails.Stream, writeFileSize, readDurationMillis);
                    var relay2 = new Relay(connectionDetails.Stream, udpStream, writeFileSize, readDurationMillis);

                    void TearDown()
                    {
                        relay1.Stop();
                        relay2.Stop();
                    }

                    relay1.RelayFinished += (s, a) => TearDown();
                    relay2.RelayFinished += (s, a) => TearDown();
                }
            };

            sharedFileManager.CreateLocalListenerRequested += (sender, createLocalListenerArgs) =>
            {
                localToRemoteTunnel.LocalListeners.Add(createLocalListenerArgs.Protocol, createLocalListenerArgs.ForwardString, true);
            };
        }

        public List<string> RemoteTcpForwards { get; }
        public SharedFileManager SharedFileManager { get; }

        void ProvideRemoteForwardsToCounterpart()
        {
            //Tell the remote side to start the Remote Forwards
            RemoteTcpForwards
                 .Select(remoteForwardStr => new CreateListener("tcp", remoteForwardStr))
                 .ToList()
                 .ForEach(remoteForwardCommand => SharedFileManager.EnqueueToSend(remoteForwardCommand));
        }
    }
}