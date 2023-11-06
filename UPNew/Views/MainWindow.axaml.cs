using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MySqlConnector;
using UP.Models;
using UPNew.ViewModels;
using Brushes = Avalonia.Media.Brushes;
using FontFamily = Avalonia.Media.FontFamily;

namespace UPNew.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
    
    public MainWindowViewModel ViewModel => (DataContext as MainWindowViewModel)!;

    private void ProfileCreate_OnClick(object? sender, RoutedEventArgs e)
    {
        var rowsAffected = ViewModel.ProfileCreate();
        if (rowsAffected > 0)
        {
            StudentList.IsVisible = true;
            RegStudentPanel.IsVisible = false;
            NewStudent.IsVisible = true;
            SearchPanel.IsVisible = true;
            RefreshDataView();
            // Успешно добавлено в базу данных
            // Можете добавить дополнительную логику, например, отображение уведомления об успешной регистрации
        }
        else
        {
            // Обработка ошибки при вставке данных
            // Можете добавить обработку ошибки, например, отображение сообщения об ошибке
        }
    }

    private void NewStudent_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.ClearBoxes();
        StudentList.IsVisible = false;
        RegStudentPanel.IsVisible = true;
        NewStudent.IsVisible = false;
        ProfileCreate.IsVisible = true;
        ProfileChange.IsVisible = false;
        SearchPanel.IsVisible = false;
    }

    private void PopupDelete_OnClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.PopupDelete();
    }

    private void ProfileCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        StudentList.IsVisible = true;
        RegStudentPanel.IsVisible = false;
        NewStudent.IsVisible = true;
        SearchPanel.IsVisible = true;
    }

    private void PopupEdit_OnClick(object? sender, RoutedEventArgs e)
    {
        int selectedClientId = ViewModel.ClientsSelectedItem.Id;

        Clients client = ViewModel.GetClientFromDatabase(selectedClientId);

        FillStackPanelWithClientData(client);

        RegStudentPanel.IsVisible = true;
        ProfileChange.IsVisible = true;
        StudentList.IsVisible = false;
        NewStudent.IsVisible = false;
        ProfileCreate.IsVisible = false;
        SearchPanel.IsVisible = false;
    }

    private void FillStackPanelWithClientData(Clients client)
    {
        FirstNameTextBox.Text = client.FirstName;
        LastNameTextBox.Text = client.LastName;
        PhoneNumberTextBox.Text = client.PhoneNumber;
        BirthDateDatePicker.SelectedDate = client.BirthDate;
        LanguageLevelComboBox.SelectedItem = client.LanguageLevel;
        LanguageNeedsTextBox.Text = client.LanguageNeeds;
        PreviousExperienceTextBox.Text = client.PreviousExperience;
    }

    private void ProfiveChange_OnClick(object? sender, RoutedEventArgs e)
    {
        Clients updatedClient = ViewModel.GetUpdatedClientData();

        ViewModel.UpdateClientInDatabase(updatedClient);

        StudentList.IsVisible = true;
        RegStudentPanel.IsVisible = false;
        NewStudent.IsVisible = false;
        RefreshDataView();
    }

    private void RefreshDataView()
    {
        ViewModel.RefreshDataView();
        NewStudent.IsVisible = true;
    }
}