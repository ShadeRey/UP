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

        this.WhenAnyValue(x => x.StudentsWithoutGroupSelectedItem)
            .DistinctUntilChanged()
            .Subscribe(x =>
            {
                if (x is null) return;
                AddClientToGroup();
            });
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
                }
                else
                {
                    // Обработка ошибки при удалении
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

        if (GroupSelectedItem is not null)
        {
            _studentsFull = GetStudentById(GroupSelectedItem.Id);
            StudentsList = new(_studentsFull);
        }

        _courseFull = GetCoursesDataFromDatabase();
        CoursesList = new(_courseFull);
        _clientsFull = GetClientsDataFromDatabase();
        ClientsList = new(_clientsFull);
        _studentsWitoutGroupsFull = GetClientsWithoutGroup();
        StudentWithoutGroupsList = new(_studentsWitoutGroupsFull);
        UpdatePaymentState();
        _financialOperationsFull = GetFinancialDataFromDatabase();
        FinancialList = new(_financialOperationsFull);
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
                if (rowsAffected > 0)
                {
                    string getLastInsertIdQuery = "SELECT LAST_INSERT_ID()";
                    cmd.CommandText = getLastInsertIdQuery;
                    int clientId = Convert.ToInt32(cmd.ExecuteScalar());

                    string insertClientGroupQuery = "INSERT INTO ClientGroup (Client) VALUES (@Client)";
                    cmd.CommandText = insertClientGroupQuery;
                    cmd.Parameters.AddWithValue("@Client", clientId);
                    rowsAffected = cmd.ExecuteNonQuery();
                }

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

            string groupsWhereQuery = """
                                       select G.*,
                                             T.FirstName AS TeacherName,
                                             T.LastName AS TeacherSurname,
                                             COUNT(CG.Client) AS CurrentStudents
                                      from ClientGroup CG
                                      join `Groups` G on CG.`Group` = G.Id
                                      join Teacher T on G.Teacher = T.Id
                                      WHERE G.Course = @Course
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


    private AvaloniaList<Clients> _studentsList = new AvaloniaList<Clients>();
    private List<Clients> _studentsFull;

    public AvaloniaList<Clients> StudentsList
    {
        get => _studentsList;
        set => this.RaiseAndSetIfChanged(ref _studentsList, value);
    }

    public List<Clients> GetStudentById(int groupId)
    {
        List<Clients> studentsList = new List<Clients>();

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            string groupsWhereQuery = """
                                       SELECT Clients.* FROM Clients
                                      INNER JOIN ClientGroup ON Clients.Id = ClientGroup.Client
                                      WHERE ClientGroup.Group = @Groups
                                      """;

            MySqlCommand cmd = new MySqlCommand(groupsWhereQuery, connection);
            cmd.Parameters.AddWithValue("@Groups", groupId);

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Clients clients = new Clients
                    {
                        Id = reader.GetInt32("Id"),
                        FirstName = reader.GetString("FirstName"),
                        LastName = reader.GetString("LastName"),
                        PhoneNumber = reader.GetString("PhoneNumber"),
                        LanguageLevel = reader.GetString("LanguageLevel"),
                        BirthDate = reader.GetDateTimeOffset("BirthDate"),
                        PreviousExperience = reader.GetString("PreviousExperience"),
                        LanguageNeeds = reader.GetString("LanguageNeeds")
                    };
                    studentsList.Add(clients);
                }
            }
        }

        return studentsList;
    }

    private List<Clients> _studentsWitoutGroupsFull;
    private Clients? _studentsWithoutGroupSelectedItem;

    public Clients? StudentsWithoutGroupSelectedItem
    {
        get => _studentsWithoutGroupSelectedItem;
        set => this.RaiseAndSetIfChanged(ref _studentsWithoutGroupSelectedItem, value);
    }

    private AvaloniaList<Clients> _clientsWithoutGroupsList = new AvaloniaList<Clients>();

    public AvaloniaList<Clients> StudentWithoutGroupsList
    {
        get => _clientsWithoutGroupsList;
        set => this.RaiseAndSetIfChanged(ref _clientsWithoutGroupsList, value);
    }

    public List<Clients> GetClientsWithoutGroup()
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            string query = "SELECT Clients.* FROM Clients " +
                           "LEFT JOIN ClientGroup ON Clients.Id = ClientGroup.Client " +
                           "WHERE ClientGroup.Group IS NULL";

            using (MySqlCommand cmd = new MySqlCommand(query, connection))
            {
                List<Clients> clients = new List<Clients>();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Clients client = new Clients
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            FirstName = reader["FirstName"].ToString(),
                            LastName = reader["LastName"].ToString(),
                            PhoneNumber = reader["PhoneNumber"].ToString(),
                            BirthDate = ParseNullableDateTimeOffset(reader["BirthDate"]),
                            PreviousExperience = reader["PreviousExperience"].ToString(),
                            LanguageNeeds = reader["LanguageNeeds"].ToString(),
                            LanguageLevel = reader["LanguageLevel"].ToString()
                        };
                        clients.Add(client);
                    }
                }

                return clients;
            }
        }
    }

    private DateTimeOffset? ParseNullableDateTimeOffset(object value)
    {
        if (value == DBNull.Value || value == null)
        {
            return null;
        }

        if (DateTimeOffset.TryParse(value.ToString(), out DateTimeOffset result))
        {
            return result;
        }

        return null;
    }

    public void AddClientToGroup()
    {
        if (StudentsWithoutGroupSelectedItem is null)
        {
            return;
        }

        int clid = StudentsWithoutGroupSelectedItem.Id;
        int groupid = GroupSelectedItem.Id;
        using MySqlConnection connection = new MySqlConnection(connectionString);
        connection.Open();

        string insertQuery = """
                             UPDATE ClientGroup
                             SET ClientGroup.`Group` = @Group
                             WHERE ClientGroup.Client = @Client;
                             select C.Price
                             from `Groups` g
                                      join pro1_23.Courses C on g.Course = C.Id
                             where g.Id = @Group
                             group by g.Id;
                             """;
        using var cmd = new MySqlCommand(insertQuery, connection);
        cmd.Parameters.AddWithValue("@Client", clid);
        cmd.Parameters.AddWithValue("@Group", groupid);
        var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            int? price = null;
            try
            {
                price = reader.GetInt32("Price");

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (price.HasValue)
            {
                using MySqlConnection connection2 = new MySqlConnection(connectionString); connection2.Open();
                string insQuery =
                    "INSERT INTO FinancialOperations(Client, Sum, OperationDate, PaymentState) VALUES (@Client, @Sum, @OperationDate, @PaymentState)";
                using MySqlCommand ccc = new MySqlCommand(insQuery, connection2);
                ccc.Parameters.AddWithValue("@Client", clid);
                ccc.Parameters.AddWithValue("Sum", price.Value);
                ccc.Parameters.AddWithValue("OperationDate", DateTimeOffset.Now);
                ccc.Parameters.AddWithValue("PaymentState", false); ccc.ExecuteNonQuery(); _financialOperationsFull = GetFinancialDataFromDatabase(); FinancialList = new(_financialOperationsFull);
                // FinancialList.Add(new FinancialOperations
                // {
                //     
                //     Client = StudentsWithoutGroupSelectedItem.Id,
                //     
                //     Sum = price.Value,
                //     OperationDate = DateTimeOffset.Now,
                //     PaymentState = false
                // });
            }
            else
            {
                throw new NullReferenceException("Price = null");
            } 
        } 
    } 
    private AvaloniaList<FinancialOperations> _financialOperationsList = new AvaloniaList<FinancialOperations>(); private FinancialOperations _financialselectedItem; private List<FinancialOperations> _financialOperationsFull;

    public AvaloniaList<FinancialOperations> FinancialList
    { 
        get => _financialOperationsList; set => this.RaiseAndSetIfChanged(ref _financialOperationsList, value);
    }

    public FinancialOperations FinancialSelectedItem 
    { 
        get => _financialselectedItem; set => this.RaiseAndSetIfChanged(ref _financialselectedItem, value);
    }

    public List<FinancialOperations> GetFinancialDataFromDatabase()
    {
        List<FinancialOperations> data = new List<FinancialOperations>();

        MySqlConnection connection = new MySqlConnection(connectionString);

        try
        {
            connection.Open();

            string query = """
                           SELECT F.*,
                                  C.FirstName AS ClientName,
                                  C.LastName AS ClientSurname,
                                  C2.Price
                           FROM FinancialOperations F
                                    JOIN Clients C ON F.Client = C.Id
                                    JOIN ClientGroup CG on C.Id = CG.Client
                                    JOIN `Groups` G on CG.`Group` = G.Id
                                    JOIN Courses C2 on G.Course = C2.Id
                           """;
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                FinancialOperations item = new FinancialOperations()
                { Id = reader.GetInt32("Id"), Client = reader.GetInt32("Client"), Sum = reader.GetInt32("Sum"),
                    OperationDate = reader.GetDateTimeOffset("OperationDate"), PaymentState = reader.GetBoolean("PaymentState"), ClientName = reader.GetString("ClientName"), ClientSurname = reader.GetString("ClientSurname") };
                data.Add(item);
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine("Ошибка подключения к базе данных: " + ex.Message);
        }
        finally
        {
            connection.Close(); } return data; }
    private MySqlConnection connection;// Метод для обновления значения PaymentState в базе данных
    private void UpdatePaymentState()
    {
        if (FinancialSelectedItem == null)
        {
            return;
        } int paymentstateid = FinancialSelectedItem.Id; bool paymentState = !FinancialSelectedItem.PaymentState; using MySqlConnection connection = new MySqlConnection(connectionString); connection.Open();

        string insertQuery = """
                             UPDATE FinancialOperations
                             SET PaymentState = @PaymentState
                             WHERE Id = @id;
                             """;
        using var cmd = new MySqlCommand(insertQuery, connection);
        cmd.Parameters.AddWithValue("@id", paymentstateid);
        cmd.Parameters.AddWithValue("@PaymentState", paymentState); cmd.ExecuteNonQuery(); if (FinancialSelectedItem.PaymentState == false) { }
    }
}