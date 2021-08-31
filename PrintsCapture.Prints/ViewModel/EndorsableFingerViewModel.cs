namespace PrintsCapture.Prints.ViewModel
{
    public class EndorsableFingerViewModel
    {
        public EndorsableFingerViewModel(int index, string name)
        {
            this.Index = index;
            this.Name = name;
        }

        public int Index { get; set; }

        public string Name { get; set; }
    }
}
