using LMS.Models.Students;

namespace LMS.DTO;

public class StudentDto
{
    public int StudentId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public ParentDto Parent { get; set; }
}