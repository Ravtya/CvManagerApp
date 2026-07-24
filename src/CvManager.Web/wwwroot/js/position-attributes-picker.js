(function () {
    const P = window.PickerUtils;
    if (!P) return;

    const restricted = () => document.getElementById('AccessMode')?.value === 'Restricted';

    function sync(card) {
        const block = card.querySelector('[data-rule-block]');
        if (!block) return;
        const on = restricted() && !!card.querySelector('[data-has-rule]')?.checked;
        block.classList.toggle('d-none', !restricted());
        card.querySelector('[data-rule-controls]')?.classList.toggle('d-none', !on);
    }

    function wire(card) {
        card.querySelector('[data-has-rule]')?.addEventListener('change', () => sync(card));
        sync(card);
    }

    P.initPicker({
        rowSelector: '[data-attr-row]',
        listSelector: '[data-attr-list]',
        removeSelector: '[data-remove]',
        onAdded: wire
    });

    document.querySelectorAll('[data-attr-row]').forEach(wire);
    document.getElementById('AccessMode')?.addEventListener('change', () => {
        document.querySelectorAll('[data-attr-row]').forEach(sync);
    });
})();
