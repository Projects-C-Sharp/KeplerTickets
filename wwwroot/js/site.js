// Kepler Tickets — Reception Module JS helpers

// Auto-dismiss alerts after 5s
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.alert-auto-dismiss').forEach(el => {
        setTimeout(() => el.style.display = 'none', 5000);
    });
});

// Enter key on email field triggers lookup
document.addEventListener('DOMContentLoaded', () => {
    const emailInput = document.getElementById('lookup-email');
    if (emailInput) {
        emailInput.addEventListener('keydown', e => {
            if (e.key === 'Enter') {
                e.preventDefault();
                window.lookupCustomer?.();
            }
        });
    }
});
