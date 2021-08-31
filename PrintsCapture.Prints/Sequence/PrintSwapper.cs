namespace PrintsCapture.Prints.Sequence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using PrintsCapture.Prints;
    using PrintsCapture.Prints.Enum;

    using UniBIO.Services.Communication.BiometricService;

    public class PrintSwapper
    {
        private readonly PrintList printList;

        private readonly PrintRules rules;        

        public PrintSwapper(PrintList printList)
        {
            this.printList = printList;
            this.rules = printList.Rules;
        }

        public List<PrintSwap> GetUnSwapList()
        {
            return
                this.printList.Prints.Where(x => x.ScanKind == HandScanKind.Rolled)
                    .Select(
                        y =>
                            new PrintSwap
                            {
                                Position = (PrintPosition)y.NistPosition,
                                ScannedPosition = (PrintPosition)y.NistPosition
                            })
                    .ToList();
        }

        public List<PrintSwap> GetSwapList()
        {
            return this.Swap(false);
        }

        private List<PrintSwap> Swap(bool unswap)
        {
            var result = new List<PrintSwap>();            

            var allRolled = this.printList.Prints.Where(x => x.ScanKind == HandScanKind.Rolled && x.SequenceAnalyzed).ToList();

            if (allRolled.Count < 2)
            {
                return result;
            }

            if (unswap || !(this.rules.IsSequenceEnabled && this.rules.IsSequenceChangingPosition))
            {
                // replace all prints to original position
                var replacedPrints = allRolled.Where(x => x.NistPosition != x.MatchedNistPosition && x.MatchedNistPosition != 0).ToList();                

                foreach (var print in replacedPrints)
                {
                    result.Add(this.GetNewSwap(print.NistPosition, print.NistPosition));
                }
            }
            else
            {
                result = this.CalculatePositions(allRolled);
            }            

            return result;
        }

        private PrintSwap GetNewSwap(int originalPosition, int newPosition)
        {
            return new PrintSwap
                   {
                       Position = (PrintPosition)newPosition,
                       ScannedPosition = (PrintPosition)originalPosition
                   };
        }

        private SequenceCheckResult GetBestAvailable(List<int> availablePositions, IEnumerable<SequenceCheckResult> sequenceValues)
        {
            var orderedValues = sequenceValues.OrderByDescending(x => x.Score).ToList();
            foreach (var orderedValue in orderedValues)
            {
                var position = orderedValue.Position;
                if (availablePositions.Contains(position))
                {
                    return orderedValue;
                }
            }


            // code should never reach this part !
            if (availablePositions.Count == 0)
            {
                throw new ApplicationException("No position was found");
            }

            return new SequenceCheckResult { Position = availablePositions[0], Score = 0 };

        }

        private List<PrintSwap> CalculatePositions(List<PrintInfo> printsToAnalyze)
        {
            var results = new List<PrintSwap>();
            
            // get all available index
            var allIndexesToMatch = printsToAnalyze.Select(x => x.NistPosition).ToList();
            var printsToMatch = new List<PrintInfo>(printsToAnalyze);

            // remove self matching prints from allindexes and non matched prints
            var selfMatchings = printsToAnalyze.Where(x => x.SequenceSelfScore > this.rules.SequenceThreshold).ToList();
            foreach (var selfMatching in selfMatchings)
            {                
                allIndexesToMatch.Remove(selfMatching.NistPosition);
                printsToMatch.Remove(selfMatching);
                // adds to swap list if matched to another print
                if (selfMatching.NistPosition != selfMatching.MatchedNistPosition && selfMatching.MatchedNistPosition > 0)
                {
                    results.Add(this.GetNewSwap(selfMatching.NistPosition, selfMatching.NistPosition));
                }
            }

            // best matching prints above threshold and position available
            var bestMatching = printsToMatch.Where(x => x.SequenceBestScore > this.rules.SequenceThreshold).ToList();
            foreach (var bestMatch in bestMatching)
            {
                var best = this.GetBestAvailable(allIndexesToMatch, bestMatch.AllScores);
                if (best.Score > this.rules.SequenceThreshold)
                {
                    results.Add(this.GetNewSwap(bestMatch.NistPosition, best.Position));
                    allIndexesToMatch.Remove(best.Position);
                    printsToMatch.Remove(bestMatch);
                }                
            }

            // Keep same position if still available
            bestMatching = printsToMatch.Select(x => x).ToList();
            foreach (var bestMatch in bestMatching)
            {
                if (allIndexesToMatch.Contains(bestMatch.NistPosition))
                {
                    allIndexesToMatch.Remove(bestMatch.NistPosition);
                    printsToMatch.Remove(bestMatch);
                }                
            }

            // best matching prints with scores above 0
            bestMatching = printsToMatch.Where(x => x.SequenceBestScore > 0).ToList();
            foreach (var bestMatch in bestMatching)
            {
                var best = this.GetBestAvailable(allIndexesToMatch, bestMatch.AllScores);
                results.Add(this.GetNewSwap(bestMatch.NistPosition, best.Position));
                allIndexesToMatch.Remove(best.Position);
                printsToMatch.Remove(bestMatch);
            }

            // everything else !
            bestMatching = printsToMatch.Select(x => x).ToList();
            foreach (var bestMatch in bestMatching)
            {
                var best = this.GetBestAvailable(allIndexesToMatch, bestMatch.AllScores);
                results.Add(this.GetNewSwap(bestMatch.NistPosition, best.Position));
                allIndexesToMatch.Remove(best.Position);
                printsToMatch.Remove(bestMatch);
            }

            return results;
        }
    }
}
