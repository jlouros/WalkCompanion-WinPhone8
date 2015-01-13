using System;
using System.Windows.Input;
using WalkCompanion.ViewModels;

namespace WalkCompanion.Commands
{
    public class PauseSession : ICommand
    {
        private TrackInformationViewModel _viewModel;

        public PauseSession(TrackInformationViewModel viewModel)
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
            if (_viewModel.Tracer.SessionPaused)
            {
                _viewModel.Tracer.ResumeCurrentSession();
            }
            else {
                _viewModel.Tracer.PauseCurrentSession();
            }
        }
    }
}
