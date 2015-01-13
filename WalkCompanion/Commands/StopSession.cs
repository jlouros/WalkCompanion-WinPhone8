using System;
using System.Windows.Input;
using WalkCompanion.ViewModels;

namespace WalkCompanion.Commands
{
    public class StopSession : ICommand
    {
        private TrackInformationViewModel _viewModel;

        public StopSession(TrackInformationViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _viewModel.Tracer.StopCurrentSession();
        }
    }
}
