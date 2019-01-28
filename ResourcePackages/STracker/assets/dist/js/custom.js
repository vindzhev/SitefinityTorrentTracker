function updateUploadName(element, defaultText) {
    var uploadButton = element.parentElement.getElementsByTagName('button')[0];
    if (uploadButton) {
        if (element.files.length > 0) {
            uploadButton.innerHTML = element.value.split('\\').pop();
        } else { uploadButton.innerHTML = defaultText; }
    }
}