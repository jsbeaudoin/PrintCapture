using System;

namespace PrintsCapture.Ui.Exceptions
{
    public class PluginPathNotFoundException : ApplicationException
    {
        public PluginPathNotFoundException(string message) : base(message)
        {}
    }
}
