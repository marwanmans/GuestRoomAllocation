using AutoMapper;
using GuestRoomAllocation.Application.Features.Guests.DTOs;
using GuestRoomAllocation.Application.Common.Models;
using GuestRoomAllocation.Domain.Entities;

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

        CreateMap<Apartment, LookupDto>()
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Name));

        CreateMap<Room, LookupDto>()
            .ForMember(d => d.Title, opt => opt.MapFrom(s => $"{s.Apartment.Name} - {s.RoomNumber}"));
    }
}