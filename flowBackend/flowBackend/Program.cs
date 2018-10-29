using iMobileDevice;
using iMobileDevice.Afc;
using iMobileDevice.HouseArrest;
using iMobileDevice.iDevice;
using iMobileDevice.Lockdown;
using iMobileDevice.Plist;
using System;
using System.Collections.ObjectModel;

namespace flowBackend
{
    class Program
    {
        static void Main(string[] args)
        {
            ReadOnlyCollection<string> udids;
            int count = 0;

            var idevice = LibiMobileDevice.Instance.iDevice;
            var lockdown = LibiMobileDevice.Instance.Lockdown;
            var plist = LibiMobileDevice.Instance.Plist;
            var hArrest = LibiMobileDevice.Instance.HouseArrest;
            var afc = LibiMobileDevice.Instance.Afc;


            var ret = idevice.idevice_get_device_list(out udids, ref count);
            var Text = ret.ToString();
            Console.WriteLine(Text);
            //var ret1 = idevice.idevice_get_udid(out udids, ref count);
            //var ret2 = idevice.idevice_get_device_list(out udids, ref count);
            //var ret3 = idevice.idevice_get_device_list(out udids, ref count);


            if (ret == iDeviceError.NoDevice)
            {
                // Not actually an error in our case
                return;
            }

            ret.ThrowOnError();

            // Get the device name
            foreach (var udid in udids)
            {
                string hostId = string.Empty;
                string sessionId = string.Empty;
                int sslEnabled = 0;

                iDeviceHandle deviceHandle;
                idevice.idevice_new(out deviceHandle, udid).ThrowOnError();

                LockdownClientHandle lockdownHandle;
                lockdown.lockdownd_client_new_with_handshake(deviceHandle, out lockdownHandle, "111").ThrowOnError();

                var errRes = lockdown.lockdownd_start_service(lockdownHandle, "com.apple.mobile.house_arrest", out LockdownServiceDescriptorHandle lockdownServ);

                //var houseArrestResult1 = hArrest.house_arrest_client_new(deviceHandle, lockdownServ, out HouseArrestClientHandle house_arrest1);


                //houseArrestResult1 = hArrest.house_arrest_send_command(house_arrest1, "VendDocuments", "com.cinamaker.directorPad"); //VendDocuments //VendContainer

                //houseArrestResult1 = hArrest.house_arrest_get_result(house_arrest1, out PlistHandle plistHandle);

                //var node1 = plist.plist_dict_get_item(plistHandle, "Error");


                //var errResolt = lockdown.lockdownd_start_session(lockdownHandle, hostId, out sessionId, ref sslEnabled);

                //var activationRecord = plist.plist_new_string("");

                /////////////////////////////////

                var appid = "com.cinamaker.directorPad";
                var service_name = "com.apple.mobile.house_arrest";

                if (service_name == "com.apple.mobile.house_arrest")
                {

                    var houseArrestResult = hArrest.house_arrest_client_new(deviceHandle, lockdownServ, out HouseArrestClientHandle house_arrest);

                    if (house_arrest.IsInvalid)
                    {
                        Text = "Could not start house_arrest service!\n";
                        Console.WriteLine(Text);
                        return;
                    }
                    houseArrestResult = hArrest.house_arrest_send_command(house_arrest, "VendContainer", appid);
                    if (houseArrestResult != HouseArrestError.Success)
                    {
                        Text = "Could not send VendContainer command!\n";
                        Console.WriteLine(Text);

                        return;
                    }

                    houseArrestResult = hArrest.house_arrest_get_result(house_arrest, out PlistHandle plistHandle);

                    if (houseArrestResult != HouseArrestError.Success)
                    {
                        Text = "Could not get result from house_arrest service!\n";
                        Console.WriteLine(Text);

                        return;
                    }

                    var node = plist.plist_dict_get_item(plistHandle, "Error");

                    //if (node.IsInvalid)
                    //{
                    //    var str = string.Empty;

                    //    //plist_get_string_val(node, str);

                    //    Text = "ERROR: %s\n"+ str;
                    //Console.WriteLine(Text);

                    //    if (str != string.Empty)
                    //    {
                    //        //free(str);
                    //    }
                    //    return;
                    //}

                    var afcResult = hArrest.afc_client_new_from_house_arrest_client(house_arrest, out AfcClientHandle afcClientHandle);

                    afcResult = afc.afc_read_directory(afcClientHandle, "/Documents", out ReadOnlyCollection<string> dirInfo);
                    afcResult = afc.afc_read_directory(afcClientHandle, "/Library", out ReadOnlyCollection<string> dirInfo1);
                    afcResult = afc.afc_read_directory(afcClientHandle, ".", out ReadOnlyCollection<string> dirInfo2);
                    afcResult = afc.afc_read_directory(afcClientHandle, "tmp", out ReadOnlyCollection<string> dirInfo3);


                    afcResult = afc.afc_read_directory(afcClientHandle, "/Library/Application Support", out ReadOnlyCollection<string> dirInfo4);
                    afcResult = afc.afc_read_directory(afcClientHandle, "/Library/Database", out ReadOnlyCollection<string> dirInfo5);
                    afcResult = afc.afc_read_directory(afcClientHandle, "/Library/Caches", out ReadOnlyCollection<string> dirInfo6);
                    afcResult = afc.afc_read_directory(afcClientHandle, "/Library/Cookies", out ReadOnlyCollection<string> dirInfo7);
                    //afcResult = afc.afc_read_directory(afcClientHandle, "/Library/Preferences", out ReadOnlyCollection<string> dirInfo8);
                    afcResult = afc.afc_read_directory(afcClientHandle, "/Library/SyncedPreferences", out ReadOnlyCollection<string> dirInfo9);

                    ulong hendle = 0;
                    afcResult = afc.afc_read_directory(afcClientHandle, "/Library/Preferences", out ReadOnlyCollection<string> dirInfo8);
                    var res = afc.afc_file_open(afcClientHandle, "/Library/Preferences/com.cinamaker.directorPad.plist", AfcFileMode.FopenRdonly, ref hendle);//com.cinamaker.directorPad.plist//iTunesMetadata.plist
                    var res1 = afc.afc_get_file_info(afcClientHandle, "/Library/Preferences/com.cinamaker.directorPad.plist", out ReadOnlyCollection<string> fileInfo);

                    //Library// .com.apple.mobile_container_manager.metadata.plist//tmp
                    //iTunesMetadata.plist

                    //hArrest.house_arrest_client_free(house_arrest);


                    //fuse_opt_add_arg(&args, "-omodules=subdir");
                    //fuse_opt_add_arg(&args, "-osubdir=Documents");
                }



                //lockdown.lockdownd_activate(lockdownHandle, activationRecord).ThrowOnError();

                deviceHandle.Dispose();
                lockdownHandle.Dispose();
            }
        }
    }
}
