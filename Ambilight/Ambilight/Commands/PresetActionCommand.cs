using System;
using System.Windows.Input;
using AmadeusW.Ambilight.DataClasses;

namespace AmadeusW.Ambilight.Commands
{
    public class PresetActionCommand : ICommand
    {
        MainWindow _viewModel;

        public PresetActionCommand(MainWindow viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            var parameterString = parameter.ToString();
            if (parameterString == "Save"
                || parameterString == "Duplicate"
                || parameterString == "Remove")
            {
                return true;
            }
            return false;
        }

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                _viewModel.ExecutePresetAction(parameter.ToString());
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}
