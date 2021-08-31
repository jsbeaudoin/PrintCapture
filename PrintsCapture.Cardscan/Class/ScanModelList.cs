namespace PrintsCapture.Cardscan
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using ViewModel;
    using Window;
    using Prints.Language;

    /// <summary>
    /// Used both as a ViewModel and to manage the list of Models
    /// </summary>
    public class ScanModelList : ObservableCollection<ScanModel>
    {
        const string EmptyModelName = @"@_Empty Model";
        

        internal ScanModelList(List<ScanModel> list)
        {
            var specialModels = new List<ScanModel>
            {
                GetEmptyModel(),
                FixedModels.C216(),
                FixedModels.FD258()
            };

            foreach (var model in specialModels)
            {
                if (!list.Exists(x => x.Name == model.Name))
                {
                    list.Add(model);
                }
            }            

            foreach (var scanModel in list.OrderBy(x => !x.IsStarred).ThenBy(x => x.Name))
            {
                this.Add(scanModel);
            }
            
        }

        internal List<ScanModel> GetSaveList()
        {
            return this.Where(x => x.Name != EmptyModelName).ToList();
        }

        private static ScanModel GetEmptyModel()
        {
            var model = new ScanModel()
            {                
                CreationDateTime = new DateTime(2000,1,1),
                IsReadOnly = true,
                Name = EmptyModelName,
                IsStarred = true
               
            };

            return model;
        }        

        public ScanModel Manage(ScanModel model, List<ScanPrintZone> zones )
        {            
            var viewModel = new ModelNameManagerViewModel();
            if (model == null)
            {
                viewModel.NewName = CommonText.New;
                model = new ScanModel() { Name = CommonText.New, IsReadOnly = true};
            }
            else
            {
                viewModel.IsCreated = true;
                viewModel.IsReadOnly = model.IsReadOnly;
                viewModel.IsStarred = model.IsStarred;
                viewModel.Name = model.Name;
                viewModel.NewName = model.Name.StartsWith("@") ? CommonText.New : model.Name + CommonText.New;
            }

            var win = new ManageModelWindow(viewModel);
            win.ShowDialog();
            ScanModel result = null;

            var savedZones = new List<ScanPrintZone>();
            zones.ForEach(savedZones.Add);

            switch (viewModel.UserAction)
            {
                case UserAction.Override:
                    model.Zones = savedZones;
                    result = model;
                    break;

                case UserAction.CreateNew:
                    var newModel = new ScanModel()
                                   {                                       
                                       CreationDateTime = DateTime.Now,
                                       Name = viewModel.NewName,
                                       Zones = savedZones                                    
                                   };
                                      
                    this.Add(newModel);                    

                    result = newModel;
                    model.SettingFileName = null;
                    break;

                case UserAction.Delete:
                    this.Remove(model);
                    break;

                case UserAction.None:
                    result = model;
                    break;
            }

            //CardScanSettings.Default.ScanModels = this;
            CardScanSettings.Save();

            return result;
        }
    }
}
