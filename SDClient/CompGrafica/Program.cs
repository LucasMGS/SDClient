using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using CompGrafica;

namespace SDClient
{
    class Program
    {
        private static readonly Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static int Porta = 3333;

        static void Main()
        {
            Console.Title = "Cliente";
            ConectarAoServidor();
            RequisicaoLoop();

            byte[] Buffer = new byte[client.SendBufferSize];
            int BytesLidos= default;
            Menu();
            var operador = string.Empty;
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream memory = new MemoryStream();
           

            while (operador != "3")
            {
                operador = Console.ReadLine();
                switch (operador)
                {
                    case "1":
                        caso1(BytesLidos: BytesLidos,
                              client:client,
                              Buffer: Buffer,
                              formatter,
                              memory);
                        break;

                    case "2":
                        Console.WriteLine("Consultando roteamento...");

                        var bf = new BinaryFormatter();
                        var ms = new MemoryStream();

                        DtoInformacao dtoInfo = new DtoInformacao()
                        {
                            Operador = 2
                        };

                        bf.Serialize(ms,dtoInfo);
                        client.Send(ms.ToArray());

                        Console.ReadKey();
                        Console.Clear();
                        Menu();
                        break;
                    case "3":
                        Console.WriteLine("Conexão desligada!");
                        client.Close();
                        break;
                }
                
            }
        
            
        }

        private static void caso1(int BytesLidos,Socket client,byte[] Buffer,BinaryFormatter formatter,MemoryStream memory)
        {
            BytesLidos = client.Receive(Buffer);
            byte[] dadosRecebidos = new byte[BytesLidos];
            Array.Copy(Buffer, dadosRecebidos, BytesLidos);
            
            var info = new DtoInformacao();

            Console.WriteLine("Digite o nó destino: ");
            var no = Console.ReadLine();

            DtoInformacao NoEnviar = new DtoInformacao()
            {
                No = no,
                Operador = 1
            };

            formatter.Serialize(memory, NoEnviar);
            client.Send(memory.ToArray());
        }

        private static void Menu()
        {
            Console.WriteLine("1 - Inserir destino");
            Console.WriteLine("2 - Pedir roteamento");
            Console.WriteLine("3 - Fechar conexão");
            Console.Write("Digite qual operação deseja usar: ");
        }

        private static void ConectarAoServidor()
        {
            int tentativas = 0;
            while (!client.Connected)
            {
                try
                {
                    tentativas++;
                    Console.WriteLine("Tentativas: {0}", tentativas);
                    client.Connect(IPAddress.Loopback, Porta);
                }
                catch (SocketException)
                {
                    Console.Clear();
                }
            }
            Console.Clear();
            Console.WriteLine("Conectado!");
        }

        private static void RequisicaoLoop()
        {
            Console.WriteLine("Digite 'sair' para fechar!");
            while (true)
            {
                EnviarRequisicao();
                ReceberRequisicao();
            }
        }

        private static void Sair()
        {
            EnviarTexto("Saindo!");
            client.Shutdown(SocketShutdown.Both);
            client.Close();
            Environment.Exit(0);
        }

        private static void EnviarRequisicao(int operador)
        {
            Console.WriteLine("Digite o no que deseja alcançar: ");
            var no = Console.ReadLine();
            var dados = new DtoInformacao()
            {
                No = no,
                Operador = operador
            };

            EnviarDados(dados);

            //if (msg.ToLower() == "sair")
            //{
            //    Sair();
            //}
        }

        private static void EnviarDados(DtoInformacao dados)
        {
            var formatter = new BinaryFormatter();
            var memory = new MemoryStream();

            formatter.Serialize(memory, dados);
            //byte[] buffer = Encoding.ASCII.GetBytes(memory.ToArray());
            client.Send(memory.ToArray());
        }

        private static void ReceberRequisicao()
        {
            var buffer = new byte[2048];
            int bytesLidos = client.Receive(buffer,SocketFlags.None);
            if (bytesLidos == 0) return;
            var bytesRecebidos = new byte[bytesLidos];
            Array.Copy(buffer,bytesRecebidos,bytesLidos);


            var texto = Encoding.ASCII.GetString(bytesRecebidos);
            
        }
    }
}
