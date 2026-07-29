(function () {
    const list = document.querySelector('[data-options]');
    if (!list) return;

    const form = list.closest('form');
    const type = list.dataset.type ?? '';
    const removeBtn =
        `<button type="button" class="btn btn-outline-secondary" data-remove>` +
        `<i class="bi bi-trash"></i></button>`;

    function reindex() {
        list.querySelectorAll('[data-option]').forEach((row, index) => {
            row.querySelectorAll('[name^="Options["]').forEach(input => {
                input.name = input.name.replace(/Options\[\d+]/, `Options[${index}]`);
            });
            row.querySelector('[data-valmsg-for]')
                ?.setAttribute('data-valmsg-for', `Options[${index}].Value`);
        });
    }

    form?.querySelector('[data-add-option]')?.addEventListener('click', () => {
        const i = list.querySelectorAll('[data-option]').length;
        const row = document.createElement('div');
        row.className = 'input-group flex-wrap mb-1';
        row.dataset.option = '';
        row.innerHTML =
            `<input type="text" name="Options[${i}].Value" class="form-control">` +
            removeBtn +
            `<span class="text-danger field-validation-valid w-100" ` +
            `data-valmsg-for="Options[${i}].Value" data-valmsg-replace="true"></span>`;
        list.appendChild(row);
    });

    list.addEventListener('click', e => {
        if (!e.target.closest('[data-remove]') ||
            list.querySelectorAll('[data-option]').length <= 1) return;
        e.target.closest('[data-option]').remove();
        reindex();
    });

    const dataType = form?.querySelector('select[name="DataType"]');
    const block = form?.querySelector('[data-options-block]');
    if (!dataType || !block) return;

    const sync = () => block.classList.toggle('d-none', dataType.value !== type);
    dataType.addEventListener('change', sync);
    sync();
})();
