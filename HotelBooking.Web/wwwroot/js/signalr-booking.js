/**
 * SignalR client for real-time booking updates.
 * Connects to /hubs/booking and updates room availability live.
 */
(function () {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/booking")
        .withAutomaticReconnect()
        .build();

    function updateBadge(selector, newStatus) {
        const el = document.querySelector(selector);
        if (el) {
            const badge = el.querySelector('.badge');
            if (badge) {
                badge.textContent = newStatus;
                badge.className = `badge badge-${(newStatus || '').toLowerCase().replace(/\s/g, '')}`;
            }
        }
    }

    connection.on("BookingCreated", function (booking) {
        const card = document.getElementById(`room-${booking.roomId}`);
        if (card) {
            const badge = card.querySelector('.room-badge');
            if (badge) { badge.textContent = 'Unavailable'; badge.className = 'room-badge badge-unavailable'; }
        }
        showToast(`Room "${booking.roomName}" was just booked!`, 'info');
    });

    connection.on("NewBookingRequest", function () { showToast('New booking request received!', 'info'); });

    connection.on("BookingApproved", function () { showToast('Your booking was approved! Please complete payment.', 'success'); });

    connection.on("BookingRejected", function (id, reason) { showToast(`Booking declined. ${reason || ''}`, 'danger'); });

    connection.on("PaymentReceived", function () { showToast('Payment received for a booking.', 'success'); });

    connection.on("BookingConfirmed", function () { showToast('Payment successful! Your booking is confirmed.', 'success'); });

    connection.on("RefundProcessed", function (id, amount) { showToast(`Refund of $${amount} processed.`, 'success'); });

    connection.on("BookingExpired", function () { showToast('Your booking has expired.', 'danger'); });

    connection.on("CheckedIn", function () { showToast('You have been checked in! Enjoy your stay.', 'success'); });

    connection.on("StayCompleted", function () { showToast('Thank you for your stay! We\'d love your review.', 'success'); });

    connection.on("NoShow", function () { showToast('A booking was marked as no-show.', 'info'); });

    connection.on("BookingCancelled", function (bookingId) {
        showToast('A room just became available!', 'success');
        setTimeout(() => location.reload(), 1500);
    });

    connection.on("BookingStatusChanged", function (bookingId, newStatus) {
        updateBadge(`#booking-${bookingId}`, newStatus);
        updateBadge(`#booking-row-${bookingId}`, newStatus);
        showToast(`Booking #${bookingId} status: ${newStatus}`, 'info');
    });

    function showToast(msg, type) {
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.innerHTML = `<i class="fas fa-bell"></i> ${msg}`;
        document.body.appendChild(toast);
        setTimeout(() => toast.classList.add('toast-show'), 100);
        setTimeout(() => { toast.classList.remove('toast-show'); setTimeout(() => toast.remove(), 500); }, 4000);
    }

    connection.start().catch(err => console.error('[BookingHub]', err));
})();
