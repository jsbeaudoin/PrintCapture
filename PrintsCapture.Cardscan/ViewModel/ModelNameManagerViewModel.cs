namespace PrintsCapture.Cardscan.ViewModel
{
    public enum UserAction
    {
        None,
        Delete,
        CreateNew,
        Override
    }

    public class ModelNameManagerViewModel
    {
        public string Name { get; set; }

        public bool IsReadOnly { get; set; }

        public bool IsStarred { get; set; }

        public bool IsCreated { get; set; }

        public string NewName { get; set; }

        public UserAction UserAction { get; set; }

        public bool IsDeleteAllowed
        {
            get
            {
                return this.IsCreated && !this.IsReadOnly;
            }
        }
    }
}
