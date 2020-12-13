using System;
using DSController.App;

namespace DSController
{
    class Program
    {
        static void Main(string[] args)
        {
            Application app = new Application(args);
            
            app.Init();
            app.Run();
        }
    }
}
