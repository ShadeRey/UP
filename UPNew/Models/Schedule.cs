using System;

namespace UP.Models;

public class Schedule
{
    public int Id { get; set; }
    public int Group { get; set; }
    public int WeekDay { get; set; }
    public TimeOnly Time { get; set; }
}