using System;

namespace UNIConsole
{
    internal abstract class Program
    {
        private static void Main(string[] args)
        {
            if(args.Length <= 7)
            {
                return;
            }

            try
            {
                // 启动服务
                var server = new ServiceServer(args[0], int.Parse(args[1]), args[2], int.Parse(args[3]), args[4], args[5], int.Parse(args[6]), int.Parse(args[7]));
            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
