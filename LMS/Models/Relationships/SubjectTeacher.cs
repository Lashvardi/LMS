using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using LMS.Models.Subjects;
using Microsoft.EntityFrameworkCore;

namespace LMS.Models.Relationships;

[Keyless]
public class SubjectTeacher
{
    public int SubjectId { get; set; }
    public int TeacherId { get; set; }
        
    // Relationships
    [JsonIgnore]
    public Subject Subject { get; set; }
    [JsonIgnore]
    public Teacher Teacher { get; set; }
}