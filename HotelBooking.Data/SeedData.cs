using HotelBooking.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HotelBooking.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        // Seed roles
        string[] roles = ["Admin", "Staff", "Customer"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed users
        var admin = await EnsureUserAsync(userManager, "admin@hotel.com", "Hotel@123", "Admin User", "Admin");
        var staff = await EnsureUserAsync(userManager, "staff@hotel.com", "Hotel@123", "Staff Member", "Staff");
        var customer = await EnsureUserAsync(userManager, "customer@hotel.com", "Hotel@123", "John Customer", "Customer");
        var customer2 = await EnsureUserAsync(userManager, "customer2@hotel.com", "Hotel@123", "Alice Smith", "Customer");
        var customer3 = await EnsureUserAsync(userManager, "customer3@hotel.com", "Hotel@123", "Bob Johnson", "Customer");

        if (await context.RoomTypes.AnyAsync()) return; // Already seeded

        // Seed room types
        var roomTypes = new List<RoomType>
        {
            new() { Name = "Standard", Description = "Comfortable room with essential amenities" },
            new() { Name = "Superior", Description = "Upgraded room with city or garden view" },
            new() { Name = "Deluxe", Description = "Spacious room with premium furnishings" },
            new() { Name = "Suite", Description = "Separate living area with luxury amenities" },
            new() { Name = "Presidential", Description = "The finest accommodation with panoramic views" }
        };
        context.RoomTypes.AddRange(roomTypes);
        await context.SaveChangesAsync();

        // Seed rooms
        var rooms = new List<Room>
        {
            new() { Name = "Standard Room 101", RoomTypeId = roomTypes[0].Id, PricePerNight = 80, MaxOccupancy = 2, Description = "Cozy room on the first floor with garden view.", ImageUrl = "/images/rooms/standard1.jpg", Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\"]" },
            new() { Name = "Standard Room 102", RoomTypeId = roomTypes[0].Id, PricePerNight = 80, MaxOccupancy = 2, Description = "Comfortable standard room with modern decor.", ImageUrl = "/images/rooms/standard2.jpg", Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\"]" },
            new() { Name = "Standard Room 201", RoomTypeId = roomTypes[0].Id, PricePerNight = 85, MaxOccupancy = 2, Description = "Second floor standard with quiet atmosphere.", ImageUrl = "/images/rooms/standard3.jpg", Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\",\"Mini Fridge\"]" },
            new() { Name = "Standard Room 202", RoomTypeId = roomTypes[0].Id, PricePerNight = 85, MaxOccupancy = 3, Description = "Family-friendly standard room.", ImageUrl = "/images/rooms/standard4.jpg", Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\",\"Mini Fridge\"]" },
            new() { Name = "Superior Room 301", RoomTypeId = roomTypes[1].Id, PricePerNight = 130, MaxOccupancy = 2, Description = "Elegant room with stunning city skyline view.", ImageUrl = "/images/rooms/superior1.jpg", Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\",\"Mini Bar\",\"Bathrobe\"]" },
            new() { Name = "Superior Room 302", RoomTypeId = roomTypes[1].Id, PricePerNight = 130, MaxOccupancy = 2, Description = "Refined superior room overlooking the garden.", ImageUrl = "/images/rooms/superior2.jpg", Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\",\"Mini Bar\",\"Bathrobe\"]" },
            new() { Name = "Superior Room 303", RoomTypeId = roomTypes[1].Id, PricePerNight = 140, MaxOccupancy = 3, Description = "Spacious superior room ideal for small families.", ImageUrl = "/images/rooms/superior3.jpg", Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\",\"Mini Bar\",\"Coffee Machine\"]" },
            new() { Name = "Superior Room 304", RoomTypeId = roomTypes[1].Id, PricePerNight = 140, MaxOccupancy = 3, Description = "Modern superior room with balcony access.", ImageUrl = "/images/rooms/superior4.jpg", Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\",\"Mini Bar\",\"Balcony\"]" },
            new() { Name = "Deluxe Room 401", RoomTypeId = roomTypes[2].Id, PricePerNight = 220, MaxOccupancy = 2, Description = "Luxurious room with king-size bed and marble bathroom.", ImageUrl = "/images/rooms/deluxe1.jpg", Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\",\"Mini Bar\",\"Bathrobe\",\"Jacuzzi\"]" },
            new() { Name = "Deluxe Room 402", RoomTypeId = roomTypes[2].Id, PricePerNight = 220, MaxOccupancy = 2, Description = "Premium deluxe with ocean-inspired decor.", ImageUrl = "/images/rooms/deluxe2.jpg", Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\",\"Mini Bar\",\"Bathrobe\",\"Workspace\"]" },
            new() { Name = "Deluxe Room 403", RoomTypeId = roomTypes[2].Id, PricePerNight = 250, MaxOccupancy = 3, Description = "Family deluxe with connecting rooms available.", ImageUrl = "/images/rooms/deluxe3.jpg", Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\",\"Mini Bar\",\"Bathrobe\",\"Jacuzzi\",\"Balcony\"]" },
            new() { Name = "Deluxe Room 404", RoomTypeId = roomTypes[2].Id, PricePerNight = 250, MaxOccupancy = 4, Description = "Grand deluxe perfect for group stays.", ImageUrl = "/images/rooms/deluxe4.jpg", Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\",\"Mini Bar\",\"Bathrobe\",\"Jacuzzi\",\"Living Area\"]" },
            new() { Name = "Suite 501", RoomTypeId = roomTypes[3].Id, PricePerNight = 380, MaxOccupancy = 3, Description = "Elegant suite with separate living and dining areas.", ImageUrl = "/images/rooms/suite1.jpg", Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\",\"Mini Bar\",\"Bathrobe\",\"Jacuzzi\",\"Living Room\",\"Kitchen\"]" },
            new() { Name = "Suite 502", RoomTypeId = roomTypes[3].Id, PricePerNight = 380, MaxOccupancy = 3, Description = "Modern suite with panoramic window wall.", ImageUrl = "/images/rooms/suite2.jpg", Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\",\"Mini Bar\",\"Bathrobe\",\"Panoramic View\",\"Living Room\"]" },
            new() { Name = "Suite 503", RoomTypeId = roomTypes[3].Id, PricePerNight = 420, MaxOccupancy = 4, Description = "Family suite with two bedrooms and lounge.", ImageUrl = "/images/rooms/suite3.jpg", Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\",\"Mini Bar\",\"Bathrobe\",\"Jacuzzi\",\"2 Bedrooms\",\"Lounge\"]" },
            new() { Name = "Suite 504", RoomTypeId = roomTypes[3].Id, PricePerNight = 420, MaxOccupancy = 4, Description = "Corner suite with wrap-around terrace.", ImageUrl = "/images/rooms/suite4.jpg", Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\",\"Mini Bar\",\"Bathrobe\",\"Terrace\",\"Living Room\",\"Dining\"]" },
            new() { Name = "Presidential Suite 601", RoomTypeId = roomTypes[4].Id, PricePerNight = 750, MaxOccupancy = 4, Description = "The ultimate in luxury. Private elevator, butler service.", ImageUrl = "/images/rooms/presidential1.jpg", Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\",\"Full Bar\",\"Bathrobe\",\"Jacuzzi\",\"Private Pool\",\"Butler\",\"3 Bedrooms\"]" },
            new() { Name = "Presidential Suite 602", RoomTypeId = roomTypes[4].Id, PricePerNight = 750, MaxOccupancy = 4, Description = "Penthouse suite with rooftop terrace and 360° views.", ImageUrl = "/images/rooms/presidential2.jpg", Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\",\"Full Bar\",\"Bathrobe\",\"Spa\",\"Rooftop Terrace\",\"Butler\",\"Grand Piano\"]" },
            new() { Name = "Presidential Suite 603", RoomTypeId = roomTypes[4].Id, PricePerNight = 900, MaxOccupancy = 6, Description = "Royal presidential suite for distinguished guests.", ImageUrl = "/images/rooms/presidential3.jpg", Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\",\"Full Bar\",\"Bathrobe\",\"Private Cinema\",\"Private Pool\",\"Butler\",\"4 Bedrooms\"]" },
            new() { Name = "Presidential Suite 604", RoomTypeId = roomTypes[4].Id, PricePerNight = 1200, MaxOccupancy = 8, Description = "The Crown Suite — our most exclusive accommodation.", ImageUrl = "/images/rooms/presidential4.jpg", Amenities = "[\"WiFi\",\"TV\",\"Air Conditioning\",\"Full Bar\",\"Bathrobe\",\"Helipad Access\",\"Private Pool\",\"Butler\",\"5 Bedrooms\",\"Private Gym\"]" },
        };
        context.Rooms.AddRange(rooms);
        await context.SaveChangesAsync();

        var bookings = new List<Booking>
        {
            // Customer 1
            new() { RoomId = rooms[0].Id, UserId = customer.Id, CheckIn = DateTime.Today.AddDays(-10), CheckOut = DateTime.Today.AddDays(-7), NumberOfGuests = 2, TotalPrice = 240, Status = BookingStatus.Completed },
            new() { RoomId = rooms[4].Id, UserId = customer.Id, CheckIn = DateTime.Today.AddDays(-5), CheckOut = DateTime.Today.AddDays(-2), NumberOfGuests = 2, TotalPrice = 390, Status = BookingStatus.Completed },
            new() { RoomId = rooms[8].Id, UserId = customer.Id, CheckIn = DateTime.Today.AddDays(5), CheckOut = DateTime.Today.AddDays(8), NumberOfGuests = 2, TotalPrice = 660, Status = BookingStatus.Confirmed },
            new() { RoomId = rooms[12].Id, UserId = customer.Id, CheckIn = DateTime.Today.AddDays(15), CheckOut = DateTime.Today.AddDays(18), NumberOfGuests = 3, TotalPrice = 1140, Status = BookingStatus.Pending },
            // Customer 2
            new() { RoomId = rooms[16].Id, UserId = customer2.Id, CheckIn = DateTime.Today.AddDays(-20), CheckOut = DateTime.Today.AddDays(-18), NumberOfGuests = 2, TotalPrice = 1500, Status = BookingStatus.Completed },
            new() { RoomId = rooms[1].Id, UserId = customer2.Id, CheckIn = DateTime.Today.AddDays(-15), CheckOut = DateTime.Today.AddDays(-12), NumberOfGuests = 1, TotalPrice = 240, Status = BookingStatus.Cancelled },
            new() { RoomId = rooms[5].Id, UserId = customer2.Id, CheckIn = DateTime.Today.AddDays(10), CheckOut = DateTime.Today.AddDays(12), NumberOfGuests = 2, TotalPrice = 260, Status = BookingStatus.Confirmed },
            // Customer 3
            new() { RoomId = rooms[2].Id, UserId = customer3.Id, CheckIn = DateTime.Today.AddDays(-8), CheckOut = DateTime.Today.AddDays(-4), NumberOfGuests = 2, TotalPrice = 340, Status = BookingStatus.Completed },
            new() { RoomId = rooms[9].Id, UserId = customer3.Id, CheckIn = DateTime.Today.AddDays(-2), CheckOut = DateTime.Today.AddDays(1), NumberOfGuests = 2, TotalPrice = 660, Status = BookingStatus.Confirmed },
            new() { RoomId = rooms[13].Id, UserId = customer3.Id, CheckIn = DateTime.Today.AddDays(20), CheckOut = DateTime.Today.AddDays(25), NumberOfGuests = 4, TotalPrice = 1900, Status = BookingStatus.Pending }
        };
        context.Bookings.AddRange(bookings);
        await context.SaveChangesAsync();

        // Seed payments for completed/confirmed bookings
        context.Payments.AddRange(
            // Customer 1 Payments
            new Payment { BookingId = bookings[0].Id, Amount = 240, Method = PaymentMethod.Card, Status = PaymentStatus.Completed, PaidAt = DateTime.Today.AddDays(-10) },
            new Payment { BookingId = bookings[1].Id, Amount = 390, Method = PaymentMethod.BankTransfer, Status = PaymentStatus.Completed, PaidAt = DateTime.Today.AddDays(-5) },
            new Payment { BookingId = bookings[2].Id, Amount = 660, Method = PaymentMethod.Card, Status = PaymentStatus.Completed, PaidAt = DateTime.Today.AddDays(-1) },
            // Customer 2 Payments
            new Payment { BookingId = bookings[4].Id, Amount = 1500, Method = PaymentMethod.Card, Status = PaymentStatus.Completed, PaidAt = DateTime.Today.AddDays(-20) },
            new Payment { BookingId = bookings[6].Id, Amount = 260, Method = PaymentMethod.Cash, Status = PaymentStatus.Completed, PaidAt = DateTime.Today.AddDays(-2) },
            // Customer 3 Payments
            new Payment { BookingId = bookings[7].Id, Amount = 340, Method = PaymentMethod.BankTransfer, Status = PaymentStatus.Completed, PaidAt = DateTime.Today.AddDays(-8) },
            new Payment { BookingId = bookings[8].Id, Amount = 660, Method = PaymentMethod.Card, Status = PaymentStatus.Completed, PaidAt = DateTime.Today.AddDays(-3) }
        );
        await context.SaveChangesAsync();

        // Seed reviews (only for completed bookings)
        var reviews = new List<Review>
        {
            new() { RoomId = rooms[0].Id, UserId = customer.Id, Rating = 4, Content = "Great stay! The room was clean and comfortable. WiFi was excellent. Would recommend for budget travelers." },
            new() { RoomId = rooms[4].Id, UserId = customer.Id, Rating = 5, Content = "Absolutely wonderful experience! The city view was breathtaking and the mini bar was well-stocked. Staff was very attentive." },
            new() { RoomId = rooms[16].Id, UserId = customer2.Id, Rating = 5, Content = "Incredible luxury. The private butler was amazing and the views are unmatched. Worth every penny." },
            new() { RoomId = rooms[2].Id, UserId = customer3.Id, Rating = 3, Content = "Decent stay but the AC was loud and the mini fridge wasn't very cold." },
            new() { RoomId = rooms[0].Id, UserId = customer3.Id, Rating = 4, Content = "Very nice standard room. The garden view is lovely in the morning." },
            new() { RoomId = rooms[4].Id, UserId = customer2.Id, Rating = 5, Content = "Perfect for a weekend getaway. Will return for sure." },
            new() { RoomId = rooms[8].Id, UserId = admin.Id, Rating = 5, Content = "We recently tested this room, flawless." },
            // Adding more to reach around 15 as per the plan
            new() { RoomId = rooms[1].Id, UserId = customer.Id, Rating = 4, Content = "Clean and nice, no issues." },
            new() { RoomId = rooms[3].Id, UserId = customer2.Id, Rating = 5, Content = "So quiet! Best sleep I've had in weeks." },
            new() { RoomId = rooms[5].Id, UserId = customer3.Id, Rating = 4, Content = "The balcony access is a huge plus." },
            new() { RoomId = rooms[6].Id, UserId = customer.Id, Rating = 5, Content = "Very spacious." },
            new() { RoomId = rooms[7].Id, UserId = customer2.Id, Rating = 3, Content = "Good bed, awful instant coffee." },
            new() { RoomId = rooms[10].Id, UserId = customer3.Id, Rating = 5, Content = "Jacuzzi was fantastic." },
            new() { RoomId = rooms[12].Id, UserId = customer.Id, Rating = 4, Content = "Dining area is a bit small but very nice overall." },
            new() { RoomId = rooms[18].Id, UserId = customer2.Id, Rating = 5, Content = "Private pool and cinema were exceptional!" }
        };
        context.Reviews.AddRange(reviews);
        await context.SaveChangesAsync();

        // Seed comments
        context.ReviewComments.AddRange(
            new ReviewComment { ReviewId = reviews[0].Id, UserId = admin.Id, Content = "Thank you for your kind words! We're glad you enjoyed your stay." },
            new ReviewComment { ReviewId = reviews[0].Id, UserId = customer.Id, Content = "Will definitely come back next month!" },
            new ReviewComment { ReviewId = reviews[1].Id, UserId = staff.Id, Content = "We appreciate your feedback! Looking forward to welcoming you again." },
            new ReviewComment { ReviewId = reviews[2].Id, UserId = staff.Id, Content = "We are thrilled you had a luxury experience." },
            new ReviewComment { ReviewId = reviews[3].Id, UserId = staff.Id, Content = "We apologize for the AC noise. Maintenance has been notified." },
            new ReviewComment { ReviewId = reviews[3].Id, UserId = customer3.Id, Content = "Thanks for the quick response." },
            new ReviewComment { ReviewId = reviews[4].Id, UserId = staff.Id, Content = "The garden is beautiful this time of year! Glad you liked it." },
            new ReviewComment { ReviewId = reviews[11].Id, UserId = staff.Id, Content = "We will look into upgrading our coffee options. Thank you." },
            new ReviewComment { ReviewId = reviews[14].Id, UserId = admin.Id, Content = "The cinema room is definitely a guest favorite!" },
            // Pad out a few more customer replies for realism
            new ReviewComment { ReviewId = reviews[1].Id, UserId = customer2.Id, Content = "Did they offer room service?" },
            new ReviewComment { ReviewId = reviews[1].Id, UserId = customer.Id, Content = "Yes, 24/7!" },
            new ReviewComment { ReviewId = reviews[8].Id, UserId = customer.Id, Content = "Do they have thick curtains?" },
            new ReviewComment { ReviewId = reviews[8].Id, UserId = customer2.Id, Content = "Yes, blackout curtains installed." },
            new ReviewComment { ReviewId = reviews[12].Id, UserId = staff.Id, Content = "Glad the Jacuzzi made your trip special." },
            new ReviewComment { ReviewId = reviews[13].Id, UserId = customer3.Id, Content = "Was it noisy?" },
            new ReviewComment { ReviewId = reviews[13].Id, UserId = customer.Id, Content = "Not at all, very well insulated." },
            new ReviewComment { ReviewId = reviews[10].Id, UserId = staff.Id, Content = "Thank you for the review." }
        );
        await context.SaveChangesAsync();

        // Seed tickets
        context.SupportTickets.AddRange(
            new SupportTicket { UserId = customer.Id, Category = TicketCategory.Room, Priority = TicketPriority.Medium, Subject = "Air conditioning noise", Description = "The AC in room 101 was making a buzzing noise during the night.", Status = TicketStatus.Resolved, AssignedToId = staff.Id },
            new SupportTicket { UserId = customer.Id, Category = TicketCategory.Payment, Priority = TicketPriority.High, Subject = "Double charge on credit card", Description = "I was charged twice for my booking #3. Please investigate.", Status = TicketStatus.InProgress, AssignedToId = staff.Id },
            new SupportTicket { UserId = customer.Id, Category = TicketCategory.Service, Priority = TicketPriority.Low, Subject = "Late checkout request", Description = "Can I get a late checkout at 2 PM for my upcoming stay?", Status = TicketStatus.Open },
            new SupportTicket { UserId = customer2.Id, Category = TicketCategory.Other, Priority = TicketPriority.Medium, Subject = "Cannot update profile picture", Description = "The system gives an error when I try to upload a JPEG.", Status = TicketStatus.Open },
            new SupportTicket { UserId = customer3.Id, Category = TicketCategory.Other, Priority = TicketPriority.Low, Subject = "Lost item", Description = "I think I left my sunglasses in room 201.", Status = TicketStatus.Closed, AssignedToId = admin.Id, ClosedAt = DateTime.Today.AddDays(-2) }
        );
        await context.SaveChangesAsync();
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager, string email, string password, string fullName, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser { UserName = email, Email = email, FullName = fullName, EmailConfirmed = true };
            await userManager.CreateAsync(user, password);
            await userManager.AddToRoleAsync(user, role);
        }
        return user;
    }
}
