using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MobileDevice;
using MobileDevice.Event;

namespace Test_ios
{
    class Program
    {
        static void Main(string[] args)
        {
            private iOSDeviceManager manager = new iOSDeviceManager();
        private iOSDevice currentiOSDevice;

        private void start() {
            Console.WriteLine("State: Waiting for device connection");
            manager.CommonConnectEvent += CommonConnectDevice;
            manager.RecoveryConnectEvent += RecoveryConnectDevice;
            //manager.ListenErrorEvent += ListenError;
            manager.StartListen();
            GetDiagnosticsInfo();
        }

        private void CommonConnectDevice(object sender, DeviceCommonConnectEventArgs args)
        {
            if (args.Message == MobileDevice.Enumerates.ConnectNotificationMessage.Connected)
            {
                currentiOSDevice = args.Device;
                Console.WriteLine("Device is connected");
                //MobileDevice.AMDeviceLookupApplications();
            }
            if (args.Message == MobileDevice.Enumerates.ConnectNotificationMessage.Disconnected)
            {
                Console.WriteLine("Device has broken link");

            }
        }
        private void RecoveryConnectDevice(object sender, DeviceRecoveryConnectEventArgs args)
        {
            if (args.Message == MobileDevice.Enumerates.ConnectNotificationMessage.Connected)
            {
                Console.WriteLine("Recovery mode device is connected");
            }
            if (args.Message == MobileDevice.Enumerates.ConnectNotificationMessage.Disconnected)
            {
                Console.WriteLine("Device has broken link");
            }
        }

        //private void Reload()
        //{
        //    if (currentiOSDevice != null && currentiOSDevice.IsConnected)
        //    {
        //        DrviceName.Text = currentiOSDevice.DeviceName;
        //        DeviceSerial.Text = currentiOSDevice.SerialNumber;
        //        DeviceVersion.Text = currentiOSDevice.ProductVersion;
        //        DeviceModelNumber.Text = currentiOSDevice.ModelNumber;
        //        ActivationState.Text = currentiOSDevice.ActivationState;
        //        DeviceBuildVersion.Text = currentiOSDevice.BuildVersion;
        //        DeviceBasebandBootloaderVersion.Text = currentiOSDevice.BasebandBootloaderVersion;
        //        DeviceBasebandVersion.Text = currentiOSDevice.BasebandVersion;
        //        DeviceFirmwareVersion.Text = currentiOSDevice.FirmwareVersion;
        //        DeviceId.Text = currentiOSDevice.UniqueDeviceID;
        //        DevicePhoneNumber.Text = currentiOSDevice.PhoneNumber;
        //        DeviceProductType.Text = currentiOSDevice.ProductType;
        //        DeviceSIMStatus.Text = currentiOSDevice.SIMStatus;
        //        DeviceWiFiAddress.Text = currentiOSDevice.WiFiAddress;
        //        DeviceColor.Text = currentiOSDevice.DeviceColor.ToString();
        //        lbBattery.Text = currentiOSDevice.GetBatteryCurrentCapacity().ToString();
        //    }
        //}

        private void GetDiagnosticsInfo()
        {
            var result = currentiOSDevice.GetBatteryInfoFormDiagnostics();
        }

    }
    }
}