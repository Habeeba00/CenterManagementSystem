// Anti-forgery token helper — used by all fetch() POST calls across phases
function getAntiForgeryToken() {
    return document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
}

// Sidebar active state
document.addEventListener('DOMContentLoaded', () => {
    const path = window.location.pathname.toLowerCase();
    document.querySelectorAll('[data-nav-controller]').forEach(link => {
        const ctrl = link.dataset.navController.toLowerCase();
        if (path.startsWith('/' + ctrl)) {
            link.classList.add('bg-white/10', 'border-l-4', 'border-primary', 'text-white');
        }
    });
});

// Notification poll — stub wired in Phase 1, fully implemented in Phase 6
async function pollNotifications() {
    try {
        const res = await fetch('/Notification/GetUnread');
        const data = await res.json();
        const badge = document.getElementById('notif-count');
        if (!badge) return;
        if (data.count > 0) {
            badge.textContent = data.count;
            badge.classList.remove('hidden');
        } else {
            badge.classList.add('hidden');
        }
    } catch (_) { /* silent */ }
}

setInterval(pollNotifications, 60000);
document.addEventListener('DOMContentLoaded', pollNotifications);
