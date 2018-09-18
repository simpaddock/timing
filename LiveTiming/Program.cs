
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveTiming;
using Nancy.Hosting.Self;

namespace LiveTiming
{
    class Program
    {
        static void Main(string[] args)
        {
            Timing timing = new Timing();

            using (var host = new NancyHost(new Uri("http://localhost:8080")))
            {
                host.Start();
                Console.ReadLine();
            }

        }
    }
}
