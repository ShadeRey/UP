using System;

namespace UP.Models;

public class Groups
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public int Course { get; set; }
    public int Teacher { get; set; }
    public int MaxStudents { get; set; }
    public int CurrentStudents { get; set; }
    public string TeacherName { get; set; }
    public string TeacherSurname { get; set; }
}