window.imageInterop = {
    resizeAndCompress: async function (base64Input, maxWidth, quality) {
        // Decodificar base64 -> bytes
        const binary = atob(base64Input);
        const bytes = new Uint8Array(binary.length);
        for (let i = 0; i < binary.length; i++) {
            bytes[i] = binary.charCodeAt(i);
        }

        const blob = new Blob([bytes]);
        const bitmap = await createImageBitmap(blob);

        let { width, height } = bitmap;
        if (width > maxWidth) {
            height = Math.round(height * (maxWidth / width));
            width = maxWidth;
        }

        const canvas = document.createElement('canvas');
        canvas.width = width;
        canvas.height = height;
        const ctx = canvas.getContext('2d');
        ctx.drawImage(bitmap, 0, 0, width, height);

        const resultBlob = await new Promise(resolve =>
            canvas.toBlob(resolve, 'image/jpeg', quality));
        const arrayBuffer = await resultBlob.arrayBuffer();
        const outBytes = new Uint8Array(arrayBuffer);

        // Codificar de vuelta a base64
        let binaryOut = '';
        for (let i = 0; i < outBytes.byteLength; i++) {
            binaryOut += String.fromCharCode(outBytes[i]);
        }
        return btoa(binaryOut);
    },

    getYouTubeThumbnail: function (url) {
        const match = url.match(/(?:youtube\.com\/(?:watch\?v=|shorts\/)|youtu\.be\/)([a-zA-Z0-9_-]{11})/);
        return match ? `https://img.youtube.com/vi/${match[1]}/hqdefault.jpg` : null;
    },

    getTikTokThumbnail: async function (url) {
        try {
            const response = await fetch(`https://www.tiktok.com/oembed?url=${encodeURIComponent(url)}`);
            if (!response.ok) return null;
            const data = await response.json();
            return data.thumbnail_url ?? null;
        } catch {
            return null; // Puede fallar por CORS, lo tratamos como "no disponible"
        }
    }
};