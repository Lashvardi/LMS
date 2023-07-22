using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LMS.Data;
using LMS.DTO;
using LMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SchoolController : Controller
{
    private readonly DataContext _context;
    
    public SchoolController(DataContext context)
    {
        _context = context;
    }

    [HttpPost("register-principal")]
    public async Task<IActionResult> RegisterPrincipal([FromForm] PrincipalDto principalDto)
    {
        // Check if a principal already exists in the database
        var existingPrincipal = await _context.Principals.FirstOrDefaultAsync();
        if (existingPrincipal != null)
        {
            return BadRequest("A principal is already registered.");
        }

        var principal = new Principal()
        {
            Id = Guid.NewGuid(),
            Name = principalDto.Name,
            Email = principalDto.Email,
            Password = principalDto.Password,
            IdentityNumber = principalDto.IdentityNumber,
            SchoolToken = Principal.GenerateToken(),
            SchoolId = null,
            Role = "principal"
        };

        await _context.Principals.AddAsync(principal);
        await _context.SaveChangesAsync();

        return Ok(new { token = principal.SchoolToken });
    }
    
    [HttpPost("login-principal")]
    public async Task<IActionResult> LoginPrincipal([FromForm] PrincipalDto principalDto)
    {
        var principal = await _context.Principals.FirstOrDefaultAsync(p => p.Email == principalDto.Email);
        if (principal == null)
        {
            return BadRequest("A principal with this email is not registered.");
        }

        // In reality, you should be hashing your passwords and comparing the hashed password here
        if (principal.Password != principalDto.Password)
        {
            return BadRequest("Invalid password.");
        }

        // Generate JWT token
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("My Top Secret Key Is This DUDUD"); // Use the same secret key
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] 
            {
                new Claim("id", principal.Id.ToString()),
                new Claim(ClaimTypes.Role, principal.Role),
                new Claim(ClaimTypes.Name, principal.Name),
                new Claim(ClaimTypes.Email, principal.Email)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = tokenHandler.WriteToken(token);

        return Ok(new { schoolToken = principal.SchoolToken, jwtToken = jwtToken });
    }

    
    [HttpPost("register-school")]
    // [Authorize(Roles = "principal")]
    public async Task<IActionResult> RegisterSchool([FromForm] SchoolDto schoolDto, string token)
    {
        if (string.IsNullOrEmpty(token) || !(await IsValidSchoolToken(token)))
        {
            return Unauthorized("Invalid or missing school token.");
        }

        var principal = await _context.Principals.FirstOrDefaultAsync();
        if (principal == null)
        {
            return BadRequest("A principal is not registered.");
        }

        if (principal.SchoolId != null)
        {
            return BadRequest("A school is already registered.");
        }

        var school = new School()
        {
            SchoolId = Guid.NewGuid(),
            Name = schoolDto.Name,
            Address = schoolDto.Address,
            PrincipalId = principal.Id,
            District = schoolDto.District,
        };

        await _context.Schools.AddAsync(school);
        await _context.SaveChangesAsync();

        principal.SchoolId = school.SchoolId;
        school.PrincipalId = principal.Id;
        await _context.SaveChangesAsync();

        var responseDto = new SchoolResponseDto
        {
            SchoolId = school.SchoolId,
            Name = school.Name,
            Address = school.Address,
            District = school.District,
            PrincipalName = principal.Name,
            Teachers = new List<TeacherGetDto>(),
            Students = new List<StudentDto>(),
        };

        return Ok(responseDto);
    }
        
    private async Task<bool> IsValidSchoolToken(string token)
    {
        var principal = await _context.Principals.FirstOrDefaultAsync();
        if (principal == null)
        {
            return false;
        }

        return principal.SchoolToken == token;
    }
    
    [HttpGet("get-school")]
    public async Task<IActionResult> GetSchool()
    {
        var school = await _context.Schools
            .Include(s => s.Principal)
            .Include(s => s.Teachers)
            .Include(s => s.Students)
            .ThenInclude(s => s.Parent)
            .FirstOrDefaultAsync();

        if (school == null)
        {
            return NotFound("School not found.");
        }

        var schoolDto = new SchoolResponseDto
        {
            SchoolId = school.SchoolId,
            Name = school.Name,
            Address = school.Address,
            District = school.District,
            PrincipalName = school.Principal.Name,
            Teachers = school.Teachers.Select(t => new TeacherGetDto
            {
                TeacherId = t.Id,
                Name = t.Name,
            }).ToList(),
            Students = school.Students.Select(s => new StudentDto 
            {
                StudentId = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                PhoneNumber = s.PhoneNumber,
                Parent = new ParentDto
                {
                    ParentId = s.Parent.Id,
                    FirstName = s.Parent.FirstName,
                    LastName = s.Parent.LastName,
                }
            }).ToList()
        };

        return Ok(schoolDto);
    }


    
}