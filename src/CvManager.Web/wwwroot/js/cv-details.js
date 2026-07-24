(function () {
    const likeForm = document.querySelector('[data-cv-like]');
    if (likeForm) {
        const btn = likeForm.querySelector('button');
        const icon = btn?.querySelector('i');
        const countEl = document.querySelector('[data-cv-likes]');
        likeForm.addEventListener('submit', async e => {
            e.preventDefault();
            if (btn) btn.disabled = true;
            try {
                const res = await fetch(likeForm.action, {
                    method: 'POST',
                    body: new FormData(likeForm)
                });
                const json = await res.json().catch(() => null);
                if (!json?.success) return;
                if (btn && icon) {
                    btn.classList.toggle('btn-primary', json.liked);
                    btn.classList.toggle('btn-outline-primary', !json.liked);
                    icon.classList.toggle('bi-heart-fill', json.liked);
                    icon.classList.toggle('bi-heart', !json.liked);
                }
                if (countEl && typeof json.likeCount === 'number')
                    countEl.textContent = json.likeCount;
            } finally {
                if (btn) btn.disabled = false;
            }
        });
    }

    function filled(card) {
        const start = card.querySelector('[name$=".PeriodStart"]');
        if (start) {
            const end = card.querySelector('[name$=".PeriodEnd"]');
            return !!(start.value.trim() && end?.value.trim());
        }
        if (card.querySelector('[name$=".BooleanValue"][type="checkbox"]'))
            return true;
        const el = card.querySelector(
            '[name$=".ImageUrl"], [name$=".StringValue"], [name$=".TextValue"], ' +
            '[name$=".NumericValue"], [name$=".DateValue"], [name$=".DropdownOptionId"]');
        return !!(el && String(el.value).trim());
    }

    function syncEmpty(e) {
        const card = e.target.closest?.('[data-cv-attr]');
        if (card) card.classList.toggle('border-danger', !filled(card));
    }

    document.addEventListener('input', syncEmpty);
    document.addEventListener('change', syncEmpty);
})();
