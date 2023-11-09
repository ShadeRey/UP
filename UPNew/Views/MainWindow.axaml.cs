using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Logging;
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

    private void CourseList_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        GroupList.IsVisible = true;
        GroupBack.IsVisible = true;
        CourseList.IsVisible = false;
        ClientsList.IsVisible = false;
        StudentsBack.IsVisible = false;
        NewStudentToGroup.IsVisible = false;
        ViewModel.CourseSelectedItem = CourseList.SelectedItem as Course;
        ViewModel.RefreshDataView();
    }

    private void Back_OnClick(object? sender, RoutedEventArgs e)
    {
        CourseList.IsVisible = true;
        GroupList.IsVisible = false;
        GroupBack.IsVisible = false;
        ClientsList.IsVisible = false;
        StudentsBack.IsVisible = false;
        NewStudentToGroup.IsVisible = false;
    }

    private void GroupList_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        StudentsBack.IsVisible = true;
        NewStudentToGroup.IsVisible = true;
        ClientsList.IsVisible = true;
        GroupList.IsVisible = false;
        CourseList.IsVisible = false;
        GroupBack.IsVisible = false;
        ViewModel.GroupSelectedItem = GroupList.SelectedItem as Groups;
        ViewModel.RefreshDataView();
    }

    private void StudentsBack_OnClick(object? sender, RoutedEventArgs e)
    {
        GroupList.IsVisible = true;
        GroupBack.IsVisible = true;
        StudentsBack.IsVisible = false;
        NewStudentToGroup.IsVisible = false;
        ClientsList.IsVisible = false;
        CourseList.IsVisible = false;
        
        GroupList.SelectedItem = null;
    }

    private void NewStudentToGroup_OnClick(object? sender, RoutedEventArgs e)
    {
        StudentsBack.IsVisible = false;
        NewStudentToGroup.IsVisible = false;
        ClientsList.IsVisible = false;
        GroupList.IsVisible = false;
        CourseList.IsVisible = false;
        GroupBack.IsVisible = false;
        StudentWithouGroupList.IsVisible = true;
        StudentsWithoutGroupsBack.IsVisible = true;
        ViewModel.GroupSelectedItem = GroupList.SelectedItem as Groups;
        ViewModel.RefreshDataView();
    }

    private void StudentWithouGroupList_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        StudentsBack.IsVisible = false;
        NewStudentToGroup.IsVisible = false;
        ClientsList.IsVisible = false;
        GroupList.IsVisible = true;
        CourseList.IsVisible = false;
        GroupBack.IsVisible = true;
        StudentWithouGroupList.IsVisible = false;
        StudentsWithoutGroupsBack.IsVisible = false;
        ViewModel.StudentsWithoutGroupSelectedItem = StudentWithouGroupList.SelectedItem as Clients;
        RefreshDataView();
    }

    private void StudentsWithoutGroupsBack_OnClick(object? sender, RoutedEventArgs e)
    {
        StudentsBack.IsVisible = false;
        NewStudentToGroup.IsVisible = false;
        ClientsList.IsVisible = false;
        GroupList.IsVisible = true;
        CourseList.IsVisible = false;
        GroupBack.IsVisible = true;
        StudentWithouGroupList.IsVisible = false;
        StudentsWithoutGroupsBack.IsVisible = false;
    }

    private void FinancialList_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        
        ViewModel.FinancialSelectedItem = (FinancialList.SelectedItem as FinancialOperations)!;
        RefreshDataView();
    }
}