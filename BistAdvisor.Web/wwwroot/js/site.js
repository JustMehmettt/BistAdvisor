function initScComboboxes(root) {
    root.querySelectorAll('.sc-combobox').forEach(combo => {
        if (combo.dataset.initialized === 'true') {
            return;
        }
        combo.dataset.initialized = 'true';

        const trigger = combo.querySelector('[data-role="trigger"]');
        const panel = combo.querySelector('[data-role="panel"]');
        const search = combo.querySelector('[data-role="search"]');
        const list = combo.querySelector('[data-role="list"]');
        const label = combo.querySelector('[data-role="label"]');
        const hiddenInput = combo.querySelector('[data-role="hidden"]');

        function closePanel() {
            panel.classList.remove('open');
            trigger.classList.remove('open');
        }

        function openPanel() {
            panel.classList.add('open');
            trigger.classList.add('open');
            search.value = '';
            filterOptions('');
            search.focus();
        }

        function filterOptions(query) {
            const lowerQuery = query.toLowerCase();
            list.querySelectorAll('.sc-combobox-option').forEach(option => {
                const text = option.textContent.toLowerCase();
                option.classList.toggle('hidden', !text.includes(lowerQuery));
            });
        }

        trigger.addEventListener('click', (e) => {
            e.stopPropagation();
            panel.classList.contains('open') ? closePanel() : openPanel();
        });

        search.addEventListener('input', () => filterOptions(search.value));

        list.querySelectorAll('.sc-combobox-option').forEach(option => {
            option.addEventListener('click', () => {
                hiddenInput.value = option.getAttribute('data-value');
                label.textContent = option.textContent;

                list.querySelectorAll('.sc-combobox-option').forEach(o => o.classList.remove('selected'));
                option.classList.add('selected');

                closePanel();

                if (combo.hasAttribute('data-auto-submit')) {
                    combo.closest('form').submit();
                }
            });

            if (option.getAttribute('data-value') === hiddenInput.value) {
                option.classList.add('selected');
                label.textContent = option.textContent;
            }
        });

        document.addEventListener('click', (e) => {
            if (!combo.contains(e.target)) {
                closePanel();
            }
        });
    });
}

document.addEventListener('DOMContentLoaded', () => initScComboboxes(document));