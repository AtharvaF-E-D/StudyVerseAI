using StudyVerse.Application.Common.Models;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Common.Interfaces;

public interface IPasswordHasher
{
    string HashPassword(User user, string password);

    PasswordVerificationResult VerifyPassword(User user, string hashedPassword, string providedPassword);
}
