using Core;
using System;

namespace Client
{
    internal static class Program
    {
        static void Main()
        {
            var oracle = new Oracle();
            Console.WriteLine(oracle.GetAnswer());
        }
    }
}
