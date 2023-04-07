using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows;

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


        private DateTime _timeInterval;

        public DateTime TimeInterval
        {
            get => _timeInterval;
            set => SetProperty(ref _timeInterval, value);
        }
        private string _selectedPath;
        public string SelectedPath
        {
            get => _selectedPath;
            set => SetProperty(ref _selectedPath, value);
        }

        public Dictionary<string, int> Tasks { get; set; } = new Dictionary<string, int>();
        #endregion

        #region КОММАНДЫ
        #region выбор файла
        public IRelayCommand ChoosePathCommand { get; }
        private void ChoosePath()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Filter = "All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SelectedPath = openFileDialog.FileName;
            }
        }
        #endregion

        #region добавление задач
        public IRelayCommand AddPathCommand { get; }
        private void AddPath()
        {
            int h = TimeInterval.Hour * 60;
            int m = TimeInterval.Minute;
            int interval = h + m;

            AddTask(SelectedPath, interval);
            SelectedPath = "";
            TimeInterval = new DateTime();
        }
        #endregion

        #region старт задач
        public IRelayCommand StartTasksCommand => new RelayCommand(StartTasks);

        private void StartTasks()
        {
            var processManager = new ProcessManager(Tasks);

            // Запускаем задачу в отдельном потоке чтобы не блокировала интерфейс
            Thread longOperationThread = new Thread(processManager.StartProcesses);
            longOperationThread.Start();

        }
        #endregion

        #endregion

        #region МЕТОДЫ
        public bool AddTask(string path, int interval)
        {
            if (Tasks.ContainsKey(path))
            {
                return false; // Путь уже есть в словаре
            }

            Tasks.Add(path, interval);
            return true;
        }
        #endregion

        //private ObservableCollection<PathData> _pathDataCollection = new ObservableCollection<PathData>();

        //public ObservableCollection<PathData> PathDataCollection
        //{
        //    get { return _pathDataCollection; }
        //    set { SetProperty(ref _pathDataCollection, value); }
        //}




        public MainViewModel()
        {
            #region КОММАНДЫ
            ChoosePathCommand = new RelayCommand(ChoosePath);
            AddPathCommand = new RelayCommand(AddPath);
            #endregion


        }

    }
}
