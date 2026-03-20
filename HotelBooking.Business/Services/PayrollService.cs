using AutoMapper;
using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using HotelBooking.Data.Entities;
using HotelBooking.Data.Repositories.Interfaces;

namespace HotelBooking.Business.Services;

public class PayrollService : IPayrollService
{
    private readonly IPayrollPeriodRepository _payrollRepo;
    private readonly IAttendanceRepository _attendanceRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IHotelRepository _hotelRepo;
    private readonly IMapper _mapper;

    public PayrollService(
        IPayrollPeriodRepository payrollRepo,
        IAttendanceRepository attendanceRepo,
        IEmployeeRepository employeeRepo,
        IHotelRepository hotelRepo,
        IMapper mapper)
    {
        _payrollRepo = payrollRepo;
        _attendanceRepo = attendanceRepo;
        _employeeRepo = employeeRepo;
        _hotelRepo = hotelRepo;
        _mapper = mapper;
    }

    public async Task<ServiceResult<IReadOnlyList<PayrollPeriodDto>>> GetPayrollPeriodsByHotelAsync(int hotelId, CancellationToken ct = default)
    {
        var hotel = await _hotelRepo.GetByIdAsync(hotelId, ct);
        if (hotel is null)
        {
            return ServiceResult<IReadOnlyList<PayrollPeriodDto>>.Failure("Hotel not found", "NOT_FOUND");
        }

        var periods = await _payrollRepo.GetByHotelAsync(hotelId, ct);
        return ServiceResult<IReadOnlyList<PayrollPeriodDto>>.Success(_mapper.Map<IReadOnlyList<PayrollPeriodDto>>(periods));
    }

    public async Task<ServiceResult<PayrollPeriodDto>> CreatePayrollPeriodAsync(CreatePayrollPeriodDto dto, CancellationToken ct = default)
    {
        var hotel = await _hotelRepo.GetByIdAsync(dto.HotelId, ct);
        if (hotel is null)
        {
            return ServiceResult<PayrollPeriodDto>.Failure("Hotel not found", "NOT_FOUND");
        }

        if (dto.EndDate.Date <= dto.StartDate.Date)
        {
            return ServiceResult<PayrollPeriodDto>.Failure("EndDate must be after StartDate", "VALIDATION");
        }

        var period = _mapper.Map<PayrollPeriod>(dto);
        await _payrollRepo.AddAsync(period, ct);
        return ServiceResult<PayrollPeriodDto>.Success(_mapper.Map<PayrollPeriodDto>(period));
    }

    public async Task<ServiceResult> CalculatePayrollAsync(int payrollPeriodId, CancellationToken ct = default)
    {
        var period = await _payrollRepo.GetWithEntriesAsync(payrollPeriodId, ct);
        if (period is null)
        {
            return ServiceResult.Failure("Payroll period not found", "NOT_FOUND");
        }

        if (period.Status != PayrollStatus.Open && period.Status != PayrollStatus.Calculated)
        {
            return ServiceResult.Failure("Payroll can only be calculated for Open/Calculated periods", "INVALID_STATE");
        }

        var start = period.StartDate.Date;
        var end = period.EndDate.Date;

        // attendance and employees in hotel
        var employees = await _employeeRepo.GetByHotelAsync(period.HotelId, ct);
        var attendance = await _attendanceRepo.GetByHotelAndDateRangeAsync(period.HotelId, start, end, ct);

        // clear existing entries
        period.Entries.Clear();

        foreach (var employee in employees)
        {
            var empAttendance = attendance.Where(a => a.EmployeeId == employee.Id).ToList();
            var totalHours = empAttendance.Sum(a => a.HoursWorked);

            decimal calculatedSalary;
            if (employee.EmploymentType == EmploymentType.FullTime)
            {
                // Simple rule: full salary if present at least 80% of days; otherwise prorate by presence ratio
                var totalDays = (end - start).TotalDays + 1;
                var presentDays = empAttendance.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late);
                var ratio = totalDays > 0 ? presentDays / totalDays : 0;
                if (ratio >= 0.8)
                {
                    calculatedSalary = employee.BaseSalary;
                }
                else
                {
                    calculatedSalary = employee.BaseSalary * (decimal)ratio;
                }
            }
            else
            {
                // PartTime/Casual: BaseSalary treated as hourly rate
                calculatedSalary = employee.BaseSalary * (decimal)totalHours;
            }

            period.Entries.Add(new PayrollEntry
            {
                EmployeeId = employee.Id,
                BaseSalary = employee.BaseSalary,
                TotalHours = totalHours,
                CalculatedSalary = calculatedSalary
            });
        }

        period.Status = PayrollStatus.Calculated;
        period.UpdatedAt = DateTime.UtcNow;
        await _payrollRepo.UpdateAsync(period, ct);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<IReadOnlyList<PayrollEntryDto>>> GetPayrollEntriesAsync(int payrollPeriodId, CancellationToken ct = default)
    {
        var period = await _payrollRepo.GetWithEntriesAsync(payrollPeriodId, ct);
        if (period is null)
        {
            return ServiceResult<IReadOnlyList<PayrollEntryDto>>.Failure("Payroll period not found", "NOT_FOUND");
        }

        var dtos = _mapper.Map<IReadOnlyList<PayrollEntryDto>>(period.Entries);
        return ServiceResult<IReadOnlyList<PayrollEntryDto>>.Success(dtos);
    }

    public async Task<ServiceResult> ApprovePayrollAsync(int payrollPeriodId, CancellationToken ct = default)
    {
        var period = await _payrollRepo.GetByIdAsync(payrollPeriodId, ct);
        if (period is null)
        {
            return ServiceResult.Failure("Payroll period not found", "NOT_FOUND");
        }

        if (period.Status != PayrollStatus.Calculated)
        {
            return ServiceResult.Failure("Only calculated payroll can be approved", "INVALID_STATE");
        }

        period.Status = PayrollStatus.Approved;
        period.UpdatedAt = DateTime.UtcNow;
        await _payrollRepo.UpdateAsync(period, ct);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> MarkPayrollPaidAsync(int payrollPeriodId, CancellationToken ct = default)
    {
        var period = await _payrollRepo.GetByIdAsync(payrollPeriodId, ct);
        if (period is null)
        {
            return ServiceResult.Failure("Payroll period not found", "NOT_FOUND");
        }

        if (period.Status != PayrollStatus.Approved)
        {
            return ServiceResult.Failure("Only approved payroll can be marked as paid", "INVALID_STATE");
        }

        period.Status = PayrollStatus.Paid;
        period.UpdatedAt = DateTime.UtcNow;
        await _payrollRepo.UpdateAsync(period, ct);

        return ServiceResult.Success();
    }
}

