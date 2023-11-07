using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using Avalonia.Collections;
using Avalonia.Media;
using DynamicData;
using MySqlConnector;
using ReactiveUI;
using UP.Models;

namespace UPNew.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    string connectionString = "Server=10.10.1.24;Database=pro1_23;User Id=user_01;Password=user01pro";
    //string connectionString = "Server=localhost;Database=UP;User Id=root;Password=sharaga228;";


    #region StudentPage

    private AvaloniaList<Clients> _clientsList = new AvaloniaList<Clients>();
    private string _firstName;
    private string _lastName;
    public static readonly string[] languageLevels = new[] { "A1", "A2", "B1", "B2", "C1", "C2" };

    public static readonly Dictionary<string, Func<Clients, string>> filterFields = new()
    {
        { "First name", it => it.FirstName },
        { "Last name", it => it.LastName },
        { "Phone number", it => it.PhoneNumber },
        { "Previous experience", it => it.PreviousExperience },
        { "Language needs", it => it.LanguageNeeds },
        { "Language level", it => it.LanguageLevel },
    };

    public string[] LanguageLevels => languageLevels;
    public Dictionary<string, Func<Clients, string>> FilterFields => filterFields;

    private string _phoneNumber;
    private DateTimeOffset _birthDate;
    private string _previousExperience;
    private Clients _clientsselectedItem;
    private string _languageNeeds;
    private string _languageLevel;
    private KeyValuePair<string, Func<Clients, string>> _selectedColumn;
    private string _searchText;
    private List<Clients> _clientsFull;

    public string FirstName
    {
        get => _firstName;
        set => this.RaiseAndSetIfChanged(ref _firstName, value);
    }

    public string LastName
    {
        get => _lastName;
        set => this.RaiseAndSetIfChanged(ref _lastName, value);
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        set => this.RaiseAndSetIfChanged(ref _phoneNumber, value);
    }

    public DateTimeOffset BirthDate
    {
        get => _birthDate;
        set => this.RaiseAndSetIfChanged(ref _birthDate, value);
    }

    public string PreviousExperience
    {
        get => _previousExperience;
        set => this.RaiseAndSetIfChanged(ref _previousExperience, value);
    }

    public string LanguageLevel
    {
        get => _languageLevel;
        set => this.RaiseAndSetIfChanged(ref _languageLevel, value);
    }

    public string LanguageNeeds
    {
        get => _languageNeeds;
        set => this.RaiseAndSetIfChanged(ref _languageNeeds, value);
    }

    public AvaloniaList<Clients> ClientsList
    {
        get => _clientsList;
        set => this.RaiseAndSetIfChanged(ref _clientsList, value);
    }

    public Clients ClientsSelectedItem
    {
        get => _clientsselectedItem;
        set => this.RaiseAndSetIfChanged(ref _clientsselectedItem, value);
    }

    public KeyValuePair<string, Func<Clients, string>> SelectedColumn
    {
        get => _selectedColumn;
        set => this.RaiseAndSetIfChanged(ref _selectedColumn, value);
    }

    public string SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public MainWindowViewModel()
    {
        RefreshDataView();
        this.WhenAnyValue(x => x.SelectedColumn)
            .DistinctUntilChanged()
            .Subscribe(x => SearchBySelectedColumn());
    }

    public List<Clients> GetClientsDataFromDatabase()
    {
        List<Clients> data = new List<Clients>();

        MySqlConnection connection = new MySqlConnection(connectionString);

        try
        {
            connection.Open();

            string query = "SELECT * FROM Clients";
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Clients item = new Clients
                {
                    Id = reader.GetInt32("Id"),
                    FirstName = reader.GetString("FirstName"),
                    LastName = reader.GetString("LastName"),
                    PhoneNumber = reader.GetString("PhoneNumber"),
                    BirthDate = reader.GetDateTimeOffset("BirthDate"),
                    PreviousExperience = reader.GetString("PreviousExperience"),
                    LanguageNeeds = reader.GetString("LanguageNeeds"),
                    LanguageLevel = reader.GetString("LanguageLevel")
                };
                data.Add(item);
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine("Ошибка подключения к базе данных: " + ex.Message);
        }
        finally
        {
            connection.Close();
        }

        return data;
    }

    public void PopupDelete()
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            int Id = ClientsSelectedItem.Id;

            string deleteQuery = "SET FOREIGN_KEY_CHECKS=0;" + "DELETE FROM Clients WHERE Id = @Id LIMIT 1";

            using (MySqlCommand cmd = new MySqlCommand(deleteQuery, connection))
            {
                cmd.Parameters.AddWithValue("@Id", Id);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    RefreshDataView();
                    // Успешно удалено из базы данных
                    // Можете добавить уведомление пользователю
                }
                else
                {
                    // Обработка ошибки при удалении
                    // Можете добавить уведомление о неудачном удалении
                }
            }
        }
    }

    public void RefreshDataView()
    {
        if (CourseSelectedItem is not null)
        {
            _groupsFull = GetGroupById(CourseSelectedItem.Id);
            GroupsList = new(_groupsFull);
        }

        _courseFull = GetCoursesDataFromDatabase();
        CoursesList = new(_courseFull);
        _clientsFull = GetClientsDataFromDatabase();
        ClientsList = new(_clientsFull);
    }

    public Clients GetClientFromDatabase(int Id)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            string selectQuery = "SELECT * FROM Clients WHERE Id = @Id";

            using (MySqlCommand cmd = new MySqlCommand(selectQuery, connection))
            {
                cmd.Parameters.AddWithValue("@Id", Id);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Clients client = new Clients
                        {
                            Id = reader.GetInt32("Id"),
                            FirstName = reader.GetString("FirstName"),
                            LastName = reader.GetString("LastName"),
                            PhoneNumber = reader.GetString("PhoneNumber"),
                            BirthDate = reader.GetDateTimeOffset("BirthDate"),
                            LanguageLevel = reader.GetString("LanguageLevel"),
                            LanguageNeeds = reader.GetString("LanguageNeeds"),
                            PreviousExperience = reader.GetString("PreviousExperience")
                        };

                        return client;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
    }

    public Clients GetUpdatedClientData()
    {
        string updatedFirstName = FirstName;
        string updatedLastName = LastName;
        string updatedPhoneNumber = PhoneNumber;
        DateTimeOffset updatedBirthDate = BirthDate;
        string updatedLanguageLevel = LanguageLevel;
        string updatedLanguageNeeds = LanguageNeeds;
        string updatedPreviousExperience = PreviousExperience;

        Clients updatedClient = new Clients
        {
            FirstName = updatedFirstName,
            LastName = updatedLastName,
            PhoneNumber = updatedPhoneNumber,
            BirthDate = updatedBirthDate,
            LanguageLevel = updatedLanguageLevel,
            LanguageNeeds = updatedLanguageNeeds,
            PreviousExperience = updatedPreviousExperience
        };

        return updatedClient;
    }

    public void UpdateClientInDatabase(Clients updatedClient)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            string updateQuery =
                "UPDATE Clients SET FirstName = @FirstName, LastName = @LastName, PhoneNumber = @PhoneNumber, BirthDate = @BirthDate, LanguageLevel = @LanguageLevel, LanguageNeeds = @LanguageNeeds, PreviousExperience = @PreviousExperience WHERE Id = @Id";

            using (MySqlCommand cmd = new MySqlCommand(updateQuery, connection))
            {
                cmd.Parameters.AddWithValue("@Id", updatedClient.Id);
                cmd.Parameters.AddWithValue("@FirstName", updatedClient.FirstName);
                cmd.Parameters.AddWithValue("@LastName", updatedClient.LastName);
                cmd.Parameters.AddWithValue("@PhoneNumber", updatedClient.PhoneNumber);
                cmd.Parameters.AddWithValue("@BirthDate", updatedClient.BirthDate);
                cmd.Parameters.AddWithValue("@LanguageLevel", updatedClient.LanguageLevel);
                cmd.Parameters.AddWithValue("@LanguageNeeds", updatedClient.LanguageNeeds);
                cmd.Parameters.AddWithValue("@PreviousExperience", updatedClient.PreviousExperience);

                cmd.ExecuteNonQuery();
            }
        }
    }

    public int ProfileCreate()
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            string insertQuery = "INSERT INTO Clients (FirstName, LastName, PhoneNumber, BirthDate, " +
                                 "PreviousExperience, LanguageLevel, LanguageNeeds) " +
                                 "VALUES (@FirstName, @LastName, @PhoneNumber, @BirthDate, " +
                                 "@PreviousExperience, @LanguageLevel, @LanguageNeeds)";
            using (MySqlCommand cmd = new MySqlCommand(insertQuery, connection))
            {
                cmd.Parameters.AddWithValue("@FirstName", FirstName);
                cmd.Parameters.AddWithValue("@LastName", LastName);
                cmd.Parameters.AddWithValue("@PhoneNumber", PhoneNumber);
                cmd.Parameters.AddWithValue("@BirthDate", BirthDate);
                cmd.Parameters.AddWithValue("@PreviousExperience", PreviousExperience);
                cmd.Parameters.AddWithValue("@LanguageLevel", LanguageLevel);
                cmd.Parameters.AddWithValue("@LanguageNeeds", LanguageNeeds);
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected;
            }
        }
    }

    public void ClearBoxes()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        PhoneNumber = string.Empty;
        BirthDate = DateTimeOffset.Now;
        LanguageLevel = LanguageLevels[0];
        LanguageNeeds = string.Empty;
        PreviousExperience = string.Empty;
    }

    private void SearchBySelectedColumn()
    {
        string searchText = SearchText;
        if (searchText != null)
        {
            List<Clients> searchResults = _clientsFull
                .Where(client =>
                    SelectedColumn.Value(client).Contains(searchText, StringComparison.InvariantCultureIgnoreCase))
                .ToList();
            ClientsList = new AvaloniaList<Clients>(searchResults);
        }
        else
        {
            Console.WriteLine("Error");
        }
    }

    #endregion


    #region CoursePage

    private AvaloniaList<Course> _coursesList = new AvaloniaList<Course>();
    private Course _courseselectedItem;
    private List<Course> _courseFull;

    public AvaloniaList<Course> CoursesList
    {
        get => _coursesList;
        set => this.RaiseAndSetIfChanged(ref _coursesList, value);
    }

    public Course CourseSelectedItem
    {
        get => _courseselectedItem;
        set => this.RaiseAndSetIfChanged(ref _courseselectedItem, value);
    }

    public List<Course> GetCoursesDataFromDatabase()
    {
        List<Course> data = new List<Course>();

        MySqlConnection connection = new MySqlConnection(connectionString);

        try
        {
            connection.Open();

            string query = "SELECT * FROM Courses";
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Course item = new Course
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    Duration = reader.GetInt32("Duration"),
                    DifficultyLevel = reader.GetString("DifficultyLevel"),
                    Price = reader.GetInt32("Price")
                };
                data.Add(item);
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine("Ошибка подключения к базе данных: " + ex.Message);
        }
        finally
        {
            connection.Close();
        }

        return data;
    }

    public List<Groups> GetGroupById(int courseId)
    {
        List<Groups> groupsList = new List<Groups>();

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            string countquery = "select COUNT(*) from ClientGroup where `Group` = @Group";
            string groupsWhereQuery = """
                            select G.*,
                                  T.FirstName AS TeacherName,
                                  T.LastName AS TeacherSurname,
                                  COUNT(CG.Client) AS CurrentStudents
                           from ClientGroup CG
                           join pro1_23.`Groups` G on CG.`Group` = G.Id
                           join pro1_23.Teacher T on G.Teacher = T.Id
                           group by CG.`Group`
                           """;

            MySqlCommand cmd = new MySqlCommand(groupsWhereQuery, connection);
            cmd.Parameters.AddWithValue("@Course", courseId);

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Groups groups = new Groups
                    {
                        Id = reader.GetInt32("Id"),
                        Name = reader.GetString("Name"),
                        StartDate = reader.GetDateTimeOffset("StartDate"),
                        EndDate = reader.GetDateTimeOffset("EndDate"),
                        Course = reader.GetInt32("Course"),
                        Teacher = reader.GetInt32("Teacher"),
                        CurrentStudents = reader.GetInt32("CurrentStudents"),
                        MaxStudents = reader.GetInt32("MaxStudents"),
                        TeacherName = reader.GetString("TeacherName"),
                        TeacherSurname = reader.GetString("TeacherSurname")
                    };
                    groupsList.Add(groups);
                }
            }
        }

        return groupsList;
    }

    #endregion


    private AvaloniaList<Groups> _groupsList = new AvaloniaList<Groups>();


    private List<Groups> _groupsFull;
    private Groups _groupsselectedItem;

    public Groups GroupSelectedItem
    {
        get => _groupsselectedItem;
        set => this.RaiseAndSetIfChanged(ref _groupsselectedItem, value);
    }

    public AvaloniaList<Groups> GroupsList
    {
        get => _groupsList;
        set => this.RaiseAndSetIfChanged(ref _groupsList, value);
    }
}