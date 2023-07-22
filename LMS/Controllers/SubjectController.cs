using LMS.Data;
using LMS.DTO;
using LMS.Models.Relationships;
using LMS.Models.Subjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubjectController : Controller
{
    private readonly DataContext _context;
    
    public SubjectController(DataContext context)
    {
        _context = context;
    }
    
    
    [HttpPost("add")]
    public async Task<IActionResult> AddSubject([FromBody] SubjectDto subjectDto)
    {
        var subject = new Subject
        {
            CourseName = subjectDto.Name,
        };

        await _context.Subjects.AddAsync(subject);
        await _context.SaveChangesAsync();

        return Ok(subject);
    }

}