// Tableau Kanban : glisser-déposer des cartes et filtre instantané.
(() => {
    const board = document.querySelector(".board");
    const moveForm = document.getElementById("move-form");
    if (!board || !moveForm) {
        return;
    }
    const token = moveForm.querySelector("input[name='__RequestVerificationToken']")?.value ?? "";

    let dragged = null;
    let origin = null;      // colonne de départ, pour annuler si besoin
    let originNext = null;  // carte qui suivait, pour retrouver la place exacte
    let dropped = false;

    const updateCounts = () => {
        board.querySelectorAll(".board-column").forEach((column) => {
            column.querySelector("[data-count]").textContent = column.querySelectorAll(".task-card").length;
        });
    };

    const restore = (card) => {
        origin.insertBefore(card, originNext);
        updateCounts();
    };

    // Première carte dont le milieu est sous le curseur : la carte déplacée s'insère juste avant.
    const cardAfter = (zone, y) =>
        [...zone.querySelectorAll(".task-card:not(.dragging)")].find((card) => {
            const box = card.getBoundingClientRect();
            return y < box.top + box.height / 2;
        });

    board.addEventListener("dragstart", (event) => {
        const card = event.target.closest(".task-card");
        if (!card) {
            return;
        }
        dragged = card;
        origin = card.parentElement;
        originNext = card.nextElementSibling;
        dropped = false;
        event.dataTransfer.effectAllowed = "move";
        event.dataTransfer.setData("text/plain", card.dataset.id);
        requestAnimationFrame(() => card.classList.add("dragging"));
    });

    board.addEventListener("dragover", (event) => {
        const zone = event.target.closest("[data-dropzone]");
        if (!zone || !dragged) {
            return;
        }
        event.preventDefault();
        event.dataTransfer.dropEffect = "move";
        board.querySelectorAll(".drop-target").forEach((z) => z !== zone && z.classList.remove("drop-target"));
        zone.classList.add("drop-target");
        zone.insertBefore(dragged, cardAfter(zone, event.clientY) ?? zone.querySelector(".board-empty"));
    });

    board.addEventListener("drop", async (event) => {
        const zone = event.target.closest("[data-dropzone]");
        if (!zone || !dragged) {
            return;
        }
        event.preventDefault();
        dropped = true;

        const card = dragged;
        const state = zone.closest(".board-column").dataset.state;
        const index = [...zone.querySelectorAll(".task-card")].indexOf(card);
        updateCounts();

        const body = new FormData();
        body.append("__RequestVerificationToken", token);
        body.append("taskId", card.dataset.id);
        body.append("state", state);
        body.append("index", index);

        try {
            const response = await fetch(moveForm.action, { method: "POST", body });
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }
            const result = await response.json();
            card.classList.toggle("is-done", result.done);
        } catch {
            restore(card);
            window.alert("Le déplacement n'a pas pu être enregistré. Vérifiez votre connexion puis réessayez.");
        }
    });

    board.addEventListener("dragend", () => {
        if (!dragged) {
            return;
        }
        dragged.classList.remove("dragging");
        board.querySelectorAll(".drop-target").forEach((zone) => zone.classList.remove("drop-target"));
        if (!dropped) {
            restore(dragged); // lâchée hors d'une colonne : retour à sa place
        }
        dragged = null;
    });

    // Filtre : texte (titre, #étiquette, personne) et « Mes tâches ».
    const filter = document.querySelector("[data-board-filter]");
    const mine = document.querySelector("[data-board-mine]");
    const me = board.dataset.me;

    const applyFilter = () => {
        const text = (filter?.value ?? "").trim().toLowerCase();
        board.querySelectorAll(".task-card").forEach((card) => {
            const matchesText = !text || card.dataset.search.includes(text);
            const matchesMine = !mine?.checked || card.dataset.assignee === me;
            card.hidden = !(matchesText && matchesMine);
        });
    };

    filter?.addEventListener("input", applyFilter);
    mine?.addEventListener("change", applyFilter);
})();
