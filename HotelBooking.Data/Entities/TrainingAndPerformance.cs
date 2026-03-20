namespace HotelBooking.Data.Entities;

public enum TrainingEnrollmentStatus
{
    Enrolled,
    Completed,
    Failed,
    Cancelled
}

public class TrainingProgram
{
    public int Id { get; set; }
    public int? HotelId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsMandatory { get; set; }

    public Hotel? Hotel { get; set; }
    public ICollection<TrainingEnrollment> Enrollments { get; set; } = new List<TrainingEnrollment>();
}

public class TrainingEnrollment
{
    public int Id { get; set; }
    public int TrainingProgramId { get; set; }
    public int EmployeeId { get; set; }
    public TrainingEnrollmentStatus Status { get; set; } = TrainingEnrollmentStatus.Enrolled;
    public double? Score { get; set; }
    public string? Feedback { get; set; }

    public TrainingProgram TrainingProgram { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
}

public class PerformanceReview
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string ReviewerId { get; set; } = string.Empty;
    public int HotelId { get; set; }
    public DateTime ReviewDate { get; set; } = DateTime.UtcNow;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int OverallRating { get; set; }
    public string Strengths { get; set; } = string.Empty;
    public string Improvements { get; set; } = string.Empty;
    public string Goals { get; set; } = string.Empty;

    public Employee Employee { get; set; } = null!;
    public ApplicationUser Reviewer { get; set; } = null!;
    public Hotel Hotel { get; set; } = null!;
}

