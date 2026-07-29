(function () {
    const key = 'cvmanager.theme';
    const root = document.documentElement;

    function apply(theme) {
        root.setAttribute('data-bs-theme', theme === 'dark' ? 'dark' : 'light');
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
})();
