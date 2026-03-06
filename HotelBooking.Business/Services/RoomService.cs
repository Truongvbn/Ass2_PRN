using AutoMapper;
using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using HotelBooking.Data.Entities;
using HotelBooking.Data.Repositories.Interfaces;

namespace HotelBooking.Business.Services;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepo;
    private readonly IReviewRepository _reviewRepo;
    private readonly IMapper _mapper;

    public RoomService(IRoomRepository roomRepo, IReviewRepository reviewRepo, IMapper mapper)
    {
        _roomRepo = roomRepo;
        _reviewRepo = reviewRepo;
        _mapper = mapper;
    }

    public async Task<ServiceResult<IReadOnlyList<RoomListDto>>> SearchRoomsAsync(
        int? roomTypeId, decimal? minPrice, decimal? maxPrice, int? minOccupancy,
        DateTime? checkIn, DateTime? checkOut, CancellationToken ct = default)
    {
        var rooms = await _roomRepo.SearchAsync(roomTypeId, minPrice, maxPrice, minOccupancy, checkIn, checkOut, ct);
        var dtos = new List<RoomListDto>();
        foreach (var room in rooms)
        {
            var avgRating = await _reviewRepo.GetAverageRatingAsync(room.Id, ct);
            dtos.Add(new RoomListDto { Id = room.Id, Name = room.Name, RoomTypeName = room.RoomType.Name, PricePerNight = room.PricePerNight, MaxOccupancy = room.MaxOccupancy, ImageUrl = room.ImageUrl, IsAvailable = room.IsAvailable, AverageRating = avgRating });
        }
        return ServiceResult<IReadOnlyList<RoomListDto>>.Success(dtos);
    }

    public async Task<ServiceResult<RoomDto>> GetRoomByIdAsync(int id, CancellationToken ct = default)
    {
        var room = await _roomRepo.GetWithDetailsAsync(id, ct);
        if (room is null) return ServiceResult<RoomDto>.Failure("Room not found", "NOT_FOUND");

        var avgRating = await _reviewRepo.GetAverageRatingAsync(id, ct);
        var dto = _mapper.Map<RoomDto>(room) with { AverageRating = avgRating, ReviewCount = room.Reviews.Count };
        return ServiceResult<RoomDto>.Success(dto);
    }

    public async Task<ServiceResult<RoomDto>> CreateRoomAsync(CreateRoomDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return ServiceResult<RoomDto>.Failure("Room name is required", "VALIDATION");
        if (dto.PricePerNight <= 0)
            return ServiceResult<RoomDto>.Failure("Price must be greater than 0", "VALIDATION");
        if (dto.MaxOccupancy <= 0)
            return ServiceResult<RoomDto>.Failure("Max occupancy must be greater than 0", "VALIDATION");

        var room = _mapper.Map<Room>(dto);
        room.CreatedAt = DateTime.UtcNow;
        room.UpdatedAt = DateTime.UtcNow;
        await _roomRepo.AddAsync(room, ct);

        var created = await _roomRepo.GetWithDetailsAsync(room.Id, ct);
        return ServiceResult<RoomDto>.Success(_mapper.Map<RoomDto>(created!) with { AverageRating = 0, ReviewCount = 0 });
    }

    public async Task<ServiceResult<RoomDto>> UpdateRoomAsync(UpdateRoomDto dto, CancellationToken ct = default)
    {
        var room = await _roomRepo.GetByIdAsync(dto.Id, ct);
        if (room is null) return ServiceResult<RoomDto>.Failure("Room not found", "NOT_FOUND");

        room.Name = dto.Name;
        room.RoomTypeId = dto.RoomTypeId;
        room.PricePerNight = dto.PricePerNight;
        room.MaxOccupancy = dto.MaxOccupancy;
        room.Description = dto.Description;
        room.ImageUrl = dto.ImageUrl;
        room.Amenities = dto.Amenities;
        room.IsAvailable = dto.IsAvailable;
        room.UpdatedAt = DateTime.UtcNow;
        await _roomRepo.UpdateAsync(room, ct);

        var updated = await _roomRepo.GetWithDetailsAsync(room.Id, ct);
        var avgRating = await _reviewRepo.GetAverageRatingAsync(room.Id, ct);
        return ServiceResult<RoomDto>.Success(_mapper.Map<RoomDto>(updated!) with { AverageRating = avgRating, ReviewCount = updated!.Reviews.Count });
    }

    public async Task<ServiceResult> DeleteRoomAsync(int id, CancellationToken ct = default)
    {
        var room = await _roomRepo.GetByIdAsync(id, ct);
        if (room is null) return ServiceResult.Failure("Room not found", "NOT_FOUND");

        room.IsDeleted = true;
        room.UpdatedAt = DateTime.UtcNow;
        await _roomRepo.UpdateAsync(room, ct);
        return ServiceResult.Success();
    }

    public async Task<IReadOnlyList<RoomTypeDto>> GetRoomTypesAsync(CancellationToken ct = default)
    {
        var types = await _roomRepo.FindAsync(_ => true, ct); // Use RoomType repo ideally
        return []; // This will be replaced with proper impl
    }
}
