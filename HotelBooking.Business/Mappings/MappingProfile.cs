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
            .ForMember(d => d.Gallery, o => o.MapFrom(_ => "[]"));

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

        // HR: Employee
        CreateMap<Employee, EmployeeListItemDto>()
            .ForMember(d => d.HotelName, o => o.MapFrom(s => s.Hotel != null ? s.Hotel.Name : null))
            .ForMember(d => d.EmploymentType, o => o.MapFrom(s => s.EmploymentType.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<Employee, EmployeeDto>()
            .ForMember(d => d.HotelName, o => o.MapFrom(s => s.Hotel != null ? s.Hotel.Name : null))
            .ForMember(d => d.EmploymentType, o => o.MapFrom(s => s.EmploymentType.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<CreateEmployeeDto, Employee>()
            .ForMember(d => d.EmploymentType, o => o.MapFrom(s => Enum.Parse<EmploymentType>(s.EmploymentType, true)))
            .ForMember(d => d.Status, o => o.MapFrom(_ => EmployeeStatus.Active));

        CreateMap<UpdateEmployeeDto, Employee>()
            .ForMember(d => d.EmploymentType, o => o.MapFrom(s => Enum.Parse<EmploymentType>(s.EmploymentType, true)))
            .ForMember(d => d.Status, o => o.MapFrom(s => Enum.Parse<EmployeeStatus>(s.Status, true)))
            .ForMember(d => d.UpdatedAt, o => o.MapFrom(_ => DateTime.UtcNow));

        // HR: WorkShift
        CreateMap<WorkShift, WorkShiftDto>()
            .ForMember(d => d.HotelName, o => o.MapFrom(s => s.Hotel != null ? s.Hotel.Name : ""));

        CreateMap<CreateWorkShiftDto, WorkShift>();
        CreateMap<UpdateWorkShiftDto, WorkShift>();

        // HR: ShiftAssignment
        CreateMap<EmployeeShiftAssignment, ShiftAssignmentDto>()
            .ForMember(d => d.EmployeeName, o => o.MapFrom(s => s.Employee != null ? s.Employee.FullName : ""))
            .ForMember(d => d.HotelName, o => o.MapFrom(s => s.Hotel != null ? s.Hotel.Name : ""))
            .ForMember(d => d.ShiftName, o => o.MapFrom(s => s.WorkShift != null ? s.WorkShift.Name : ""))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<CreateShiftAssignmentDto, EmployeeShiftAssignment>()
            .ForMember(d => d.Status, o => o.MapFrom(_ => ShiftAssignmentStatus.Planned));

        // HR: Attendance
        CreateMap<AttendanceRecord, AttendanceDto>()
            .ForMember(d => d.EmployeeName, o => o.MapFrom(s => s.Employee != null ? s.Employee.FullName : ""))
            .ForMember(d => d.HotelName, o => o.MapFrom(s => s.Hotel != null ? s.Hotel.Name : ""))
            .ForMember(d => d.ShiftName, o => o.MapFrom(s => s.WorkShift != null ? s.WorkShift.Name : null))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<RecordAttendanceDto, AttendanceRecord>()
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.HoursWorked, o => o.Ignore());

        // HR: Payroll
        CreateMap<PayrollPeriod, PayrollPeriodDto>()
            .ForMember(d => d.HotelName, o => o.MapFrom(s => s.Hotel != null ? s.Hotel.Name : ""))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<CreatePayrollPeriodDto, PayrollPeriod>()
            .ForMember(d => d.Status, o => o.MapFrom(_ => PayrollStatus.Open))
            .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.UpdatedAt, o => o.MapFrom(_ => DateTime.UtcNow));

        CreateMap<PayrollEntry, PayrollEntryDto>()
            .ForMember(d => d.EmployeeName, o => o.MapFrom(s => s.Employee != null ? s.Employee.FullName : ""));

        // HR: Training
        CreateMap<TrainingProgram, TrainingProgramDto>()
            .ForMember(d => d.HotelName, o => o.MapFrom(s => s.Hotel != null ? s.Hotel.Name : null));

        CreateMap<CreateTrainingProgramDto, TrainingProgram>();

        CreateMap<UpdateTrainingProgramDto, TrainingProgram>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.IsMandatory, o => o.MapFrom(s => s.IsMandatory));

        CreateMap<TrainingEnrollment, TrainingEnrollmentDto>()
            .ForMember(d => d.TrainingTitle, o => o.MapFrom(s => s.TrainingProgram != null ? s.TrainingProgram.Title : ""))
            .ForMember(d => d.EmployeeName, o => o.MapFrom(s => s.Employee != null ? s.Employee.FullName : ""))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<EnrollTrainingDto, TrainingEnrollment>()
            .ForMember(d => d.Status, o => o.MapFrom(_ => TrainingEnrollmentStatus.Enrolled));

        // HR: Performance
        CreateMap<PerformanceReview, PerformanceReviewDto>()
            .ForMember(d => d.EmployeeName, o => o.MapFrom(s => s.Employee != null ? s.Employee.FullName : ""))
            .ForMember(d => d.ReviewerName, o => o.MapFrom(s => s.Reviewer != null ? s.Reviewer.FullName : ""))
            .ForMember(d => d.HotelName, o => o.MapFrom(s => s.Hotel != null ? s.Hotel.Name : ""));

        CreateMap<CreatePerformanceReviewDto, PerformanceReview>();

        // HR: Legal & Insurance
        CreateMap<EmploymentContract, EmploymentContractDto>()
            .ForMember(d => d.EmployeeName, o => o.MapFrom(s => s.Employee != null ? s.Employee.FullName : ""))
            .ForMember(d => d.HotelName, o => o.MapFrom(s => s.Hotel != null ? s.Hotel.Name : ""))
            .ForMember(d => d.ContractType, o => o.MapFrom(s => s.ContractType.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<CreateEmploymentContractDto, EmploymentContract>()
            .ForMember(d => d.ContractType, o => o.MapFrom(s => Enum.Parse<ContractType>(s.ContractType, true)))
            .ForMember(d => d.Status, o => o.MapFrom(s => Enum.Parse<ContractStatus>(s.Status, true)));

        CreateMap<InsuranceRecord, InsuranceRecordDto>()
            .ForMember(d => d.EmployeeName, o => o.MapFrom(s => s.Employee != null ? s.Employee.FullName : ""));

        CreateMap<CreateInsuranceRecordDto, InsuranceRecord>();
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
