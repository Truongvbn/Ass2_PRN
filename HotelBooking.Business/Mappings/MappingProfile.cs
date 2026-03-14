using AutoMapper;
using HotelBooking.Business.DTOs;
using HotelBooking.Data.Entities;
using System.Text.Json;

namespace HotelBooking.Business.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Room
        CreateMap<Room, RoomDto>()
            .ForMember(d => d.HotelName, o => o.MapFrom(s => s.Hotel != null ? s.Hotel.Name : ""))
            .ForMember(d => d.HotelLatitude, o => o.MapFrom(s => s.Hotel != null ? (double?)s.Hotel.Latitude : null))
            .ForMember(d => d.HotelLongitude, o => o.MapFrom(s => s.Hotel != null ? (double?)s.Hotel.Longitude : null))
            .ForMember(d => d.RoomTypeName, o => o.MapFrom(s => s.RoomType.Name))
            .ForMember(d => d.Gallery, o => o.MapFrom(s => DeserializeGallery(s.Gallery)))
            .ForMember(d => d.AverageRating, o => o.Ignore())
            .ForMember(d => d.ReviewCount, o => o.Ignore());

        CreateMap<Room, RoomListDto>()
            .ForMember(d => d.HotelName, o => o.MapFrom(s => s.Hotel != null ? s.Hotel.Name : ""))
            .ForMember(d => d.RoomTypeName, o => o.MapFrom(s => s.RoomType.Name))
            .ForMember(d => d.Gallery, o => o.MapFrom(s => DeserializeGallery(s.Gallery)))
            .ForMember(d => d.AverageRating, o => o.Ignore());

        CreateMap<CreateRoomDto, Room>()
            .ForMember(d => d.Gallery, o => o.MapFrom(s => SerializeGallery(s.Gallery)));

        CreateMap<UpdateRoomDto, Room>()
            .ForMember(d => d.Gallery, o => o.MapFrom(s => SerializeGallery(s.Gallery)))
            .ForMember(d => d.UpdatedAt, o => o.MapFrom(_ => DateTime.UtcNow));

        // RoomType
        CreateMap<RoomType, RoomTypeDto>();

        // Booking
        CreateMap<Booking, BookingDto>()
            .ForMember(d => d.HotelId, o => o.MapFrom(s => s.Room != null ? s.Room.HotelId : 0))
            .ForMember(d => d.HotelName, o => o.MapFrom(s => s.Room != null && s.Room.Hotel != null ? s.Room.Hotel.Name : ""))
            .ForMember(d => d.RoomName, o => o.MapFrom(s => s.Room != null ? s.Room.Name : ""))
            .ForMember(d => d.RoomTypeName, o => o.MapFrom(s => s.Room != null && s.Room.RoomType != null ? s.Room.RoomType.Name : ""))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User != null ? s.User.FullName : ""))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.RefundAmount, o => o.MapFrom(s => s.Payment != null ? s.Payment.RefundAmount : (decimal?)null))
            .ForMember(d => d.RefundedAt, o => o.MapFrom(s => s.Payment != null ? s.Payment.RefundedAt : null));

        // Hotel
        CreateMap<Hotel, HotelDto>()
            .ForMember(d => d.Gallery, o => o.MapFrom(s => DeserializeGallery(s.Gallery)))
            .ForMember(d => d.RoomCount, o => o.MapFrom(s => s.Rooms.Count))
            .ForMember(
                d => d.MinPricePerNight,
                o => o.MapFrom(s => s.Rooms.Any() ? (decimal?)s.Rooms.Min(r => r.PricePerNight) : null)
            );

        CreateMap<CreateHotelDto, Hotel>()
            .ForMember(d => d.Gallery, o => o.Ignore());

        CreateMap<UpdateHotelDto, Hotel>()
            .ForMember(d => d.Gallery, o => o.Ignore());

        CreateMap<HotelStaff, HotelStaffDto>()
            .ForMember(d => d.HotelName, o => o.MapFrom(s => s.Hotel.Name))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()));

        // Payment
        CreateMap<Payment, PaymentDto>()
            .ForMember(d => d.Method, o => o.MapFrom(s => s.Method.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        // Review
        CreateMap<Review, ReviewDto>()
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.Comments, o => o.MapFrom(s => s.Comments));

        // ReviewComment
        CreateMap<ReviewComment, ReviewCommentDto>()
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.FullName));

        // SupportTicket
        CreateMap<SupportTicket, TicketDto>()
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.AssignedToName, o => o.MapFrom(s => s.AssignedTo != null ? s.AssignedTo.FullName : null))
            .ForMember(d => d.Category, o => o.MapFrom(s => s.Category.ToString()))
            .ForMember(d => d.Priority, o => o.MapFrom(s => s.Priority.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
    }

    private static string[] DeserializeGallery(string? gallery)
    {
        if (string.IsNullOrWhiteSpace(gallery))
        {
            return Array.Empty<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<string[]>(gallery) ?? Array.Empty<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    private static string SerializeGallery(string[]? gallery)
    {
        if (gallery is null || gallery.Length == 0)
        {
            return "[]";
        }

        return JsonSerializer.Serialize(gallery);
    }
}
