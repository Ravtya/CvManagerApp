(function () {
    const form = document.getElementById('profileForm');
    const P = window.PickerUtils;
    if (!form) return;

    const rowVersionInput = form.querySelector('[name="RowVersion"]');
    const statusEl = document.getElementById('autoSaveStatus');
    const bannerEl = document.getElementById('autoSaveConflict');
    const alertContainer = document.getElementById('autoSaveAlert');
    const T = statusEl?.dataset ?? {};
    const DEBOUNCE_MS = 7000;

    let dirty = false;
    let timer = null;
    let inFlight = false;
    let stopped = false;

    function setStatus(text, cls) {
        if (!statusEl) return;
        statusEl.textContent = text ?? '';
        statusEl.className = `badge ${cls}`;
    }

    function showError(msg) {
        if (!alertContainer) return;
        const el = document.createElement('div');
        el.className = 'alert alert-danger';
        el.textContent = msg;
        alertContainer.replaceChildren(el);
    }

    function markDirty() {
        if (stopped) return;
        dirty = true;
        setStatus(T.textUnsaved, 'text-bg-warning');
        if (!timer) timer = setTimeout(save, DEBOUNCE_MS);
    }

    async function save() {
        timer = null;
        if (!dirty || inFlight || stopped) return;
        inFlight = true;
        setStatus(T.textSaving, 'text-bg-info');
        try {
            const res = await fetch('/Profile/AutoSave', { method: 'POST', body: new FormData(form) });
            const json = await res.json();
            if (json.success) {
                if (rowVersionInput) rowVersionInput.value = json.rowVersion;
                dirty = false;
                setStatus(T.textSaved, 'text-bg-success');
                alertContainer?.replaceChildren();
                if (document.querySelector('.project-card[data-project-id="0"]')) {
                    location.hash = 'projects';
                    location.reload();
                    return;
                }
            } else if (json.conflict) {
                stopped = true;
                setStatus(T.textConflict, 'text-bg-danger');
                bannerEl?.classList.remove('d-none');
            } else {
                setStatus(T.textError, 'text-bg-danger');
                showError(json.message || T.textSaveFailed);
            }
        } catch {
            setStatus(T.textError, 'text-bg-danger');
            showError(T.textSaveFailedUnavailable);
        } finally {
            inFlight = false;
            if (dirty && !stopped) timer = setTimeout(save, DEBOUNCE_MS);
        }
    }

    form.addEventListener('input', markDirty);
    form.addEventListener('change', markDirty);
    form.addEventListener('submit', () => {
        clearTimeout(timer);
        stopped = true;
    });

    window.profileAutoSave = { markDirty };

    const removedInfo = document.getElementById('removedInfoValueIds');
    if (P) {
        P.initPicker({
            rowSelector: '[data-attr-row]',
            listSelector: '[data-attr-list]',
            removeSelector: '[data-remove]',
            onAdded(el) {
                removedInfo?.querySelectorAll(`input[data-def-id="${el.getAttribute('data-attr-def-id')}"]`)
                    .forEach(x => x.remove());
                window.ImageUpload?.init(el);
                markDirty();
            },
            onRemoved(row) {
                const defId = row.getAttribute('data-attr-def-id');
                const valueId = parseInt(row.getAttribute('data-value-id') ?? '', 10);
                if (valueId > 0 && defId && removedInfo) {
                    const input = document.createElement('input');
                    input.type = 'hidden';
                    input.name = 'RemoveInfoValueIds';
                    input.value = String(valueId);
                    input.dataset.defId = defId;
                    removedInfo.appendChild(input);
                }
                markDirty();
            }
        });
    }

    const projectsList = document.getElementById('projectsList');
    const projectTemplate = document.getElementById('projectCardTemplate');
    const removedProjects = document.getElementById('removedProjectIds');

    if (projectsList && projectTemplate) {
        const reindex = () => P?.reindexByNamePrefix(projectsList, '.project-card', 'Projects');

        document.getElementById('projectAddBtn')?.addEventListener('click', () => {
            if (!('content' in projectTemplate)) return;
            const card = projectTemplate.content.cloneNode(true).firstElementChild;
            if (!card) return;
            projectsList.appendChild(card);
            reindex();
            window.TagInput?.init(card);
            markDirty();
            card.querySelector('input[name$=".Name"]')?.focus();
        });

        projectsList.addEventListener('click', e => {
            const card = e.target.closest('[data-remove]')?.closest('.project-card');
            if (!card) return;
            const projectId = parseInt(card.dataset.projectId || '0', 10);
            if (projectId > 0 && removedProjects) {
                const hidden = document.createElement('input');
                hidden.type = 'hidden';
                hidden.name = 'RemoveProjectIds';
                hidden.value = String(projectId);
                removedProjects.appendChild(hidden);
            }
            card.remove();
            reindex();
            markDirty();
        });
    }
})();
