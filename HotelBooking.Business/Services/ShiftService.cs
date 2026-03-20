using AutoMapper;
using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using HotelBooking.Data.Entities;
using HotelBooking.Data.Repositories.Interfaces;

namespace HotelBooking.Business.Services;

public class ShiftService : IShiftService
{
    private readonly IWorkShiftRepository _shiftRepo;
    private readonly IEmployeeShiftAssignmentRepository _assignmentRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IHotelRepository _hotelRepo;
    private readonly IMapper _mapper;

    public ShiftService(
        IWorkShiftRepository shiftRepo,
        IEmployeeShiftAssignmentRepository assignmentRepo,
        IEmployeeRepository employeeRepo,
        IHotelRepository hotelRepo,
        IMapper mapper)
    {
        _shiftRepo = shiftRepo;
        _assignmentRepo = assignmentRepo;
        _employeeRepo = employeeRepo;
        _hotelRepo = hotelRepo;
        _mapper = mapper;
    }

    public async Task<ServiceResult<IReadOnlyList<WorkShiftDto>>> GetShiftsByHotelAsync(int hotelId, CancellationToken ct = default)
    {
        var hotel = await _hotelRepo.GetByIdAsync(hotelId, ct);
        if (hotel is null)
        {
            return ServiceResult<IReadOnlyList<WorkShiftDto>>.Failure("Hotel not found", "NOT_FOUND");
        }

        var shifts = await _shiftRepo.GetByHotelAsync(hotelId, ct);
        return ServiceResult<IReadOnlyList<WorkShiftDto>>.Success(_mapper.Map<IReadOnlyList<WorkShiftDto>>(shifts));
    }

    public async Task<ServiceResult<WorkShiftDto>> CreateShiftAsync(CreateWorkShiftDto dto, CancellationToken ct = default)
    {
        var hotel = await _hotelRepo.GetByIdAsync(dto.HotelId, ct);
        if (hotel is null)
        {
            return ServiceResult<WorkShiftDto>.Failure("Hotel not found", "NOT_FOUND");
        }

        var shift = _mapper.Map<WorkShift>(dto);
        await _shiftRepo.AddAsync(shift, ct);
        return ServiceResult<WorkShiftDto>.Success(_mapper.Map<WorkShiftDto>(shift));
    }

    public async Task<ServiceResult<WorkShiftDto>> UpdateShiftAsync(UpdateWorkShiftDto dto, CancellationToken ct = default)
    {
        var shift = await _shiftRepo.GetByIdAsync(dto.Id, ct);
        if (shift is null)
        {
            return ServiceResult<WorkShiftDto>.Failure("Shift not found", "NOT_FOUND");
        }

        _mapper.Map(dto, shift);
        await _shiftRepo.UpdateAsync(shift, ct);
        return ServiceResult<WorkShiftDto>.Success(_mapper.Map<WorkShiftDto>(shift));
    }

    public async Task<ServiceResult<WorkShiftDto>> GetShiftByIdAsync(int id, CancellationToken ct = default)
    {
        var shift = await _shiftRepo.GetByIdAsync(id, ct);
        if (shift is null)
        {
            return ServiceResult<WorkShiftDto>.Failure("Shift not found", "NOT_FOUND");
        }

        return ServiceResult<WorkShiftDto>.Success(_mapper.Map<WorkShiftDto>(shift));
    }

    public async Task<ServiceResult> ToggleShiftActiveAsync(int id, bool isActive, CancellationToken ct = default)
    {
        var shift = await _shiftRepo.GetByIdAsync(id, ct);
        if (shift is null)
        {
            return ServiceResult.Failure("Shift not found", "NOT_FOUND");
        }

        shift.IsActive = isActive;
        await _shiftRepo.UpdateAsync(shift, ct);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult<IReadOnlyList<ShiftAssignmentDto>>> GetScheduleByHotelAsync(int hotelId, DateTime start, DateTime end, CancellationToken ct = default)
    {
        var hotel = await _hotelRepo.GetByIdAsync(hotelId, ct);
        if (hotel is null)
        {
            return ServiceResult<IReadOnlyList<ShiftAssignmentDto>>.Failure("Hotel not found", "NOT_FOUND");
        }

        var normalizedStart = start.Date;
        var normalizedEnd = end.Date;
        var assignments = await _assignmentRepo.GetByHotelAndDateRangeAsync(hotelId, normalizedStart, normalizedEnd, ct);
        return ServiceResult<IReadOnlyList<ShiftAssignmentDto>>.Success(_mapper.Map<IReadOnlyList<ShiftAssignmentDto>>(assignments));
    }

    public async Task<ServiceResult<IReadOnlyList<ShiftAssignmentDto>>> GetScheduleByEmployeeAsync(int employeeId, DateTime start, DateTime end, CancellationToken ct = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(employeeId, ct);
        if (employee is null)
        {
            return ServiceResult<IReadOnlyList<ShiftAssignmentDto>>.Failure("Employee not found", "NOT_FOUND");
        }

        var normalizedStart = start.Date;
        var normalizedEnd = end.Date;
        var assignments = await _assignmentRepo.GetByEmployeeAndDateRangeAsync(employeeId, normalizedStart, normalizedEnd, ct);
        return ServiceResult<IReadOnlyList<ShiftAssignmentDto>>.Success(_mapper.Map<IReadOnlyList<ShiftAssignmentDto>>(assignments));
    }

    public async Task<ServiceResult<ShiftAssignmentDto>> AssignShiftAsync(CreateShiftAssignmentDto dto, CancellationToken ct = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(dto.EmployeeId, ct);
        if (employee is null)
        {
            return ServiceResult<ShiftAssignmentDto>.Failure("Employee not found", "NOT_FOUND");
        }

        var hotel = await _hotelRepo.GetByIdAsync(dto.HotelId, ct);
        if (hotel is null)
        {
            return ServiceResult<ShiftAssignmentDto>.Failure("Hotel not found", "NOT_FOUND");
        }

        var shift = await _shiftRepo.GetByIdAsync(dto.WorkShiftId, ct);
        if (shift is null || shift.HotelId != dto.HotelId)
        {
            return ServiceResult<ShiftAssignmentDto>.Failure("Shift not found for this hotel", "NOT_FOUND");
        }

        var assignment = _mapper.Map<EmployeeShiftAssignment>(dto);
        assignment.ShiftDate = dto.ShiftDate.Date;
        await _assignmentRepo.AddAsync(assignment, ct);

        // re-load with navigation for mapping
        var rangeAssignments = await _assignmentRepo.GetByEmployeeAndDateRangeAsync(dto.EmployeeId, dto.ShiftDate.Date, dto.ShiftDate.Date, ct);
        var created = rangeAssignments.First(a => a.Id == assignment.Id);
        return ServiceResult<ShiftAssignmentDto>.Success(_mapper.Map<ShiftAssignmentDto>(created));
    }
}

