using System;

namespace UP.Models;

public class Groups
{
    public int Id { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public int Course { get; set; }
    public int Teacher { get; set; }
}