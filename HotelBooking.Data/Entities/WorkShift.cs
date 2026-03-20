namespace HotelBooking.Data.Entities;

public class WorkShift
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsOvernight { get; set; }
    public bool IsActive { get; set; } = true;

    public Hotel Hotel { get; set; } = null!;
    public ICollection<EmployeeShiftAssignment> ShiftAssignments { get; set; } = new List<EmployeeShiftAssignment>();
}

