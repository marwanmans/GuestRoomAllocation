﻿namespace GuestRoomAllocation.Application.Common.Interfaces;

public interface IDateTime
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
    DateTime Today { get; }
}