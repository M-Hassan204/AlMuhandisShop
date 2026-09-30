// Sidebar toggle
document.addEventListener('DOMContentLoaded', function () {
    const toggleBtn = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('sidebar');
    const mainContent = document.querySelector('.main-content');

    if (toggleBtn) {
        toggleBtn.addEventListener('click', function () {
            if (sidebar) sidebar.classList.toggle('open');
            if (mainContent) mainContent.classList.toggle('expanded');
        });
    }

    // Auto-close alerts after 4s
    document.querySelectorAll('.alert-dismissible').forEach(function (alert) {
        setTimeout(function () {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            bsAlert.close();
        }, 4000);
    });

    // Confirm delete
    document.querySelectorAll('[data-confirm]').forEach(function (el) {
        el.addEventListener('click', function (e) {
            if (!confirm(el.dataset.confirm || 'هل أنت متأكد؟')) {
                e.preventDefault();
            }
        });
    });
});
