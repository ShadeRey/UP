using System;
using System.Runtime.CompilerServices;

namespace UP.Models;

public class Clients
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string LanguageLevel { get; set; }
    public Nullable<DateTimeOffset> BirthDate { get; set; }
    public string PreviousExperience { get; set; }
    public string LanguageNeeds { get; set; }
}