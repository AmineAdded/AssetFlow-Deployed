/* ═══════════════════════════════════════════════════════════════
   Lecteur vocal pro — wwwroot/js/voice-player.js
   Compatible avec les DEUX messageries :
     • Achat / IT  → préfixe `mai-`  (.mai-voice / .mai-voice-wave / .mai-voice-time)
     • Employé     → préfixe `msg-`  (.msg-audio-player / .msg-audio-wave / .msg-audio-dur)
   À référencer dans index.html / _Host.cshtml :
     <script src="js/voice-player.js"></script>
   ═══════════════════════════════════════════════════════════════ */

(function () {
    let currentAudio = null;
    let currentBtn   = null;
    let rafId        = null;

    // ── Helpers : retrouve les éléments quel que soit le préfixe ──
    function findWrap(btn) {
        // Le wrap est soit .mai-voice (Achat) soit .msg-audio-player (Employé)
        return btn.closest('.mai-voice, .msg-audio-player') || btn.parentElement;
    }
    function findWave(wrap) {
        if (!wrap) return null;
        return wrap.querySelector('.mai-voice-wave, .msg-audio-wave');
    }
    function findTimeEl(wrap) {
        if (!wrap) return null;
        return wrap.querySelector('.mai-voice-time, .msg-audio-dur');
    }

    function stopCurrent() {
        if (currentAudio) {
            currentAudio.pause();
            currentAudio.currentTime = 0;
        }
        if (currentBtn) {
            currentBtn.classList.remove('playing');
            const wrap = findWrap(currentBtn);
            const wave = findWave(wrap);
            if (wave) {
                wave.querySelectorAll('span').forEach(s => s.classList.remove('played'));
            }
        }
        if (rafId) cancelAnimationFrame(rafId);
        currentAudio = null;
        currentBtn   = null;
        rafId        = null;
    }

    function updateProgress(audio, wave, timeEl) {
        if (!audio.duration || !isFinite(audio.duration)) return;
        const ratio  = audio.currentTime / audio.duration;
        const bars   = wave.querySelectorAll('span');
        const filled = Math.floor(ratio * bars.length);
        bars.forEach((b, i) => b.classList.toggle('played', i < filled));

        // Mise à jour du temps restant
        if (timeEl) {
            const remaining = Math.max(0, audio.duration - audio.currentTime);
            const m = Math.floor(remaining / 60);
            const s = Math.floor(remaining % 60);
            timeEl.textContent = `${m}:${s.toString().padStart(2,'0')}`;
        }
    }

    window.toggleVoice = function (audioId, btn) {
        const audio = document.getElementById(audioId);
        if (!audio) return;

        const wrap   = findWrap(btn);
        const wave   = findWave(wrap);
        const timeEl = findTimeEl(wrap);

        // Si on clique sur le bouton déjà actif → pause
        if (currentAudio === audio && !audio.paused) {
            stopCurrent();
            return;
        }

        // Stoppe tout autre lecteur en cours
        if (currentAudio && currentAudio !== audio) stopCurrent();

        currentAudio = audio;
        currentBtn   = btn;
        btn.classList.add('playing');

        audio.play().catch(err => {
            console.warn('Lecture vocal impossible:', err);
            stopCurrent();
        });

        const tick = () => {
            if (!audio.paused) {
                updateProgress(audio, wave, timeEl);
                rafId = requestAnimationFrame(tick);
            }
        };
        rafId = requestAnimationFrame(tick);

        audio.onended = () => stopCurrent();
    };

    // Permet de cliquer sur la waveform pour se positionner
    document.addEventListener('click', (e) => {
        const wave = e.target.closest?.('.mai-voice-wave, .msg-audio-wave');
        if (!wave) return;
        const wrap  = wave.closest('.mai-voice, .msg-audio-player');
        const audio = wrap?.querySelector('audio');
        if (!audio || !audio.duration || !isFinite(audio.duration)) return;
        const rect   = wave.getBoundingClientRect();
        const ratio  = Math.min(1, Math.max(0, (e.clientX - rect.left) / rect.width));
        audio.currentTime = ratio * audio.duration;
        updateProgress(audio, wave, findTimeEl(wrap));
    });
})();
