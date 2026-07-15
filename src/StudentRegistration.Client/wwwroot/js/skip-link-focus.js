document.addEventListener("click", event => {
    const link = event.target.closest("a.srs-skip-link[href^='#']");
    if (!link) {
        return;
    }

    const targetId = link.getAttribute("href").slice(1);
    const target = document.getElementById(targetId);
    if (!target) {
        return;
    }

    event.preventDefault();
    target.focus({ preventScroll: true });
    target.scrollIntoView({ block: "start" });
});
