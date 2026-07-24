(function () {
    const root = document.getElementById('discussionPanel');
    if (!root) return;

    const list = root.querySelector('[data-discussion-list]');
    const empty = root.querySelector('[data-discussion-empty]');
    const errorEl = root.querySelector('[data-discussion-error]');
    const manageTpl = root.querySelector('#discussionManageTpl');
    const positionId = Number(root.dataset.positionId);
    const profileTpl = root.dataset.profileUrl || '';
    const currentUserId = root.dataset.currentUserId || '';
    const isAdmin = root.dataset.isAdmin === 'true';

    function showError(msg) {
        if (!errorEl) return;
        errorEl.textContent = msg || '';
        errorEl.classList.toggle('d-none', !msg);
    }

    function attachManage(el, post) {
        if (!manageTpl || !currentUserId || (!isAdmin && post.authorUserId !== currentUserId)) return;
        const wrap = manageTpl.content.cloneNode(true).firstElementChild;
        if (!wrap) return;
        wrap.querySelectorAll('[name="postId"]').forEach(i => { i.value = post.id; });
        const ta = wrap.querySelector('textarea[name="content"]');
        if (ta) ta.value = post.content || '';
        el.appendChild(wrap);
    }

    function append(post) {
        if (!list || list.querySelector(`[data-post-id="${post.id}"]`)) return;

        const el = document.createElement('div');
        el.className = 'mb-3';
        el.dataset.postId = post.id;

        const meta = document.createElement('div');
        meta.className = 'text-muted mb-1';
        const when = window.LocalTime?.format?.(post.createdAt) || post.createdAt || '';
        if (profileTpl && post.authorUserId) {
            const a = document.createElement('a');
            a.href = profileTpl.replace('__id__', encodeURIComponent(post.authorUserId));
            a.textContent = post.authorName || '';
            meta.append(a, document.createTextNode(` · ${when}`));
        } else {
            meta.textContent = `${post.authorName || ''} · ${when}`;
        }

        const body = document.createElement('div');
        body.dataset.discussionBody = '';
        body.innerHTML = post.contentHtml || '';

        el.append(meta, body);
        attachManage(el, post);
        list.appendChild(el);
        empty?.classList.add('d-none');
    }

    function update(post) {
        const item = list?.querySelector(`[data-post-id="${post.id}"]`);
        if (!item) return;
        const body = item.querySelector('[data-discussion-body]');
        if (body) body.innerHTML = post.contentHtml || '';
        const ta = item.querySelector('[data-discussion-form="edit"] textarea');
        if (ta && typeof post.content === 'string') ta.value = post.content;
    }

    function remove(post) {
        list?.querySelector(`[data-post-id="${post.id}"]`)?.remove();
        if (list && list.children.length === 0) empty?.classList.remove('d-none');
    }

    root.addEventListener('submit', async e => {
        const form = e.target.closest('form[data-discussion-form]');
        if (!form || !root.contains(form)) return;
        e.preventDefault();
        showError('');
        const btn = form.querySelector('[type="submit"]');
        if (btn) btn.disabled = true;
        try {
            const res = await fetch(form.action, { method: 'POST', body: new FormData(form) });
            const json = await res.json().catch(() => null);
            if (!json?.success) {
                showError(json?.message || 'Error');
                return;
            }
            if (form.dataset.discussionForm === 'add') {
                const ta = form.querySelector('textarea[name="content"]');
                if (ta) ta.value = '';
            } else if (form.dataset.discussionForm === 'edit') {
                const details = form.closest('details');
                if (details) details.open = false;
            }
        } catch {
            showError('Network error');
        } finally {
            if (btn) btn.disabled = false;
        }
    });

    if (typeof signalR === 'undefined') return;

    const conn = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/discussion')
        .withAutomaticReconnect()
        .build();

    conn.on('post', append);
    conn.on('updated', update);
    conn.on('deleted', remove);
    conn.start().then(() => conn.invoke('JoinPosition', positionId));
})();
