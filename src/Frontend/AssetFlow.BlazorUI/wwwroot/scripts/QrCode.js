window.printQrCode = function (dataUrl, designation, reference, ficheUrl) {

    // 1. Ouvrir la fenêtre EN PREMIER — synchrone, avant tout await
    //    Le popup blocker laisse passer window.open() appelé ici
    var w = window.open('', '_blank');
    if (!w) {
        alert("Veuillez autoriser les popups pour imprimer le QR Code.");
        return;
    }

    var html = `<!DOCTYPE html>
<html lang="fr">
<head>
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width, initial-scale=1"/>
<title>QR \u2014 ${designation}</title>
<style>
  * { box-sizing: border-box; margin: 0; padding: 0; }
  body { font-family: sans-serif; display: flex; flex-direction: column;
         align-items: center; justify-content: center;
         min-height: 100vh; padding: 2rem; background: white; color: #111; }
  img  { width: 220px; height: 220px; display: block; }
  h2   { font-size: 1.2rem; font-weight: 800; margin: 1rem 0 0.25rem; text-align: center; }
  p    { font-size: 0.85rem; color: #555; text-align: center; margin-top: 0.25rem; }
  code { font-size: 0.6rem; color: #333; margin-top: 0.75rem; display: block;
         word-break: break-all; text-align: center; max-width: 260px; }
  @media print { body { padding: 0.5rem; } }
</style>
</head>
<body>
<img src="${dataUrl}" alt="QR Code"/>
<h2>${designation}</h2>
<p>Numéro de série : ${reference}</p>
<code>${ficheUrl}</code>
<script>
  window.onload = function() { setTimeout(function() { window.print(); }, 400); };
<\/script>
</body>
</html>`;

    // 2. Écrire le HTML dans la fenêtre déjà ouverte
    w.document.open();
    w.document.write(html);
    w.document.close();
};