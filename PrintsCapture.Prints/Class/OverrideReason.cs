namespace PrintsCapture.Prints
{
    using System.Collections.Generic;

    public class OverrideReason
    {
        
        public static List<OverrideReason> GetBaseList()
        {
            var list = new List<OverrideReason>();
            list.Add(new OverrideReason(1, "en", "Ridge Detail Indistinct Due To Individual's Employment"));
            list.Add(new OverrideReason(2, "en", "Ridge Detail Indistinct Due To Age"));
            list.Add(new OverrideReason(3, "en", "Ridge Detail Indistinct Due To Skin Conditions"));
            list.Add(new OverrideReason(4, "en", "Finger Temporarily Injured And Cannot Be Easily Rolled"));
            list.Add(new OverrideReason(5, "en", "Finger Is Deformed / Paralyzed And Cannot Be Easily Rolled"));
            list.Add(new OverrideReason(6, "en", "Finger Is Partially Amputated"));
            list.Add(new OverrideReason(7, "en", "Finger Is Permanently Scarred Or Burned"));            
            list.Add(new OverrideReason(99, "en", "Other", true));

            list.Add(new OverrideReason(1, "fr", "Détails des sillons indistinct dû à l'emploi de l'individu"));
            list.Add(new OverrideReason(2, "fr", "Détails des sillons indistinct dû à l'âge"));
            list.Add(new OverrideReason(3, "fr", "Détails des sillons indistinct dû à la condition de la peau"));
            list.Add(new OverrideReason(4, "fr", "Doigt temporairement blessé et ne peut être roulé facilement"));
            list.Add(new OverrideReason(5, "fr", "Doigt partiellement déformé / paralysé et ne peut être roulé facilement"));
            list.Add(new OverrideReason(6, "fr", "Doigt partiellement amputé"));
            list.Add(new OverrideReason(7, "fr", "Doigt détérioré ou brûlé de manière permanente"));
            list.Add(new OverrideReason(99, "fr", "Autre", true));

            return list;
        }

        public OverrideReason()
        {
            
        }

        public OverrideReason(int code, string language, string text, bool hasUserText = false)
        {
            this.Code = code;
            this.Text = text;
            this.HasUserText = hasUserText;
            this.Language = language;
        }

        public string Language { get; set; }

        public int Code { get; set; }

        public string Text { get; set; }

        public bool HasUserText { get; set; }
    }
}