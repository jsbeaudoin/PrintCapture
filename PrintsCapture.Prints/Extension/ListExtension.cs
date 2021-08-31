namespace PrintsCapture.Prints.Extension
{
    using System.Collections.Generic;

    public static class ListExtension
    {
        public static void RemoveList<TObject>(this List<TObject> sourceList, List<TObject> listToRemove)
        {
            if (listToRemove == null)
            {
                return;
            }

            foreach (var item in listToRemove)
            {
                sourceList.Remove(item);
            }

        }
    }
}
