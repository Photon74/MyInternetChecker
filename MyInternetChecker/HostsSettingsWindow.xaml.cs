using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyInternetChecker;

public partial class HostsSettingsWindow : Window
{
    private List<string> _allFileLines = new List<string>();
    private ObservableCollection<string> _hosts;

    public HostsSettingsWindow()
    {
        InitializeComponent();
        LoadHosts();
    }

    private void LoadHosts()
    {
        try
        {
            _allFileLines.Clear();

            if (File.Exists(Config.SettingsFilePath))
            {
                _allFileLines = File.ReadAllLines(Config.SettingsFilePath).ToList();


                var lines = _allFileLines
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrEmpty(line) && !line.StartsWith("#"))
                    .ToList();

                _hosts = new ObservableCollection<string>(lines);
            }
            else
            {
                _hosts = new ObservableCollection<string> { "ya.ru", "google.com" };
                _allFileLines = new List<string> { "ya.ru", "google.com" };
            }

            HostsListView.ItemsSource = _hosts;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки настроек: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
            _hosts = new ObservableCollection<string>();
            _allFileLines = new List<string>();
            HostsListView.ItemsSource = _hosts;
        }

        NewHostTextBox.Focus();
    }

    private void SaveHosts()
    {
        try
        {
            // Убедимся, что папка существует (это у нас уже есть)
            string directory = Path.GetDirectoryName(Config.SettingsFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // СОХРАНЯЕМ ПОЛНЫЙ СПИСОК ВСЕХ СТРОК, ВКЛЮЧАЯ КОММЕНТАРИИ
            File.WriteAllLines(Config.SettingsFilePath, _allFileLines);

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AddHostButton_Click(object sender, RoutedEventArgs e)
    {
        AddNewHost();
    }

    private void NewHostTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            AddNewHost();
        }
    }

    private void AddNewHost()
    {
        var newHost = NewHostTextBox.Text.Trim();
        if (string.IsNullOrEmpty(newHost))
        {
            MessageBox.Show("Введите имя хоста", "Внимание",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (_hosts.Contains(newHost))
        {
            MessageBox.Show("Этот хост уже есть в списке", "Внимание",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _hosts.Add(newHost);
        _allFileLines.Add(newHost);
        NewHostTextBox.Clear();
        NewHostTextBox.Focus();
    }

    private void DeleteHostButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string hostToDelete)
        {
            // 1. Удаляем хост из списка для отображения
            _hosts.Remove(hostToDelete);

            // 2. Находим и удаляем строку с этим хостом из общего списка
            // Ищем строку, которая после очистки от пробелов совпадает с именем хоста
            var lineToDelete = _allFileLines.FirstOrDefault(line => line.Trim() == hostToDelete);
            if (lineToDelete != null)
            {
                _allFileLines.Remove(lineToDelete);
            }
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        SaveHosts();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    // Позволяет перетаскивать окно за любую область
    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }
}