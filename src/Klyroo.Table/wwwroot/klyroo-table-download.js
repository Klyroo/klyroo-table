// The browser side of KlyDataListExport: saves a text as a file, as the browser's download does.

/** Saves `text` as a file named `fileName`. */
export function downloadText(fileName, text, mimeType) {
    const url = URL.createObjectURL(new Blob([text], { type: mimeType }));
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    link.style.display = 'none';
    document.body.appendChild(link);
    link.click();
    link.remove();
    // Revoke after the click has been handled, or some browsers cancel the download.
    setTimeout(() => URL.revokeObjectURL(url), 0);
}
