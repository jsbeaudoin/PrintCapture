namespace PrintsCapture.Prints.Sequence
{
    using PrintsCapture.Prints;

    public class PrintSetWarning
    {        

        public PrintInfo Print { get; set; }

        public bool WsqCreationError { get; set; }

        public bool ServerSwapDifferent { get; set; }

        public bool ServerMissingOrOverrideDifferent { get; set; }        
    }
}
