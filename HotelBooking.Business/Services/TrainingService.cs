using AutoMapper;
using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using HotelBooking.Data.Repositories.Interfaces;

namespace HotelBooking.Business.Services;

public class TrainingService : ITrainingService
{
    private readonly ITrainingProgramRepository _programRepo;
    private readonly ITrainingEnrollmentRepository _enrollmentRepo;
    private readonly IEmployeeRepository _employeeRepo;
    private readonly IMapper _mapper;

    public TrainingService(
        ITrainingProgramRepository programRepo,
        ITrainingEnrollmentRepository enrollmentRepo,
        IEmployeeRepository employeeRepo,
        IMapper mapper)
    {
        _programRepo = programRepo;
        _enrollmentRepo = enrollmentRepo;
        _employeeRepo = employeeRepo;
        _mapper = mapper;
    }

    public async Task<ServiceResult<IReadOnlyList<TrainingProgramDto>>> GetTrainingProgramsAsync(int? hotelId, CancellationToken ct = default)
    {
        var programs = await _programRepo.GetByHotelOrGlobalAsync(hotelId, ct);
        return ServiceResult<IReadOnlyList<TrainingProgramDto>>.Success(_mapper.Map<IReadOnlyList<TrainingProgramDto>>(programs));
    }

    public async Task<ServiceResult<TrainingProgramDto>> CreateTrainingProgramAsync(CreateTrainingProgramDto dto, CancellationToken ct = default)
    {
        var program = _mapper.Map<Data.Entities.TrainingProgram>(dto);
        await _programRepo.AddAsync(program, ct);
        return ServiceResult<TrainingProgramDto>.Success(_mapper.Map<TrainingProgramDto>(program));
    }

    public async Task<ServiceResult<TrainingProgramDto>> GetTrainingProgramByIdAsync(int id, CancellationToken ct = default)
    {
        var program = await _programRepo.GetByIdAsync(id, ct);
        if (program is null)
        {
            return ServiceResult<TrainingProgramDto>.Failure("Training program not found", "NOT_FOUND");
        }

        return ServiceResult<TrainingProgramDto>.Success(_mapper.Map<TrainingProgramDto>(program));
    }

    public async Task<ServiceResult<TrainingProgramDto>> UpdateTrainingProgramAsync(UpdateTrainingProgramDto dto, CancellationToken ct = default)
    {
        var program = await _programRepo.GetByIdAsync(dto.Id, ct);
        if (program is null)
        {
            return ServiceResult<TrainingProgramDto>.Failure("Training program not found", "NOT_FOUND");
        }

        _mapper.Map(dto, program);
        await _programRepo.UpdateAsync(program, ct);
        return ServiceResult<TrainingProgramDto>.Success(_mapper.Map<TrainingProgramDto>(program));
    }

    public async Task<ServiceResult<IReadOnlyList<TrainingEnrollmentDto>>> GetEnrollmentsByEmployeeAsync(int employeeId, CancellationToken ct = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(employeeId, ct);
        if (employee is null)
        {
            return ServiceResult<IReadOnlyList<TrainingEnrollmentDto>>.Failure("Employee not found", "NOT_FOUND");
        }

        var enrollments = await _enrollmentRepo.GetByEmployeeAsync(employeeId, ct);
        return ServiceResult<IReadOnlyList<TrainingEnrollmentDto>>.Success(_mapper.Map<IReadOnlyList<TrainingEnrollmentDto>>(enrollments));
    }

    public async Task<ServiceResult<TrainingEnrollmentDto>> EnrollEmployeeAsync(EnrollTrainingDto dto, CancellationToken ct = default)
    {
        var employee = await _employeeRepo.GetByIdAsync(dto.EmployeeId, ct);
        if (employee is null)
        {
            return ServiceResult<TrainingEnrollmentDto>.Failure("Employee not found", "NOT_FOUND");
        }

        var program = await _programRepo.GetByIdAsync(dto.TrainingProgramId, ct);
        if (program is null)
        {
            return ServiceResult<TrainingEnrollmentDto>.Failure("Program not found", "NOT_FOUND");
        }

        var existing = await _enrollmentRepo.GetByEmployeeAsync(dto.EmployeeId, ct);
        if (existing.Any(e => e.TrainingProgramId == dto.TrainingProgramId))
        {
            return ServiceResult<TrainingEnrollmentDto>.Failure("Employee already enrolled in this program", "DUPLICATE");
        }

        var enrollment = _mapper.Map<Data.Entities.TrainingEnrollment>(dto);
        await _enrollmentRepo.AddAsync(enrollment, ct);

        var reloaded = (await _enrollmentRepo.GetByEmployeeAsync(dto.EmployeeId, ct)).First(e => e.Id == enrollment.Id);
        return ServiceResult<TrainingEnrollmentDto>.Success(_mapper.Map<TrainingEnrollmentDto>(reloaded));
    }
}

