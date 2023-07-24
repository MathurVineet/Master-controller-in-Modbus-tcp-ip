using System;
using EasyModbus;

namespace DotNetEvents
{
    // Define a class to hold custom event info
    public class CustomEventArgs : EventArgs
    {
        public CustomEventArgs(string message, int readCoilsAddress, int readCoilsQuantity, ModbusClient modbusclient)
        {
            Message = message;
            ReadCoilsAddress = readCoilsAddress;
            ReadCoilsQuantity = readCoilsQuantity;
            modbusClient = modbusclient;
        }

        public string Message { get; set; }
        public int ReadCoilsAddress { get; set; }
        public int ReadCoilsQuantity { get; set; }
        public ModbusClient modbusClient { get; set; }
    }

    // Class that publishes an event
    class Publisher
    {
        // Declare the event using EventHandler<T>
        public event EventHandler<CustomEventArgs> RaiseCustomEvent;

        public void DoSomething()
        {
            // Write some code that does something useful here
            Console.WriteLine("Enter ip address of slave");
            string slaveIpAddress = Console.ReadLine();

            ModbusClient modbusClient = new ModbusClient(slaveIpAddress.ToString(), 502);    //Ip-Address and Port of Modbus-TCP-Server
            modbusClient.Connect();

            Console.WriteLine("Enter address of first coil");
            int ReadCoilsAddress = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Enter quantity of coils to read");
            int ReadCoilsQuantity = Convert.ToInt32(Console.ReadLine());


            // then raise the event. You can also raise an event
            // before you execute a block of code.
            OnRaiseCustomEvent(new CustomEventArgs("Event triggered", ReadCoilsAddress, ReadCoilsQuantity, modbusClient));
        }

        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void OnRaiseCustomEvent(CustomEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<CustomEventArgs> raiseEvent = RaiseCustomEvent;

            // Event will be null if there are no subscribers
            if (raiseEvent != null)
            {
                // Format the string to send inside the CustomEventArgs parameter
                e.Message += $" at {DateTime.Now}";

                // Call to raise the event.
                raiseEvent(this, e);
            }
        }
    }

    //Class that subscribes to an event
    class Subscriber
    {
        private readonly string _id;

        public Subscriber(string id, Publisher pub)
        {
            _id = id;

            // Subscribe to the event
            pub.RaiseCustomEvent += HandleCustomEvent;
        }

        // Define what actions to take when the event is raised.
        void HandleCustomEvent(object sender, CustomEventArgs e)
        {
            //Console.Clear();

            Console.WriteLine($"{_id} received this message: {e.Message}");
            
            bool[] readCoils = e.modbusClient.ReadCoils(e.ReadCoilsAddress, e.ReadCoilsQuantity);//Read 10 Coils from Server, starting with address 10

            for (int i = 0; i < readCoils.Length; i++)
                Console.WriteLine("Value of Coil " + (e.ReadCoilsAddress + i) + " " + readCoils[i].ToString());

        }
    }

    class Program
    {
        static void Main()
        {
            var pub = new Publisher();
            var sub1 = new Subscriber("sub1", pub);
            //var sub2 = new Subscriber("sub2", pub);

            // Call the method that raises the event.
            pub.DoSomething();

            // Keep the console window open
            Console.WriteLine("Press any key to continue...");
            Console.ReadLine();
        }
    }
}