namespace HotelBooking.Data.Entities;

public class RoomType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Navigation
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
