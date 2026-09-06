window.backpackResizer = window.backpackResizer || {
    dirty: false,
    setDirty(isDirty) {
        this.dirty = isDirty;
    }
};

window.addEventListener("beforeunload", (e) => {
    if (!window.backpackResizer.dirty) {
        return;
    }

    e.preventDefault();
    // noinspection JSDeprecatedSymbols - preventDefault called above and this added is technically 'legacy' support
    e.returnValue = "";
});
