namespace UP.Models;

public class Course
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Duration { get; set; }
    public string DifficultyLevel { get; set; }
    public int MaxStudents { get; set; }
    public int Price { get; set; }
    public int CurrentStudent { get; set; }
}