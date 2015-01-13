using System;
using System.Windows.Input;
using WalkCompanion;
using WalkCompanion.ViewModels;

namespace WalkCompanion.Commands
{
    public class StartSession : ICommand
    {
        private TrackInformationViewModel _viewModel;

        public StartSession(TrackInformationViewModel viewModel)
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
            App.TrackInformationViewModel.KickNewSession = true;

            _viewModel.Tracer.StartNewSession();
        }
    }
}
