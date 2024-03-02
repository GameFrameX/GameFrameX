using System.Collections.Concurrent;
using Server.NetWork.UDPSocket;

static class AppStartUpGate
{
    static void Main(string[] args)
    {
        // UDP server address
        string address = "127.0.0.1";
        if (args.Length > 0)
            address = args[0];

        // UDP server port
        int port = 3333;
        if (args.Length > 1)
            port = int.Parse(args[1]);

        Console.WriteLine($"UDP server address: {address}");
        Console.WriteLine($"UDP server port: {port}");

        Console.WriteLine();
        ConcurrentDictionary<string, EchoUdpClient> clients = new ConcurrentDictionary<string, EchoUdpClient>();

        for (int i = 0; i < 200; i++)
        {
            // Create a new TCP chat client
            var client = new EchoUdpClient(address, port);
            clients[client.Id] = client;
            // Connect the client
            Console.Write("Client connecting...");
            client.Connect();
            Console.WriteLine("Done!");

            Console.WriteLine("Press Enter to stop the client or '!' to reconnect the client...");

            // Perform text input

            string line = Random.Shared.NextInt64().ToString();
            if (string.IsNullOrEmpty(line))
            {
                break;
            }

            // Send the entered text to the chat server
            client.Send(line);
        }

        while (true)
        {
            Thread.Sleep(2000);
            string line = Random.Shared.NextInt64().ToString();
            if (line == "!")
            {
                Console.Write("Client disconnecting...");
                foreach (var udpClient in clients)
                {
                    udpClient.Value.Disconnect();
                }

                Console.WriteLine("Done!");
                continue;
            }

            // Send the entered text to the chat server
            foreach (var udpClient in clients)
            {
                udpClient.Value.Send(line);
            }
        }

        // Disconnect the client
        Console.Write("全部断开...");
        Console.WriteLine("Done!");
    }
}