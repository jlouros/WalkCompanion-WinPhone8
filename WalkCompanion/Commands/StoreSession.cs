using System;
using System.Windows.Input;
using WalkCompanion.ViewModels;

namespace WalkCompanion.Commands
{
    public class StoreSession : ICommand
    {
        private TrackInformationViewModel _viewModel;

        public StoreSession(TrackInformationViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
            //return _viewModel.Tracer.SessionUnderway == false && _viewModel.Tracer.StoredCoordinates.Count > 0;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _viewModel.Tracer.StoreCurrentSession();
        }
    }
}