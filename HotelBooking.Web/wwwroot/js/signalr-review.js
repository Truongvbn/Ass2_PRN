/**
 * SignalR client for real-time review and comment updates.
 * Connects to /hubs/review and updates review list live.
 */
(function () {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/review")
        .withAutomaticReconnect()
        .build();

    connection.on("ReviewCreated", function (review) {
        const container = document.getElementById('reviews-container');
        if (!container) return;
        const div = document.createElement('div');
        div.className = 'review-card';
        div.id = `review-${review.id}`;
        div.innerHTML = `
            <div class="review-header">
                <div class="review-user"><i class="fas fa-user-circle"></i> <strong>${review.userName}</strong></div>
                <div class="review-rating">${'<i class="fas fa-star text-gold"></i>'.repeat(review.rating)}${'<i class="fas fa-star text-muted"></i>'.repeat(5 - review.rating)}</div>
                <small class="review-date">${new Date(review.createdAt).toLocaleDateString('en-US', { month: 'short', day: '2-digit', year: 'numeric' })}</small>
            </div>
            <p class="review-content">${review.content}</p>
            <div class="review-comments"></div>`;
        container.prepend(div);
        showToast(`New review from ${review.userName}!`, 'success');
    });

    connection.on("ReviewDeleted", function (reviewId) {
        const el = document.getElementById(`review-${reviewId}`);
        if (el) el.remove();
    });

    connection.on("CommentAdded", function (comment) {
        const reviewComments = document.querySelector(`#review-${comment.reviewId} .review-comments`);
        if (!reviewComments) return;
        const div = document.createElement('div');
        div.className = 'comment';
        div.id = `comment-${comment.id}`;
        div.innerHTML = `<i class="fas fa-reply"></i> <strong>${comment.userName}</strong>: ${comment.content} <small>${new Date(comment.createdAt).toLocaleDateString()}</small>`;
        reviewComments.appendChild(div);
    });

    connection.on("CommentDeleted", function (commentId) {
        const el = document.getElementById(`comment-${commentId}`);
        if (el) el.remove();
    });

    function showToast(msg, type = 'info') {
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        toast.innerHTML = `<i class="fas fa-comment"></i> ${msg}`;
        document.body.appendChild(toast);
        setTimeout(() => toast.classList.add('toast-show'), 100);
        setTimeout(() => { toast.classList.remove('toast-show'); setTimeout(() => toast.remove(), 500); }, 4000);
    }

    connection.start().catch(err => console.error('[ReviewHub]', err));
})();
