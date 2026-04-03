// wwwroot/js/voice-recorder.js
// Remplace voice-assistant.js — utilise MediaRecorder au lieu de Web Speech API
window.VoiceAssistant = (function () {
    let dotNetRef   = null;
    let mediaStream = null;
    let recorder    = null;
    let chunks      = [];

    function init(ref) {
        dotNetRef = ref;
    }

    async function start() {
        chunks = [];
        try {
            mediaStream = await navigator.mediaDevices.getUserMedia({ audio: true });
        } catch (err) {
            dotNetRef.invokeMethodAsync('OnError', 'Micro non autorisé');
            return;
        }

        // Choisir le meilleur format supporté
        const mimeType = MediaRecorder.isTypeSupported('audio/webm;codecs=opus')
            ? 'audio/webm;codecs=opus'
            : MediaRecorder.isTypeSupported('audio/webm')
                ? 'audio/webm'
                : 'audio/ogg';

        recorder = new MediaRecorder(mediaStream, { mimeType });

        recorder.ondataavailable = (e) => {
            if (e.data && e.data.size > 0)
                chunks.push(e.data);
        };

        recorder.onstop = async () => {
            const blob     = new Blob(chunks, { type: mimeType });
            const baseMime = mimeType.split(';')[0]; // "audio/webm"

            // Convertir en base64
            const reader = new FileReader();
            reader.onloadend = () => {
                // reader.result = "data:audio/webm;base64,XXXX..."
                const base64 = reader.result.split(',')[1];
                dotNetRef.invokeMethodAsync('OnAudioReady', base64, baseMime);
            };
            reader.readAsDataURL(blob);

            // Libérer le micro
            mediaStream?.getTracks().forEach(t => t.stop());
            mediaStream = null;
        };

        recorder.start();
    }

    function stop() {
        if (recorder && recorder.state !== 'inactive') {
            recorder.stop();
        }
    }

    return { init, start, stop };
})();