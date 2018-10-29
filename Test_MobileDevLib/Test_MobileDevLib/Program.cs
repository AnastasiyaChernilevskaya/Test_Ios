using iMobileDevice;
using iMobileDevice.Afc;
using iMobileDevice.HouseArrest;
using iMobileDevice.iDevice;
using iMobileDevice.Lockdown;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_MobileDevLib
{
    class Program
    {
        static void Main(string[] args)
        {
            //ConnectedDevices connectedDevices = new ConnectedDevices();
            //connectedDevices.LoadDevices();
            Console.WriteLine("Starting the imobiledevice-net demo");

            // This demo application will use the libimobiledevice API to list all iOS devices currently
            // connected to this PC.

            // First, we need to make sure the unmanaged (native) libimobiledevice libraries are loaded correctly
            NativeLibraries.Load();

            ReadOnlyCollection<string> udids;
            int count = 0;

            var idevice = LibiMobileDevice.Instance.iDevice;
            var lockdown = LibiMobileDevice.Instance.Lockdown;
            var lockdown1 = LibiMobileDevice.Instance.Lockdown;

            var house_arrest = LibiMobileDevice.Instance.HouseArrest;
            //var lockdown = LibiMobileDevice.Instance.Lockdown;

            var ret = idevice.idevice_get_device_list(out udids, ref count);

            if (ret == iDeviceError.NoDevice)
            {
                // Not actually an error in our case
                Console.WriteLine("No devices found");
                return;
            }

            ret.ThrowOnError();

            // Get the device name
            foreach (var udid in udids)
            {
                iDeviceHandle deviceHandle;
                idevice.idevice_new(out deviceHandle, udid).ThrowOnError();


                lockdown.lockdownd_client_new_with_handshake(deviceHandle, out LockdownClientHandle lockdownHandle_IP, "installation_proxy").ThrowOnError();

                lockdown.lockdownd_start_service(lockdownHandle_IP, "com.apple.mobile.installation_proxy", out LockdownServiceDescriptorHandle lockdownServiceHandle_ip).ThrowOnError();

                lockdown1.lockdownd_start_service(lockdownHandle_IP, "com.apple.mobile.house_arrest", out LockdownServiceDescriptorHandle lockdownServiceHandle).ThrowOnError();


                house_arrest.house_arrest_client_start_service(deviceHandle, out HouseArrestClientHandle houseArrest, "houseArrest");
                               
                house_arrest.house_arrest_client_new(deviceHandle, lockdownServiceHandle, out houseArrest);
                string command = "VendContainer"; //"VendDocuments"; //"";

                //instproxy_lookup();
                string appid = "com.cinamaker.directorPad";

                house_arrest.house_arrest_send_command(houseArrest, command, appid);
                //house_arrest.afc_client_new_from_house_arrest_client(houseArrest, out afcHandle);
                //afc.afc_read_directory(afcHandle, "", out ReadOnlyCollection<string> directoryInformation).ThrowOnError();

                //afc.afc_client_free();
                //house_arrest.house_arrest_client_free();

                deviceHandle.Dispose();
                //lockdownHandle.Dispose();
            }
        }
    }
}
//com.apple.afc //com.apple.mobile.house_arrest //com.apple.mobile.file_relay
//LockdownClientHandle lockdownHandle;

//lockdown.lockdownd_client_new_with_handshake(deviceHandle, out lockdownHandle, "Quamotion").ThrowOnError();

//string deviceName;
//lockdown.lockdownd_get_device_name(lockdownHandle, out deviceName).ThrowOnError();

//Console.WriteLine($"{deviceName} ({udid})");