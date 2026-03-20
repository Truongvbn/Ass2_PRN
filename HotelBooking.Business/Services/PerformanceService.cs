using AutoMapper;
using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using HotelBooking.Data.Entities;
using HotelBooking.Data.Repositories.Interfaces;

namespace HotelBooking.Business.Services;

public class PerformanceService : IPerformanceService
{
    private readonly IPerformanceReviewRepository _reviewRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IHotelRepository _hotelRepo;
    private readonly IMapper _mapper;

    public PerformanceService(
        IPerformanceReviewRepository reviewRepo,
        IEmployeeRepository employeeRepo,
        IHotelRepository hotelRepo,
        IMapper mapper)
    {
        _reviewRepo = reviewRepo;
        _employeeRepo = employeeRepo;
        _hotelRepo = hotelRepo;
        _mapper = mapper;
    }

    public async Task<ServiceResult<IReadOnlyList<PerformanceReviewDto>>> GetReviewsByEmployeeAsync(int employeeId, CancellationToken ct = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(employeeId, ct);
        if (employee is null)
        {
            return ServiceResult<IReadOnlyList<PerformanceReviewDto>>.Failure("Employee not found", "NOT_FOUND");
        }

        var reviews = await _reviewRepo.GetByEmployeeAsync(employeeId, ct);
        return ServiceResult<IReadOnlyList<PerformanceReviewDto>>.Success(_mapper.Map<IReadOnlyList<PerformanceReviewDto>>(reviews));
    }

    public async Task<ServiceResult<PerformanceReviewDto>> CreateReviewAsync(CreatePerformanceReviewDto dto, string reviewerId, CancellationToken ct = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(dto.EmployeeId, ct);
        if (employee is null)
        {
            return ServiceResult<PerformanceReviewDto>.Failure("Employee not found", "NOT_FOUND");
        }

        var hotel = await _hotelRepo.GetByIdAsync(dto.HotelId, ct);
        if (hotel is null)
        {
            return ServiceResult<PerformanceReviewDto>.Failure("Hotel not found", "NOT_FOUND");
        }

        if (dto.OverallRating < 1 || dto.OverallRating > 5)
        {
            return ServiceResult<PerformanceReviewDto>.Failure("OverallRating must be between 1 and 5", "VALIDATION");
        }

        var review = _mapper.Map<PerformanceReview>(dto);
        review.ReviewerId = reviewerId;
        await _reviewRepo.AddAsync(review, ct);

        var reviews = await _reviewRepo.GetByEmployeeAsync(dto.EmployeeId, ct);
        var created = reviews.First(r => r.Id == review.Id);
        return ServiceResult<PerformanceReviewDto>.Success(_mapper.Map<PerformanceReviewDto>(created));
    }
}

