using AutoMapper;
using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using HotelBooking.Data.Repositories.Interfaces;

namespace HotelBooking.Business.Services;

public class LegalComplianceService : ILegalComplianceService
{
    private readonly IEmploymentContractRepository _contractRepo;
    private readonly IInsuranceRecordRepository _insuranceRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IMapper _mapper;

    public LegalComplianceService(
        IEmploymentContractRepository contractRepo,
        IInsuranceRecordRepository insuranceRepo,
        IEmployeeRepository employeeRepo,
        IMapper mapper)
    {
        _contractRepo = contractRepo;
        _insuranceRepo = insuranceRepo;
        _employeeRepo = employeeRepo;
        _mapper = mapper;
    }

    public async Task<ServiceResult<IReadOnlyList<EmploymentContractDto>>> GetContractsByEmployeeAsync(int employeeId, CancellationToken ct = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(employeeId, ct);
        if (employee is null)
        {
            return ServiceResult<IReadOnlyList<EmploymentContractDto>>.Failure("Employee not found", "NOT_FOUND");
        }

        var contracts = await _contractRepo.GetByEmployeeAsync(employeeId, ct);
        return ServiceResult<IReadOnlyList<EmploymentContractDto>>.Success(_mapper.Map<IReadOnlyList<EmploymentContractDto>>(contracts));
    }

    public async Task<ServiceResult<EmploymentContractDto>> CreateContractAsync(CreateEmploymentContractDto dto, CancellationToken ct = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(dto.EmployeeId, ct);
        if (employee is null)
        {
            return ServiceResult<EmploymentContractDto>.Failure("Employee not found", "NOT_FOUND");
        }

        var contracts = _mapper.Map<Data.Entities.EmploymentContract>(dto);
        await _contractRepo.AddAsync(contracts, ct);

        var reloaded = (await _contractRepo.GetByEmployeeAsync(dto.EmployeeId, ct)).First(c => c.Id == contracts.Id);
        return ServiceResult<EmploymentContractDto>.Success(_mapper.Map<EmploymentContractDto>(reloaded));
    }

    public async Task<ServiceResult<IReadOnlyList<InsuranceRecordDto>>> GetInsuranceByEmployeeAsync(int employeeId, CancellationToken ct = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(employeeId, ct);
        if (employee is null)
        {
            return ServiceResult<IReadOnlyList<InsuranceRecordDto>>.Failure("Employee not found", "NOT_FOUND");
        }

        var records = await _insuranceRepo.GetByEmployeeAsync(employeeId, ct);
        return ServiceResult<IReadOnlyList<InsuranceRecordDto>>.Success(_mapper.Map<IReadOnlyList<InsuranceRecordDto>>(records));
    }

    public async Task<ServiceResult<InsuranceRecordDto>> CreateInsuranceRecordAsync(CreateInsuranceRecordDto dto, CancellationToken ct = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(dto.EmployeeId, ct);
        if (employee is null)
        {
            return ServiceResult<InsuranceRecordDto>.Failure("Employee not found", "NOT_FOUND");
        }

        var record = _mapper.Map<Data.Entities.InsuranceRecord>(dto);
        await _insuranceRepo.AddAsync(record, ct);

        var reloaded = (await _insuranceRepo.GetByEmployeeAsync(dto.EmployeeId, ct)).First(r => r.Id == record.Id);
        return ServiceResult<InsuranceRecordDto>.Success(_mapper.Map<InsuranceRecordDto>(reloaded));
    }
}

