using System.Configuration;
using System.Runtime.CompilerServices;
using System.Diagnostics.PerformanceData;
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
        private Pos pos;
        static void Main(string[] args)
        {
            Connect connect = new Connect();
            connect.scanner = new NetworkScanner();
            ControllerInfo controller = null;

            ControllerInfoCollection controllers = connect.scanner.Controllers;
            connect.networkWatcher = new NetworkWatcher(controllers);
            connect.networkWatcher.Found += FoundController;

            //looking for controllers
            connect.scanner.Scan();
            
            if(connect.scanner.Controllers.Count > 0)
            {
                controller = connect.scanner.Controllers[0];

                //controller = connect.scanner.Controllers.Where(controllerInfo => controllerInfo.ControllerName == "Kitt").ToList()[0];
                Console.WriteLine("Connecting to: "+ controller.IPAddress.ToString() + " " + controller.Name.ToString());
            }


            //Logging onto controller
            connect.controller = Controller.Connect(controller, ConnectionType.Standalone);
            connect.controller.Logon(UserInfo.DefaultUser);
            Console.WriteLine("Connected");

            Start(connect);


        }
        public static void FoundController(object obj, NetworkWatcherEventArgs arg)
        {
            ControllerInfo controller = arg.Controller;
            Console.WriteLine(controller.IPAddress.ToString());
            Console.ReadLine();
        }
        public static void Start(Connect connect)
        {
            connect.pos.X = 300;
            connect.pos.Y = 300;
            connect.pos.Z = 100;
            
            try
            {
                if (connect.controller.OperatingMode == ControllerOperatingMode.Auto)
                {
                    //get the listed tasks
                    connect.tasks = connect.controller.Rapid.GetTasks();

                    //how to get data from controller
                    Module ControllerAccess = connect.controller.Rapid.GetTask("T_ROB1").GetModule("Module1");
                    Routine ourRoutine = ControllerAccess.GetRoutine("main");

                    //reading data
                    Bool rapidData = (Bool)ControllerAccess.GetRapidData("switch").Value;

                    Console.WriteLine(rapidData.ToString());

                    //Changing data on controller
                    using (Mastership m = Mastership.Request(connect.controller))
                    {

                        ControllerAccess.GetRapidData("switch").Value = new Bool(true);

                        Console.WriteLine((ControllerAccess.GetRapidData("switch").Value).ToString());
                        ControllerAccess.GetRapidData("startpose").Value = connect.pos;
                        //Perform operation

                        connect.tasks[0].SetProgramPointer("Module1", "main");
                        connect.controller.Rapid.Start(RegainMode.Continue, ExecutionMode.Continuous, ExecutionCycle.AsIs, StartCheck.None, true, TaskPanelExecutionMode.NormalTasks);
                        Console.ReadLine();
                    }

                    //how to get current position
                    JointTarget currentPose = connect.controller.MotionSystem.ActiveMechanicalUnit.GetPosition();
                    Console.WriteLine(currentPose.ToString());
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