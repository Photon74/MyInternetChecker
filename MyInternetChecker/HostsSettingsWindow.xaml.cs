using MyInternetChecker.Config;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyInternetChecker;

/// <summary>Окно для редактирования списка хостов</summary>
public partial class HostsSettingsWindow
{
    private ObservableCollection<string> _hosts;

    /// <summary>Инициализирует окно и загружает текущие хосты</summary>
    public HostsSettingsWindow()
    {
        InitializeComponent();
        LoadHosts();
    }

    /// <summary>Загружает хосты из файла настроек</summary>
    private void LoadHosts()
    {
        try
        {
            _hosts = new ObservableCollection<string>(ConfigManager.Hosts);
            HostsListView.ItemsSource = _hosts;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки настроек: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
            _hosts = new ObservableCollection<string>();
            HostsListView.ItemsSource = _hosts;
        }

        NewHostTextBox.Focus();
    }

    /// <summary>Сохраняет хосты в файл настроек</summary>
    private void SaveHosts()
    {
        try
        {
            ConfigManager.SaveHosts(_hosts.ToArray());
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
        NewHostTextBox.Clear();
        NewHostTextBox.Focus();
    }

    private void DeleteHostButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string hostToDelete)
        {
            _hosts.Remove(hostToDelete);
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
