using System;
using System.Threading.Tasks;

namespace IgnoreError
{
    class Program
    {
        static void Main(string[] args)
        {
            Task t = Task.Factory.StartNew(() => { throw new Exception("Boom"); });
            t = null;
            var objects = new object[1000];
            int i = 0;
            while (true)
            {
                objects[i++] = new object();
                i = i % objects.Length;
            }
        }
    }
}
