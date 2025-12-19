#nullable enable
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using MyInternetChecker.Mvvm;

namespace MyInternetChecker.ViewModels;

/// <summary>ViewModel окна редактирования списка хостов</summary>
public sealed class HostsSettingsViewModel : ViewModelBase
{
    private readonly ObservableCollection<string> _AllFileLines = new();

    private string _NewHost = string.Empty;
    private bool? _DialogResult;

    public HostsSettingsViewModel()
    {
        AddHostCommand = new RelayCommand(AddNewHost);
        DeleteHostCommand = new RelayCommand<string>(DeleteHost);
        SaveCommand = new RelayCommand(SaveAndClose);
        CancelCommand = new RelayCommand(CancelAndClose);

        Load();
    }

    /// <summary>Отображаемые хосты</summary>
    public ObservableCollection<string> Hosts { get; } = new();

    /// <summary>Текст нового хоста</summary>
    public string NewHost { get => _NewHost; set => Set(ref _NewHost, value); }

    /// <summary>Результат диалогового окна</summary>
    public bool? DialogResult { get => _DialogResult; set => Set(ref _DialogResult, value); }

    public RelayCommand AddHostCommand { get; }
    public RelayCommand<string> DeleteHostCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public event EventHandler<string>? ErrorOccurred;

    public void Load()
    {
        Hosts.Clear();
        _AllFileLines.Clear();

        try
        {
            if (File.Exists(Config.SettingsFilePath))
            {
                var all_lines = File.ReadAllLines(Config.SettingsFilePath);
                foreach (var line in all_lines)
                    _AllFileLines.Add(line);

                var hosts = all_lines
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrEmpty(line) && !line.StartsWith('#'))
                    .ToArray();

                foreach (var host in hosts)
                    Hosts.Add(host);
            }
            else
            {
                var defaults = new[] { "ya.ru", "google.com" };
                foreach (var host in defaults)
                {
                    Hosts.Add(host);
                    _AllFileLines.Add(host);
                }
            }
        }
        catch (Exception ex)
        {
            Hosts.Clear();
            _AllFileLines.Clear();
            ErrorOccurred?.Invoke(this, $"Ошибка загрузки настроек: {ex.Message}");
        }
    }

    private void Save()
    {
        try
        {
            var directory_path = Path.GetDirectoryName(Config.SettingsFilePath);
            if (!Directory.Exists(directory_path))
                Directory.CreateDirectory(directory_path!);

            File.WriteAllLines(Config.SettingsFilePath, _AllFileLines);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Ошибка сохранения: {ex.Message}");
        }
    }

    private void SaveAndClose()
    {
        Save();
        DialogResult = true;
    }

    private void CancelAndClose() => DialogResult = false;

    private void AddNewHost()
    {
        var new_host = (NewHost ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(new_host))
        {
            ErrorOccurred?.Invoke(this, "Введите имя хоста");
            return;
        }

        if (Hosts.Contains(new_host))
        {
            ErrorOccurred?.Invoke(this, "Этот хост уже есть в списке");
            return;
        }

        Hosts.Add(new_host);
        _AllFileLines.Add(new_host);
        NewHost = string.Empty;
    }

    private void DeleteHost(string? HostToDelete)
    {
        if (string.IsNullOrWhiteSpace(HostToDelete))
            return;

        Hosts.Remove(HostToDelete);

        var line_to_delete = _AllFileLines.FirstOrDefault(line => line.Trim() == HostToDelete);
        if (line_to_delete is not null)
            _AllFileLines.Remove(line_to_delete);
    }
}
