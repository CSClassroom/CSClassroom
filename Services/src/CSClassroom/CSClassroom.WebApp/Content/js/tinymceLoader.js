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

window.initTinyMce = function(selector, extraSettings) {
    var settings = {
        selector: selector,
        menubar: false,
        plugins: [
            'advlist autolink lists link charmap preview anchor',
            'searchreplace visualblocks fullscreen',
            'textcolor table autoresize'
        ],
        toolbar: 'undo redo | styleselect | bold italic underline | forecolor backcolor | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link table | preview',
        forced_root_block: false,
        branding: false,
        elementpath: false,
        height: 200,
        autoresize_min_height: 200,
        autoresize_bottom_margin: 0,
        relative_urls: false,
        remove_script_host: false
    };

    for (var propName in extraSettings) {
        settings[propName] = extraSettings[propName];
    }

    tinyMCE.init(settings);
}

