using AutoMapper;
using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using HotelBooking.Data.Entities;
using HotelBooking.Data.Repositories.Interfaces;

namespace HotelBooking.Business.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendanceRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IHotelRepository _hotelRepo;
    private readonly IWorkShiftRepository _shiftRepo;
    private readonly IMapper _mapper;

    public AttendanceService(
        IAttendanceRepository attendanceRepo,
        IEmployeeRepository employeeRepo,
        IHotelRepository hotelRepo,
        IWorkShiftRepository shiftRepo,
        IMapper mapper)
    {
        _attendanceRepo = attendanceRepo;
        _employeeRepo = employeeRepo;
        _hotelRepo = hotelRepo;
        _shiftRepo = shiftRepo;
        _mapper = mapper;
    }

    public async Task<ServiceResult<AttendanceDto>> RecordAttendanceAsync(RecordAttendanceDto dto, CancellationToken ct = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(dto.EmployeeId, ct);
        if (employee is null)
        {
            return ServiceResult<AttendanceDto>.Failure("Employee not found", "NOT_FOUND");
        }

        var hotel = await _hotelRepo.GetByIdAsync(dto.HotelId, ct);
        if (hotel is null)
        {
            return ServiceResult<AttendanceDto>.Failure("Hotel not found", "NOT_FOUND");
        }

        WorkShift? shift = null;
        if (dto.WorkShiftId.HasValue)
        {
            shift = await _shiftRepo.GetByIdAsync(dto.WorkShiftId.Value, ct);
            if (shift is null || shift.HotelId != dto.HotelId)
            {
                return ServiceResult<AttendanceDto>.Failure("Shift not found for this hotel", "NOT_FOUND");
            }
        }

        var date = dto.ShiftDate.Date;
        var existing = (await _attendanceRepo.GetByEmployeeAndDateRangeAsync(dto.EmployeeId, date, date, ct)).FirstOrDefault();
        var record = existing ?? new AttendanceRecord
        {
            EmployeeId = dto.EmployeeId,
            HotelId = dto.HotelId,
            ShiftDate = date
        };

        record.WorkShiftId = dto.WorkShiftId;
        record.CheckInTime = dto.CheckInTime;
        record.CheckOutTime = dto.CheckOutTime;
        record.Notes = dto.Notes;

        // Simple rule for status + hours
        if (record.CheckInTime.HasValue && record.CheckOutTime.HasValue && record.CheckOutTime > record.CheckInTime)
        {
            record.Status = AttendanceStatus.Present;
            record.HoursWorked = (record.CheckOutTime.Value - record.CheckInTime.Value).TotalHours;
        }
        else if (!record.CheckInTime.HasValue && !record.CheckOutTime.HasValue)
        {
            record.Status = AttendanceStatus.Absent;
            record.HoursWorked = 0;
        }
        else
        {
            record.Status = AttendanceStatus.Late;
            record.HoursWorked = 0;
        }

        if (existing is null)
        {
            await _attendanceRepo.AddAsync(record, ct);
        }
        else
        {
            await _attendanceRepo.UpdateAsync(record, ct);
        }

        // reload for mapping
        var reloaded = (await _attendanceRepo.GetByEmployeeAndDateRangeAsync(dto.EmployeeId, date, date, ct)).First(a => a.Id == record.Id);
        return ServiceResult<AttendanceDto>.Success(_mapper.Map<AttendanceDto>(reloaded));
    }

    public async Task<ServiceResult<IReadOnlyList<AttendanceDto>>> GetAttendanceByHotelAsync(int hotelId, DateTime start, DateTime end, CancellationToken ct = default)
    {
        var hotel = await _hotelRepo.GetByIdAsync(hotelId, ct);
        if (hotel is null)
        {
            return ServiceResult<IReadOnlyList<AttendanceDto>>.Failure("Hotel not found", "NOT_FOUND");
        }

        var records = await _attendanceRepo.GetByHotelAndDateRangeAsync(hotelId, start.Date, end.Date, ct);
        return ServiceResult<IReadOnlyList<AttendanceDto>>.Success(_mapper.Map<IReadOnlyList<AttendanceDto>>(records));
    }

    public async Task<ServiceResult<IReadOnlyList<AttendanceDto>>> GetAttendanceByEmployeeAsync(int employeeId, DateTime start, DateTime end, CancellationToken ct = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(employeeId, ct);
        if (employee is null)
        {
            return ServiceResult<IReadOnlyList<AttendanceDto>>.Failure("Employee not found", "NOT_FOUND");
        }

        var records = await _attendanceRepo.GetByEmployeeAndDateRangeAsync(employeeId, start.Date, end.Date, ct);
        return ServiceResult<IReadOnlyList<AttendanceDto>>.Success(_mapper.Map<IReadOnlyList<AttendanceDto>>(records));
    }
}

