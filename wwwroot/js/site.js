// Bascule clair/sombre (le thème initial est appliqué dans <head>, voir _Layout.cshtml).
document.addEventListener("click", (event) => {
    if (!event.target.closest("[data-theme-toggle]")) {
        return;
    }
    const root = document.documentElement;
    const next = root.getAttribute("data-bs-theme") === "dark" ? "light" : "dark";
    root.setAttribute("data-bs-theme", next);
    try {
        localStorage.setItem("theme", next);
    } catch {
        // stockage indisponible (navigation privée) : le thème ne sera simplement pas mémorisé
    }
});

// Confirmation avant les actions destructrices : data-confirm sur le formulaire ou le bouton.
document.addEventListener("submit", (event) => {
    const message = event.submitter?.dataset.confirm ?? event.target.dataset.confirm;
    if (message && !window.confirm(message)) {
        event.preventDefault();
    }
});

// Filtres : les champs data-autosubmit envoient leur formulaire dès qu'ils changent.
document.addEventListener("change", (event) => {
    if (event.target.matches("[data-autosubmit]")) {
        event.target.form?.requestSubmit();
    }
});
