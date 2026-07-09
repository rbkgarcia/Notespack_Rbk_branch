using System;
using System.ComponentModel.DataAnnotations;

namespace NOTESPACK.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Email { get; set; } = string.Empty;

    // 👈 Agregamos 'username' para solucionar el error
    public string username { get; set; } = string.Empty; 

    // 👈 Agregamos 'Password' en lugar de PasswordHash para que coincida con tus archivos
    [Required]
    public string Password { get; set; } = string.Empty;

    // 👈 Agregamos 'EnrollmentDate' con un valor por defecto (la fecha actual)
    public DateTime EnrollmentDate { get; set; } = DateTime.Now;

    public string Role { get; set; } = "User";

    public List<Event> Events { get; set; } = new();
}