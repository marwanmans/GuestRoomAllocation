// GuestRoomAllocation.Application/Common/Mappings/MappingProfile.cs
using AutoMapper;
using GuestRoomAllocation.Application.Common.Models;
using GuestRoomAllocation.Application.Features.Allocations.DTOs;
using GuestRoomAllocation.Application.Features.Apartments.DTOs;
using GuestRoomAllocation.Application.Features.Guests.DTOs;
using GuestRoomAllocation.Application.Features.Maintenance.DTOs;
using GuestRoomAllocation.Application.Features.Rooms.DTOs;
using GuestRoomAllocation.Application.Features.Users.DTOs;
using GuestRoomAllocation.Domain.Entities;
using GuestRoomAllocation.Domain.Entities.GuestRoomAllocation.Domain.Entities;
using GuestRoomAllocation.Domain.Entities.GuestRoomAllocation.Domain.Entities.GuestRoomAllocation.Domain.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GuestRoomAllocation.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Guest, GuestDto>()
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.ContactInfo.Email))
            .ForMember(d => d.Phone, opt => opt.MapFrom(s => s.ContactInfo.Phone));

        CreateMap<Guest, GuestDetailDto>()
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.ContactInfo.Email))
            .ForMember(d => d.Phone, opt => opt.MapFrom(s => s.ContactInfo.Phone))
            .ForMember(d => d.TotalAllocations, opt => opt.MapFrom(s => s.Allocations.Count))
            .ForMember(d => d.CurrentAllocations, opt => opt.MapFrom(s => s.Allocations.Count(a => a.Status == Domain.Enums.AllocationStatus.Current)));

        CreateMap<Guest, LookupDto>()
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.FullName));

        CreateMap<Apartment, ApartmentDto>()
            .ForMember(d => d.Address, opt => opt.MapFrom(s => s.Address.ToString()))
            .ForMember(d => d.TotalRooms, opt => opt.MapFrom(s => s.Rooms.Count));

        CreateMap<Apartment, ApartmentDetailDto>()
            .ForMember(d => d.Street, opt => opt.MapFrom(s => s.Address.Street))
            .ForMember(d => d.City, opt => opt.MapFrom(s => s.Address.City))
            .ForMember(d => d.State, opt => opt.MapFrom(s => s.Address.State))
            .ForMember(d => d.ZipCode, opt => opt.MapFrom(s => s.Address.ZipCode))
            .ForMember(d => d.Country, opt => opt.MapFrom(s => s.Address.Country))
            .ForMember(d => d.TotalRooms, opt => opt.MapFrom(s => s.Rooms.Count));

        CreateMap<Apartment, LookupDto>()
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Name));

        CreateMap<Room, RoomDto>()
            .ForMember(d => d.ApartmentName, opt => opt.MapFrom(s => s.Apartment.Name));

        CreateMap<Room, RoomDetailDto>()
            .ForMember(d => d.ApartmentName, opt => opt.MapFrom(s => s.Apartment.Name))
            .ForMember(d => d.TotalAllocations, opt => opt.MapFrom(s => s.Allocations.Count));

        CreateMap<Room, LookupDto>()
            .ForMember(d => d.Title, opt => opt.MapFrom(s => $"{s.Apartment.Name} - {s.RoomNumber}"));

        CreateMap<Allocation, AllocationDto>()
            .ForMember(d => d.GuestName, opt => opt.MapFrom(s => s.Guest.FullName))
            .ForMember(d => d.GuestEmail, opt => opt.MapFrom(s => s.Guest.ContactInfo.Email))
            .ForMember(d => d.ApartmentName, opt => opt.MapFrom(s => s.Room.Apartment.Name))
            .ForMember(d => d.RoomNumber, opt => opt.MapFrom(s => s.Room.RoomNumber))
            .ForMember(d => d.CheckInDate, opt => opt.MapFrom(s => s.DateRange.StartDate))
            .ForMember(d => d.CheckOutDate, opt => opt.MapFrom(s => s.DateRange.EndDate))
            .ForMember(d => d.Duration, opt => opt.MapFrom(s => s.DateRange.Duration));

        CreateMap<Allocation, AllocationDetailDto>()
            .ForMember(d => d.GuestName, opt => opt.MapFrom(s => s.Guest.FullName))
            .ForMember(d => d.GuestEmail, opt => opt.MapFrom(s => s.Guest.ContactInfo.Email))
            .ForMember(d => d.GuestPhone, opt => opt.MapFrom(s => s.Guest.ContactInfo.Phone))
            .ForMember(d => d.GuestJobPosition, opt => opt.MapFrom(s => s.Guest.JobPosition))
            .ForMember(d => d.ApartmentName, opt => opt.MapFrom(s => s.Room.Apartment.Name))
            .ForMember(d => d.RoomNumber, opt => opt.MapFrom(s => s.Room.RoomNumber))
            .ForMember(d => d.RoomSize, opt => opt.MapFrom(s => s.Room.Size))
            .ForMember(d => d.HasPrivateBathroom, opt => opt.MapFrom(s => s.Room.HasPrivateBathroom))
            .ForMember(d => d.CheckInDate, opt => opt.MapFrom(s => s.DateRange.StartDate))
            .ForMember(d => d.CheckOutDate, opt => opt.MapFrom(s => s.DateRange.EndDate))
            .ForMember(d => d.Duration, opt => opt.MapFrom(s => s.DateRange.Duration));

        CreateMap<MaintenancePeriod, MaintenanceDto>()
            .ForMember(d => d.Location, opt => opt.MapFrom(s => s.Target))
            .ForMember(d => d.StartDate, opt => opt.MapFrom(s => s.DateRange.StartDate))
            .ForMember(d => d.EndDate, opt => opt.MapFrom(s => s.DateRange.EndDate))
            .ForMember(d => d.Duration, opt => opt.MapFrom(s => s.DateRange.Duration));

        CreateMap<MaintenancePeriod, MaintenanceDetailDto>()
            .ForMember(d => d.Location, opt => opt.MapFrom(s => s.Target))
            .ForMember(d => d.ApartmentName, opt => opt.MapFrom(s => s.Apartment != null ? s.Apartment.Name : s.Room!.Apartment.Name))
            .ForMember(d => d.RoomNumber, opt => opt.MapFrom(s => s.Room != null ? s.Room.RoomNumber : null))
            .ForMember(d => d.StartDate, opt => opt.MapFrom(s => s.DateRange.StartDate))
            .ForMember(d => d.EndDate, opt => opt.MapFrom(s => s.DateRange.EndDate))
            .ForMember(d => d.Duration, opt => opt.MapFrom(s => s.DateRange.Duration));

        CreateMap<User, UserDto>();

        CreateMap<User, UserDetailDto>()
            .ForMember(d => d.TotalApartmentAccess, opt => opt.MapFrom(s => s.ApartmentAccess.Count))
            .ForMember(d => d.TotalGuestAccess, opt => opt.MapFrom(s => s.GuestAccess.Count));

        CreateMap<User, LookupDto>()
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.FullName));
    }
}