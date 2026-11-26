// The browser side of KlyDataListExport: saves a stream of bytes as a file, as the browser's download does.

/** Saves the bytes of a DotNetStreamReference as a file named `fileName`. */
export async function downloadStream(fileName, streamReference, mimeType) {
    const buffer = await streamReference.arrayBuffer();
    const url = URL.createObjectURL(new Blob([buffer], { type: mimeType }));
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
