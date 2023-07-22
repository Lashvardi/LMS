using LMS.Data;
using LMS.DTO;
using LMS.Models;
using LMS.Models.Relationships;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeacherController : Controller
{
    private readonly DataContext _context;
    
    public TeacherController(DataContext context)
    {
        _context = context;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> RegisterTeacher([FromBody] TeacherDto teacherDto)
    {
        var school = await _context.Schools.SingleOrDefaultAsync(s => s.SchoolId == teacherDto.SchoolId);

        if (school == null)
        {
            return BadRequest("The school does not exist.");
        }

        var teacher = new Teacher()
        {
            Name = teacherDto.Name,
            SchoolId = school.SchoolId,
        };

        await _context.Teachers.AddAsync(teacher);
        await _context.SaveChangesAsync();

        return Ok(teacher);
    }
    
    
    [HttpPost("assign")]
    public async Task<IActionResult> AssignSubjectToTeacher(int teacherId, int subjectId)
    {
        // Check if the Teacher and Subject exist.
        var teacher = await _context.Teachers.FindAsync(teacherId);
        if (teacher == null) return NotFound("Teacher not found");

        var subject = await _context.Subjects.FindAsync(subjectId);
        if (subject == null) return NotFound("Subject not found");

        // Check if the assignment already exists.
        var subjectTeacherExists = await _context.SubjectTeachers
            .AnyAsync(st => st.TeacherId == teacherId && st.SubjectId == subjectId);

        if (subjectTeacherExists)
        {
            return BadRequest("This teacher is already assigned to this subject.");
        }

        // If not, create and save the new assignment.
        var subjectTeacher = new SubjectTeacher
        {
            TeacherId = teacherId,
            SubjectId = subjectId
        };

        await _context.SubjectTeachers.AddAsync(subjectTeacher);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            TeacherName = teacher.Name,
            AssignedSubject = subject.CourseName
        });
    }
    
    [HttpGet("{teacherId}/subjects")]
    public async Task<IActionResult> GetTeacherSubjects(int teacherId)
    {
        // Check if the teacher exists.
        var teacher = await _context.Teachers.FindAsync(teacherId);
        if (teacher == null) return NotFound("Teacher not found");

        // Fetch the subjects assigned to the teacher.
        var subjects = await _context.SubjectTeachers
            .Where(st => st.TeacherId == teacherId)
            .Select(st => st.Subject)
            .ToListAsync();

        // If you only want to return the course names, you can do that by using Select
        var subjectNames = subjects.Select(s => s.CourseName);

        return Ok(subjectNames);
    }




}