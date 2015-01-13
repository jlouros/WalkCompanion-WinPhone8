using System;
using System.Windows;
using System.Windows.Input;
using WalkCompanion.ViewModels;

namespace WalkCompanion.Commands
{
    public class SaveSettings : ICommand
    {
        private TrackInformationViewModel _viewModel;

        public SaveSettings(TrackInformationViewModel viewModel)
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
            MessageBox.Show("Your settings were successfully saved");
        }
    }
}
