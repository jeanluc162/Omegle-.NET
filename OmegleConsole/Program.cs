using System;
using System.Threading.Tasks;
using OmegleLibrary;

namespace OmegleConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Client omegleclient = new Client();
            await omegleclient.StartNormalAsync();
        }
    }
}
