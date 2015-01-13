using System;
using System.Windows.Input;
using WalkCompanion.ViewModels;

namespace WalkCompanion.Commands
{
    public class PickSession: ICommand
    {
        private TrackInformationViewModel _viewModel =null;

        public PickSession(TrackInformationViewModel viewModel)
        {
            viewModel = _viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _viewModel.Tracer.PickSession();
        }
    }
}
