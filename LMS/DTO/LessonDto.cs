namespace LMS.DTO;

public class LessonDto
{
    public int SubjectId { get; set; }
    public string DaysOfWeek { get; set; } // "Monday,Wednesday,Friday"
    public TimeSpan StartTime { get; set; } // "08:00"
    public TimeSpan EndTime { get; set; } // "09:30"
    public int TeacherId { get; set; }
}
