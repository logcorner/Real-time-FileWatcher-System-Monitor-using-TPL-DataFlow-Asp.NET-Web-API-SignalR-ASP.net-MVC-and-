using System;

namespace DataflowSignalrAngularExecute
{
    using DataflowFileService;

    internal class Program
    {
        private static readonly IOrchestratorService orchestratorService = new OrchestratorService();

        private const string PathString = @"D:\Samples\dumpdir";

        private static void Main()
        {
            orchestratorService.Execute();
            // Thread.Sleep(new TimeSpan(0, 0, 05));
            CreateFiles(PathString);
            Console.Read();
        }

        private static void CreateFiles(string pathString)
        {
            /*int i = 0;
            while (i < 20)
            {
                i++;
                string fileName = string.Format("{0}.csv", i);
                Thread.Sleep(10);
                var filePathString = System.IO.Path.Combine(pathString, fileName);
                System.IO.FileStream fs = System.IO.File.Create(filePathString);
            }*/
        }
    }
}
