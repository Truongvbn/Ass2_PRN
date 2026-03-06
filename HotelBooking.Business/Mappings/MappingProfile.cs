using AutoMapper;
using HotelBooking.Business.DTOs;
using HotelBooking.Data.Entities;

namespace HotelBooking.Business.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Room
        CreateMap<Room, RoomDto>()
            .ForMember(d => d.RoomTypeName, o => o.MapFrom(s => s.RoomType.Name))
            .ForMember(d => d.AverageRating, o => o.Ignore())
            .ForMember(d => d.ReviewCount, o => o.Ignore());

        CreateMap<Room, RoomListDto>()
            .ForMember(d => d.RoomTypeName, o => o.MapFrom(s => s.RoomType.Name))
            .ForMember(d => d.AverageRating, o => o.Ignore());

        CreateMap<CreateRoomDto, Room>();
        CreateMap<UpdateRoomDto, Room>()
            .ForMember(d => d.UpdatedAt, o => o.MapFrom(_ => DateTime.UtcNow));

        // RoomType
        CreateMap<RoomType, RoomTypeDto>();

        // Booking
        CreateMap<Booking, BookingDto>()
            .ForMember(d => d.RoomName, o => o.MapFrom(s => s.Room.Name))
            .ForMember(d => d.RoomTypeName, o => o.MapFrom(s => s.Room.RoomType.Name))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

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
}
