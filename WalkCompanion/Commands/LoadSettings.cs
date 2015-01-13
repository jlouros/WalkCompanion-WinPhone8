using System;
using System.Windows;
using System.Windows.Input;
using WalkCompanion.ViewModels;

namespace WalkCompanion.Commands
{
    public class LoadSettings: ICommand
    {
        private TrackInformationViewModel _viewModel = null;

        public LoadSettings(TrackInformationViewModel viewModel)
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
            MessageBox.Show("Display settings");
        }
    }
}
