namespace HotelBooking.Business.DTOs;

// ── Hotel ──
public record HotelDto
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string City { get; init; } = "";
    public string Address { get; init; } = "";
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string PhoneNumber { get; init; } = "";
    public string Email { get; init; } = "";
    public string ImageUrl { get; init; } = "";
    public string[] Gallery { get; init; } = Array.Empty<string>();
    public int StarRating { get; init; }
    public bool IsActive { get; init; }
    public int RoomCount { get; init; }
    public decimal? MinPricePerNight { get; init; }
}

public record CreateHotelDto
{
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Address { get; init; } = "";
    public string City { get; init; } = "";
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string PhoneNumber { get; init; } = "";
    public string Email { get; init; } = "";
    public string ImageUrl { get; init; } = "";
    public int StarRating { get; init; } = 3;
}

public record UpdateHotelDto
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Description { get; init; } = "";
    public string Address { get; init; } = "";
    public string City { get; init; } = "";
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string PhoneNumber { get; init; } = "";
    public string Email { get; init; } = "";
    public string ImageUrl { get; init; } = "";
    public int StarRating { get; init; } = 3;
    public bool IsActive { get; init; } = true;
}

public record HotelStaffDto
{
    public int Id { get; init; }
    public int HotelId { get; init; }
    public string HotelName { get; init; } = "";
    public string UserId { get; init; } = "";
    public string UserName { get; init; } = "";
    public string Role { get; init; } = "";
    public DateTime AssignedAt { get; init; }
}

public record AssignStaffDto
{
    public int HotelId { get; init; }
    public string UserId { get; init; } = "";
    public string Role { get; init; } = "";
}

// ── Room ──
public record RoomDto
{
    public int Id { get; init; }
    public int HotelId { get; init; }
    public string HotelName { get; init; } = "";
    public double? HotelLatitude { get; init; }
    public double? HotelLongitude { get; init; }
    public string Name { get; init; } = "";
    public int RoomTypeId { get; init; }
    public string RoomTypeName { get; init; } = "";
    public decimal PricePerNight { get; init; }
    public int MaxOccupancy { get; init; }
    public string Description { get; init; } = "";
    public string ImageUrl { get; init; } = "";
    public string[] Gallery { get; init; } = Array.Empty<string>();
    public string Amenities { get; init; } = "";
    public bool IsAvailable { get; init; }
    public double AverageRating { get; init; }
    public int ReviewCount { get; init; }
}

public record RoomListDto
{
    public int Id { get; init; }
    public int HotelId { get; init; }
    public string HotelName { get; init; } = "";
    public string Name { get; init; } = "";
    public string RoomTypeName { get; init; } = "";
    public decimal PricePerNight { get; init; }
    public int MaxOccupancy { get; init; }
    public string ImageUrl { get; init; } = "";
    public string[] Gallery { get; init; } = Array.Empty<string>();
    public bool IsAvailable { get; init; }
    public double AverageRating { get; init; }
}

public record CreateRoomDto
{
    public int HotelId { get; init; }
    public string Name { get; init; } = "";
    public int RoomTypeId { get; init; }
    public decimal PricePerNight { get; init; }
    public int MaxOccupancy { get; init; }
    public string Description { get; init; } = "";
    public string ImageUrl { get; init; } = "";
    public string[] Gallery { get; init; } = Array.Empty<string>();
    public string Amenities { get; init; } = "";
}

public record UpdateRoomDto
{
    public int Id { get; init; }
    public int HotelId { get; init; }
    public string Name { get; init; } = "";
    public int RoomTypeId { get; init; }
    public decimal PricePerNight { get; init; }
    public int MaxOccupancy { get; init; }
    public string Description { get; init; } = "";
    public string ImageUrl { get; init; } = "";
    public string[] Gallery { get; init; } = Array.Empty<string>();
    public string Amenities { get; init; } = "";
    public bool IsAvailable { get; init; }
}

// ── RoomType ──
public record RoomTypeDto { public int Id { get; init; } public string Name { get; init; } = ""; public string Description { get; init; } = ""; }

// ── Booking ──
public record BookingDto { public int Id { get; init; } public int HotelId { get; init; } public string HotelName { get; init; } = ""; public int RoomId { get; init; } public string RoomName { get; init; } = ""; public string RoomTypeName { get; init; } = ""; public string UserId { get; init; } = ""; public string UserName { get; init; } = ""; public DateTime CheckIn { get; init; } public DateTime CheckOut { get; init; } public int NumberOfGuests { get; init; } public decimal TotalPrice { get; init; } public string Status { get; init; } = ""; public DateTime CreatedAt { get; init; } public string? GuestNotes { get; init; } public string? CancellationReason { get; init; } public DateTime? ConfirmedAt { get; init; } public DateTime? PaymentDeadline { get; init; } public DateTime? CheckedInAt { get; init; } public DateTime? CheckedOutAt { get; init; } public DateTime? CancelledAt { get; init; } public decimal? RefundAmount { get; init; } public DateTime? RefundedAt { get; init; } public decimal ExtraChargeAmount { get; init; } public string? ExtraChargeDescription { get; init; } public bool IsExtraChargePaid { get; init; } public string? LostAndFoundNotes { get; init; } public string? LostAndFoundImageUrl { get; init; } }
public record CreateBookingDto { public int RoomId { get; init; } public DateTime CheckIn { get; init; } public DateTime CheckOut { get; init; } public int NumberOfGuests { get; init; } public string? GuestNotes { get; init; } }
public record CheckoutBookingDto { public decimal ExtraChargeAmount { get; init; } public string? ExtraChargeDescription { get; init; } public string? LostAndFoundNotes { get; init; } public string? LostAndFoundImageUrl { get; init; } }

// ── Payment ──
public record PaymentDto { public int Id { get; init; } public int BookingId { get; init; } public decimal Amount { get; init; } public string Method { get; init; } = ""; public string Status { get; init; } = ""; public DateTime? PaidAt { get; init; } public decimal RefundAmount { get; init; } public DateTime? RefundedAt { get; init; } }
public record CreatePaymentDto { public int BookingId { get; init; } public string Method { get; init; } = ""; }

// ── Review ──
public record ReviewDto { public int Id { get; init; } public int RoomId { get; init; } public string UserName { get; init; } = ""; public int Rating { get; init; } public string Content { get; init; } = ""; public DateTime CreatedAt { get; init; } public List<ReviewCommentDto> Comments { get; init; } = new(); }
public record CreateReviewDto { public int RoomId { get; init; } public int Rating { get; init; } public string Content { get; init; } = ""; }
public record UpdateReviewDto { public int Id { get; init; } public int Rating { get; init; } public string Content { get; init; } = ""; }

// ── ReviewComment ──
public record ReviewCommentDto { public int Id { get; init; } public int ReviewId { get; init; } public string UserName { get; init; } = ""; public string Content { get; init; } = ""; public DateTime CreatedAt { get; init; } }
public record CreateReviewCommentDto { public int ReviewId { get; init; } public string Content { get; init; } = ""; }

// ── SupportTicket ──
public record TicketDto { public int Id { get; init; } public string UserId { get; init; } = ""; public string UserName { get; init; } = ""; public string? AssignedToName { get; init; } public string Category { get; init; } = ""; public string Priority { get; init; } = ""; public string Subject { get; init; } = ""; public string Description { get; init; } = ""; public string Status { get; init; } = ""; public DateTime CreatedAt { get; init; } public DateTime? ClosedAt { get; init; } }
public record CreateTicketDto { public string Category { get; init; } = ""; public string Subject { get; init; } = ""; public string Description { get; init; } = ""; }
public record UpdateTicketStatusDto { public int Id { get; init; } public string Status { get; init; } = ""; }

// ── AI ──
public record RoomPreferenceDto { public decimal? MaxBudget { get; init; } public int? GuestCount { get; init; } public List<string>? DesiredAmenities { get; init; } public DateTime? CheckIn { get; init; } public DateTime? CheckOut { get; init; } }

// ── HR: Employee ──
public record EmployeeListItemDto
{
    public int Id { get; init; }
    public string UserId { get; init; } = "";
    public string FullName { get; init; } = "";
    public string Email { get; init; } = "";
    public string PhoneNumber { get; init; } = "";
    public string Position { get; init; } = "";
    public string EmploymentType { get; init; } = "";
    public string Status { get; init; } = "";
    public int? HotelId { get; init; }
    public string? HotelName { get; init; }
}

public record EmployeeDto
{
    public int Id { get; init; }
    public string UserId { get; init; } = "";
    public int? HotelId { get; init; }
    public string? HotelName { get; init; }
    public string FullName { get; init; } = "";
    public DateTime DateOfBirth { get; init; }
    public string Gender { get; init; } = "";
    public string PhoneNumber { get; init; } = "";
    public string Email { get; init; } = "";
    public string Address { get; init; } = "";
    public string IdentityNumber { get; init; } = "";
    public string Position { get; init; } = "";
    public DateTime HireDate { get; init; }
    public string EmploymentType { get; init; } = "";
    public string Status { get; init; } = "";
    public decimal BaseSalary { get; init; }
}

public record CreateEmployeeDto
{
    public string UserId { get; init; } = "";
    public int? HotelId { get; init; }
    public string FullName { get; init; } = "";
    public DateTime DateOfBirth { get; init; }
    public string Gender { get; init; } = "";
    public string PhoneNumber { get; init; } = "";
    public string Email { get; init; } = "";
    public string Address { get; init; } = "";
    public string IdentityNumber { get; init; } = "";
    public string Position { get; init; } = "";
    public DateTime HireDate { get; init; }
    public string EmploymentType { get; init; } = "";
    public decimal BaseSalary { get; init; }
}

public record UpdateEmployeeDto
{
    public int Id { get; init; }
    public int? HotelId { get; init; }
    public string FullName { get; init; } = "";
    public DateTime DateOfBirth { get; init; }
    public string Gender { get; init; } = "";
    public string PhoneNumber { get; init; } = "";
    public string Email { get; init; } = "";
    public string Address { get; init; } = "";
    public string IdentityNumber { get; init; } = "";
    public string Position { get; init; } = "";
    public DateTime HireDate { get; init; }
    public string EmploymentType { get; init; } = "";
    public decimal BaseSalary { get; init; }
    public string Status { get; init; } = "";
}

// ── HR: Shifts & Schedule ──
public record WorkShiftDto
{
    public int Id { get; init; }
    public int HotelId { get; init; }
    public string HotelName { get; init; } = "";
    public string Name { get; init; } = "";
    public TimeSpan StartTime { get; init; }
    public TimeSpan EndTime { get; init; }
    public bool IsOvernight { get; init; }
    public bool IsActive { get; init; }
}

public record CreateWorkShiftDto
{
    public int HotelId { get; init; }
    public string Name { get; init; } = "";
    public TimeSpan StartTime { get; init; }
    public TimeSpan EndTime { get; init; }
    public bool IsOvernight { get; init; }
}

public record UpdateWorkShiftDto
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public TimeSpan StartTime { get; init; }
    public TimeSpan EndTime { get; init; }
    public bool IsOvernight { get; init; }
    public bool IsActive { get; init; }
}

public record ShiftAssignmentDto
{
    public int Id { get; init; }
    public int EmployeeId { get; init; }
    public string EmployeeName { get; init; } = "";
    public int HotelId { get; init; }
    public string HotelName { get; init; } = "";
    public int WorkShiftId { get; init; }
    public string ShiftName { get; init; } = "";
    public DateTime ShiftDate { get; init; }
    public string Status { get; init; } = "";
    public string? Notes { get; init; }
}

public record CreateShiftAssignmentDto
{
    public int EmployeeId { get; init; }
    public int HotelId { get; init; }
    public int WorkShiftId { get; init; }
    public DateTime ShiftDate { get; init; }
    public string? Notes { get; init; }
}

// ── HR: Attendance ──
public record AttendanceDto
{
    public int Id { get; init; }
    public int EmployeeId { get; init; }
    public string EmployeeName { get; init; } = "";
    public int HotelId { get; init; }
    public string HotelName { get; init; } = "";
    public DateTime ShiftDate { get; init; }
    public int? WorkShiftId { get; init; }
    public string? ShiftName { get; init; }
    public DateTime? CheckInTime { get; init; }
    public DateTime? CheckOutTime { get; init; }
    public string Status { get; init; } = "";
    public double HoursWorked { get; init; }
    public string? Notes { get; init; }
}

public record RecordAttendanceDto
{
    public int EmployeeId { get; init; }
    public int HotelId { get; init; }
    public DateTime ShiftDate { get; init; }
    public int? WorkShiftId { get; init; }
    public DateTime? CheckInTime { get; init; }
    public DateTime? CheckOutTime { get; init; }
    public string? Notes { get; init; }
}

// ── HR: Payroll ──
public record PayrollPeriodDto
{
    public int Id { get; init; }
    public int HotelId { get; init; }
    public string HotelName { get; init; } = "";
    public string Name { get; init; } = "";
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string Status { get; init; } = "";
}

public record CreatePayrollPeriodDto
{
    public int HotelId { get; init; }
    public string Name { get; init; } = "";
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}

public record PayrollEntryDto
{
    public int Id { get; init; }
    public int PayrollPeriodId { get; init; }
    public int EmployeeId { get; init; }
    public string EmployeeName { get; init; } = "";
    public decimal BaseSalary { get; init; }
    public double TotalHours { get; init; }
    public decimal CalculatedSalary { get; init; }
    public string? Notes { get; init; }
}

// ── HR: Training ──
public record TrainingProgramDto
{
    public int Id { get; init; }
    public int? HotelId { get; init; }
    public string? HotelName { get; init; }
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IsMandatory { get; init; }
}

public record CreateTrainingProgramDto
{
    public int? HotelId { get; init; }
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IsMandatory { get; init; }
}

public record UpdateTrainingProgramDto
{
    public int Id { get; init; }
    public int? HotelId { get; init; }
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IsMandatory { get; init; }
}

public record TrainingEnrollmentDto
{
    public int Id { get; init; }
    public int TrainingProgramId { get; init; }
    public string TrainingTitle { get; init; } = "";
    public int EmployeeId { get; init; }
    public string EmployeeName { get; init; } = "";
    public string Status { get; init; } = "";
    public double? Score { get; init; }
    public string? Feedback { get; init; }
}

public record EnrollTrainingDto
{
    public int TrainingProgramId { get; init; }
    public int EmployeeId { get; init; }
}

// ── HR: Performance ──
public record PerformanceReviewDto
{
    public int Id { get; init; }
    public int EmployeeId { get; init; }
    public string EmployeeName { get; init; } = "";
    public string ReviewerId { get; init; } = "";
    public string ReviewerName { get; init; } = "";
    public int HotelId { get; init; }
    public string HotelName { get; init; } = "";
    public DateTime ReviewDate { get; init; }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public int OverallRating { get; init; }
    public string Strengths { get; init; } = "";
    public string Improvements { get; init; } = "";
    public string Goals { get; init; } = "";
}

public record CreatePerformanceReviewDto
{
    public int EmployeeId { get; init; }
    public int HotelId { get; init; }
    public DateTime ReviewDate { get; init; }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public int OverallRating { get; init; }
    public string Strengths { get; init; } = "";
    public string Improvements { get; init; } = "";
    public string Goals { get; init; } = "";
}

// ── HR: Legal & Insurance ──
public record EmploymentContractDto
{
    public int Id { get; init; }
    public int EmployeeId { get; init; }
    public string EmployeeName { get; init; } = "";
    public int HotelId { get; init; }
    public string HotelName { get; init; } = "";
    public string ContractNumber { get; init; } = "";
    public string ContractType { get; init; } = "";
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public decimal BaseSalary { get; init; }
    public bool InsuranceIncluded { get; init; }
    public string Status { get; init; } = "";
    public string? FileUrl { get; init; }
}

public record CreateEmploymentContractDto
{
    public int EmployeeId { get; init; }
    public int HotelId { get; init; }
    public string ContractNumber { get; init; } = "";
    public string ContractType { get; init; } = "";
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public decimal BaseSalary { get; init; }
    public bool InsuranceIncluded { get; init; }
    public string Status { get; init; } = "";
    public string? FileUrl { get; init; }
}

public record InsuranceRecordDto
{
    public int Id { get; init; }
    public int EmployeeId { get; init; }
    public string EmployeeName { get; init; } = "";
    public string ProviderName { get; init; } = "";
    public string PolicyNumber { get; init; } = "";
    public DateTime EffectiveDate { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public string? Notes { get; init; }
}

public record CreateInsuranceRecordDto
{
    public int EmployeeId { get; init; }
    public string ProviderName { get; init; } = "";
    public string PolicyNumber { get; init; } = "";
    public DateTime EffectiveDate { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public string? Notes { get; init; }
}
