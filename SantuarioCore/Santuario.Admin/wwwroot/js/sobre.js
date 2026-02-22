// wwwroot/js/sobre.js
// Funciona com modal carregado via AJAX (scripts do partial não executam).
// Usa delegação + evento do Bootstrap para sempre funcionar ao abrir o modal.

(function () {

    function getModal() {
        return document.querySelector('#ModalSobre');
    }

    function getTable(modal) {
        return modal ? modal.querySelector('#tblTopicos') : null;
    }

    function reindex(tbl) {
        const rows = tbl.querySelectorAll('tbody tr');
        rows.forEach((tr, i) => {
            tr.querySelectorAll('input[name^="Topicos["]').forEach(inp => {
                inp.name = inp.name.replace(/Topicos\[\d+\]/, `Topicos[${i}]`);
            });
        });
    }

    function addRow(tbl) {
        const tbody = tbl.querySelector('tbody');
        const idx = tbody.querySelectorAll('tr').length;

        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>
                <input class="form-control"
                       type="number"
                       min="1"
                       name="Topicos[${idx}].Ordem"
                       value="${idx + 1}">
            </td>
            <td>
                <input class="form-control"
                       name="Topicos[${idx}].Texto"
                       value="">
            </td>
            <td>
                <div class="form-check">
                    <input class="form-check-input"
                           type="checkbox"
                           name="Topicos[${idx}].Ativo"
                           value="true"
                           checked>
                    <input type="hidden"
                           name="Topicos[${idx}].Ativo"
                           value="false">
                </div>
            </td>
            <td class="text-center">
                <button type="button"
                        class="btn btn-sm btn-outline-danger btnRemoverTopico"
                        title="Remover">
                    <i class="fas fa-times"></i>
                </button>
            </td>
        `;

        tbody.appendChild(tr);
        reindex(tbl);
    }

    function ensureOneRow() {
        const modal = getModal();
        if (!modal) return;

        const tbl = getTable(modal);
        if (!tbl) return;

        const qtd = tbl.querySelectorAll('tbody tr').length;
        if (qtd === 0) addRow(tbl);

        reindex(tbl);
    }

    // ✅ Quando o modal abre (Bootstrap), garante init
    document.addEventListener('shown.bs.modal', function (e) {
        if (e.target && e.target.id === 'ModalSobre') {
            ensureOneRow();
        }
    });

    // ✅ Delegação: clique em "Adicionar tópico"
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('#btnAddTopico');
        if (!btn) return;

        const modal = getModal();
        const tbl = getTable(modal);
        if (!tbl) return;

        addRow(tbl);
    });

    // ✅ Delegação: remover tópico
    document.addEventListener('click', function (e) {
        const btn = e.target.closest('.btnRemoverTopico');
        if (!btn) return;

        const modal = getModal();
        const tbl = getTable(modal);
        if (!tbl) return;

        const tr = btn.closest('tr');
        if (tr) tr.remove();

        const qtd = tbl.querySelectorAll('tbody tr').length;
        if (qtd === 0) addRow(tbl);
        else reindex(tbl);
    });

    // ✅ Delegação: preview de imagem
    document.addEventListener('change', function (e) {
        const input = e.target.closest('#arquivoImagemSobre');
        if (!input) return;

        const modal = getModal();
        if (!modal) return;

        const imgPreview = modal.querySelector('#previewImagemSobre');
        const file = input.files && input.files[0];
        if (!file || !imgPreview) return;

        const url = URL.createObjectURL(file);
        imgPreview.src = url;
    });

})();