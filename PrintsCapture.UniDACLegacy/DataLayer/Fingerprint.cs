namespace PrintsCapture.UniDACLegacy.DataLayer
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// Fingerprint file transferred information
    /// </summary>
    public class Fingerprint
    {

        public Fingerprint()
        {
        }        

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName { get; set; }

        [XmlAttribute("NistIndex")]
        public int NistFingerIndex { get; set; }

        public int NistEndorsementFingerIndex { get; set; }

        public bool FingerIsMissing { get; set; }

        public string FingerMissingCode { get; set; }

        public DateTime? FingerMissingDate { get; set; }

        public int FingerprintOverrideCode { get; set; }

        public string FingerprintOverrideReason { get; set; }

        public int Dpi { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }
}
