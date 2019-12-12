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
        }

        private static void Menu()
        {
            Console.WriteLine("1 - Inserir destino para busca");
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
                catch (SocketException ex)
                {
                    Console.Clear();
                    Console.WriteLine("Erro: " + ex.Message);
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

        private static void EnviarRequisicao()
        {
            Menu();
            var operador = Console.ReadLine();
            DtoGrafo dados;
            switch (operador)
            {
                case "1":
                    Console.WriteLine("Digite o no de partida: ");
                    var noPartida = Console.ReadLine();
                    Console.WriteLine("Digite o no que deseja buscar: ");
                    var noDestino = Console.ReadLine();

                    dados = new DtoGrafo()
                    {
                        NoPartida = Int32.Parse(noPartida),
                        NoDestino = Int32.Parse(noDestino),
                        Operador = 1
                    };

                    EnviarDados(dados);
                    break;

                case "2":
                    Console.WriteLine("Solicitando ao servidor a busca pelo roteamento vizinho!");
                    dados = new DtoGrafo()
                    {
                        Operador = 2
                    };

                    EnviarDados(dados);
                    break;

                case "3":

                    dados = new DtoGrafo()
                    {
                        Operador = 3
                    };

                    EnviarDados(dados);
                    break;

                default:
                    Console.WriteLine("Requisição incorreta!");
                    Console.ReadLine();
                    Console.Clear();
                    Menu();
                    break;
            }
            //if (msg.ToLower() == "sair")
            //{
            //    Sair();
            //}
        }

        private static void EnviarTexto(string texto)
        {
            byte[] bufferTexto = Encoding.ASCII.GetBytes(texto);
            client.Send(bufferTexto);
        }

        private static void EnviarDados(DtoGrafo dados)
        {
            var formatter = new BinaryFormatter();
            var memory = new MemoryStream();

            formatter.Serialize(memory, dados);
            client.Send(memory.ToArray());
        }

        private static void ReceberRequisicao()
        {
            var buffer = new byte[2048];
            int bytesLidos = client.Receive(buffer, SocketFlags.None);
            if (bytesLidos == 0) return;
            var bytesRecebidos = new byte[bytesLidos];
            Array.Copy(buffer, bytesRecebidos, bytesLidos);


            var texto = Encoding.ASCII.GetString(bytesRecebidos);

        }
    }
}
