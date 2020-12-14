using Mono.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using DSController.Serial;
using DSController.UI;

namespace DSController.App
{
    class Application
    {
        protected Action Action = Action.Start; // Default action is to start the app
        protected string ConfigPath;
        protected Config Configuration;
        protected KeyboardWindow Window;
        protected SerialConnector SerialConnector;

        public Application(string[] args)
        {
            OptionSet options = new OptionSet {
                {"c|config=", "The configuration file to use", c => this.ConfigPath = c},
                {"l|list", "List the available ports", a => this.Action = Action.List},
                {"s|generate-skeleton", "Generates a skeleton of the configuration file to the standard output", a => this.Action = Action.GenerateSkeleton},
                {"p|probe", "Probes the controller's buttons to get the controller code for this specific installation", a => this.Action = Action.Probe}
            };

            options.Parse(args);
        }

        public bool Init()
        {
            this.SerialConnector = new SerialConnector();
            bool connectionOpened;

            switch(this.Action) {
                case Action.Start:
                    this.Window = new KeyboardWindow();
                    this.Window.Init();
                    this.Window.OnKeyAction += this.OnKeyboardAction;

                    this.Configuration = Config.LoadFromFile(this.ConfigPath);
                    if(!this.Configuration.IsValid()) {
                        return this.DisplayError("Configuration is invalid.");
                    }

                    this.Window.ShowStatus("Configuration loaded, connecting...");
                    connectionOpened = this.SerialConnector.Open(this.Configuration.Device, this.Configuration.ControllerCode);

                    if(!connectionOpened) {
                        return this.DisplayError("Could not open serial connection: \n" + this.SerialConnector.LastError);
                    }

                    this.Window.ShowStatus("Connected.", 0x00AA00FF);
                    break;
                
                case Action.List:
                    var availablePorts = this.SerialConnector.GetAvailablePorts();

                    if(availablePorts.Count > 0) {
                        foreach(string port in availablePorts) {
                            Console.WriteLine(port);
                        }
                    } else {
                        Console.WriteLine("No ports available.");
                    }
                    break;

                case Action.GenerateSkeleton:
                    Console.WriteLine(Config.GenerateSkeleton());
                    break;
                
                case Action.Probe:
                    StringBuilder controllerCodeBuilder;
                    int portNumber;
                    Dictionary<string,int> buttons = new Dictionary<string, int>();
                    
                    Console.WriteLine("Calibration mode:");
                    Console.WriteLine("Loading configuration...");


                    this.Configuration = Config.LoadFromFile(this.ConfigPath);
                    if(!this.Configuration.IsValid()) {
                        return this.DisplayError("Configuration is invalid.");
                    }
                    
                    Console.WriteLine("Connecting to controller...");

                    connectionOpened = this.SerialConnector.Open(this.Configuration.Device);
                    if(!connectionOpened) {
                        Console.WriteLine("Cannot connect to the controller. Check connection and/or base configuration.");
                        return false;
                    }

                    Console.WriteLine("Connected.");
                    
                    Thread.Sleep(1); // Wait for 1 ms to avoid overloading the microcontroller with commands

                    Console.WriteLine("Setting controller in probe mode...");
                    this.SerialConnector.ChangeMode(SerialConnector.MODE_PROBE);
                    Console.WriteLine("Ready.");
                    Console.WriteLine("");

                    Thread.Sleep(10);
                    this.SerialConnector.Clean();

                    // Get button mapping
                    foreach(string name in Enum.GetNames(typeof(NDSButton))) {
                        Console.WriteLine("Press button " + name + "... ");
                        do {
                            portNumber = this.SerialConnector.WaitByte() - 48;
                        } while(buttons.ContainsValue(portNumber));

                        buttons.Add(name, portNumber);
                    }

                    controllerCodeBuilder = new StringBuilder();

                    foreach(KeyValuePair<string, int> kv in buttons) {
                        controllerCodeBuilder.Append(Convert.ToChar(kv.Value + 63));
                    }

                    Console.WriteLine("Your controller code is: " + controllerCodeBuilder.ToString());
                    break;
            }

            return true;
        }

        public void Run()
        {
            if(this.Action == Action.Start) {
                this.Window.Loop();
            }
        }
  
        public bool DisplayError(string errorMessage)
        {
            this.Window.ShowStatus(errorMessage, 0xAA0000FF);
            this.Window.Loop();

            return false;
        }

        public void OnKeyboardAction(object sender, KeyActionEventArgs ev)
        {
            // Finding if the key pressed is in the mapping
            foreach(KeyValuePair<string,string> kv in this.Configuration.ButtonMapping) {
                if(ev.Key == kv.Value) {
                    this.SerialConnector.SendKey(kv.Key, ev.Pressed);
                }
            }
        }
    }
}