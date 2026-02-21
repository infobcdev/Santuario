// defaultjs.js

// --------------------------------------------------
// Módulo de Helpers Gerais
// --------------------------------------------------
const Utils = {
    // Cria um input hidden para checkboxes desmarcados
    ensureCheckboxHidden: function (form) {
        form.querySelectorAll('input[type="checkbox"][name]').forEach(cb => {
            if (!cb.checked) {
                const hidden = document.createElement('input');
                hidden.type = 'hidden';
                hidden.name = cb.name;
                hidden.value = 'false';
                form.appendChild(hidden);
            }
        });
    },

    // Reseta todos os campos de um form dentro de um modal
    resetForm: function (form) {
        form.reset();
        form.querySelectorAll('input[type="text"], input[type="number"], textarea')
            .forEach(el => el.value = '');
        form.querySelectorAll('input[type="checkbox"]')
            .forEach(el => el.checked = false);
        form.querySelectorAll('input[type="hidden"]')
            .forEach(el => el.value = '0');
    }
};


// --------------------------------------------------
// Módulo de Limpeza Dinâmica de Modals
// --------------------------------------------------
const ModalCleaner = (function () {
    function onDOMContentLoaded() {
        const modalParaLimpar = window.modalParaLimpar;
        if (modalParaLimpar) {
            const modal = document.querySelector(`[data-id-modal="${modalParaLimpar}"]`);
            if (modal) Utils.resetForm(modal.querySelector('form'));
        }
    }

    return {
        init: function () {
            document.addEventListener('DOMContentLoaded', onDOMContentLoaded);
        }
    };
})();


// --------------------------------------------------
// Módulo de Controle de Modals
// --------------------------------------------------
const ModalManager = (function () {
    // Garante que só um modal do mesmo ID fica ativo
    function onShowModal(e) {
        const id = e.target.id;
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
// Módulo de Loading Lock para abrirModal()
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

            const ctrl = String(controller).toLowerCase();
            const act = String(action).toLowerCase();

            // 🔹 MODO GENÉRICO: se routeVal for objeto, trata como dicionário de parâmetros
            if (routeVal && typeof routeVal === 'object' && !Array.isArray(routeVal)) {
                for (const [name, value] of Object.entries(routeVal)) {
                    if (value !== undefined && value !== null && value !== '') {
                        qs.set(name, value);
                    }
                }
            } else {
                // 🔹 MODO LEGADO: comportamento antigo (id / installId / idItem)

                // ----- PARAMETRO PRINCIPAL (id / installId / etc) -----
                if (routeVal !== undefined && routeVal !== null && routeVal !== "") {

                    // Caso especial Monitoramento/VerLogs (id + installId)
                    if (ctrl === 'monitoramento' && act === 'verlogs') {
                        qs.set('id', routeVal);
                        qs.set('installId', routeVal);
                    } else {
                        // padrão: id
                        qs.set('id', routeVal);
                    }
                }

                // ----- SEGUNDO PARÂMETRO (idItem / idApi / etc) -----
                if (routeVal2 !== undefined && routeVal2 !== null && routeVal2 !== "") {

                    // ✅ padrão
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
                window.location.href = '/Login/Index';
                return;
            }
            if (!res.ok) throw new Error('Falha ao buscar o conteúdo do modal.');

            const html = await res.text();
            const container = document.getElementById('modalContainer');
            container.innerHTML = html;

            const modalEl = container.querySelector('.modal');
            new bootstrap.Modal(modalEl).show();

            // evento custom
            const evt = new CustomEvent('modalloader:opened', {
                detail: { controller, action, routeVal, routeVal2 }
            });
            document.dispatchEvent(evt);

        } catch {
            Swal.fire({
                title: 'Ops!',
                text: 'Não foi possível abrir o modal. Tente novamente.',
                icon: 'error',
                confirmButtonText: 'OK',
                confirmButtonColor: '#ca3a2d'
            });
        } finally {
            setTimeout(() => loadingSet.delete(key), 300);
        }
    }

    return {
        open: abrirModal
    };

})();
// ✅ EXPÕE PRA OUTROS SCRIPTS (pedidonovo.js)
window.ModalLoader = ModalLoader;
// --------------------------------------------------
// ModalBridge – Reabrir um modal "pai" quando um modal "filho" fechar
//
// Uso: marcar o botão que ABRE o modal filho com:
//   data-modal-bridge="true"
//   data-modal-bridge-child-id="ModalComboItem"      <-- ID do modal filho
//   data-modal-bridge-controller="Combo"            <-- controller do modal pai
//   data-modal-bridge-action="CarregarModalItensCombo" <-- action do modal pai
//   data-modal-bridge-param-idCombo="123"           <-- vira ?idCombo=123
//   data-modal-bridge-param-idUsuario="45"          <-- vira &idUsuario=45
//
// Quando o modal filho for fechado, o ModalBridge chama:
//   ModalLoader.open(controller, action, { idCombo: ..., idUsuario: ... })
// --------------------------------------------------
const ModalBridge = (function () {

    // childModalId -> { controller, action, params }
    const bridgeMap = new Map();

    function onClick(e) {
        const btn = e.target.closest('[data-modal-bridge="true"]');
        if (!btn) return;

        const childId = btn.getAttribute('data-modal-bridge-child-id');
        const controller = btn.getAttribute('data-modal-bridge-controller');
        const action = btn.getAttribute('data-modal-bridge-action');

        if (!childId || !controller || !action) {
            console.warn('[ModalBridge] Botão marcado sem child-id/controller/action.');
            return;
        }

        // Monta objeto de parâmetros a partir de data-modal-bridge-param-*
        const params = {};
        for (const attr of btn.attributes) {
            if (attr.name.startsWith('data-modal-bridge-param-')) {
                const paramName = attr.name.replace('data-modal-bridge-param-', '');
                if (paramName) {
                    params[paramName] = attr.value;
                }
            }
        }

        bridgeMap.set(childId, { controller, action, params });
        // não impede o clique normal – o open do modal acontece normalmente (via open-modal-button / onclick)
    }

    function onModalHidden(e) {
        const modal = e.target;
        if (!modal || !modal.id) return;

        const cfg = bridgeMap.get(modal.id);
        if (!cfg) return; // este modal não tem bridge configurado

        bridgeMap.delete(modal.id);

        const { controller, action, params } = cfg;

        setTimeout(() => {
            // limpa possíveis backdrops órfãos
            if (!document.querySelector('.modal.show')) {
                document.querySelectorAll('.modal-backdrop').forEach(b => b.remove());
                document.body.classList.remove('modal-open');
                document.body.style.removeProperty('padding-right');
            }

            if (typeof ModalLoader !== 'undefined' &&
                ModalLoader &&
                typeof ModalLoader.open === 'function') {

                ModalLoader.open(controller, action, params || {});
            } else {
                console.warn('[ModalBridge] ModalLoader não está disponível.');
            }

        }, 150);
    }

    return {
        init() {
            document.addEventListener('click', onClick, true);
            document.addEventListener('hidden.bs.modal', onModalHidden);
        }
    };
})();
// --------------------------------------------------
// AjaxHandler – intercepta forms com data-ajax="true"
// Envia via fetch e trata JSON ou HTML (partial de modal)
// ✅ Ajuste: NÃO fecha o modal quando success=false (erro)
// --------------------------------------------------
const AjaxHandler = (function () {

    async function onSubmit(ev) {
        const form = ev.target;

        // Garante que é um <form> e que tem data-ajax="true"
        if (!(form instanceof HTMLFormElement)) return;
        if (form.getAttribute('data-ajax') !== 'true') return;

        ev.preventDefault();
        // ✅ confirmação opcional
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
                    confirmButtonColor: '#ca3a2d'
                });
                okConfirm = r.isConfirmed;
            } else {
                okConfirm = window.confirm(confirmText);
            }

            if (!okConfirm) return;
        }

        try {
            // ✅ antes de montar FormData, injeta os selecionados de todas as páginas
            copiarCheckboxesDeTodasAsPaginas(form);
            const fd = new FormData(form);
            const action = form.getAttribute('action') || window.location.href;
            const method = (form.getAttribute('method') || 'POST').toUpperCase();

            const resp = await fetch(action, {
                method,
                body: fd,
                credentials: 'include'
            });

            const contentType = resp.headers.get('content-type') || '';

            // ====== RESPOSTA JSON ======
            if (contentType.includes('application/json')) {
                const json = await resp.json();
                const ok = !!json.success;

                const msg =
                    json.message ||
                    (ok ? (form.getAttribute('data-success-msg') || 'Operação realizada com sucesso!') : 'Falha ao salvar.');

                // ✅ Só fecha o modal se deu sucesso
                if (ok) {
                    const modalEl = form.closest('.modal');
                    if (modalEl) {
                        const bs = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
                        bs.hide();
                    }
                }

                // ✅ Swal sempre (erro/sucesso), mas no erro o modal fica aberto
                if (window.Swal) {
                    await Swal.fire({
                        icon: ok ? 'success' : 'error',
                        title: ok ? 'Sucesso' : 'Erro',
                        text: msg,
                        confirmButtonText: 'OK',
                        confirmButtonColor: '#ca3a2d'
                    });
                } else {
                    alert(msg);
                }

                // 🔁 Refresh só se deu certo
                if (ok) {
                    const refreshMode = form.getAttribute('data-refresh') || 'page';

                    if (refreshMode === 'page') {
                        window.location.reload();
                    }
                    // else if (refreshMode === 'none') { /* não faz nada */ }
                    // else if (refreshMode === 'custom' && window.MeuModulo?.atualizarLista) { ... }
                }

                return;
            }

            // ====== RESPOSTA HTML (partial de modal com validação, etc.) ======
            const html = await resp.text();
            const container = document.getElementById('modalContainer');

            if (container) {
                container.innerHTML = html;

                const modalEl = container.querySelector('.modal');
                if (modalEl) {
                    new bootstrap.Modal(modalEl).show();
                }
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
                    confirmButtonColor: '#ca3a2d'
                });
            } else {
                alert('Erro ao enviar os dados. Tente novamente.');
            }
        }
    }

    return {
        init() {
            // capture = true pra pegar o submit antes de qualquer outro handler
            document.addEventListener('submit', onSubmit, true);
        }
    };
})();


// --------------------------------------------------
// DeleteAjax – trata botões com data-delete-button="true"
// Faz confirmação (Swal se tiver) e POST via fetch, espera JSON
// Usa data-refresh: "page" | "row" | "table" | etc.
// --------------------------------------------------
const DeleteAjax = (function () {

    async function onClick(ev) {
        // Agora olha para data-delete-button, não mais data-ajax-delete
        const btn = ev.target.closest('[data-delete-button="true"]');
        if (!btn) return;

        ev.preventDefault();

        const url = btn.getAttribute('data-url');
        // tenta pegar data-confirm-text, se não tiver usa data-confirm (do TagHelper)
        const confirmText =
            btn.getAttribute('data-confirm-text') ||
            btn.getAttribute('data-confirm') ||
            'Confirmar exclusão?';

        const successMsg = btn.getAttribute('data-success-msg') || 'Excluído com sucesso!';

        if (!url) return;

        // 1) Pergunta confirmação com Swal se existir, senão window.confirm
        let confirmado = true;
        if (window.Swal) {
            const result = await Swal.fire({
                title: 'Confirmação',
                text: confirmText,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Sim',
                cancelButtonText: 'Não',
                confirmButtonColor: '#ca3a2d'
            });
            confirmado = result.isConfirmed;
        } else {
            confirmado = window.confirm(confirmText);
        }

        if (!confirmado) return;

        // 2) Monta headers + antiforgery
        const headers = {
            'X-Requested-With': 'XMLHttpRequest'
        };

        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        if (tokenInput) {
            headers['RequestVerificationToken'] = tokenInput.value;
        }

        try {
            const resp = await fetch(url, {
                method: 'POST',
                headers,
                credentials: 'include'
            });

            const contentType = resp.headers.get('content-type') || '';

            if (contentType.includes('application/json')) {
                const json = await resp.json();
                const ok = !!json.success;
                const msg = json.message || (ok ? successMsg : 'Erro ao excluir.');

                // 3) Mostra resultado com Swal se tiver, senão alert
                if (window.Swal) {
                    await Swal.fire({
                        icon: ok ? 'success' : 'error',
                        title: ok ? 'Sucesso' : 'Erro',
                        text: msg,
                        confirmButtonText: 'OK',
                        confirmButtonColor: '#ca3a2d'
                    });
                } else {
                    alert(msg);
                }

                // 4) Se deu certo, decide o tipo de refresh
                if (ok) {
                    const mode = btn.getAttribute('data-refresh') || 'page';

                    if (mode === 'page') {
                        window.location.reload();
                    } else if (mode === 'row' || mode === 'table') {
                        const tr = btn.closest('tr');
                        if (tr) tr.remove();
                    }
                    // Se no futuro quiser um modo custom:
                    // else if (mode === 'combo-itens' && window.ComboItens?.recarregarModalLista) {
                    //     window.ComboItens.recarregarModalLista();
                    // }
                }

                return;
            }

            const texto = await resp.text();
            console.warn('[DeleteAjax] Resposta não-JSON:', texto);
        } catch (err) {
            console.error('[DeleteAjax] Erro na exclusão:', err);
            if (window.Swal) {
                Swal.fire({
                    icon: 'error',
                    title: 'Erro',
                    text: 'Erro ao excluir. Tente novamente.',
                    confirmButtonText: 'OK',
                    confirmButtonColor: '#ca3a2d'
                });
            } else {
                alert('Erro ao excluir. Tente novamente.');
            }
        }
    }

    return {
        init() {
            // capture = true pra garantir que ninguém mate o evento antes
            document.addEventListener('click', onClick, true);
        }
    };
})();

// --------------------------------------------------
// MonitoramentoModalBridge
// - Memoriza o installId ao abrir VerLogs
// - Reabre VerLogs quando ModalLogDetalhe fechar
// --------------------------------------------------
(function () {
    // Guarda o último installId usado no Monitoramento/VerLogs
    let lastInstallId = null;

    // Captura aberturas de modais feitas pelo ModalLoader.open
    document.addEventListener('modalloader:opened', function (ev) {
        const d = ev.detail || {};
        if (String(d.controller).toLowerCase() === 'monitoramento' &&
            String(d.action).toLowerCase() === 'verlogs') {
            lastInstallId = (d.routeVal ?? '').toString();
        }
    });

    // Quando o ModalLogDetalhe for fechado, reabre o VerLogs
    document.addEventListener('hidden.bs.modal', function (ev) {
        const el = ev.target;
        if (!el || el.id !== 'ModalLogDetalhe') return;

        // Aguarda o Bootstrap limpar a transição/backdrop
        setTimeout(function () {
            // Se nenhum modal está aberto, limpe possíveis backdrops órfãos
            if (!document.querySelector('.modal.show')) {
                document.querySelectorAll('.modal-backdrop').forEach(b => b.remove());
                document.body.classList.remove('modal-open');
                document.body.style.removeProperty('padding-right');
            }

            if (lastInstallId) {
                // Reabre o ModalLogs da mesma máquina
                ModalLoader.open('Monitoramento', 'VerLogs', lastInstallId);
            }
        }, 150);
    });

    // Se por acaso o ModalLogDetalhe abrir enquanto o ModalLogs ainda existir no DOM,
    // esconda o Logs para evitar sobreposição (não é obrigatório, mas ajuda)
    document.addEventListener('show.bs.modal', function (ev) {
        const el = ev.target;
        if (!el || el.id !== 'ModalLogDetalhe') return;

        const logsEl = document.getElementById('ModalLogs');
        if (logsEl && logsEl.classList.contains('show')) {
            bootstrap.Modal.getOrCreateInstance(logsEl).hide();
        }
    });
})();

// --------------------------------------------------
// CpfMask – máscara para CPF
// Classe:
//  - .mask-cpf   => 000.000.000-00
// --------------------------------------------------
const CpfMask = (function () {

    const ALLOWED_KEYS = new Set([
        'Backspace', 'Delete', 'ArrowLeft', 'ArrowRight', 'Home', 'End', 'Tab'
    ]);

    function onlyDigits(v) {
        return (v ?? '').toString().replace(/\D/g, '');
    }

    function formatCpf(value) {
        const d = onlyDigits(value).slice(0, 11);

        // 000.000.000-00
        return d
            .replace(/^(\d{3})(\d)/, '$1.$2')
            .replace(/^(\d{3})\.(\d{3})(\d)/, '$1.$2.$3')
            .replace(/^(\d{3})\.(\d{3})\.(\d{3})(\d)/, '$1.$2.$3-$4');
    }

    function setCursorToEnd(el) {
        requestAnimationFrame(() => {
            try { el.setSelectionRange(el.value.length, el.value.length); } catch { }
        });
    }

    function applyMask(el) {
        el.value = formatCpf(el.value);
        setCursorToEnd(el);
    }

    function onKeyDown(e) {
        const el = e.target;
        if (!(el instanceof HTMLInputElement)) return;
        if (!el.classList.contains('mask-cpf')) return;

        const key = e.key;

        // permitir navegação/edição
        if (ALLOWED_KEYS.has(key)) return;

        // permitir dígitos
        if (/^\d$/.test(key)) return;

        // bloquear o resto
        e.preventDefault();
    }

    function onInput(e) {
        const el = e.target;
        if (!(el instanceof HTMLInputElement)) return;
        if (!el.classList.contains('mask-cpf')) return;
        applyMask(el);
    }

    function onPaste(e) {
        const el = e.target;
        if (!(el instanceof HTMLInputElement)) return;
        if (!el.classList.contains('mask-cpf')) return;

        e.preventDefault();
        const text = (e.clipboardData?.getData('text') || '');
        el.value = onlyDigits(text);
        applyMask(el);
    }

    function onFocus(e) {
        const el = e.target;
        if (!(el instanceof HTMLInputElement)) return;
        if (!el.classList.contains('mask-cpf')) return;
        applyMask(el);
    }

    return {
        init() {
            document.addEventListener('keydown', onKeyDown, true);
            document.addEventListener('input', onInput, true);
            document.addEventListener('paste', onPaste, true);
            document.addEventListener('focusin', onFocus, true);

            // ✅ garante que ao carregar página/partial já formata se houver valor
            document.querySelectorAll('input.mask-cpf').forEach(applyMask);
        }
    };
})();

// --------------------------------------------------
// CnpjPhoneMask – máscara para CNPJ e Telefone/Celular
// Classes:
//  - .mask-cnpj
//  - .mask-phone   (10 dígitos fixo / 11 dígitos celular)
// --------------------------------------------------
const CnpjPhoneMask = (function () {

    const ALLOWED_KEYS = new Set([
        'Backspace', 'Delete', 'ArrowLeft', 'ArrowRight', 'Home', 'End', 'Tab'
    ]);

    function onlyDigits(v) {
        return (v ?? '').toString().replace(/\D/g, '');
    }

    function formatCnpj(digits) {
        const d = onlyDigits(digits).slice(0, 14);
        // 00.000.000/0001-00
        return d
            .replace(/^(\d{2})(\d)/, "$1.$2")
            .replace(/^(\d{2})\.(\d{3})(\d)/, "$1.$2.$3")
            .replace(/^(\d{2})\.(\d{3})\.(\d{3})(\d)/, "$1.$2.$3/$4")
            .replace(/^(\d{2})\.(\d{3})\.(\d{3})\/(\d{4})(\d)/, "$1.$2.$3/$4-$5");
    }

    function formatPhone(digits) {
        const d = onlyDigits(digits).slice(0, 11);

        // 10 dígitos => (11) 3333-4444
        if (d.length <= 10) {
            return d
                .replace(/^(\d{2})(\d)/, "($1) $2")
                .replace(/(\d{4})(\d)/, "$1-$2");
        }

        // 11 dígitos => (11) 99999-9999
        return d
            .replace(/^(\d{2})(\d)/, "($1) $2")
            .replace(/(\d{5})(\d)/, "$1-$2");
    }

    function setCursorToEnd(el) {
        requestAnimationFrame(() => {
            try { el.setSelectionRange(el.value.length, el.value.length); } catch { }
        });
    }

    function applyMask(el) {
        if (el.classList.contains('mask-cnpj')) {
            el.value = formatCnpj(el.value);
            setCursorToEnd(el);
            return;
        }

        if (el.classList.contains('mask-phone')) {
            el.value = formatPhone(el.value);
            setCursorToEnd(el);
            return;
        }
    }

    function onKeyDown(e) {
        const el = e.target;
        if (!(el instanceof HTMLInputElement)) return;
        if (!el.classList.contains('mask-cnpj') && !el.classList.contains('mask-phone')) return;

        const key = e.key;

        // permitir navegação/edição
        if (ALLOWED_KEYS.has(key)) return;

        // permitir dígitos
        if (/^\d$/.test(key)) return;

        // bloquear o resto
        e.preventDefault();
    }

    function onInput(e) {
        const el = e.target;
        if (!(el instanceof HTMLInputElement)) return;
        if (!el.classList.contains('mask-cnpj') && !el.classList.contains('mask-phone')) return;
        applyMask(el);
    }

    function onPaste(e) {
        const el = e.target;
        if (!(el instanceof HTMLInputElement)) return;
        if (!el.classList.contains('mask-cnpj') && !el.classList.contains('mask-phone')) return;

        e.preventDefault();
        const text = (e.clipboardData?.getData('text') || '');
        el.value = onlyDigits(text);
        applyMask(el);
    }

    function onFocus(e) {
        const el = e.target;
        if (!(el instanceof HTMLInputElement)) return;
        if (!el.classList.contains('mask-cnpj') && !el.classList.contains('mask-phone')) return;
        // se vier do server já preenchido (editar), garante formatação
        applyMask(el);
    }

    // opcional: antes do submit, se quiser enviar só dígitos, descomente.
    // function unmaskForSubmit(el) { el.value = onlyDigits(el.value); }
    // function onSubmitCapture(e) {
    //     const form = e.target.closest ? e.target.closest('form') : e.target;
    //     if (!form) return;
    //     form.querySelectorAll('.mask-cnpj, .mask-phone').forEach(unmaskForSubmit);
    // }

    return {
        init() {
            document.addEventListener('keydown', onKeyDown, true);
            document.addEventListener('input', onInput, true);
            document.addEventListener('paste', onPaste, true);
            document.addEventListener('focusin', onFocus, true);
            // document.addEventListener('submit', onSubmitCapture, true);
        }
    };
})();
// --------------------------------------------------
// WhatsAppE164Mask – máscara BR E.164: +55DDDNÚMERO
// Classe: .mask-whatsapp-e164
// Ex: 11988935256 -> +5511988935256
// --------------------------------------------------
const WhatsAppE164Mask = (function () {

    const ALLOWED_KEYS = new Set([
        'Backspace', 'Delete', 'ArrowLeft', 'ArrowRight', 'Home', 'End', 'Tab'
    ]);

    function onlyDigits(v) {
        return (v ?? '').toString().replace(/\D/g, '');
    }

    function formatE164BR(value) {
        let d = onlyDigits(value);

        // Se o usuário digitou/colou com 55 no começo, remove pra normalizar
        if (d.startsWith('55')) d = d.slice(2);

        // Agora esperamos DDD(2) + número(8 ou 9). Mantém no máximo 11 dígitos (sem o 55)
        d = d.slice(0, 11);

        // Se não tiver nada, deixa vazio
        if (!d) return '';

        // Sempre prefixa +55
        return `+55${d}`;
    }

    function setCursorToEnd(el) {
        requestAnimationFrame(() => {
            try { el.setSelectionRange(el.value.length, el.value.length); } catch { }
        });
    }

    function applyMask(el) {
        el.value = formatE164BR(el.value);
        setCursorToEnd(el);
    }

    function onKeyDown(e) {
        const el = e.target;
        if (!(el instanceof HTMLInputElement)) return;
        if (!el.classList.contains('mask-whatsapp-e164')) return;

        const key = e.key;

        if (ALLOWED_KEYS.has(key)) return;

        // permite dígitos
        if (/^\d$/.test(key)) return;

        // permite '+' apenas se for o primeiro caractere
        if (key === '+') {
            if ((el.value || '').length === 0) return;
        }

        e.preventDefault();
    }

    function onInput(e) {
        const el = e.target;
        if (!(el instanceof HTMLInputElement)) return;
        if (!el.classList.contains('mask-whatsapp-e164')) return;
        applyMask(el);
    }

    function onPaste(e) {
        const el = e.target;
        if (!(el instanceof HTMLInputElement)) return;
        if (!el.classList.contains('mask-whatsapp-e164')) return;

        e.preventDefault();
        const text = (e.clipboardData?.getData('text') || '');
        el.value = text;
        applyMask(el);
    }

    function onFocus(e) {
        const el = e.target;
        if (!(el instanceof HTMLInputElement)) return;
        if (!el.classList.contains('mask-whatsapp-e164')) return;
        applyMask(el);
    }

    return {
        init() {
            document.addEventListener('keydown', onKeyDown, true);
            document.addEventListener('input', onInput, true);
            document.addEventListener('paste', onPaste, true);
            document.addEventListener('focusin', onFocus, true);
        }
    };
})();

// --------------------------------------------------
// SimplePriceMask – máscara para .preco-simples
// Digitação: "1" -> 0,01 -> 0,11 -> 1,11 -> 11,11 ...
// Formata 0–999.999.999,99 (pt-BR), sem "R$"
// Envia com vírgula no submit (ex.: "1234,56")
// --------------------------------------------------
const SimplePriceMask = (function () {
    const MAX_VALUE = 999999999.99; // 9 inteiros + 2 decimais
    const MAX_DIGITS = 11;           // 9 (inteiros) + 2 (decimais)
    const ALLOWED_KEYS = new Set([
        'Backspace', 'Delete', 'ArrowLeft', 'ArrowRight', 'Home', 'End', 'Tab'
    ]);

    function digitsFrom(el) {
        return (el.value || '').replace(/\D/g, '').slice(0, MAX_DIGITS);
    }

    function format(el) {
        let digits = digitsFrom(el);
        let raw = parseInt(digits || '0', 10);
        let num = (raw / 100) || 0;

        if (num > MAX_VALUE) num = MAX_VALUE;

        el.value = num.toLocaleString('pt-BR', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });

        // Cursor no final para não “quebrar” a digitação contínua
        requestAnimationFrame(() => {
            try { el.setSelectionRange(el.value.length, el.value.length); } catch { }
        });
    }

    // Remove pontos (milhar) e mantém vírgula para submission pt-BR
    function unmaskForSubmit(el) {
        if (!el || !el.value) return;
        el.value = el.value.replace(/\./g, '');
    }

    function onKeyDown(e) {
        const el = e.target;
        if (!el.classList || !el.classList.contains('preco-simples')) return;

        const key = e.key;

        // permite teclas de navegação/edição
        if (ALLOWED_KEYS.has(key)) return;

        // permite dígitos
        if (/^\d$/.test(key)) return;

        // bloqueia o resto (letras, símbolos etc.)
        e.preventDefault();
    }

    function onInput(e) {
        const el = e.target;
        if (!el.classList || !el.classList.contains('preco-simples')) return;
        format(el);
    }

    function onPaste(e) {
        const el = e.target;
        if (!el.classList || !el.classList.contains('preco-simples')) return;

        e.preventDefault();
        const text = (e.clipboardData?.getData('text') || '')
            .replace(/\D/g, '')
            .slice(0, MAX_DIGITS);

        // Coloca os dígitos crus e formata (0,01 / 0,11 / 1,11 ...)
        el.value = text;
        format(el);
    }

    function onFocus(e) {
        const el = e.target;
        if (!el.classList || !el.classList.contains('preco-simples')) return;
        // Garante formatação já no foco (ex.: valor vindo do servidor)
        format(el);
    }

    function onModalShown(e) {
        if (e.target && e.target.id === 'ModalCupomDesconto') {
            e.target.querySelectorAll('.preco-simples').forEach(format);
        }
    }

    function onSubmitCapture(e) {
        const form = e.target.closest ? e.target.closest('form') : e.target;
        if (!form) return;
        form.querySelectorAll('.preco-simples').forEach(unmaskForSubmit);
    }

    return {
        init() {
            document.addEventListener('keydown', onKeyDown, true);
            document.addEventListener('focusin', onFocus);
            document.addEventListener('input', onInput);
            document.addEventListener('paste', onPaste);
            document.addEventListener('shown.bs.modal', onModalShown);
            document.addEventListener('submit', onSubmitCapture, true);
        }
    };
})();


// --------------------------------------------------
// Módulo de Máscara de Preço
// --------------------------------------------------
const PriceMask = (function () {
    const MAX_VALUE = 999999999.99;

    function format(el) {
        let digits = el.value.replace(/\D/g, '').slice(0, 11);
        let num = parseInt(digits || '0', 10) / 100;
        if (num > MAX_VALUE) num = MAX_VALUE;
        el.value = Number(num.toFixed(2))
            .toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
    }
    function unmaskPrecoInput(el) {
        // remove espaços e R$, tira os pontos de milhar e troca vírgula por ponto
        el.value = el.value
            .replace(/\s/g, '')
            .replace('R$', '')
            .replace(/\./g, '')
            .replace(/,/g, '.');
    }

    function onInput(e) {
        if (e.target.classList.contains('preco')) {
            format(e.target);
        }
    }

    function onModalShown(e) {
        // dispara tanto para Produto quanto para Extra
        if (e.target.id === 'ModalProduto' || e.target.id === 'ModalExtra') {
            const inp = e.target.querySelector('.preco');
            if (inp) format(inp);
        }
    }

    return {
        init: function () {
            document.addEventListener('input', onInput);
            document.addEventListener('shown.bs.modal', onModalShown);
        }
    };
})();
// --------------------------------------------------
// DiscountMask v2 – máscara para .desconto-promocao (0–100, 2 casas)
// Guarda os dígitos crus em data-raw e renderiza formatado (pt-BR)
// Envia COM VÍRGULA no submit (ex.: "12,34")
// --------------------------------------------------
const DiscountMask = (function () {
    const MAX_RAW = 10000; // 100,00 => 10000 "centésimos"
    const DIGIT = /^[0-9]$/;

    function getRaw(el) {
        let raw = el.dataset.raw;
        if (raw == null) raw = (el.value || '').replace(/\D/g, '');
        if (raw === '') raw = '0';
        let n = parseInt(raw, 10);
        if (isNaN(n)) n = 0;
        if (n > MAX_RAW) n = MAX_RAW;
        return String(n);
    }

    function setRaw(el, raw) {
        if (!raw) raw = '0';
        raw = raw.replace(/\D/g, '');
        if (raw === '') raw = '0';
        let n = parseInt(raw, 10);
        if (isNaN(n)) n = 0;
        if (n > MAX_RAW) n = MAX_RAW;
        el.dataset.raw = String(n);
        render(el);
    }

    function render(el) {
        const n = parseInt(el.dataset.raw || '0', 10) || 0;
        const num = n / 100;
        el.value = num.toLocaleString('pt-BR', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
        requestAnimationFrame(() => {
            try { el.setSelectionRange(el.value.length, el.value.length); } catch { }
        });
    }

    function onFocus(e) {
        const el = e.target;
        if (!el.classList.contains('desconto-promocao')) return;
        setRaw(el, (el.value || '').replace(/\D/g, ''));
    }

    function onKeyDown(e) {
        const el = e.target;
        if (!el.classList.contains('desconto-promocao')) return;

        const code = e.key;
        if (code === 'Tab' || code === 'ArrowLeft' || code === 'ArrowRight' || code === 'Home' || code === 'End') return;

        e.preventDefault();

        let raw = getRaw(el);

        if (code === 'Backspace' || code === 'Delete') {
            raw = raw.slice(0, -1);
            if (raw === '') raw = '0';
            setRaw(el, raw);
            return;
        }

        if (DIGIT.test(code)) {
            raw = (raw === '0') ? code : raw + code;
            setRaw(el, raw);
            return;
        }
    }

    function onPaste(e) {
        const el = e.target;
        if (!el.classList.contains('desconto-promocao')) return;
        e.preventDefault();
        const text = (e.clipboardData?.getData('text') || '').replace(/\D/g, '');
        setRaw(el, text);
    }

    function onInputMobile(e) {
        const el = e.target;
        if (!el.classList.contains('desconto-promocao')) return;
        const digits = (el.value || '').replace(/\D/g, '');
        setRaw(el, digits);
    }

    // <<< AQUI: envia sempre com vírgula >>>
    function unmaskPercentInput(el) {
        const n = parseInt(el.dataset.raw || '0', 10) || 0;
        const val = (n / 100).toFixed(2).replace('.', ','); // "12,34"
        el.value = val;
    }

    function onModalShown(e) {
        if (!e.target) return;
        if (e.target.id === 'ModalPromocao' || e.target.id === 'ModalPromocaoProduto') {
            e.target.querySelectorAll('.desconto-promocao').forEach(inp => {
                setRaw(inp, (inp.value || '').replace(/\D/g, ''));
            });
        }
    }

    function onSubmit(e) {
        const form = e.target.closest ? e.target.closest('form') : e.target;
        if (!form) return;
        form.querySelectorAll('.desconto-promocao').forEach(unmaskPercentInput);
    }

    return {
        init: function () {
            document.addEventListener('focusin', onFocus);
            document.addEventListener('keydown', onKeyDown);
            document.addEventListener('paste', onPaste);
            document.addEventListener('input', onInputMobile);
            document.addEventListener('shown.bs.modal', onModalShown);
            document.addEventListener('submit', onSubmit, true);
        }
    };
})();


// --------------------------------------------------
// Módulo de Arquivos e Imagem Preview
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
// Módulo de Divisões por Tipo de Uso
// --------------------------------------------------
async function atualizarDivisoesPorTipoUso() {
    const selectTipo = document.getElementById('ddlTipoUso');
    const idTipoUso = selectTipo?.value;
    const idGrupo = document.querySelector('input[name="IdGrupo"]')?.value;
    if (!idTipoUso || !idGrupo) return;

    try {
        const res = await fetch(`/GrupoProduto/BuscarDivisoesPorTipoUso?idTipoUso=${idTipoUso}&idGrupo=${idGrupo}`, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        if (!res.ok) throw new Error();
        document.getElementById('divCheckDivisoes').innerHTML = await res.text();
    } catch {
        Swal.fire({ title: 'Erro', text: 'Não foi possível carregar as divisões.', icon: 'error', confirmButtonText: 'OK', confirmButtonColor: '#ca3a2d' });
    }
}

// --------------------------------------------------
// Tabela Paginacao
// --------------------------------------------------

document.addEventListener('DOMContentLoaded', () => {
    const tabelas = document.querySelectorAll("table[id^='tabelapaginacao_']");
    tabelas.forEach((tabela) => {
        aplicarDataTablePaginado(tabela);
    });
});

function aplicarDataTablePaginado(tabela) {
    const id = tabela.id;

    if (!$.fn.DataTable.isDataTable(`#${id}`)) {
        $(`#${id}`).DataTable({
            paging: true,
            pageLength: 6,
            lengthChange: false,
            searching: false,
            ordering: false,
            info: false,
            language: {
                paginate: {
                    previous: "Anterior",
                    next: "Próximo"
                },
                emptyTable: "Nenhum registro encontrado"
            },
            dom: "tp"
        });
    }
}

// --------------------------------------------------
// Garante que todos os checkboxes de tabelas paginadas
// sejam enviados no POST mesmo que estejam fora da página atual
// --------------------------------------------------

function copiarCheckboxesDeTodasAsPaginas(form) {
    if (!form) return;

    // limpa hiddens antigos desse mecanismo (pra não acumular)
    form.querySelectorAll('input[type="hidden"][data-dt-hidden="1"]').forEach(el => el.remove());

    const tabelas = form.querySelectorAll("table[id^='tabelapaginacao_']");
    tabelas.forEach(tabela => {
        const id = tabela.id;

        // só se DataTables estiver ativo nessa tabela
        if (!window.jQuery || !jQuery.fn.DataTable) return;
        if (!jQuery.fn.DataTable.isDataTable(`#${id}`)) return;

        const dt = jQuery(`#${id}`).DataTable();

        // ✅ dt.$ busca em TODAS as páginas (não depende do DOM da página atual)
        const checked = dt.$('input[type="checkbox"]:checked');

        checked.each(function () {
            const name = this.name;
            const value = this.value;

            if (!name) return;

            // não duplica
            const exists = form.querySelector(`input[type="hidden"][name="${CSS.escape(name)}"][value="${CSS.escape(value)}"]`);
            if (exists) return;

            const inputHidden = document.createElement("input");
            inputHidden.type = "hidden";
            inputHidden.name = name;
            inputHidden.value = value;
            inputHidden.setAttribute("data-dt-hidden", "1");
            form.appendChild(inputHidden);
        });
    });
}

// Inicializa comportamentos do modal de Versão quando ele é mostrado
document.addEventListener('shown.bs.modal', (ev) => {
    const modal = ev.target;
    if (!modal || modal.id !== 'ModalVersaoProd') return;

    const chk = modal.querySelector('#chkUploadAgora');
    const bloco = modal.querySelector('#blocoUpload');
    const url = modal.querySelector('#txtUrlPacote');

    const sync = () => {
        const up = chk?.checked === true;
        if (bloco) bloco.style.display = up ? '' : 'none';
        if (url) {
            url.readOnly = up;
            if (!up) url.removeAttribute('readonly');
        }
    };

    chk?.addEventListener('change', sync);
    sync();
});

//document.addEventListener('submit', async (ev) => {
//    const form = ev.target;
//    if (!(form instanceof HTMLFormElement)) return;

//    // Só intercepta se for data-ajax="true" E multipart
//    if (form.getAttribute('data-ajax') !== 'true') return;
//    const enc = (form.getAttribute('enctype') || '').toLowerCase();
//    if (enc !== 'multipart/form-data') return;

//    ev.preventDefault();

//    try {
//        const fd = new FormData(form);
//        const action = form.getAttribute('action') || window.location.href;
//        const method = (form.getAttribute('method') || 'POST').toUpperCase();

//        // OBS: Não setar Content-Type manualmente; o browser define o boundary.
//        const resp = await fetch(action, {
//            method,
//            body: fd,
//            credentials: 'include' // mantém cookies p/ antiforgery
//        });

//        // Se o server responder partial (em erro de validação), renderize-a no modal
//        const contentType = resp.headers.get('content-type') || '';
//        if (contentType.includes('application/json')) {
//            const json = await resp.json();

//            if (json.success) {
//                // Sucesso → fecha modal, mostra toast, recarrega listagem se precisar
//                const msg = json.message || form.getAttribute('data-success-msg') || 'OK';
//                // feche o modal
//                const modalEl = document.getElementById('ModalVersaoProd');
//                if (modalEl) {
//                    const bsModal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
//                    bsModal.hide();
//                }
//                // notificação simples
//                console.log(msg);
//                // se tiver função global de reload, chame-a aqui. Ex:
//                // window.Grid?.reload?.();
//                // ou faça location.reload();
//            } else {
//                // falhou com JSON (padrão seu) → mostre erro
//                alert(json.message || 'Falha ao salvar.');
//            }
//        } else {
//            // Provavelmente voltou a partial com erros de validação → substitui o corpo do modal
//            const html = await resp.text();
//            const container = document.querySelector('#modalContainer');
//            if (container) {
//                container.innerHTML = html;
//                // reabre modal (caso seu loader não faça isso automaticamente)
//                const newModal = document.getElementById('ModalVersaoProd');
//                if (newModal) new bootstrap.Modal(newModal).show();
//            } else {
//                // fallback
//                document.body.insertAdjacentHTML('beforeend', html);
//            }
//        }
//    } catch (err) {
//        console.error('Upload falhou', err);
//        alert('Erro ao enviar o arquivo. Tente novamente.');
//    }
//});


// --------------------------------------------------
// Inicialização de Todos os Módulos
// --------------------------------------------------

(function initAll() {
    ModalCleaner.init();
    ModalManager.init();
    AjaxHandler.init();
    PriceMask.init();
    DiscountMask.init();
    FilePreview.init();
    SimplePriceMask.init();
    DeleteAjax.init();
    ModalBridge.init();
    CnpjPhoneMask.init();
    CpfMask.init();    
    WhatsAppE164Mask.init();
})();

// --------------------------------------------------
// Reaplica DataTable ao abrir modais (caso esteja em modal)
// --------------------------------------------------
$(document).on('shown.bs.modal', function () {
    $("table[id^='tabelapaginacao_']").each(function () {
        aplicarDataTablePaginado(this);
    });
});
document.addEventListener("click", async function (e) {
    const btn = e.target.closest(".js-reimprimir");
    if (!btn) return;

    e.preventDefault();

    const url = btn.getAttribute("data-url");
    const confirmText =
        btn.getAttribute("data-confirm-text") ||
        btn.getAttribute("data-confirm") ||
        "Deseja reimprimir?";

    if (!url) return;

    // ✅ confirmação com Swal (Sim/Não)
    let confirmado = true;
    if (window.Swal) {
        const r = await Swal.fire({
            title: "Confirmação",
            text: confirmText,
            icon: "warning",
            showCancelButton: true,
            confirmButtonText: "Sim",
            cancelButtonText: "Não",
            confirmButtonColor: "#ca3a2d"
        });
        confirmado = r.isConfirmed;
    } else {
        confirmado = window.confirm(confirmText);
    }

    if (!confirmado) return;

    // ✅ antiforgery (pega do modal aberto)
    const modal = btn.closest(".modal") || document;
    const tokenEl = modal.querySelector('input[name="__RequestVerificationToken"]')
        || document.querySelector('input[name="__RequestVerificationToken"]');

    const headers = {
        "X-Requested-With": "XMLHttpRequest"
    };

    if (tokenEl?.value) headers["RequestVerificationToken"] = tokenEl.value;

    try {
        const resp = await fetch(url, {
            method: "POST",
            headers,
            credentials: "include"
        });

        const data = await resp.json().catch(() => null);

        const ok = resp.ok && data && data.success === true;
        const msg = (data && data.message)
            ? data.message
            : (ok ? "Pedido marcado como pendente de impressão." : `Falha (${resp.status}).`);

        if (window.Swal) {
            await Swal.fire({
                icon: ok ? "success" : "error",
                title: ok ? "Sucesso" : "Erro",
                text: msg,
                confirmButtonText: "OK",
                confirmButtonColor: "#ca3a2d"
            });
        } else {
            alert(msg);
        }

        if (ok) {
            // ✅ aqui você escolhe o comportamento:
            // 1) recarregar a página (mais simples)
            window.location.reload();

            // 2) ou se quiser reabrir o modal do pedido (usando seu ModalBridge/ModalLoader)
            // ModalLoader.open("Pedido", "CarregarModalPedido", { id: /* id do pedido */ });
        }

    } catch (err) {
        console.error(err);
        if (window.Swal) {
            Swal.fire({
                icon: "error",
                title: "Erro",
                text: "Erro ao solicitar reimpressão.",
                confirmButtonText: "OK",
                confirmButtonColor: "#ca3a2d"
            });
        } else {
            alert("Erro ao solicitar reimpressão.");
        }
    }
});
