namespace HotelBooking.Data.Entities;

public enum HotelStaffRole
{
    Manager,
    Receptionist,
    Housekeeping
}

public class HotelStaff
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public HotelStaffRole Role { get; set; } = HotelStaffRole.Receptionist;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public Hotel Hotel { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}

