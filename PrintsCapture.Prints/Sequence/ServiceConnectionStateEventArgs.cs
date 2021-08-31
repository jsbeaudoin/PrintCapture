namespace PrintsCapture.Prints.Sequence
{
    using System;

    public class ServiceConnectionStateEventArgs : EventArgs
    {
        public bool Connected { get; private set; }        

        public ServiceConnectionStateEventArgs(bool connected)
        {
            this.Connected = connected;        
        }
    }
}
