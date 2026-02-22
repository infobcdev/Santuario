(function () {

    // ==========================
    // PREVIEW IMAGEM CAPA
    // ==========================
    let lastObjectUrl = null;

    document.addEventListener('change', function (e) {
        const input = e.target;
        if (!input || input.id !== 'arquivoImagemCapa') return;

        const modal = input.closest('#ModalNoticia');
        if (!modal) return;

        const imgPreview = modal.querySelector('#previewImagemCapa');
        if (!imgPreview) return;

        const file = input.files && input.files[0];
        if (!file) return;

        if (lastObjectUrl) URL.revokeObjectURL(lastObjectUrl);
        lastObjectUrl = URL.createObjectURL(file);
        imgPreview.src = lastObjectUrl;
    });

    document.addEventListener('hidden.bs.modal', function (ev) {
        if (!ev.target || ev.target.id !== 'ModalNoticia') return;
        if (lastObjectUrl) URL.revokeObjectURL(lastObjectUrl);
        lastObjectUrl = null;
    });

    // ==========================
    // QUILL: fontes/tamanho
    // ==========================
    const FONT_WHITELIST = [
        'arial', 'times', 'georgia', 'tahoma', 'trebuchet', 'verdana', 'courier',
        'roboto', 'opensans', 'inter', 'poppins', 'montserrat', 'lora', 'merriweather', 'playfair'
    ];
    const SIZE_WHITELIST = Array.from({ length: 72 }, (_, i) => String(i + 1));

    function injectQuillSizeCss() {
        if (document.getElementById('quillSizeCss_1_72')) return;

        let css = '';
        for (let i = 1; i <= 72; i++) {
            css += `
            #ModalNoticia #noticiaEditor .ql-editor .ql-size-${i} { font-size:${i}px; }
            #ModalNoticia .noticia-preview .ql-size-${i} { font-size:${i}px; }
            `;
        }

        const style = document.createElement('style');
        style.id = 'quillSizeCss_1_72';
        style.textContent = css;
        document.head.appendChild(style);
    }

    function ensureQuillFormats() {
        if (!window.Quill) return;

        const Font = Quill.import('formats/font');
        Font.whitelist = FONT_WHITELIST;
        Quill.register(Font, true);

        const Size = Quill.import('formats/size');
        Size.whitelist = SIZE_WHITELIST;
        Quill.register(Size, true);

        injectQuillSizeCss();
    }

    // ==========================
    // Preenche select ql-size 1..72
    // ==========================
    function ensureSizeSelect(modal) {
        const sel = modal.querySelector('#noticiaSizeSelect');
        if (!sel) return;

        if (sel.dataset.ready === '1') return;
        sel.dataset.ready = '1';

        sel.innerHTML = '';
        for (let i = 1; i <= 72; i++) {
            const opt = document.createElement('option');
            opt.value = String(i);
            opt.textContent = `${i}px`;
            if (i === 15) opt.selected = true;
            sel.appendChild(opt);
        }
    }

    // ==========================
    // CORES
    // ==========================
    function wireColorPickers(modal, quill) {
        const textColorInput = modal.querySelector('#noticiaTextColor');
        const bgColorInput = modal.querySelector('#noticiaBgColor');
        const clearBtn = modal.querySelector('#noticiaClearColor');

        if (!textColorInput || !bgColorInput) return;

        textColorInput.addEventListener('input', function () {
            quill.format('color', this.value, 'user');
        });

        bgColorInput.addEventListener('input', function () {
            quill.format('background', this.value, 'user');
        });

        clearBtn?.addEventListener('click', function (e) {
            e.preventDefault();
            quill.format('color', false, 'user');
            quill.format('background', false, 'user');
        });
    }

    // ==========================
    // INIT
    // ==========================
    function initEditor(modal) {
        if (!modal) return;

        ensureQuillFormats();
        ensureSizeSelect(modal);

        const editorEl = modal.querySelector('#noticiaEditor');
        const toolbarEl = modal.querySelector('#noticiaToolbar');
        const previewEl = modal.querySelector('#noticiaPreview');
        const jsonField = modal.querySelector('#ConteudoJson');
        const htmlField = modal.querySelector('#ConteudoHtml');

        if (!editorEl || !toolbarEl || !previewEl || !jsonField || !htmlField) return;
        if (editorEl.__quill) return;

        const modules = {
            toolbar: { container: toolbarEl }
        };

        // Resize de imagem 
        if (window.ImageResize) {
            modules.imageResize = {
                modules: ['Resize', 'DisplaySize']
            };
        }

        const quill = new Quill(editorEl, {
            theme: 'snow',
            modules
        });

        editorEl.__quill = quill;

        wireColorPickers(modal, quill);

        // carregar conteúdo (delta > html)
        let loaded = false;
        try {
            const raw = (jsonField.value || '').trim();
            if (raw) {
                quill.setContents(JSON.parse(raw));
                loaded = true;
            }
        } catch { }

        if (!loaded) {
            const html = (htmlField.value || '').trim();
            if (html) {
                quill.clipboard.dangerouslyPasteHTML(html);
                loaded = true;
            }
        }

        sync();
        quill.on('text-change', sync);

        function sync() {
            const delta = quill.getContents();
            const html = quill.root.innerHTML;
            jsonField.value = JSON.stringify(delta);
            htmlField.value = html;
            previewEl.innerHTML = html;
        }

        modal.querySelector('#formNoticia')?.addEventListener('submit', sync);
    }

    document.addEventListener('shown.bs.modal', function (ev) {
        const modal = ev.target;
        if (!modal || modal.id !== 'ModalNoticia') return;
        initEditor(modal);
    });

})();