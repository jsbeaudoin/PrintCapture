namespace PrintsCapture.Prints
{
    using System;
    using System.IO;
    using System.Reflection;

    public static class PrintAppPath
    {
        private static readonly string BasePath;

        static PrintAppPath()
        {
            BasePath = (Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\").Replace(@"file:\", String.Empty);            
        }

        public static string AppPath => BasePath;        

        public static string LivescanPluginPath => Path.Combine(BasePath, "Plugins\\Livescan\\");

        public static string CardscanPluginPath => Path.Combine(BasePath, "Plugins\\Cardscan\\");
    }
}
