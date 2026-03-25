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

    connection.on("BookingApproved", function () { showToast('Your booking is ready! Please complete payment.', 'success'); });

    // First-come-first-serve: disable the booking form if someone else just locked this room
    connection.on("RoomLocked", function (roomId, roomName) {
        const form = document.getElementById('booking-form');
        if (form && parseInt(form.dataset.roomId, 10) === roomId) {
            // Disable all inputs and the submit button
            form.querySelectorAll('input, textarea, select, button[type="submit"]').forEach(el => {
                el.disabled = true;
            });
            // Show the locked banner
            const banner = document.getElementById('room-locked-banner');
            if (banner) banner.style.display = '';
            showToast(`Room "${roomName}" was just booked by someone else.`, 'danger');
        } else {
            // On other pages (room listing etc.) just update the badge
            const card = document.getElementById(`room-${roomId}`);
            if (card) {
                const badge = card.querySelector('.room-badge');
                if (badge) { badge.textContent = 'Unavailable'; badge.className = 'room-badge badge-unavailable'; }
            }
        }
    });

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
    
    connection.on("RolePromoted", function () {
        showToast('Your account was upgraded to Staff. Refreshing...', 'success');
        setTimeout(() => location.reload(), 800);
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
