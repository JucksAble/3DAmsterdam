mergeInto(LibraryManager.library, {
    SetUniqueShareURL: function (uniqueToken) {
        sharedUrlText = window.location.href.split('?')[0] + "?view=" + Pointer_stringify(uniqueToken);

        //inject copy button if we do not have it yet
        if (!window.copySharedURLButton) {
            window.copySharedURLButton = document.createElement("input");
            window.copySharedURLButton.type = 'text';
            window.copySharedURLButton.style.cssText = 'display:none; position: fixed; bottom: 0; left: 0; z-index: 2; width: 0px; height: 0px;';
            window.copySharedURLButton.id = 'copySharedURLButton';
            window.copySharedURLButton.onclick = function () {
                window.copySharedURLButton.focus();
                window.copySharedURLButton.select();
                preventNativeCopyEvents = false;
                document.execCommand('copy');
                //feedback animation in unity
                unityInstance.SendMessage("SharedURL", "CopiedText");

                console.log("Copied the url to clipboard: " + sharedUrlText);
            };
            document.body.appendChild(window.copySharedURLButton);
        }
        window.copySharedURLButton.value = sharedUrlText;
    }
});