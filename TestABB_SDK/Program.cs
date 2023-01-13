using System.Configuration;
using System.Runtime.CompilerServices;
using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.Discovery;
using ABB.Robotics.Controllers.IOSystemDomain;
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

            ControllerInfoCollection controllers = connect.scanner.Controllers;
            connect.networkWatcher = new NetworkWatcher(controllers);
            connect.networkWatcher.Found += FoundController;
            connect.scanner.Scan();
            
            ControllerInfo controller = null;
            if(connect.scanner.Controllers.Count > 0)
            {
                
              controller= connect.scanner.Controllers.Where(controllerInfo => controllerInfo.ControllerName == "Kitt").ToList()[0];
            }

            //while(connect.scanner.Controllers.Count <= 0) 
            //{
            //    Console.Write("Searching");
            //    connect.scanner.Scan();
            //    Console.Write(".");
            //    Thread.Sleep(1000);
            //}
            //ControllerInfo controller = controllers[0];


            Console.WriteLine("Press any key to connect to: "+ controller.IPAddress.ToString() + " " + controller.Name.ToString());
            Console.ReadLine();


            //Logging onto controller
            //connect.controller = ControllerFactory.CreateFrom(controller);
            connect.controller = Controller.Connect(controller, ConnectionType.Standalone);
            connect.controller.Logon(UserInfo.DefaultUser);
            Console.WriteLine("Connected");

            //Start(connect);


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
                    double counterValue = ((Num)connect.controller.Rapid.GetTask("T_ROB1").GetModule("MainMod").GetRapidData("counter").Value).Value;
                    using (Mastership m = Mastership.Request(connect.controller))
                    {
                        connect.controller.Rapid.GetTask("T_ROB1").GetModule("MainMod").GetRapidData("counter").Value = new Num(5);
                        //Perform operation
                        connect.tasks[0].Start();
                    }
                    connect.controller.MotionSystem.ActiveMechanicalUnit.GetPosition()
                    DigitalSignal digitalSignal = ((DigitalSignal)connect.controller.IOSystem.GetSignal("DoorOpen"));
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