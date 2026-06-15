// Session JWT via localStorage
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
            localStorage.removeItem('auth_token');
            localStorage.removeItem('auth_expires');
        } catch (e) { }
    }
};
