﻿namespace NaSpontanie.API.Dtos.Users;

public class PasswordUpdateDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

