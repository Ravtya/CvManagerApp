(function () {
    const PAGE_SIZE = 20;

    function reindexByNamePrefix(container, rowSelector, namePrefix) {
        if (!container) return;
        container.querySelectorAll(rowSelector).forEach((row, index) => {
            row.querySelectorAll(`[name^="${namePrefix}["]`).forEach(input => {
                input.name = input.name.replace(
                    new RegExp(`${namePrefix}\\[\\d+]`),
                    `${namePrefix}[${index}]`);
            });
        });
    }

    function initPicker(opts) {
        const root = document.querySelector('[data-attr-picker]');
        if (!root) return;

        const results = root.querySelector('[data-picker-results]');
        const qInput = root.querySelector('[data-picker-q]');
        const catSelect = root.querySelector('[data-picker-category]');
        const { suggestUrl, rowUrl } = root.dataset;
        const excludeBuiltIn = root.dataset.excludeBuiltIn === 'true';
        let timer = 0, skip = 0, hasMore = true, loading = false;

        const onFormIds = () => new Set(
            [...document.querySelectorAll(opts.rowSelector)]
                .map(r => r.getAttribute('data-attr-def-id'))
                .filter(Boolean));

        function buildUrl(skipVal) {
            const url = new URL(suggestUrl, location.origin);
            const q = (qInput?.value ?? '').trim();
            const cat = catSelect?.value ?? '';
            if (q) url.searchParams.set('q', q);
            if (cat) url.searchParams.set('categoryId', cat);
            if (excludeBuiltIn) url.searchParams.set('excludeBuiltIn', 'true');
            if (skipVal > 0) url.searchParams.set('skip', String(skipVal));
            return url;
        }

        async function load(reset) {
            if (!results || !suggestUrl || loading) return;
            if (reset) {
                skip = 0;
                hasMore = true;
                results.replaceChildren();
            }
            if (!hasMore) return;

            loading = true;
            try {
                const items = await fetch(buildUrl(skip)).then(r => r.ok ? r.json() : []);
                hasMore = items.length >= PAGE_SIZE;
                skip += items.length;

                const taken = onFormIds();
                const frag = document.createDocumentFragment();
                for (const item of items) {
                    const id = String(item.id);
                    if (taken.has(id)) continue;
                    const btn = document.createElement('button');
                    btn.type = 'button';
                    btn.className = 'picker-item btn btn-sm text-start w-100';
                    btn.dataset.defId = id;
                    btn.dataset.categoryId = item.categoryId;
                    btn.textContent = `${item.name} (${item.dataType})`;
                    frag.appendChild(btn);
                }
                results.appendChild(frag);
            } finally {
                loading = false;
            }
        }

        async function addFromButton(btn) {
            const defId = parseInt(btn.dataset.defId ?? '', 10);
            if (!(defId > 0) || !rowUrl) return;

            const list = document
                .querySelector(`[data-category="${btn.dataset.categoryId}"]`)
                ?.querySelector(opts.listSelector);
            if (!list || list.querySelector(`[data-attr-def-id="${defId}"]`)) return;

            const url = new URL(rowUrl, location.origin);
            url.searchParams.set('defId', String(defId));
            const html = await fetch(url).then(r => r.ok ? r.text() : '');
            if (!html) return;

            const wrap = document.createElement('div');
            wrap.innerHTML = html.trim();
            const el = wrap.firstElementChild;
            if (!el) return;

            list.appendChild(el);
            opts.onAdded?.(el);
            btn.remove();
        }

        qInput?.addEventListener('input', () => {
            clearTimeout(timer);
            timer = setTimeout(() => load(true), 200);
        });
        catSelect?.addEventListener('change', () => load(true));
        results?.addEventListener('scroll', () => {
            if (hasMore && !loading && results.scrollTop + results.clientHeight >= results.scrollHeight - 40)
                load(false);
        });
        results?.addEventListener('click', e => {
            const btn = e.target.closest('.picker-item');
            if (btn) addFromButton(btn);
        });

        if (opts.removeSelector) {
            document.addEventListener('click', e => {
                const row = e.target.closest(opts.removeSelector)?.closest(opts.rowSelector);
                if (!row) return;
                opts.onRemoved?.(row);
                row.remove();
                load(true);
            });
        }

        load(true);
    }

    window.PickerUtils = Object.freeze({ reindexByNamePrefix, initPicker });
})();
