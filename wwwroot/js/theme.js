// Chargé de façon bloquante dans <head> : applique le thème avant l'affichage (pas de flash blanc en mode sombre).
// Fichier séparé plutôt que script en ligne, pour que la politique de sécurité (CSP) puisse interdire tout script en ligne.
(function () {
    var theme = null;
    try { theme = localStorage.getItem("theme"); } catch (e) { }
    if (theme !== "light" && theme !== "dark") {
        theme = window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
    }
    document.documentElement.setAttribute("data-bs-theme", theme);
})();
