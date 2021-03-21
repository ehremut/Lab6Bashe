using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
//using Blog;
using System.IO;
using System.Net;
using System.Net.Sockets;
//using Microsoft.EntityFrameworkCore.Storage;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;


namespace Server
{
    class Program
    {
        private static TcpListener listener;
        private const int serverPort = 2365;
        private static bool run;
        private static int step;
        private static int gameCount;
        private static bool isGaming = true;
        private static TcpClient client;
        private static NetworkStream stream;
        private static StreamReader reader;
        private static StreamWriter writer;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Server start");
            listener = new TcpListener(IPAddress.Any, serverPort);
            run = true;
            await Listen();
        }

        private static async Task Listen()
        {
            listener.Start();
            client = await listener.AcceptTcpClientAsync();
            stream = client.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
            do
            {
                string sendMessage = await reader.ReadLineAsync();
                if (sendMessage != null)
                {
                    string[] listMessage = sendMessage.Split(" ");
                    if (int.TryParse(listMessage[0], out gameCount) && int.TryParse(listMessage[1], out step))
                    {
                        break;
                    }
                }
            } while (gameCount <= step);
            await Game();
        }

        private static async Task Game()
        {
            while (isGaming)
            {
                if (gameCount > 0)
                {
                    var serverRemoveCount = CheckStatus();
                    gameCount -= serverRemoveCount;
                    if (gameCount <= 0)
                    {
                        writer = new StreamWriter(stream);
                        await writer.WriteLineAsync("Server is win!");
                        await writer.FlushAsync();
                        isGaming = false;
                    }
                    var str = "Server choice is " + serverRemoveCount + " Left count " + gameCount; 
                    await writer.WriteLineAsync(str);
                    await writer.FlushAsync();
                    var clientResponce = await reader.ReadLineAsync();
                    int clientStep;
                    if (int.TryParse(clientResponce, out clientStep))
                    {
                        gameCount -= clientStep;
                        if (gameCount <= 0)
                        {
                            writer = new StreamWriter(stream);
                            await writer.WriteLineAsync("Client is win!");
                            await writer.FlushAsync();
                            isGaming = false;
                        }
                    }
                }
            }
            client.Close();
        }
        private static int CheckStatus()
        {
            var checkStep = step + 1;
            Random rnd = new Random();
            var howTake = 0;
            var checkLeft = gameCount / checkStep;
            var needForWin = gameCount - checkLeft * checkStep;

            if (needForWin > 0 && needForWin <= step)
            {
                howTake = (int) needForWin;
            }
            else
            {
                howTake = rnd.Next(1, (int) step);
            }
            return howTake;
        }
    }
}