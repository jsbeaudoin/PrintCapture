namespace PrintsCapture.Ui.Extension
{
    using System.Collections.Generic;

    using PrintsCapture.Device;

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
