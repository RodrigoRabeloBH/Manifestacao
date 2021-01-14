using System;
using System.Threading;
using System.Threading.Tasks;
using Service;

namespace App
{
    class Program
    {
        private static readonly ManifestacaoAutomaticaServices _service = new ManifestacaoAutomaticaServices();
        static async Task Main()
        {
            while (true)
            {
                Console.WriteLine("\t\t Iniciando Manifestações \n\n");

                await _service.ManifestaNotas();

                Thread.Sleep(2 * 60 * 1000);

                Console.WriteLine("\t\t Termino das Manifestações \n\n");
            }
        }
    }
}
