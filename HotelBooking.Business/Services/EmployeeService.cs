using AutoMapper;
using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using HotelBooking.Data.Entities;
using HotelBooking.Data.Repositories.Interfaces;

namespace HotelBooking.Business.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IHotelRepository _hotelRepo;
    private readonly IMapper _mapper;

    public EmployeeService(IEmployeeRepository employeeRepo, IHotelRepository hotelRepo, IMapper mapper)
    {
        _employeeRepo = employeeRepo;
        _hotelRepo = hotelRepo;
        _mapper = mapper;
    }

    public async Task<ServiceResult<IReadOnlyList<EmployeeListItemDto>>> GetEmployeesByHotelAsync(int hotelId, CancellationToken ct = default)
    {
        var hotel = await _hotelRepo.GetByIdAsync(hotelId, ct);
        if (hotel is null)
        {
            return ServiceResult<IReadOnlyList<EmployeeListItemDto>>.Failure("Hotel not found", "NOT_FOUND");
        }

        var employees = await _employeeRepo.GetByHotelAsync(hotelId, ct);
        var dtos = _mapper.Map<IReadOnlyList<EmployeeListItemDto>>(employees);
        return ServiceResult<IReadOnlyList<EmployeeListItemDto>>.Success(dtos);
    }

    public async Task<ServiceResult<EmployeeDto>> GetEmployeeByIdAsync(int id, CancellationToken ct = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(id, ct);
        if (employee is null)
        {
            return ServiceResult<EmployeeDto>.Failure("Employee not found", "NOT_FOUND");
        }

        return ServiceResult<EmployeeDto>.Success(_mapper.Map<EmployeeDto>(employee));
    }

    public async Task<ServiceResult<EmployeeDto>> CreateEmployeeAsync(CreateEmployeeDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.UserId))
        {
            return ServiceResult<EmployeeDto>.Failure("UserId is required", "VALIDATION");
        }

        var existing = await _employeeRepo.GetByUserIdAsync(dto.UserId, ct);
        if (existing is not null)
        {
            return ServiceResult<EmployeeDto>.Failure("Employee profile already exists for this user", "DUPLICATE");
        }

        if (dto.HotelId.HasValue)
        {
            var hotel = await _hotelRepo.GetByIdAsync(dto.HotelId.Value, ct);
            if (hotel is null)
            {
                return ServiceResult<EmployeeDto>.Failure("Hotel not found", "NOT_FOUND");
            }
        }

        var employee = _mapper.Map<Employee>(dto);
        employee.CreatedAt = DateTime.UtcNow;
        employee.UpdatedAt = DateTime.UtcNow;

        await _employeeRepo.AddAsync(employee, ct);
        return await GetEmployeeByIdAsync(employee.Id, ct);
    }

    public async Task<ServiceResult<EmployeeDto>> UpdateEmployeeAsync(UpdateEmployeeDto dto, CancellationToken ct = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(dto.Id, ct);
        if (employee is null)
        {
            return ServiceResult<EmployeeDto>.Failure("Employee not found", "NOT_FOUND");
        }

        if (dto.HotelId.HasValue)
        {
            var hotel = await _hotelRepo.GetByIdAsync(dto.HotelId.Value, ct);
            if (hotel is null)
            {
                return ServiceResult<EmployeeDto>.Failure("Hotel not found", "NOT_FOUND");
            }
        }

        _mapper.Map(dto, employee);
        employee.UpdatedAt = DateTime.UtcNow;
        await _employeeRepo.UpdateAsync(employee, ct);

        return await GetEmployeeByIdAsync(employee.Id, ct);
    }

    public async Task<ServiceResult> DeactivateEmployeeAsync(int id, CancellationToken ct = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(id, ct);
        if (employee is null)
        {
            return ServiceResult.Failure("Employee not found", "NOT_FOUND");
        }

        employee.Status = EmployeeStatus.Inactive;
        employee.UpdatedAt = DateTime.UtcNow;
        await _employeeRepo.UpdateAsync(employee, ct);

        return ServiceResult.Success();
    }
}

