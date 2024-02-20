using UP.Models;
using UPNew.ViewModels;

namespace UPTest;

public class UnitTest1
{
    [Fact]
    public void GetClientsDataFromDatabase_Returns_Data()
    {
        var viewModel = new MainWindowViewModel();

        var clientsData = viewModel.GetClientsDataFromDatabase();

        Assert.NotNull(clientsData);
        Assert.NotEmpty(clientsData);
    }

    [Fact]
    public void GetClientFromDatabase_Returns_Null_For_Nonexistent_Id()
    {
        var viewModel = new MainWindowViewModel();

        var nonExistentClient = viewModel.GetClientFromDatabase(-1);

        Assert.Null(nonExistentClient);
    }

    [Fact]
    public void UpdateClientInDatabase_Updates_Client()
    {
        var viewModel = new MainWindowViewModel();
        var client = new Clients { Id = 1, FirstName = "John", LastName = "Doe", PhoneNumber = "1234567890" };

        viewModel.UpdateClientInDatabase(client);
        var updatedClient = viewModel.GetClientFromDatabase(client.Id);

        Assert.NotNull(updatedClient);
        Assert.Equal(client.FirstName, updatedClient.FirstName);
        Assert.Equal(client.LastName, updatedClient.LastName);
        Assert.Equal(client.PhoneNumber, updatedClient.PhoneNumber);
    }

    [Fact]
    public void ProfileCreate_Creates_Client()
    {
        var viewModel = new MainWindowViewModel();
        var initialClientsCount = viewModel.GetClientsDataFromDatabase().Count;

        viewModel.FirstName = "Test";
        viewModel.LastName = "User";
        viewModel.PhoneNumber = "0987654321";
        viewModel.BirthDate = DateTimeOffset.Now;
        viewModel.PreviousExperience = "Some experience";
        viewModel.LanguageLevel = "B1";
        viewModel.LanguageNeeds = "English";
        viewModel.ProfileCreate();
        var clientsData = viewModel.GetClientsDataFromDatabase();
        var newClientsCount = clientsData.Count;

        Assert.Equal(initialClientsCount + 1, newClientsCount);
        var createdClient = clientsData.Find(c => c.FirstName == "Test" && c.LastName == "User");
        Assert.NotNull(createdClient);
    }

    [Fact]
    public void SearchBySelectedColumn_Filters_Clients()
    {
        var viewModel = new MainWindowViewModel();
        viewModel.SearchText = "John";
        viewModel.SelectedColumn =
            new System.Collections.Generic.KeyValuePair<string, Func<Clients, string>>("First name", c => c.FirstName);

        viewModel.SearchBySelectedColumn();
        var filteredClients = viewModel.ClientsList;

        Assert.NotNull(filteredClients);
        Assert.NotEmpty(filteredClients);
        Assert.All(filteredClients, c => Assert.Contains("John", c.FirstName, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void RefreshDataView_Retrieves_Data()
    {
        var viewModel = new MainWindowViewModel();
        var initialClientsCount = viewModel.ClientsList.Count;
        var initialCoursesCount = viewModel.CoursesList.Count;

        viewModel.RefreshDataView();
        var refreshedClientsCount = viewModel.ClientsList.Count;
        var refreshedCoursesCount = viewModel.CoursesList.Count;

        Assert.True(refreshedClientsCount > initialClientsCount);
        Assert.True(refreshedCoursesCount > initialCoursesCount);
    }

    [Fact]
    public void PopupDelete_Removes_Client()
    {
        var viewModel = new MainWindowViewModel();
        var initialClientsCount = viewModel.GetClientsDataFromDatabase().Count;
        var clientToDelete = viewModel.GetClientsDataFromDatabase().FirstOrDefault();

        viewModel.ClientsSelectedItem = clientToDelete;
        viewModel.PopupDelete();
        var clientsAfterDeleteCount = viewModel.GetClientsDataFromDatabase().Count;

        Assert.True(initialClientsCount > clientsAfterDeleteCount);
    }

    [Fact]
    public void ClearBoxes_Resets_Properties()
    {
        var viewModel = new MainWindowViewModel();
        viewModel.FirstName = "John";
        viewModel.LastName = "Doe";
        viewModel.PhoneNumber = "1234567890";
        viewModel.BirthDate = DateTimeOffset.Now;
        viewModel.PreviousExperience = "Some experience";
        viewModel.LanguageLevel = "B1";
        viewModel.LanguageNeeds = "English";

        viewModel.ClearBoxes();

        Assert.Equal(string.Empty, viewModel.FirstName);
        Assert.Equal(string.Empty, viewModel.LastName);
        Assert.Equal(string.Empty, viewModel.PhoneNumber);
        Assert.Equal(DateTimeOffset.Now, viewModel.BirthDate);
        Assert.Equal(MainWindowViewModel.languageLevels[0], viewModel.LanguageLevel);
        Assert.Equal(string.Empty, viewModel.LanguageNeeds);
        Assert.Equal(string.Empty, viewModel.PreviousExperience);
    }

    [Fact]
    public void GetCoursesDataFromDatabase_Returns_Data()
    {
        var viewModel = new MainWindowViewModel();

        var coursesData = viewModel.GetCoursesDataFromDatabase();

        Assert.NotNull(coursesData);
        Assert.NotEmpty(coursesData);
    }

    [Fact]
    public void GetGroupById_Returns_Groups_For_Valid_CourseId()
    {
        var viewModel = new MainWindowViewModel();
        var courseId = 1;

        var groups = viewModel.GetGroupById(courseId);

        Assert.NotNull(groups);
        Assert.NotEmpty(groups);
    }
}