﻿namespace GuestRoomAllocation.Application.Common.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Username { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}