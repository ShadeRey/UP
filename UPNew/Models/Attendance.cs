using System;

namespace UP.Models;

public class Attendance
{
    public int Id { get; set; }
    public int Client { get; set; }
    public int Group { get; set; }
    public DateTimeOffset AttendanceDate { get; set; }
    public bool AttendanceStatus { get; set; }
}