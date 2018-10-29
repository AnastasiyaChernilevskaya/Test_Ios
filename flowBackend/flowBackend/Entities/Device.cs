using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using flowBackend;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace flowBackend.Entities
{
    class Device
    {
        private string serialNumber = string.Empty;
        private string deviceName = string.Empty;

        private DeviceColorKey deviceColor;
        private string deviceColorString = string.Empty;
        private string deviceColorBgString = string.Empty;


        public IntPtr DevicePtr;
        private bool isSessionOpen = false;
        private bool isConnected = false;
        public bool IsConnected
        {
            get
            {
                return this.isConnected;
            }
        }

        public string DeviceName
        {
            get
            {
                if (this.deviceName.Length == 0)
                {
                    object deviceValue = this.GetDeviceValue(DeviceInfoKey.DeviceName);
                    if (deviceValue != null)
                    {
                        this.deviceName = deviceValue.ToString();
                    }
                }
                return this.deviceName;
            }
        }


        public string SerialNumber
        {
            get
            {
                if (this.serialNumber.Length == 0)
                {
                    object deviceValue = this.GetDeviceValue(DeviceInfoKey.SerialNumber);
                    if (deviceValue != null)
                    {
                        this.serialNumber = deviceValue.ToString();
                    }
                }
                return this.serialNumber;
            }
        }

        public DeviceColorKey DeviceColor
        {
            get
            {
                if (this.deviceColor == DeviceColorKey.Default)
                {
                    var deviceColorValue = GetDeviceValue(DeviceInfoKey.DeviceColor);
                    if (deviceColorValue != null)
                    {
                        var deviceColorString = deviceColorValue.ToString();
                        if (!string.IsNullOrWhiteSpace(deviceColorString))
                        {
                            switch (deviceColorString.ToLower())
                            {
                                case "black":
                                case "#99989b":
                                case "1":
                                    this.deviceColor = DeviceColorKey.Black;
                                    break;
                                case "silver":
                                case "#d7d9d8":
                                case "2":
                                    this.deviceColor = DeviceColorKey.Silver;
                                    break;
                                case "gold":
                                case "#d4c5b3":
                                case "3":
                                    this.deviceColor = DeviceColorKey.Gold;
                                    break;
                                case "rose gold":
                                case "#e1ccb5":
                                case "4":
                                    this.deviceColor = DeviceColorKey.Rose_Gold;
                                    break;
                                case "jet black":
                                case "#0a0a0a":
                                case "5":
                                    this.deviceColor = DeviceColorKey.Jet_Black;
                                    break;
                                default:
                                    this.deviceColor = DeviceColorKey.Unknown;
                                    break;
                            }
                        }
                    }
                }
                return this.deviceColor;
            }
        }

        public object GetDeviceValue(string domain, DeviceInfoKey key)
        {
            return this.GetDeviceValue(domain, key.ToString());
        }

        public object GetDeviceValue(DeviceInfoKey key)
        {
            return this.GetDeviceValue(null, key);
        }

        public object GetDeviceValue(string domain, string key)
        {
            object resultValue = null;
            try
            {
                var isReconnect = false;
                var isReOpenSession = false;
                if (!this.isConnected)
                {
                    if (Connect() != (int)kAMDError.kAMDSuccess)
                    {
                        return null;
                    }
                    isReconnect = true;
                }
                if (!this.isSessionOpen)
                {
                    if (StartSession(false) == (int)kAMDError.kAMDSuccess)
                    {
                        isReOpenSession = true;
                    }
                    else
                    {
                        if (isReconnect)
                        {
                            Disconnect();
                        }
                    }
                }
                resultValue = MobileDevice.AMDeviceCopyValue(this.DevicePtr, domain, key);
                if (isReOpenSession)
                {
                    StopSession();
                }
                if (isReconnect)
                {
                    Disconnect();
                }
            }
            catch
            {
            }
            return resultValue;
        }

        public kAMDError Connect()
        {
            kAMDError kAMDSuccess = kAMDError.kAMDSuccess;
            try
            {
                if (!this.isConnected)
                {
                    kAMDSuccess = (kAMDError)MobileDevice.AMDeviceConnect(this.DevicePtr);
                    if (kAMDSuccess == kAMDError.kAMDSuccess)
                    {
                        this.isConnected = true;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
            return kAMDSuccess;
        }


        public kAMDError Disconnect()
        {
            var kAMDSuccess = kAMDError.kAMDSuccess;
            this.isConnected = false;
            try
            {
                kAMDSuccess = (kAMDError)MobileDevice.AMDeviceDisconnect(DevicePtr);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
            return kAMDSuccess;
        }

        public kAMDError StartSession(bool isRretry = false)
        {
            var kAMDSuccess = kAMDError.kAMDSuccess;
            try
            {
                if (!this.isSessionOpen)
                {
                    kAMDSuccess = (kAMDError)MobileDevice.AMDeviceStartSession(this.DevicePtr);
                    if (kAMDSuccess != kAMDError.kAMDInvalidHostIDError)
                    {
                        if (kAMDSuccess != (int)kAMDError.kAMDSuccess)
                        {
                            //Исправление: проблема, что сеанс не может быть снова открыт после закрытия
                            //на некоторое время
                            if (!isRretry)
                            {
                                this.Disconnect();
                                this.Connect();
                                return StartSession(true);
                            }
                            return kAMDSuccess;
                        }
                        this.isSessionOpen = true;
                        return kAMDSuccess;
                    }
                    if ((MobileDevice.AMDeviceUnpair(this.DevicePtr) == (int)kAMDError.kAMDSuccess) &&
                        (MobileDevice.AMDevicePair(this.DevicePtr) == (int)kAMDError.kAMDSuccess))
                    {
                        kAMDSuccess = (kAMDError)MobileDevice.AMDeviceStartSession(this.DevicePtr);
                        if (kAMDSuccess != kAMDError.kAMDSuccess)
                        {
                            return kAMDSuccess;
                        }
                        this.isSessionOpen = true;
                        return kAMDSuccess;
                    }
                }
                return kAMDSuccess;
            }
            catch
            {
                kAMDSuccess = kAMDError.kAMDUndefinedError;
            }
            return kAMDSuccess;
        }

        private kAMDError StopSession()
        {
            this.isSessionOpen = false;
            try
            {
                return (kAMDError)MobileDevice.AMDeviceStopSession(this.DevicePtr);
            }
            catch
            {
                return kAMDError.kAMDUndefinedError;
            }
        }

    }

    public enum DeviceColorKey
    {
        Unknown = -1,
        Default = 0,
        Black = 1,
        Silver = 2,
        Gold = 3,
        Rose_Gold = 4,
        Jet_Black = 5
    }
    public enum DeviceInfoKey
    {
        DeviceColor,
        DeviceName,
        SerialNumber,
    }

    public enum kAMDError
    {
        kAMDAlreadyArchivedError = -402653094,
        kAMDApplicationAlreadyInstalledError = -402653130,
        kAMDApplicationMoveFailedError = -402653129,
        kAMDApplicationSandboxFailedError = -402653127,
        kAMDApplicationSINFCaptureFailedError = -402653128,
        kAMDApplicationVerificationFailedError = -402653126,
        kAMDArchiveDestructionFailedError = -402653125,
        kAMDBadHeaderError = -402653182,
        kAMDBundleVerificationFailedError = -402653124,
        kAMDBusyError = -402653167,
        kAMDCarrierBundleCopyFailedError = -402653123,
        kAMDCarrierBundleDirectoryCreationFailedError = -402653122,
        kAMDCarrierBundleMissingSupportedSIMsError = -402653121,
        kAMDCheckinTimeoutError = -402653148,
        kAMDCommCenterNotificationFailedError = -402653120,
        kAMDContainerCreationFailedError = -402653119,
        kAMDContainerP0wnFailedError = -402653118,
        kAMDContainerRemovalFailedError = -402653117,
        kAMDCryptoError = -402653166,
        kAMDDigestFailedError = -402653135,
        kAMDEmbeddedProfileInstallFailedError = -402653116,
        kAMDEOFError = -402653170,
        kAMDErrorError = -402653115,
        kAMDExecutableTwiddleFailedError = -402653114,
        kAMDExistenceCheckFailedError = -402653113,
        kAMDFileExistsError = -402653168,
        kAMDGetProhibitedError = -402653162,
        kAMDImmutableValueError = -402653159,
        kAMDInstallMapUpdateFailedError = -402653112,
        kAMDInvalidActivationRecordError = -402653146,
        kAMDInvalidArgumentError = -402653177,
        kAMDInvalidCheckinError = -402653149,
        kAMDInvalidDiskImageError = -402653133,
        kAMDInvalidHostIDError = -402653156,
        kAMDInvalidResponseError = -402653165,
        kAMDInvalidServiceError = -402653150,
        kAMDInvalidSessionIDError = -402653152,
        kAMDIsDirectoryError = -402653175,
        kAMDiTunesArtworkCaptureFailedError = -402653096,
        kAMDiTunesMetadataCaptureFailedError = -402653095,
        kAMDManifestCaptureFailedError = -402653111,
        kAMDMapGenerationFailedError = -402653110,
        kAMDMissingActivationRecordError = -402653145,
        kAMDMissingBundleExecutableError = -402653109,
        kAMDMissingBundleIdentifierError = -402653108,
        kAMDMissingBundlePathError = -402653107,
        kAMDMissingContainerError = -402653106,
        kAMDMissingDigestError = -402653132,
        kAMDMissingHostIDError = -402653157,
        kAMDMissingImageTypeError = -402653136,
        kAMDMissingKeyError = -402653164,
        kAMDMissingOptionsError = -402653137,
        kAMDMissingPairRecordError = -402653147,
        kAMDMissingServiceError = -402653151,
        kAMDMissingSessionIDError = -402653153,
        kAMDMissingValueError = -402653163,
        kAMDMuxError = -402653131,
        kAMDNoResourcesError = -402653181,
        kAMDNotConnectedError = -402653173,
        kAMDNotFoundError = -402653176,
        kAMDNotificationFailedError = -402653105,
        kAMDOverrunError = -402653171,
        kAMDPackageExtractionFailedError = -402653104,
        kAMDPackageInspectionFailedError = -402653103,
        kAMDPackageMoveFailedError = -402653102,
        kAMDPasswordProtectedError = -402653158,
        kAMDPathConversionFailedError = -402653101,
        kAMDPermissionError = -402653174,
        kAMDProvisioningProfileNotValid = -402653140,
        kAMDReadError = -402653180,
        kAMDReceiveMessageError = -402653138,
        kAMDRemoveProhibitedError = -402653160,
        kAMDRestoreContainerFailedError = -402653100,
        kAMDSeatbeltProfileRemovalFailedError = -402653099,
        kAMDSendMessageError = -402653139,
        kAMDSessionActiveError = -402653155,
        kAMDSessionInactiveError = -402653154,
        kAMDSetProhibitedError = -402653161,
        kAMDStageCreationFailedError = -402653098,
        kAMDStartServiceError = -402653134,
        kAMDSuccess = 0,
        kAMDSUFirmwareError = -402653141,
        kAMDSUPatchError = -402653142,
        kAMDSUVerificationError = -402653143,
        kAMDSymlinkFailedError = -402653097,
        kAMDTimeOutError = -402653172,
        kAMDTrustComputerError = -402653034,
        kAMDUndefinedError = -402653183,
        kAMDUnknownPacketError = -402653178,
        kAMDUnsupportedError = -402653169,
        kAMDWriteError = -402653179,
        kAMDWrongDroidError = -402653144
    }


}
