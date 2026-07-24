(function () {
    const ready = new WeakSet();

    function applyUrl(root, url) {
        const urlInput = root.querySelector('input[data-image-url]');
        const preview = root.querySelector('[data-image-preview]');
        if (urlInput) {
            urlInput.value = url;
            urlInput.dispatchEvent(new Event('input', { bubbles: true }));
            urlInput.dispatchEvent(new Event('change', { bubbles: true }));
        }
        if (preview) {
            preview.src = url;
            preview.classList.toggle('d-none', !url);
        }
        window.profileAutoSave?.markDirty?.();
    }

    function initOne(root) {
        if (!root || ready.has(root)) return;
        root.querySelector('[data-image-upload-btn]')?.addEventListener('click', () => {
            const apiKey = window.ImageUploadConfig?.apiKey;
            if (!apiKey || typeof Bytescale?.UploadWidget === 'undefined') return;
            Bytescale.UploadWidget.open({ apiKey, maxFileCount: 1, mimeTypes: ['image/*'] })
                .then(files => {
                    const url = files[0]?.fileUrl || '';
                    if (url) applyUrl(root, url);
                })
                .catch(() => { });
        });
        root.querySelector('[data-image-clear-btn]')?.addEventListener('click', () => applyUrl(root, ''));
        ready.add(root);
    }

    function init(scope) {
        const root = scope?.querySelectorAll ? scope : document;
        root.querySelectorAll('[data-image-upload]').forEach(el => {
            if (!el.closest('template')) initOne(el);
        });
    }

    window.ImageUpload = Object.freeze({ init });
})();
