namespace LMS.DTO;

public class SchoolResponseDto
{
    public Guid SchoolId { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string District { get; set; }
    public string PrincipalName { get; set; }
    
    public List<TeacherGetDto> Teachers { get; set; }
    public List<StudentDto> Students { get; set; }
}
