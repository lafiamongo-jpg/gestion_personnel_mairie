// Session persistante via localStorage
window.authStorage = {
    save: (key, value) => {
        try { localStorage.setItem(key, value); } catch (e) { }
    },
    load: (key) => {
        try { return localStorage.getItem(key); } catch (e) { return null; }
    },
    remove: (key) => {
        try { localStorage.removeItem(key); } catch (e) { }
    },
    clear: () => {
        try {
            localStorage.removeItem('auth_email');
            localStorage.removeItem('auth_nom');
            localStorage.removeItem('auth_role');
            localStorage.removeItem('auth_agentId');
            localStorage.removeItem('auth_userId');
        } catch (e) { }
    }
};
