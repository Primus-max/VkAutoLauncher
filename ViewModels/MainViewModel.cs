using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.IO;

namespace VkLauncher.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        #region СВОЙСТВА
        private string _title = "VkLauncher";

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string? _selectedPath;

        public string? SelectedPath
        {
            get => _selectedPath;
            set => SetProperty(ref _selectedPath, value);
        }

        private int _hours;

        public int Hours
        {
            get => _hours;
            set => SetProperty(ref _hours, value);
        }

        #endregion



        #region КОММАНДЫ
        public IRelayCommand ChoosePathCommand { get; }
        private void ChoosePath()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedPath = Path.GetDirectoryName(openFileDialog.FileName);
            }
        }
        #endregion



        public MainViewModel()
        {
            #region КОММАНДЫ
            ChoosePathCommand = new RelayCommand(ChoosePath);
            #endregion
        }

    }
}
