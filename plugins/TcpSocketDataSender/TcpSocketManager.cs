﻿using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace TcpSocketDataSender
{
    public class TcpSocketManager : IDisposable
    {
        private TcpClient _tcpClient;
        BinaryWriter _writer = null;

        public int ServerPort = 7839;
        public string ServerIp = "127.0.0.1";
        public bool AutoReconnect = false;
        public async Task<bool> Connect()
        {
            if (_writer != null)
                return true;
            _tcpClient = new TcpClient();
            try
            {
                await _tcpClient.ConnectAsync(IPAddress.Parse(ServerIp), ServerPort);
                _writer = new BinaryWriter(_tcpClient.GetStream());
            }
            catch (SocketException)
            {
                //No server avaliable, or it is busy/full.
                return false;
            }
            return true;
        }

        public async Task Write(string data)
        {
            bool written = false;
            try
            {
                if (_tcpClient?.Connected ?? false)
                {
                    _writer?.Write(data);
                    written = true;
                }
            }
            catch (IOException)
            {
                //connection most likely closed
                _writer?.Dispose();
                _writer = null;
                ((IDisposable)_tcpClient)?.Dispose();
            }
            if (!written && AutoReconnect)
            {
                if(await Connect())
                    await Write(data);
            }
        }

        public void Dispose()
        {
            ((IDisposable)_tcpClient)?.Dispose();
            _writer?.Dispose();
        }
    }
}