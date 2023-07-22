using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LMS.Data;
using LMS.DTO;
using LMS.Models.Students;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace LMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ParentController : Controller
{
    public readonly DataContext _Context;
    
    public ParentController(DataContext context)
    {
        _Context = context;
    }
    
    // Registering a parent
    [HttpPost("register-parent")]
    public async Task<IActionResult> RegisterParent([FromBody] ParentRegisterDto parent)
    {
        if (parent == null)
        {
            return BadRequest("Parent is null.");
        }
        
        var addParent = new Parent
        {
            FirstName = parent.FirstName,
            LastName = parent.LastName,
            Email = parent.Email,
            PhoneNumber = parent.PhoneNumber,
            Role = "parent"
        };

        _Context.Parents.Add(addParent);
        await _Context.SaveChangesAsync();

        var token = GenerateJwtToken(parent);

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            expiration = token.ValidTo
        });
    }
    
    [HttpPost("register-student")]
    public async Task<IActionResult> RegisterStudent([FromBody] StudentRegistrationModel registration)
    {
        // Check if the student is null
        if (registration.Student == null)
        {
            return BadRequest("Student is null.");
        }

        // Check if the parent exists
        var parent = await _Context.Parents.FindAsync(registration.Student.ParentId);
        if (parent == null)
        {
            return BadRequest("Parent does not exist.");
        }

        // Check if the school exists
        var school = await _Context.Schools.FindAsync(registration.SchoolId);
        if (school == null)
        {
            return BadRequest("School does not exist.");
        }

        // Create a new Student object from the registration data
        var newStudent = new Student
        {
            ParentId = registration.Student.ParentId,
            SchoolId = registration.SchoolId,
            FirstName = registration.Student.FirstName,
            LastName = registration.Student.LastName,
            Age = registration.Student.Age,
            Email = registration.Student.Email,
            PhoneNumber = registration.Student.PhoneNumber,
            Role = "student"
        };

        // Add the new student to the context
        _Context.Students.Add(newStudent);

        // Save changes
        await _Context.SaveChangesAsync();

        // Return success message
        return Ok("Student registered successfully.");
    }


    private JwtSecurityToken GenerateJwtToken(ParentRegisterDto parent)
    {
        // Secret Key
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSuperSecureKey!"));

        // Generate Token
        var token = new JwtSecurityToken(
            claims: new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, parent.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{parent.FirstName} {parent.LastName}"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, parent.Role)
            },
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return token;
    }
    
    
}
