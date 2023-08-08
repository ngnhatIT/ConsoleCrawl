using System;
using Infrastructure;

namespace projects
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }

        private static void checkServer()
        {
            Console.WriteLine("Start");
            using (var context = new ConsoleContext())
            {
                
            }
        }
    }
}
