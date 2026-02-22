// wwwroot/js/defaultjs.js
// ===============================================
// SANTUÁRIO - defaultjs.js (enxuto e genérico)
// ✅ ModalLoader (carrega PartialView no #modalContainer)
// ✅ ModalManager (evita duplicar modal com mesmo ID)
// ✅ AjaxHandler (forms com data-ajax="true" => fetch, trata JSON/HTML)
// ✅ DeleteAjax (botões data-delete-button="true" => confirma + POST, trata JSON)
// ✅ FilePreview (preview de imagem para input#arquivoImagem -> img#previewImagem)
// ✅ Utils + ModalCleaner (opcional: reset por window.modalParaLimpar)
// ===============================================

// --------------------------------------------------
// Helpers Gerais
// --------------------------------------------------
const Utils = {
    // Cria input hidden para checkboxes desmarcados (ASP.NET precisa)
    ensureCheckboxHidden: function (form) {
        form.querySelectorAll('input[type="checkbox"][name]').forEach(cb => {
            if (!cb.checked) {
                // evita duplicar
                const exists = form.querySelector(`input[type="hidden"][name="${CSS.escape(cb.name)}"][data-cb-hidden="1"]`);
                if (exists) return;

                const hidden = document.createElement('input');
                hidden.type = 'hidden';
                hidden.name = cb.name;
                hidden.value = 'false';
                hidden.setAttribute('data-cb-hidden', '1');
                form.appendChild(hidden);
            }
        });
    },

    resetForm: function (form) {
        if (!form) return;
        form.reset();

        form.querySelectorAll('input[type="text"], input[type="number"], textarea')
            .forEach(el => el.value = '');

        form.querySelectorAll('input[type="checkbox"]')
            .forEach(el => el.checked = false);

        // cuidado: só zera hidden comuns (não antiforgery)
        form.querySelectorAll('input[type="hidden"]')
            .forEach(el => {
                if (el.name === '__RequestVerificationToken') return;
                if (el.id && el.id.toLowerCase().includes('token')) return;
                el.value = el.value === '0' ? '0' : el.value; // não mexe agressivamente
            });
    }
};

// --------------------------------------------------
// Limpeza dinâmica de Modals (opcional)
// window.modalParaLimpar = "ModalUsuario"
// no DOMContentLoaded ele reseta o form do modal marcado via data-id-modal="ModalUsuario"
// --------------------------------------------------
const ModalCleaner = (function () {
    function onDOMContentLoaded() {
        const modalParaLimpar = window.modalParaLimpar;
        if (!modalParaLimpar) return;

        const modal = document.querySelector(`[data-id-modal="${modalParaLimpar}"]`);
        if (!modal) return;

        const form = modal.querySelector('form');
        Utils.resetForm(form);
    }

    return {
        init: function () {
            document.addEventListener('DOMContentLoaded', onDOMContentLoaded);
        }
    };
})();

// --------------------------------------------------
// ModalManager – garante que só um modal por ID fique ativo
// (evita duplicações quando você abre várias vezes o mesmo modal)
// --------------------------------------------------
const ModalManager = (function () {
    function onShowModal(e) {
        const id = e.target?.id;
        if (!id) return;

        document.querySelectorAll(`.modal[id="${id}"]`).forEach(modalEl => {
            if (modalEl !== e.target) {
                const inst = bootstrap.Modal.getInstance(modalEl);
                if (inst) inst.hide();
                modalEl.remove();
            }
        });
    }

    return {
        init: function () {
            document.addEventListener('show.bs.modal', onShowModal);
        }
    };
})();

// --------------------------------------------------
// ModalLoader – carrega PartialView no #modalContainer
// Uso:
//   ModalLoader.open('Usuario','CarregarModalUsuario', 0)
//   ModalLoader.open('Noticia','CarregarModalNoticia', { id: 10, modo: 'edit' })
// --------------------------------------------------
const ModalLoader = (function () {
    const loadingSet = new Set();

    async function abrirModal(controller, action, routeVal = 0, routeVal2 = null) {
        const key = `${controller}_${action}_${JSON.stringify(routeVal)}_${routeVal2 ?? ''}`;
        if (loadingSet.has(key)) return;
        loadingSet.add(key);

        try {
            let url = `/${controller}/${action}`;
            const qs = new URLSearchParams();

            // MODO GENÉRICO (objeto => querystring)
            if (routeVal && typeof routeVal === 'object' && !Array.isArray(routeVal)) {
                for (const [name, value] of Object.entries(routeVal)) {
                    if (value !== undefined && value !== null && value !== '') {
                        qs.set(name, value);
                    }
                }
            } else {
                // MODO LEGADO: ?id=...
                if (routeVal !== undefined && routeVal !== null && routeVal !== "") {
                    qs.set('id', routeVal);
                }
                // Segundo parâmetro opcional: ?idItem=...
                if (routeVal2 !== undefined && routeVal2 !== null && routeVal2 !== "") {
                    qs.set('idItem', routeVal2);
                }
            }

            const query = qs.toString();
            if (query) url += `?${query}`;

            const res = await fetch(url, {
                headers: { 'X-Requested-With': 'XMLHttpRequest' },
                credentials: 'include'
            });

            if (res.status === 401) {
                // se tiver login separado no Admin do Santuário, ajuste aqui
                window.location.href = '/Login/Index';
                return;
            }
            if (!res.ok) throw new Error(`Falha ao buscar modal (${res.status}).`);

            const html = await res.text();
            const container = document.getElementById('modalContainer');
            if (!container) throw new Error('#modalContainer não encontrado.');

            container.innerHTML = html;

            const modalEl = container.querySelector('.modal');
            if (!modalEl) throw new Error('Partial não retornou um .modal.');

            bootstrap.Modal.getOrCreateInstance(modalEl).show();

            // evento custom (se você quiser plugar coisas depois)
            document.dispatchEvent(new CustomEvent('modalloader:opened', {
                detail: { controller, action, routeVal, routeVal2 }
            }));

        } catch (err) {
            console.error('[ModalLoader]', err);

            if (window.Swal) {
                Swal.fire({
                    title: 'Ops!',
                    text: 'Não foi possível abrir o modal. Tente novamente.',
                    icon: 'error',
                    confirmButtonText: 'OK',
                    confirmButtonColor: '#bd935f'
                });
            } else {
                alert('Não foi possível abrir o modal. Tente novamente.');
            }
        } finally {
            setTimeout(() => loadingSet.delete(key), 250);
        }
    }

    return { open: abrirModal };
})();

window.ModalLoader = ModalLoader;

// --------------------------------------------------
// AjaxHandler – intercepta <form data-ajax="true">
// Espera:
//  - JSON => { success: true/false, message: "...", refresh: "page|none" }
//  - HTML => partial de modal (com validação)
// --------------------------------------------------
const AjaxHandler = (function () {
    async function onSubmit(ev) {
        const form = ev.target;

        if (!(form instanceof HTMLFormElement)) return;
        if (form.getAttribute('data-ajax') !== 'true') return;

        ev.preventDefault();

        // confirmação opcional
        const confirmText = form.getAttribute('data-confirm-text') || form.getAttribute('data-confirm');
        if (confirmText) {
            let okConfirm = true;

            if (window.Swal) {
                const r = await Swal.fire({
                    title: 'Confirmação',
                    text: confirmText,
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonText: 'Sim',
                    cancelButtonText: 'Não',
                    confirmButtonColor: '#bd935f'
                });
                okConfirm = r.isConfirmed;
            } else {
                okConfirm = window.confirm(confirmText);
            }

            if (!okConfirm) return;
        }

        try {
            // checkbox hidden para enviar false
            Utils.ensureCheckboxHidden(form);

            const fd = new FormData(form);
            const action = form.getAttribute('action') || window.location.href;
            const method = (form.getAttribute('method') || 'POST').toUpperCase();

            const resp = await fetch(action, {
                method,
                body: fd,
                credentials: 'include',
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });

            const contentType = resp.headers.get('content-type') || '';

            // ===== JSON =====
            if (contentType.includes('application/json')) {
                const json = await resp.json().catch(() => ({}));
                const ok = !!json.success;

                const msg =
                    json.message ||
                    (ok ? (form.getAttribute('data-success-msg') || 'Operação realizada com sucesso!') : 'Falha ao salvar.');

                // fecha modal somente se sucesso
                if (ok) {
                    const modalEl = form.closest('.modal');
                    if (modalEl) {
                        bootstrap.Modal.getOrCreateInstance(modalEl).hide();
                    }
                }

                if (window.Swal) {
                    await Swal.fire({
                        icon: ok ? 'success' : 'error',
                        title: ok ? 'Sucesso' : 'Erro',
                        text: msg,
                        confirmButtonText: 'OK',
                        confirmButtonColor: '#bd935f'
                    });
                } else {
                    alert(msg);
                }

                if (ok) {
                    const refreshMode = (json.refresh || form.getAttribute('data-refresh') || 'page').toLowerCase();
                    if (refreshMode === 'page') window.location.reload();
                }

                return;
            }

            // ===== HTML (partial modal com validação) =====
            const html = await resp.text();
            const container = document.getElementById('modalContainer');

            if (container) {
                container.innerHTML = html;

                const modalEl = container.querySelector('.modal');
                if (modalEl) bootstrap.Modal.getOrCreateInstance(modalEl).show();
            } else {
                document.body.insertAdjacentHTML('beforeend', html);
            }

        } catch (err) {
            console.error('[AjaxHandler] Erro no submit AJAX', err);

            if (window.Swal) {
                Swal.fire({
                    icon: 'error',
                    title: 'Erro',
                    text: 'Erro ao enviar os dados. Tente novamente.',
                    confirmButtonText: 'OK',
                    confirmButtonColor: '#bd935f'
                });
            } else {
                alert('Erro ao enviar os dados. Tente novamente.');
            }
        }
    }

    return {
        init() {
            // capture=true pra interceptar antes de qualquer outro handler
            document.addEventListener('submit', onSubmit, true);
        }
    };
})();

// --------------------------------------------------
// DeleteAjax – botões com data-delete-button="true"
// Espera JSON => { success: true/false, message: "..." }
// Atributos:
//   data-url="/Usuario/Excluir?id=1"
//   data-confirm-text="Deseja realmente inativar?"
//   data-refresh="page|row|none"
// --------------------------------------------------
const DeleteAjax = (function () {
    async function onClick(ev) {
        const btn = ev.target.closest('[data-delete-button="true"]');
        if (!btn) return;

        ev.preventDefault();

        const url = btn.getAttribute('data-url');
        if (!url) return;

        const confirmText =
            btn.getAttribute('data-confirm-text') ||
            btn.getAttribute('data-confirm') ||
            'Confirmar ação?';

        const successMsg = btn.getAttribute('data-success-msg') || 'Operação realizada com sucesso!';

        // confirmação
        let confirmado = true;
        if (window.Swal) {
            const result = await Swal.fire({
                title: 'Confirmação',
                text: confirmText,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Sim',
                cancelButtonText: 'Não',
                confirmButtonColor: '#bd935f'
            });
            confirmado = result.isConfirmed;
        } else {
            confirmado = window.confirm(confirmText);
        }

        if (!confirmado) return;

        // headers + antiforgery
        const headers = { 'X-Requested-With': 'XMLHttpRequest' };

        const tokenInput =
            document.querySelector('input[name="__RequestVerificationToken"]') ||
            btn.closest('.modal')?.querySelector('input[name="__RequestVerificationToken"]');

        if (tokenInput?.value) headers['RequestVerificationToken'] = tokenInput.value;

        try {
            const resp = await fetch(url, {
                method: 'POST',
                headers,
                credentials: 'include'
            });

            const contentType = resp.headers.get('content-type') || '';

            if (contentType.includes('application/json')) {
                const json = await resp.json().catch(() => ({}));
                const ok = !!json.success;
                const msg = json.message || (ok ? successMsg : 'Erro ao executar a ação.');

                if (window.Swal) {
                    await Swal.fire({
                        icon: ok ? 'success' : 'error',
                        title: ok ? 'Sucesso' : 'Erro',
                        text: msg,
                        confirmButtonText: 'OK',
                        confirmButtonColor: '#bd935f'
                    });
                } else {
                    alert(msg);
                }

                if (ok) {
                    const mode = (btn.getAttribute('data-refresh') || 'page').toLowerCase();

                    if (mode === 'page') {
                        window.location.reload();
                    } else if (mode === 'row') {
                        const tr = btn.closest('tr');
                        if (tr) tr.remove();
                    }
                }

                return;
            }

            // fallback se não vier JSON
            const texto = await resp.text();
            console.warn('[DeleteAjax] Resposta não-JSON:', texto);

        } catch (err) {
            console.error('[DeleteAjax] Erro:', err);

            if (window.Swal) {
                Swal.fire({
                    icon: 'error',
                    title: 'Erro',
                    text: 'Erro ao executar a ação. Tente novamente.',
                    confirmButtonText: 'OK',
                    confirmButtonColor: '#bd935f'
                });
            } else {
                alert('Erro ao executar a ação. Tente novamente.');
            }
        }
    }

    return {
        init() {
            document.addEventListener('click', onClick, true);
        }
    };
})();

// --------------------------------------------------
// FilePreview – preview de imagem (carrossel/notícia)
// HTML esperado:
//  <input type="file" id="arquivoImagem" ...>
//  <img id="previewImagem" class="d-none" />
// --------------------------------------------------
const FilePreview = (function () {
    function onChange(e) {
        if (!e.target.matches('#arquivoImagem')) return;

        const file = e.target.files && e.target.files[0];
        if (!file) return;

        const modal = e.target.closest('.modal') || document;
        const preview = modal.querySelector('#previewImagem');
        if (!preview) return;

        const reader = new FileReader();
        reader.onload = ev => {
            preview.src = ev.target.result;
            preview.classList.remove('d-none');
        };
        reader.readAsDataURL(file);
    }

    return {
        init: function () {
            document.body.addEventListener('change', onChange);
        }
    };
})();

// --------------------------------------------------
// Init
// --------------------------------------------------
(function initAll() {
    ModalCleaner.init();
    ModalManager.init();
    AjaxHandler.init();
    DeleteAjax.init();
    FilePreview.init();
})();