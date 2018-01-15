var tinymce = require('tinymce/tinymce');
require('tinymce/themes/modern/theme');
require('tinymce/plugins/advlist');
require('tinymce/plugins/autolink');
require('tinymce/plugins/lists');
require('tinymce/plugins/link');
require('tinymce/plugins/charmap');
require('tinymce/plugins/preview');
require('tinymce/plugins/anchor');
require('tinymce/plugins/searchreplace');
require('tinymce/plugins/visualblocks');
require('tinymce/plugins/fullscreen');
require('tinymce/plugins/textcolor');
require('tinymce/plugins/table');
require('tinymce/plugins/autoresize');
require('./tinymceSaveAjaxPlugin');

window.initTinyMce = function(selector, modifySettings) {
    var defaultSettings = {
        selector: selector,
        menubar: false,
        plugins: [
            'advlist autolink lists link charmap preview anchor',
            'searchreplace visualblocks fullscreen',
            'textcolor table autoresize'
        ],
        toolbar: 'styleselect | bold italic underline | forecolor backcolor | bullist numlist outdent indent | link table | preview',
        forced_root_block: false,
        branding: false,
        elementpath: false,
        height: 200,
        autoresize_min_height: 200,
        autoresize_bottom_margin: 0,
        relative_urls: false,
        remove_script_host: false,
        setup: (editor) => editor.on('change', () => editor.save())
    };

    if (modifySettings) {
        modifySettings(defaultSettings);
    }

    tinyMCE.init(defaultSettings);
};
