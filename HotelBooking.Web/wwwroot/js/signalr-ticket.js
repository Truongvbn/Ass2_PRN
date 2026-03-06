/**
 * SignalR client for real-time support ticket updates.
 * Connects to /hubs/ticket and updates dashboard live.
 */
(function () {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/ticket")
        .withAutomaticReconnect()
        .build();

    connection.on("TicketCreated", function (ticket) {
        showToast(`New ticket: "${ticket.subject}"`, 'info');
        // Staff dashboard — reload to show new ticket
        const container = document.getElementById('tickets-container');
        if (container) setTimeout(() => location.reload(), 1500);
    });

    connection.on("TicketAssigned", function (ticketId, staffName) {
        const card = document.getElementById(`ticket-${ticketId}`);
        if (card) {
            const assignedEl = card.querySelector('.ticket-footer span');
            if (assignedEl) assignedEl.innerHTML = `<i class="fas fa-user-tie"></i> Assigned to ${staffName}`;
        }
        const row = document.getElementById(`ticket-row-${ticketId}`);
        if (row) {
            const assignedCell = row.cells[6];
            if (assignedCell) assignedCell.textContent = staffName;
        }
        showToast(`Ticket #${ticketId} assigned to ${staffName}`, 'info');
    });

    connection.on("TicketStatusChanged", function (ticketId, newStatus) {
        // Update card badge
        const card = document.getElementById(`ticket-${ticketId}`);
        if (card) {
            const badge = card.querySelector('.badge-status, [class*="badge-status"]');
            if (badge) { badge.textContent = newStatus; badge.className = `badge badge-status-${newStatus.toLowerCase()}`; }
        }
        // Update table row badge
        const row = document.getElementById(`ticket-row-${ticketId}`);
        if (row) {
            const badge = row.querySelector('[class*="badge-status"]');
            if (badge) { badge.textContent = newStatus; badge.className = `badge badge-status-${newStatus.toLowerCase()}`; }
        }
        showToast(`Ticket #${ticketId} status changed to ${newStatus}`, 'info');
    });

    connection.on("TicketClosed", function (ticketId) {
        const card = document.getElementById(`ticket-${ticketId}`);
        if (card) card.style.opacity = '0.5';
        showToast(`Ticket #${ticketId} has been closed.`, 'success');
    });

    function showToast(msg, type = 'info') {
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.innerHTML = `<i class="fas fa-headset"></i> ${msg}`;
        document.body.appendChild(toast);
        setTimeout(() => toast.classList.add('toast-show'), 100);
        setTimeout(() => { toast.classList.remove('toast-show'); setTimeout(() => toast.remove(), 500); }, 4000);
    }

    connection.start().catch(err => console.error('[TicketHub]', err));
})();
