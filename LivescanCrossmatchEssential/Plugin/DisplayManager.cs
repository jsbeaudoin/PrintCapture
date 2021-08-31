using PrintsCapture.Prints;
using PrintsCapture.Prints.Enum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Livescan.Scanners.DriverEssential.Sdk;

namespace PrintsCapture.Device.LivescanCrossmatchEssential.Plugin
{
    internal class TemplateManager
    {
        private string templatePath;
        private const string Standby = "index_standby.html";
        private const string Final = "index_final.html";

        private const string LeftHand = "index_standard_left.html";
        private const string RightHand = "index_standard_right.html";

        public TemplateManager()
        {
            string localPath = new Uri(this.GetType().Assembly.GetName().CodeBase).LocalPath;
            this.templatePath = Path.Combine(Path.GetDirectoryName(localPath) ?? "C:\\", "Templates");
        }

        public string GetStandbyTemplateUrl()
        {
            return this.PathToUrl(Standby);
        }

        public string GetTemplateFileUrl(HandPart part, Hand hand, HandScanKind scanKind, bool initial)
        {
            return this.PathToUrl(hand == Hand.Left ? LeftHand : RightHand);            
        }

        private string PathToUrl(string path)
        {
            return "file:///" + Path.Combine(this.templatePath, path).Replace("\\", "/") + "\0";
        }
    }


    /// <summary>
    ///  Supports the touch screen display only at the moment.
    /// </summary>
    internal class DisplayManager
    {
        private readonly int deviceHandle;
        private readonly IEnumerable<PhysicalHandPart> handParts;
        private TemplateManager templates;
        private bool supportsDisplay = true;
        

        public DisplayManager(int deviceHandle, IEnumerable<PhysicalHandPart> handParts)
        {
            this.deviceHandle = deviceHandle;
            this.handParts = handParts;

            this.templates = new TemplateManager();

            supportsDisplay = LSE_SDK.LSCAN_Controls_TouchDisplaySetTemplate(this.deviceHandle,
                templates.GetStandbyTemplateUrl()) == LSE_ErrorCode.LSCAN_STATUS_OK;
        }

        public void DisplayStandby()
        {
            if (!supportsDisplay)
            {
                return;
            }
            
            LSE_SDK.LSCAN_Controls_TouchDisplaySetTemplate(this.deviceHandle,
                templates.GetStandbyTemplateUrl());
        }

        public void InitForCapture(HandPart part, Hand hand, HandScanKind scanKind)
        {
            if (!supportsDisplay)
            {
                return;
            }
            var valManager = new LscanControlArray();

            valManager.DefaultValue = (scanKind == HandScanKind.Rolled ? "2" : "1");            
            
            if (part == HandPart.FourFlats)
            {
                valManager.Add(this.handParts.Where(
                        x =>
                            x.Hand == hand && x.Kind == HandPartKind.Finger && x.HandPart != HandPart.Thumb &&
                            !x.IsMissing)
                    .Select(x => x.EndorsementIndex));


                valManager.Add(hand == Hand.Left ? "HP1" : "HP2");                

            } else if (part == HandPart.TwoThumbs)
            {
                valManager.Add(this.handParts.Where(x => x.HandPart == HandPart.Thumb && !x.IsMissing)
                    .Select(x => x.EndorsementIndex));
            }
            else if (part == HandPart.UpperPalm)
            {
                valManager.Add(this.handParts.Where(
                        x => x.Hand == hand && x.Kind == HandPartKind.Finger && !x.IsMissing)
                    .Select(x => x.EndorsementIndex));
                valManager.Add(hand == Hand.Left ? "HP1" : "HP2");
            }
            else
            {
                valManager.Add(this.handParts.Where(x => x.Hand == hand && x.HandPart == part && !x.IsMissing)
                    .Select(x => x.EndorsementIndex));
            }                                
                      

            LSE_SDK.LSCAN_Controls_TouchDisplaySetTemplate_and_ExternalParameters(
                this.deviceHandle,
                templates.GetTemplateFileUrl(part, hand, scanKind, false),
                valManager.GetArrayPointer(), // parameters
                valManager.GetLength());

        }

        //private IntPtr GetArray(IEnumerable<LSCAN_Controls_KeyValue> values)
        //{
        //    var lst = new List<LSCAN_Controls_KeyValue2>();

        //    foreach (var keyValue in values)
        //    {
        //        var i = new LSCAN_Controls_KeyValue2();
        //        i.key = Marshal.StringToHGlobalAnsi(keyValue.key);
        //        i.value = Marshal.StringToHGlobalAnsi(keyValue.value);
        //        lst.Add(i);
        //    }

        //    var parameters = lst.ToArray();

        //    var hndl = GCHandle.Alloc(parameters, GCHandleType.Pinned);
        //    return hndl.AddrOfPinnedObject();
        //}

        private class LscanControlArray
        {

            private List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            
            public string DefaultValue { get; set; }

            public void Add(string key, string value = null)
            {
                list.Add(new KeyValuePair<string, string>(key,  value ?? DefaultValue));
            }

            public void Add(IEnumerable<int> fingerIndexes)
            {
                foreach (var index in fingerIndexes)
                {
                    list.Add(new KeyValuePair<string, string>($"FP{index}", DefaultValue));
                }
                
            }

            public IntPtr GetArrayPointer()
            {


                var lst = new List<LSCAN_Controls_KeyValue2>();

                foreach (var keyValue in list)
                {
                    var i = new LSCAN_Controls_KeyValue2();
                    i.key = Marshal.StringToHGlobalAnsi(keyValue.Key + "\0");
                    i.value = Marshal.StringToHGlobalAnsi(keyValue.Value + "\0");
                    lst.Add(i);
                }

                var parameters = lst.ToArray();

                var hndl = GCHandle.Alloc(parameters, GCHandleType.Pinned);
                return hndl.AddrOfPinnedObject();
            }

            public int GetLength()
            {
                return list.Count;
            }

        }

    }
}
