using AutoMapper;
using HotelBooking.Business.DTOs;
using HotelBooking.Business.Services.Interfaces;
using HotelBooking.Data.Entities;
using HotelBooking.Data.Repositories.Interfaces;

namespace HotelBooking.Business.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepo;
    private readonly IMapper _mapper;

    public TicketService(ITicketRepository ticketRepo, IMapper mapper)
    {
        _ticketRepo = ticketRepo;
        _mapper = mapper;
    }

    public async Task<ServiceResult<TicketDto>> CreateTicketAsync(CreateTicketDto dto, string userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Subject))
            return ServiceResult<TicketDto>.Failure("Subject is required", "VALIDATION");
        if (string.IsNullOrWhiteSpace(dto.Description))
            return ServiceResult<TicketDto>.Failure("Description is required", "VALIDATION");
        if (!Enum.TryParse<TicketCategory>(dto.Category, out var category))
            return ServiceResult<TicketDto>.Failure("Invalid category", "VALIDATION");

        var ticket = new SupportTicket
        {
            UserId = userId,
            Category = category,
            Priority = TicketPriority.Medium,
            Subject = dto.Subject,
            Description = dto.Description,
            Status = TicketStatus.Open,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _ticketRepo.AddAsync(ticket, ct);
        // Re-fetch with includes
        var tickets = await _ticketRepo.GetByUserAsync(userId, ct);
        var created = tickets.First(t => t.Id == ticket.Id);
        return ServiceResult<TicketDto>.Success(_mapper.Map<TicketDto>(created));
    }

    public async Task<ServiceResult<TicketDto>> GetTicketByIdAsync(int id, CancellationToken ct = default)
    {
        var tickets = await _ticketRepo.GetActiveTicketsAsync(ct);
        var ticket = tickets.FirstOrDefault(t => t.Id == id);
        if (ticket is null)
        {
            // Try closed tickets
            var all = await _ticketRepo.GetAllAsync(ct);
            var found = all.FirstOrDefault(t => t.Id == id);
            if (found is null) return ServiceResult<TicketDto>.Failure("Ticket not found", "NOT_FOUND");
            return ServiceResult<TicketDto>.Success(_mapper.Map<TicketDto>(found));
        }
        return ServiceResult<TicketDto>.Success(_mapper.Map<TicketDto>(ticket));
    }

    public async Task<ServiceResult<IReadOnlyList<TicketDto>>> GetUserTicketsAsync(string userId, CancellationToken ct = default)
    {
        var tickets = await _ticketRepo.GetByUserAsync(userId, ct);
        return ServiceResult<IReadOnlyList<TicketDto>>.Success(_mapper.Map<IReadOnlyList<TicketDto>>(tickets));
    }

    public async Task<ServiceResult<IReadOnlyList<TicketDto>>> GetActiveTicketsAsync(CancellationToken ct = default)
    {
        var tickets = await _ticketRepo.GetActiveTicketsAsync(ct);
        return ServiceResult<IReadOnlyList<TicketDto>>.Success(_mapper.Map<IReadOnlyList<TicketDto>>(tickets));
    }

    public async Task<ServiceResult> AssignTicketAsync(int ticketId, string staffId, CancellationToken ct = default)
    {
        var ticket = await _ticketRepo.GetByIdAsync(ticketId, ct);
        if (ticket is null) return ServiceResult.Failure("Ticket not found", "NOT_FOUND");
        if (ticket.Status == TicketStatus.Closed)
            return ServiceResult.Failure("Cannot assign a closed ticket", "INVALID_STATE");

        ticket.AssignedToId = staffId;
        if (ticket.Status == TicketStatus.Open)
            ticket.Status = TicketStatus.InProgress;
        ticket.UpdatedAt = DateTime.UtcNow;
        await _ticketRepo.UpdateAsync(ticket, ct);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateTicketStatusAsync(int ticketId, string newStatus, string userId, bool isStaff, CancellationToken ct = default)
    {
        var ticket = await _ticketRepo.GetByIdAsync(ticketId, ct);
        if (ticket is null) return ServiceResult.Failure("Ticket not found", "NOT_FOUND");

        if (!Enum.TryParse<TicketStatus>(newStatus, out var targetStatus))
            return ServiceResult.Failure("Invalid status", "VALIDATION");

        // Validate state machine transitions
        var validTransition = (ticket.Status, targetStatus) switch
        {
            (TicketStatus.Open, TicketStatus.InProgress) when isStaff => true,
            (TicketStatus.Open, TicketStatus.Closed) when ticket.UserId == userId => true,
            (TicketStatus.InProgress, TicketStatus.Resolved) when isStaff => true,
            (TicketStatus.Resolved, TicketStatus.Closed) => true, // Anyone can close resolved
            (TicketStatus.Resolved, TicketStatus.Open) when ticket.UserId == userId => true, // Customer reopen
            _ => false
        };

        if (!validTransition)
            return ServiceResult.Failure($"Cannot transition from {ticket.Status} to {targetStatus}", "INVALID_STATE");

        ticket.Status = targetStatus;
        ticket.UpdatedAt = DateTime.UtcNow;
        if (targetStatus == TicketStatus.Closed)
            ticket.ClosedAt = DateTime.UtcNow;
        await _ticketRepo.UpdateAsync(ticket, ct);
        return ServiceResult.Success();
    }
}
