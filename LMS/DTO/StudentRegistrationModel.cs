using LMS.Models.Students;

namespace LMS.DTO;

public class StudentRegistrationModel
{
    public StudentRegistrationDto Student { get; set; }
    public Guid SchoolId { get; set; }
}

public class StudentRegistrationDto
{
    public int ParentId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
}