(function () {
    document.querySelectorAll('[data-select-all]').forEach(master => {
        master.addEventListener('change', () => {
            document.querySelectorAll(master.dataset.selectAll)
                .forEach(cb => { cb.checked = master.checked; });
        });
    });

    function formatDateTime(value) {
        const d = value instanceof Date ? value : new Date(value);
        if (Number.isNaN(d.getTime())) return typeof value === 'string' ? value : '';
        return d.toLocaleString(undefined, { dateStyle: 'short', timeStyle: 'short' });
    }

    function formatLocalTimes(root) {
        (root || document).querySelectorAll('time[data-local-time][datetime]').forEach(el => {
            el.textContent = formatDateTime(el.getAttribute('datetime'));
        });
    }

    formatLocalTimes();
    window.LocalTime = Object.freeze({ format: formatDateTime, apply: formatLocalTimes });
})();
