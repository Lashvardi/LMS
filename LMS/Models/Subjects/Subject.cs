using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using LMS.Models.Relationships;
using LMS.Models.Students;

namespace LMS.Models.Subjects;

public class Subject
{
    public int Id { get; set; }
    public string CourseName { get; set; }

    // Relationships
    [JsonIgnore]
    public Teacher Teacher { get; set; }
    public ICollection<SubjectTeacher> SubjectTeachers { get; set; }

}