namespace HotelBooking.Data.Entities;

public enum EmploymentType
{
    FullTime,
    PartTime,
    Casual
}

public enum EmployeeStatus
{
    Active,
    Inactive
}

public class Employee
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public int? HotelId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string IdentityNumber { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateTime HireDate { get; set; } = DateTime.UtcNow;
    public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    public decimal BaseSalary { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Hotel? Hotel { get; set; }
    public ApplicationUser? User { get; set; }

    public ICollection<EmployeeShiftAssignment> ShiftAssignments { get; set; } = new List<EmployeeShiftAssignment>();
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    public ICollection<PayrollEntry> PayrollEntries { get; set; } = new List<PayrollEntry>();
    public ICollection<TrainingEnrollment> TrainingEnrollments { get; set; } = new List<TrainingEnrollment>();
    public ICollection<PerformanceReview> PerformanceReviews { get; set; } = new List<PerformanceReview>();
    public ICollection<EmploymentContract> Contracts { get; set; } = new List<EmploymentContract>();
    public ICollection<InsuranceRecord> InsuranceRecords { get; set; } = new List<InsuranceRecord>();
}

