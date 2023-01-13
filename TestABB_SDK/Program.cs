using System.Configuration;
using System.Runtime.CompilerServices;
using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.Discovery;
using ABB.Robotics.Controllers.RapidDomain;
using Task = ABB.Robotics.Controllers.RapidDomain.Task;

namespace Connection
{
    class Connect
    {
        private NetworkScanner scanner = null;
        private Controller controller = null;
        private Task[] tasks = null;
        private NetworkWatcher networkWatcher = null;
        static void Main(string[] args)
        {
            Connect connect = new Connect();
            connect.scanner = new NetworkScanner();
            connect.scanner.Scan();

            ControllerInfoCollection controllers = connect.scanner.Controllers;
            connect.networkWatcher = new NetworkWatcher(controllers);

            connect.networkWatcher.Found += FoundController;
            Console.Write("Searching");

            while(connect.scanner.Controllers.Count <= 0) 
            {
                connect.scanner.Scan();
                Console.Write(".");
                Thread.Sleep(1000);
            }
            //ControllerInfo controller = controllers[0];


            //Console.WriteLine("Press any key to connect to: "+ controller.IPAddress.ToString());
            Console.ReadLine();


            //Logging onto controller
            //connect.controller = ControllerFactory.CreateFrom(controller);
            connect.controller.Logon(UserInfo.DefaultUser);
            Console.WriteLine("Connected");

            Start(connect);


        }
        public static void FoundController(object obj, NetworkWatcherEventArgs arg)
        {
                ControllerInfo controller = arg.Controller;
                Console.WriteLine(controller.IPAddress.ToString());
            Console.WriteLine("Found you damn it");
            Console.ReadLine();
        }
        public static void Start(Connect connect)
        {
            try
            {
                if (connect.controller.OperatingMode == ControllerOperatingMode.Auto)
                {
                    connect.tasks = connect.controller.Rapid.GetTasks();
                    using (Mastership m = Mastership.Request(connect.controller.Rapid))
                    {
                        //Perform operation
                        connect.tasks[0].Start();
                    }
                }
                else
                {
                   Console.WriteLine("Automatic mode is required to start execution from a remote client.");
                }
            }
            catch (System.InvalidOperationException ex)
            {
               Console.WriteLine("Mastership is held by another client." + ex.Message);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Unexpected error occurred: " + ex.Message);
            }
        }
    }

    
}