using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PwnieProxy
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var proxy = new PwnieProxy();
            var proxyTasks = new List<Task>();
            for (ushort i = 3000; i < 3006; i++)
            {
                proxyTasks.Add(proxy.Start("78.47.102.132", i, i));
            }
            await Task.WhenAll(proxyTasks);
        }
    }
}