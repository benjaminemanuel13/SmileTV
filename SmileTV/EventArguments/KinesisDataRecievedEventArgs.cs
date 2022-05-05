namespace SmileTV.EventArguments
{
    public class KinesisDataRecievedEventArgs : EventArgs
    {
        public string Message { get; set; }

        public KinesisDataRecievedEventArgs(string message)
        { 
            Message = message;
        }
    }
}
