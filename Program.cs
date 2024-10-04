using System;
using System.Collections.Generic;
using System.Threading;
using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.GenericMotorCLI.AdvancedMotor;
using Thorlabs.MotionControl.GenericMotorCLI.ControlParameters;
using Thorlabs.MotionControl.GenericMotorCLI.Settings;
using Thorlabs.MotionControl.TCube.DCServoCLI;

namespace TDC001
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // If you are using simulated devices
            SimulationManager.Instance.InitializeSimulations();

            try
            {
                // Bring devices connnected to the computer
                Console.WriteLine("Bringing devices connected to the computer....");
                DeviceManagerCLI.BuildDeviceList();     
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DeviceMangerCLi error: {ex}");
                Console.ReadKey();
                return;
            }

            // Get available Controller and Print it
            List<string> serialNumbers = DeviceManagerCLI.GetDeviceList(TCubeDCServo.DevicePrefix);
            if (serialNumbers.Count == 0)
            {
                Console.WriteLine("No device found");
                Console.ReadKey();
                return;
            }
            for (int i = 0; i < serialNumbers.Count; i++)
            {
                Console.WriteLine(i + ": " + serialNumbers[i]);
            }

            // Get serial no. from the user
            string selectedIndex, serialNumber;
            selectedIndex = Console.ReadLine();
            try
            {
                if (selectedIndex == null)
                {
                    Console.WriteLine("Need Input....");
                    Console.ReadKey();
                    return;
                }
                int selectedIntIndex = Int32.Parse(selectedIndex);
                serialNumber = serialNumbers[selectedIntIndex];
            }
            catch (Exception ex)
            {
                Console.WriteLine("Invalid Input....");
                Console.WriteLine($"Error message {ex}");
                Console.ReadKey();
                return;
            }

            // Create device And Open a connection to the device
            TCubeDCServo device = TCubeDCServo.CreateTCubeDCServo(serialNumber);
            if (device == null)
            {
                Console.WriteLine($"{serialNumber} not found.");
                Console.ReadKey();
                return;
            }
            try
            {
                Console.WriteLine($"Opening device {serialNumber}....");
                device.Connect(serialNumber);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open device {serialNumber}");
                Console.WriteLine($"Error message {ex}");
                Console.ReadKey();
                return;
            }

            // Initialize
            if (!device.IsSettingsInitialized())
            {
                try
                {
                    device.WaitForSettingsInitialized(5000);
                }
                catch (Exception)
                {
                    Console.WriteLine("Fail to init");
                    return;
                }
            }

            // Start the device polling
            device.StartPolling(250);
            Thread.Sleep(500);
            device.EnableDevice();
            Thread.Sleep(500);

            // Get configuration
            MotorConfiguration motorConfiguration = device.LoadMotorConfiguration(serialNumber);
            DCMotorSettings currentDeviceSetting = device.MotorDeviceSettings as DCMotorSettings;

            // Display info about device
            DeviceInfo deviceInfo = device.GetDeviceInfo();
            Console.WriteLine("====Device Info====");
            Console.WriteLine($"Serial Number: {deviceInfo.SerialNumber}");
            Console.WriteLine($"Name: {deviceInfo.Name}");

            // Homing device
            try
            {
                Console.WriteLine("Homing device");
                device.Home(60000);
                Console.WriteLine("Device Homed!");
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to home device");
                Console.ReadKey();
                return;
            }


            // Get User Params
            Console.Write("Velocity: ");
            int velocity = Convert.ToInt32(Console.ReadLine());
            Console.Write("From position: ");
            int fromPosition = Convert.ToInt32(Console.ReadLine());
            Console.Write("To position: ");
            int toPosition = Convert.ToInt32(Console.ReadLine());
            Console.Write("Iter: ");
            int iter = Convert.ToInt32(Console.ReadLine());


            // Setting velocity
            VelocityParameters velPars = device.GetVelocityParams();
            velPars.MaxVelocity = velocity;
            device.SetVelocityParams(velPars);
            Thread.Sleep(500);

            //Repeating Motion
            for (int i = 0; i < iter; i++)
            {
                device.MoveTo(toPosition, 60000);
                Thread.Sleep(500);
                device.MoveTo(fromPosition, 60000);
                Thread.Sleep(500);
            }


            // Stop Process
            device.StopPolling();
            device.Disconnect(true);

            SimulationManager.Instance.InitializeSimulations();
            Console.ReadKey();
        }
    }
}
