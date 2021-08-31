using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrintsCapture.Cardscan 
{    
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Windows.Forms;

    using NLog;
    
    
    using PrintsCapture.Cardscan.ViewModel;
    using PrintsCapture.Cardscan.Window;
    using PrintsCapture.Device;
    using PrintsCapture.Device.DataLayer;
    using PrintsCapture.Device.Enum;
    using PrintsCapture.Device.Extension;
    using PrintsCapture.Device.Interface;
    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.Extension;
    using PrintsCapture.Prints.Language;

    using XL_ID.Utilities.Image;
    using XL_ID.Utilities.Setting;
    using XL_ID.Utilities.XML;

    public class CardscanCapture : DeviceCaptureDriver
    {
        private readonly ICardscanDevice cardscanDevice;

        private const string SerialSettingName = "serial";

        private readonly Logger logger;

        private Bitmap lastBitmap;

        private PrintResolution lastResolution;

        public CardscanCapture(ICaptureDevice device, PrintList printList) : base(device, printList)
        {            
            cardscanDevice = device as ICardscanDevice;
            this.logger = LogManager.GetCurrentClassLogger();
        }

        public override string GetDeviceSecondaryInfo()
        {
            if (this.CaptureDevice == null)
            {
                return string.Empty;
            }

            return this.CaptureDevice.SerialNumber;
        }

        public override ScanResult CaptureAuto()
        {
            return this.Scan();
        }

        public override ScanResult CaptureSingle(PrintInfo print)
        {
            return this.Scan();
        }

        public override void ConfigureDefaultValue(List<ICaptureSdk> sdkList, string selectedKey)
        {
            var conf = CardScanSettings.Default.DeviceConfig;

            // load serial numbers !            
            if (conf == null)
            {
                return;
            }

            sdkList.ForEach(
                x =>
                    x.SupportedDeviceList.ToList()
                        .ForEach(
                            dev =>
                            {
                                string serial = null;
                                var deviceSettings = conf.Devices.SingleOrDefault(cf => cf.Device == dev.InternalKey);
                                if (deviceSettings != null)
                                {
                                    var serialSetting = deviceSettings.Settings.SingleOrDefault(set => set.Name == SerialSettingName);
                                    if (serialSetting != null)
                                    {
                                        serial = serialSetting.Value;
                                    }

                                }                                
                                var cardDevice = dev as ICardscanDevice;
                                if (cardDevice != null && ! string.IsNullOrEmpty(serial))
                                {
                                    cardDevice.SetSerialNumber(serial);
                                }                                
                            }));
        }

        public override bool? Configure(DeviceConfigurationViewModel viewModel)
        {
            
            // build device list
            var deviceList = new List<DeviceViewModel>();

            viewModel.Sdks.ForEach(
                x => x.SupportedDeviceList.ToList().ForEach(dev => deviceList.Add(new DeviceViewModel(dev))));

            // build viewModel            
            var selected = deviceList.FirstOrDefault(x => x.InternalKey == viewModel.SelectedDeviceKey);

            var vm = new CardScanConfigurationViewModel { DeviceList = deviceList, SelectedDevice = selected };

            var configWin = new DeviceConfigurationWindow(vm);

            var winResult = configWin.ShowDialog();

            if (!(winResult.HasValue && winResult.Value))
            {
                return winResult;
            }

            var deviceConfig = CardScanSettings.Default.DeviceConfig;

            if (deviceConfig == null)
            {
                deviceConfig = new DeviceSettingList();
                CardScanSettings.Default.DeviceConfig = deviceConfig;
            }

            if (vm.SelectedDevice != null)
            {
                var currentSettings = deviceConfig.Devices.SingleOrDefault(x => x.Device == vm.SelectedDevice.InternalKey);

                if (currentSettings == null)
                {
                    currentSettings = new DeviceSetting { Device = vm.SelectedDevice.InternalKey };
                    deviceConfig.Devices.Add(currentSettings);
                }

                //var currentSerial = serials.FirstOrDefault(x => x.InternalKey == vm.SelectedDevice.InternalKey);

                if (currentSettings.Settings.All(x => x.Name != SerialSettingName))
                {
                    currentSettings.Settings.Add(new SettingValue() { Name = SerialSettingName, Value = vm.SelectedDevice.SerialNumber });
                }
                else
                {
                    currentSettings.Settings.Single(x => x.Name == SerialSettingName).Value = vm.SelectedDevice.SerialNumber;
                }
            }

            CardScanSettings.Save();

            if (vm.SelectedDevice == null)
            {
                viewModel.SelectedDeviceKey = string.Empty;
                return true;
            }
            viewModel.SelectedDeviceKey = vm.SelectedDevice.InternalKey;
            var scanner = viewModel.Sdks.GetDeviceFromKey(viewModel.SelectedDeviceKey) as ICardscanDevice;

            if (scanner != null)
            {
                scanner.SetSerialNumber(vm.SelectedDevice.SerialNumber);
            }
                       
            return true;
        }

        public override bool? DoOperation(PrintInfo print, PrintZoomAction op)
        {
            if (op != PrintZoomAction.Edit)
            {
                return false;
            }
            var editWin = new FingerEditorWindow();
            var editResult = editWin.EditFinger(print.Image, print.OriginalImage, PrintList.GetName(print), print.PrintList.Rules.CropTolerance);
            if (editResult.HasValue && editResult.Value)
            {
                print.Image = editWin.ResultImage;
                print.ProcessStatus = PrintProcessStatus.InProcess;
                PrintModificationDispatcher.PrintModified(print);
                this.TriggerPrintCaptured(print, true);
            }

            return editResult;
        }

        public override ScanResult CapturePositions()
        {
            if (lastBitmap == null)
            {
                return ScanResult.Canceled;
            }

            return this.CapturePrintZones(this.lastBitmap, this.lastResolution);
        }

        /// <summary>
        /// Scan a page. Throws exception on failure to do so.
        /// </summary>
        /// <returns>Status of scanning by user : Canceled, Completed or NoDevice.</returns>
        public ScanResult Scan()
        {
            var device = this.cardscanDevice;
            if (device == null)
            {
                this.TriggerCaptureCompleted(true);
                return ScanResult.NoDevice;
            }

            if (this.lastBitmap != null)
            {
                var last = lastBitmap;
                this.lastBitmap = null;

                last.Dispose();
            }
            

            if (device.ModelName == "FOLDER")
            {
                return this.ProcessFolderImport();                
            }

            var scanVm = new ScanParameterViewModel(device.DisplayName, device.SupportedResolutions, device.SerialNumber, CardScanSettings.Default.PresetCode);

            var scanWin = new ScanParameterWindow(scanVm);

            var winResult = scanWin.ShowDialog();

            if (!(winResult.HasValue && winResult.Value))
            {
                this.TriggerCaptureCompleted(true);
                return ScanResult.Canceled;
            }

            float offset = scanVm.CustomOffset / 100F;
            float size = scanVm.CustomSize / 100F;
            string associatedModel = null;

            if (scanVm.IsCustom)
            {
                CardScanSettings.Default.PresetCode = 0;
                CardScanSettings.Default.PresetManualOffset = offset;
                CardScanSettings.Default.PresetManualSize = size;
            }
            else
            {
                try
                {
                    CardScanSettings.Default.PresetCode = scanVm.PresetCode;
                    var preset = scanVm.Presets.Single(x => x.Code == scanVm.PresetCode);
                    offset = preset.Offset;
                    size = preset.Size;
                    associatedModel = preset.AssociatedModel;
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex, "Error inititing cardscan");
                    if (scanVm.Presets.Count(x => x.Code == scanVm.PresetCode) > 1)
                    {
                        // Settings corrupted. delete settings !
                        this.logger.Warn("Corrupted settings detected");
                        this.TriggerSettingsCorrupted();
                        MessageBox.Show("Corrupted Settings. Settings have been reinitialized");
                        return ScanResult.Canceled;
                    }
                    return ScanResult.Canceled;
                }

                
            }

            CardScanSettings.Save();

            Bitmap resultImage;

            // Exceptions here are processed by the caller
            try
            {
                device.Open();

                resultImage = device.Scan(scanVm.DeviceResolution.ToResolution(), offset, size);
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Device error on CardscanCapture.Scan");
                resultImage = null;
                MessageBox.Show(
                    CommonText.VerifyDeviceMsg + " (" + ex.Message + ")",
                    CommonText.DeviceInternalError,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                try
                {
                    device.Close();
                }
                catch (Exception ex)
                {
                    // whatever happens, try to close the device
                    this.logger.Error(ex, "Error when closing scanner '{0}'",device.DisplayName);
                }
            }

            if (resultImage == null)
            {
                this.TriggerCaptureCompleted(true);
                return ScanResult.Canceled;
            }

            lastBitmap = resultImage;
            lastResolution = scanVm.DeviceResolution.ToResolution();

            return CapturePrintZones(resultImage, lastResolution, associatedModel);
        }

        private ScanResult CapturePrintZones(Bitmap resultImage, PrintResolution resolution, string associatedModelName = null)
        {
            var win = new PageScanWindow(resultImage, resolution, this.PrintList.GetEndorsableFingers(), associatedModelName,
                this.PrintList.Rules.IsFlatCaptureMode
                );

            var result = win.ShowDialog();

            if (!(result.HasValue && result.Value))
            {
                this.TriggerCaptureCompleted(true);
                return ScanResult.Canceled;
            }

            var scannedPrints = win.GetScannedFingerList();

            // Indicate which fingers have been scanned                        
            foreach (var print in scannedPrints)
            {
                if (!IsPrintZoneAllowed(print))
                {
                    continue;
                }
                
                var printInfo = this.PrintList.GetPrint(print.Hand, print.HandPart, print.ScanKind);
                printInfo.Reset();
                printInfo.ProcessStatus = PrintProcessStatus.InProcess;
                PrintModificationDispatcher.PrintModified(printInfo);                                
            }

            Task.Factory.StartNew(() => this.ProcessScannedPrintsAsync(win.GetResults().Results));

            this.TriggerCaptureCompleted(false);
            return ScanResult.Completed;
        }

        private bool IsPrintZoneAllowed(PrintZoneInfo pzi)
        {

            if (pzi.HandPart == HandPart.CompletePalm || pzi.HandPart == HandPart.LowerPalm ||
                pzi.HandPart == HandPart.UpperPalm || pzi.HandPart == HandPart.Hypothenar)
            {
                return this.PrintList.Rules.CaptureGroup == PrintCaptureGroup.StandardAndPalm;
            }

            if (pzi.HandPart == HandPart.Endorsement)
            {
                return this.PrintList.Rules.IsEndorsementAllowed;
            }

            if (pzi.ScanKind == HandScanKind.Rolled)
            {
                return this.PrintList.Rules.CaptureGroup == PrintCaptureGroup.StandardAndPalm ||
                       this.PrintList.Rules.CaptureGroup == PrintCaptureGroup.Standard14;
            }

            return true;
        }

        private void ProcessScannedPrintsAsync(IEnumerable<PageScanZoneResultViewModel> scannedPrints)
        {
            var save = Directory.Exists("D:\\Temp\\Cardscan");
            var savePath = "D:\\Temp\\Cardscan\\{0}-{1}.bmp";

            var isFlat = this.PrintList.Rules.IsFlatCaptureMode;

            // process images and display them as InProcess
            foreach (var print in scannedPrints)
            {
                if (!IsPrintZoneAllowed(print.ZoneInfo))
                {
                    continue;
                }

                var printInfo = this.PrintList.GetPrint(print.ZoneInfo.Hand, print.ZoneInfo.HandPart, print.ZoneInfo.ScanKind);
                // auto crop and resize the print
                printInfo.Resolution = print.Resolution;

                if (print.ZoneInfo.HandPart == HandPart.Endorsement)
                {
                    var zone = print.ZoneInfo as EndorsementZoneInfo;
                    if (zone != null && zone.EndorsementIndex != 0)
                    {
                        var finger =
                            this.PrintList.PhysicalParts.SingleOrDefault(
                                x => x.EndorsementIndex == zone.EndorsementIndex);
                        printInfo.EndorsementFinger = finger;

                    }
                }

                var clearedImage = this.ClearImage(print.Image, print.Resolution.ToDpi(), printInfo.CaptureSize(isFlat));

                if (save)
                {
                    clearedImage.Save(string.Format(savePath, print.ZoneInfo.Id, "cropped"), ImageFormat.Bmp);
                    print.Image.Save(string.Format(savePath, print.ZoneInfo.Id, "scanned"), ImageFormat.Bmp);
                }
                printInfo.Image = clearedImage;
                printInfo.ProcessStatus = PrintProcessStatus.InProcess;
                PrintModificationDispatcher.PrintModified(printInfo);

                this.TriggerPrintCaptured(printInfo, false, false);
            }
            this.TriggerPrintCaptured(null, false, true);
        }

        private Bitmap ClearImage(Bitmap imgIn, int dpi, Size captureSize)
        {
            if (imgIn == null)
            {
                return null;
            }

            var replacer = new ColorReplacer();
            replacer.ReplaceColor(Color.White, 20, Color.White, imgIn);

            var autoCropped = ImageUtilities.AutoCropAndCenter(imgIn, Color.White, this.PrintList.Rules.CropTolerance, Color.White, captureSize);
            autoCropped.SetResolution(imgIn.HorizontalResolution, imgIn.VerticalResolution);

            return autoCropped;
        }

        private ScanResult ProcessFolderImport()
        {            
            var filter = "*.bmp";
            
            var fd = new FolderBrowserDialog();

            if (fd.ShowDialog() != DialogResult.OK)
            {
                this.TriggerCaptureCompleted(true);
                return ScanResult.Canceled;
            }

            var path = fd.SelectedPath;

            var imageFiles = Directory.GetFiles(path, filter);
            foreach (var imageFile in imageFiles)
            {
                var name = Path.GetFileNameWithoutExtension(imageFile);
                int nistNo;
                if (int.TryParse(name, out nistNo))
                {
                    var print = this.PrintList.Prints.SingleOrDefault(x => x.NistPosition == nistNo);
                    if (print != null)
                    {
                        print.Reset();
                        print.Resolution = PrintResolution.Dpi500;
                        print.ProcessStatus = PrintProcessStatus.InProcess;
                        print.Image = (Bitmap)Image.FromFile(imageFile);
                        PrintModificationDispatcher.PrintModified(print);

                        this.TriggerPrintCaptured(print, false, false);
                    }
                }
            }
            this.TriggerPrintCaptured(null, false, true);
            this.TriggerCaptureCompleted(false);
            return ScanResult.Completed;
        }
    }
}
