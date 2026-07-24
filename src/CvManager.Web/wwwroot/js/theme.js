(function () {
    const key = 'cvmanager.theme';
    const root = document.documentElement;

    function syncButtons() {
        const dark = root.getAttribute('data-bs-theme') === 'dark';
        document.querySelectorAll('[data-theme-toggle]').forEach(btn => {
            const text = dark ? btn.dataset.labelDark : btn.dataset.labelLight;
            if (text) {
                btn.title = text;
                btn.setAttribute('aria-label', text);
            }
        });
    }

    function apply(theme) {
        root.setAttribute('data-bs-theme', theme === 'dark' ? 'dark' : 'light');
        syncButtons();
    }

    let saved = 'light';
    try { saved = localStorage.getItem(key) || 'light'; } catch { }
    apply(saved);

    function toggle() {
        const next = root.getAttribute('data-bs-theme') === 'dark' ? 'light' : 'dark';
        apply(next);
        try { localStorage.setItem(key, next); } catch { }
    }

    document.addEventListener('click', e => {
        if (e.target.closest('[data-theme-toggle]')) toggle();
    });

    if (document.readyState === 'loading')
        document.addEventListener('DOMContentLoaded', syncButtons);
})();
