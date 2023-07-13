using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace UDPTrafficGenerator
{
    public class Program
    {
        private static int packetSize = 1024; // Размер пакета в байтах
        private static int duration = 10; // Длительность генерации трафика в секундах

        private static long sentPackets = 0;
        private static long receivedPackets = 0;

        public static void Main(string[] args)
        {
            Console.WriteLine("Введите MAC-адрес назначения (формат: FF:FF:FF:FF:FF:FF):");
            var destinationMacAddress = PhysicalAddress.Parse(Console.ReadLine());

            Console.WriteLine("Введите IP-адрес в виде строки:");
            var ipAddressString = Console.ReadLine();
            var ipAddress = IPAddress.Parse(ipAddressString);

            var client = new UdpClient();

            try
            {
                client.Connect(new IPEndPoint(ipAddress, 0));

                // Запускаем отдельный поток для приёма пакетов
                var receiveThread = new System.Threading.Thread(() =>
                {
                    try
                    {
                        while (true)
                        {
                            var data = client;
                            receivedPackets++;
                        }
                    }
                    catch (SocketException e)
                    {
                        // Обработка исключения при закрытии сокета
                    }
                });
                receiveThread.Start();

                // Отправляем пакеты заданной загрузки канала
                SendUDPTraffic(client, destinationMacAddress);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                client.Close();
            }

            Console.WriteLine("Пакетов отправлено: " + sentPackets);
            Console.WriteLine("Пакетов принято: " + receivedPackets);
            Console.WriteLine("Пакетов потеряно: " + (sentPackets - receivedPackets));
        }

        private static void SendUDPTraffic(UdpClient client, PhysicalAddress destinationMacAddress)
        {
            var random = new Random();
            var packetData = new byte[packetSize];

            var startTime = DateTime.Now;

            // Пока не прошло нужное количество времени, генерируем и отправляем пакеты
            while ((DateTime.Now - startTime).TotalSeconds < duration)
            {
                random.NextBytes(packetData);

                // Задаем MAC-адрес в заголовке Ethernet-фрейма
                Array.Copy(destinationMacAddress.GetAddressBytes(), 0, packetData, 0, 6);

                // Отправляем пакет через UDP сокет
                client.Send(packetData, packetData.Length);
                sentPackets++;
            }
        }
    }
}