/**
 * Downloads a file from a base64 encoded string
 * @param {string} filename - The name of the file to download
 * @param {string} base64Data - The base64 encoded data
 */
function downloadFileFromStream(filename, base64Data) {
    console.log(`Starting download of ${filename}`);
    
    try {
        // Create a link element
        const link = document.createElement('a');
        
        // Convert the base64 string to a blob URL
        const byteCharacters = atob(base64Data);
        console.log(`Decoded base64 string, length: ${byteCharacters.length}`);
        
        const byteArrays = [];
        
        for (let offset = 0; offset < byteCharacters.length; offset += 512) {
            const slice = byteCharacters.slice(offset, offset + 512);
            
            const byteNumbers = new Array(slice.length);
            for (let i = 0; i < slice.length; i++) {
                byteNumbers[i] = slice.charCodeAt(i);
            }
            
            const byteArray = new Uint8Array(byteNumbers);
            byteArrays.push(byteArray);
        }
        
        // Determine file type based on extension
        let contentType = 'application/octet-stream';
        if (filename.endsWith('.xlsx')) {
            contentType = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';
        } else if (filename.endsWith('.xls')) {
            contentType = 'application/vnd.ms-excel';
        }
        
        console.log(`Creating blob with content type: ${contentType}`);
        const blob = new Blob(byteArrays, { type: contentType });
        const url = URL.createObjectURL(blob);
        
        // Set the download link properties
        link.href = url;
        link.download = filename;
        console.log(`Download link created: ${url}`);
        
        // Append to the body, click, and remove
        document.body.appendChild(link);
        console.log('Link added to document, triggering click');
        link.click();
        
        // Clean up
        setTimeout(() => {
            document.body.removeChild(link);
            URL.revokeObjectURL(url);
            console.log('Download complete, resources cleaned up');
        }, 100);
        
        return true;
    } catch (error) {
        console.error(`Error downloading file: ${error.message}`);
        return false;
    }
} 