using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintsCapture.LocalNeuroSeqCheck
{
    using System.Drawing;

    using Neurotec.Biometrics;

    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;
    using PrintsCapture.Prints.Sequence;

    public class LocalNeuroSequenceCheck : BaseSeqCheckService
    {

        private class CapturedPrintInfo
        {
            public Guid ImageKey { get; set; }
            
            public int Position { get; set; }

            public bool IsSlap { get; set; }
            
        }

        private class TemplateInfo
        {
            public CapturedPrintInfo OriginatingPrint { get; set; }

            public int TemplatePosition { get; set; }

            public byte[] Template { get; set; }            
        }

        private NeuroHelper neuroHelper;

        private List<PhysicalHandPart> missings;

        private List<TemplateInfo> templatesInformation;

        private Dictionary<PrintInfo, CapturedPrintInfo> capturedPrints;

        public LocalNeuroSequenceCheck(CaptureKind captureProvenance, PrintList prints)
            : base(captureProvenance, prints)
        {
            neuroHelper = new NeuroHelper(5000);
        }

        public override void Test()
        {
            throw new NotImplementedException();
        }

        public override bool Connect()
        {
            
            this.Connected = this.neuroHelper.ActivateLicence();
            
            this.OnConnectionStateChanged(this.Connected);
            return this.Connected;
        }

        public override bool StartSession()
        {
            if (this.InSession)
            {
                return true;
            }

            this.templatesInformation = new List<TemplateInfo>();            
            this.capturedPrints = new Dictionary<PrintInfo, CapturedPrintInfo>();

            // set missing fingers
            this.missings = this.Prints.PhysicalParts.Where(x => x.IsMissing).ToList();

            this.OnSessionStartOrEnd(true);

            return true;
        }

        public override void AddPrint(PrintInfo print)
        {
            this.AddOrUpdateTemplate(print);            
        }

        private CapturedPrintInfo GetCapturedPrint(PrintInfo print)
        {
            CapturedPrintInfo result;
            if (!this.capturedPrints.ContainsKey(print))
            {
                result = new CapturedPrintInfo
                         {
                             ImageKey = print.ImageKey,
                             IsSlap = print.IsSlap,
                             Position = print.PhysicalPart.EndorsementIndex
                         };
                this.capturedPrints.Add(print, result);
            }
            else
            {
                result = this.capturedPrints[print];
            }

            return result;
        }

        private void AddOrUpdateTemplate(PrintInfo print)
        {
            var capt = this.GetCapturedPrint(print);

            // different images means different templates
            if (print.ImageKey != capt.ImageKey)
            {
                // remove existing templates
                foreach (var templateInfo in templatesInformation.Where(x => x.OriginatingPrint == capt))
                {
                    templatesInformation.Remove(templateInfo);
                }

                // generate templates
                NeuroPrintInfo neuroInfo = null;
                try
                {
                    neuroInfo = this.neuroHelper.AnalyzePrint(
                    print.ImageForProcessing,
                    print.PhysicalPart.EndorsementIndex,
                    this.missings.Select(x => x.EndorsementIndex).ToList());
                }
                catch (Exception ex)
                {
                    this.Logger.Error(ex, "Cannot generate template for {0}. An exception occured", print.Key);
                    print.ProcessStatus = PrintProcessStatus.TemplateError;
                    this.OnPrintModified(print);
                    return;
                }

                foreach (var fingerTemplate in neuroInfo.Templates)
                {
                    var t = new TemplateInfo
                            {
                                OriginatingPrint = capt,
                                Template = fingerTemplate.Template,
                                TemplatePosition = (int)fingerTemplate.Position
                            };
                    this.templatesInformation.Add(t);

                }

                if (print.HasSegments)
                {
                    try
                    {
                        foreach (var fingerTemplate in neuroInfo.Templates)
                        {
                            FingerTemplate template = fingerTemplate;
                            var seg = print.Segments.SingleOrDefault(x => x.Part.EndorsementIndex == (int)template.Position);
                            if (seg != null)
                            {
                                seg.QualityScore = fingerTemplate.Score;
                                seg.Position = new Rectangle(
                                    fingerTemplate.RectLeft,
                                    fingerTemplate.RectTop,
                                    fingerTemplate.RectWidth,
                                    fingerTemplate.RectHeight);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Logger.Error(ex, "Cannot process segments for {0}. An exception occured", print.Key);
                        print.ProcessStatus = PrintProcessStatus.TemplateError;
                        this.OnPrintModified(print);
                        return;
                    }
                }

                print.QualityScore = neuroInfo.Quality;
            }
                                   
            this.CalculateSequenceScore(print);
            
            print.ProcessStatus = PrintProcessStatus.Success;
            print.SequenceAnalyzed = true;

            this.OnPrintModified(print);
        }

        private void CalculateSequenceScore(PrintInfo print)
        {
            var captured = this.capturedPrints[print];
            List<int> selfIndexes;

            print.SequenceBestScore = 0;
            print.SequenceBestScorePosition = 0;
            print.SequenceSelfScore = 0;
            print.MatchedKey = string.Empty;

            // calculate sequence score
            if (!captured.IsSlap)
            {
                // rolled or endorsement
                var template = this.templatesInformation.SingleOrDefault(x => x.OriginatingPrint == captured);
                if (template == null)
                {
                    return;
                }
                
                print.AllScores = this.CalculateScore(template).Select(x => new Prints.SequenceCheckResult { Position = (int)x.Position, Score = x.Score }).ToList();

                if (print.IsEndorsement && print.EndorsementFinger != null)
                {
                    selfIndexes = new List<int>() { print.EndorsementFinger.EndorsementIndex };
                }
                else
                {
                    selfIndexes = new List<int>() { template.TemplatePosition };
                }
                
            }
            else
            {
                // many to many
                var templates = this.templatesInformation.Where(x => x.OriginatingPrint == captured).ToList();
                var allScores = this.templatesInformation.ToDictionary(x => x.TemplatePosition, y => 0);
                // keep best score for each position
                foreach (var templateInfo in templates)
                {
                    var score=  this.CalculateScore(templateInfo);
                    foreach (var checkResult in score)
                    {
                        var key = (int)checkResult.Position;
                        if (checkResult.Score > allScores[key])
                        {
                            allScores[key] = checkResult.Score;
                        }
                    }
                }
                
                selfIndexes = templates.Select(x => x.TemplatePosition).ToList();
                print.AllScores = allScores.Select(x => new Prints.SequenceCheckResult { Position = x.Key, Score = x.Value }).ToList();
            }

            this.SetPrintScores(print, print.AllScores, selfIndexes);
        }

        private List<int> AssignSelfIndexes(List<int> indexes)
        {
            // TODO : Verify that 1,6,11,12 are treated the correct way ...
            var special = new Dictionary<int, int> { { 1, 1}, { 6, 6}, { 11, 1 }, { 12, 6} };
            // transform special values
            //List<int> selfIndexes;
            return null;
        }

        private void SetPrintScores(PrintInfo print, List<Prints.SequenceCheckResult> scores, IEnumerable<int> selfIndexes)
        {            
            // find the best entry
            var best = scores.First(x => x.Score == scores.Max(y => y.Score));
            
            // find the self score entry
            var selfScores = scores.Where(x => selfIndexes.Any(y => y == x.Position)).ToList();
            var self = selfScores.First(x => x.Score == selfScores.Max(y => y.Score));

            // TODO : for slap, correct the positions found for slap positions
            //if (print.IsSlap)
            //{
                
            //}

            print.SequenceBestScore = best.Score;
            print.SequenceBestScorePosition = best.Position;
            print.SequenceSelfScore = self.Score;            

            if (best.Position != self.Position && print.PrintList.Rules.IsSequenceChangingPosition)
            {
                var matched = print.PrintList.Prints.FirstOrDefault(x => x.PhysicalPart.EndorsementIndex == best.Position);
                // change position !
                print.MatchedKey = matched.Key;
                
            }
            
            print.SequenceScore = best.Score;            
        }

        private List<SequenceCheckResult> CalculateScore(TemplateInfo template)
        {
            // get flat templates
            var flatTemplates = this.templatesInformation.Where(x => x.OriginatingPrint.IsSlap).ToList();
            var flatDictionary = flatTemplates.ToDictionary(x => (NFPosition)x.TemplatePosition, y => y.Template);

            return this.neuroHelper.SequenceCheck(template.Template, flatDictionary);
        }

        public override void AddPrintRange(List<PrintInfo> printsToSequence)
        {
            throw new NotImplementedException();
        }

        public override string EndSession(bool accept)
        {
            this.OnSessionStartOrEnd(false);
            return accept ? "1" : null;
        }

        public override bool ResetSession()
        {
            throw new NotImplementedException();
        }

        public override void SetPrintOverride(PrintInfo print)
        {
            // nothing
        }

        public override void SetSegmentOverride(PrintInfo print, PrintSegment segment)
        {
            // nothing
        }

        public override void SequencePositionRuleChanged()
        {
            // nothing
        }

        public override void OnDeviceChanged()
        {
            // nothing !
        }

        protected override List<PrintSetWarning> CheckSessionIntegrity()
        {
            return new List<PrintSetWarning>();
        }
    }
}
