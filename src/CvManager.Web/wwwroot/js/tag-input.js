(function () {
    const SUGGEST_URL = '/Position/SuggestTags';

    function initOne(input) {
        if (input.__tagify || typeof Tagify !== 'function') return;

        const tagify = new Tagify(input, {
            delimiters: ',',
            maxTags: 50,
            addTagOn: ['blur', 'enter'],
            dropdown: {
                enabled: 0,
                maxItems: 20,
                closeOnSelect: true,
                highlightFirst: false,
                searchKeys: ['value']
            },
            originalInputValueFormat: values => values.map(v => v.value).join(', ')
        });

        tagify.DOM.input.addEventListener('keydown', e => {
            if (e.key === 'Enter') e.preventDefault();
        });

        let timer = null;
        let requestId = 0;
        tagify.on('input', e => {
            const value = e.detail.value ?? '';
            clearTimeout(timer);
            timer = setTimeout(async () => {
                const id = ++requestId;
                tagify.loading(true);
                try {
                    const url = new URL(SUGGEST_URL, location.origin);
                    url.searchParams.set('q', value);
                    const res = await fetch(url);
                    if (!res.ok) throw new Error();
                    const list = await res.json();
                    if (id !== requestId) return;
                    tagify.whitelist = Array.isArray(list) ? list : [];
                    tagify.loading(false).dropdown.show(value);
                } catch {
                    if (id === requestId) tagify.loading(false);
                }
            }, 200);
        });

        if (window.profileAutoSave?.markDirty)
            tagify.on('change', () => window.profileAutoSave.markDirty());
    }

    window.TagInput = Object.freeze({
        init(root) {
            const scope = root?.querySelectorAll ? root : document;
            scope.querySelectorAll('input[data-tag-input]').forEach(input => {
                if (!input.closest('template')) initOne(input);
            });
        }
    });
})();
