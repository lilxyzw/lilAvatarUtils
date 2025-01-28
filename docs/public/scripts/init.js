var scripts = ['/lilAvatarUtils/scripts/jquery.min.js', '/lilAvatarUtils/scripts/lightbox.min.js', '/lilAvatarUtils/scripts/script.js'];
var i = 0;

function appendScript() {
    var script = document.createElement('script');
    script.src = scripts[i];
    document.body.appendChild(script);

    if (i++ < 2) {
        script.onload = appendScript;
    }
}

appendScript();