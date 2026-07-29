import * as UC from 'https://cdn.jsdelivr.net/npm/@uploadcare/file-uploader@1/web/uc-file-uploader-regular.min.js';

UC.defineComponents(UC);
await customElements.whenDefined('uc-upload-ctx-provider');

const ready = new WeakSet();

function applyUrl(root, url) {
    const input = root.querySelector('[data-image-url]');
    const preview = root.querySelector('[data-image-preview]');
    if (input) {
        input.value = url;
        input.dispatchEvent(new Event('input', { bubbles: true }));
    }
    if (preview) {
        preview.src = url;
        preview.classList.toggle('d-none', !url);
    }
    root.querySelector('[data-image-clear-btn]')?.classList.toggle('d-none', !url);
    window.profileAutoSave?.markDirty?.();
}

function initOne(root) {
    if (!root || ready.has(root)) return;
    const ctx = root.querySelector('uc-upload-ctx-provider');
    const pubkey = window.ImageUploadConfig?.publicKey;
    if (pubkey) root.querySelector('uc-config')?.setAttribute('pubkey', pubkey);

    root.querySelector('[data-image-upload-btn]')?.addEventListener('click', () => ctx?.getAPI()?.initFlow());
    root.querySelector('[data-image-clear-btn]')?.addEventListener('click', () => {
        ctx?.getAPI()?.removeAllFiles();
        applyUrl(root, '');
    });
    ctx?.addEventListener('file-upload-success', e => {
        if (!e.detail?.cdnUrl) return;
        applyUrl(root, e.detail.cdnUrl);
        ctx.getAPI()?.doneFlow();
    });

    ready.add(root);
}

window.ImageUpload = {
    init(scope) {
        const root = scope?.querySelectorAll ? scope : document;
        root.querySelectorAll('[data-image-upload]').forEach(el => {
            if (!el.closest('template')) initOne(el);
        });
    }
};
window.ImageUpload.init();
