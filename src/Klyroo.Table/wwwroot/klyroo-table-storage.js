// The remembered filters of KlyDataList (DataListFilter.WithPersist): one localStorage entry per filter.
// Storage can be missing or refuse a call (private browsing, disabled site data, a full quota, a sandboxed frame): every call is guarded and reports nothing rather than throwing.

function storage() {
    try {
        return globalThis.localStorage ?? null;
    } catch {
        return null;
    }
}

// The stored text of each key, in order; null for a key with no entry or when storage cannot be read.
export function readAll(keys) {
    const store = storage();
    return (keys ?? []).map(key => {
        try {
            return store?.getItem(key) ?? null;
        } catch {
            return null;
        }
    });
}

// Stores a value, or removes the entry when the value is empty. Returns whether storage took it.
export function write(key, value) {
    const store = storage();
    if (!store || !key) return false;
    try {
        if (value) store.setItem(key, value);
        else store.removeItem(key);
        return true;
    } catch {
        return false;
    }
}
