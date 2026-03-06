/**
 * SignalR client for real-time booking updates.
 * Connects to /hubs/booking and updates room availability live.
 */
(function () {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/booking")
        .withAutomaticReconnect()
        .build();

    // Room card availability update
    connection.on("BookingCreated", function (booking) {
        const card = document.getElementById(`room-${booking.roomId}`);
        if (card) {
            const badge = card.querySelector('.room-badge');
            if (badge) {
                badge.textContent = 'Unavailable';
                badge.className = 'room-badge badge-unavailable';
            }
        }
        showToast(`Room "${booking.roomName}" was just booked!`, 'info');
    });

    connection.on("BookingCancelled", function (bookingId) {
        showToast('A room just became available!', 'success');
        // Reload to refresh availability
        setTimeout(() => location.reload(), 1500);
    });

    connection.on("BookingStatusChanged", function (bookingId, newStatus) {
        const card = document.getElementById(`booking-${bookingId}`);
        if (card) {
            const badge = card.querySelector('.badge');
            if (badge) {
                badge.textContent = newStatus;
                badge.className = `badge badge-${newStatus.toLowerCase()}`;
            }
        }
        // Admin table row
        const row = document.getElementById(`booking-row-${bookingId}`);
        if (row) {
            const badge = row.querySelector('.badge');
            if (badge) {
                badge.textContent = newStatus;
                badge.className = `badge badge-${newStatus.toLowerCase()}`;
            }
        }
        showToast(`Booking #${bookingId} status: ${newStatus}`, 'info');
    });

    function showToast(msg, type = 'info') {
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.innerHTML = `<i class="fas fa-bell"></i> ${msg}`;
        document.body.appendChild(toast);
        setTimeout(() => toast.classList.add('toast-show'), 100);
        setTimeout(() => {
            toast.classList.remove('toast-show');
            setTimeout(() => toast.remove(), 500);
        }, 4000);
    }

    connection.start().catch(err => console.error('[BookingHub]', err));
})();
