// Run with: node --test tests/js/*.test.mjs
// The row activation decides in the browser which clicks and keys reach .NET. The DOM is a minimal stand-in: a row, a cell and the controls inside them.
import test from 'node:test';
import assert from 'node:assert/strict';

globalThis.Element = class {
    constructor(tag, parent, props = {}) {
        this.tag = tag;
        this.parent = parent;
        this.props = props;
    }

    closest(selector) {
        for (let e = this; e; e = e.parent) {
            if (selector.includes('tr[data-slot=row]') && e.tag === 'tr' && (!selector.includes('[data-clickable]') || e.props.clickable)) return e;
            if (selector.startsWith('a[href]') && e.props.interactive) return e;
        }
        return null;
    }

    contains(other) {
        for (let e = other; e; e = e.parent) if (e === this) return true;
        return false;
    }
};

const { rowActivation } = await import('../../src/Klyroo.Table/wwwroot/Klyroo.Table.lib.module.js');

const row = new Element('tr', null, { clickable: true });
const cell = new Element('td', row);
const button = new Element('button', cell, { interactive: true });
const ignoredCell = new Element('td', row, { interactive: true });
const plainRow = new Element('tr', null, {});
const plainCell = new Element('td', plainRow);
const event = (target, extra = {}) => ({ target, preventDefault() { this.prevented = true; }, ...extra });

test('a click on a cell is an activation of its row with the modifier keys', () => {
    const activation = rowActivation(event(cell, { ctrlKey: true }), 'click');
    assert.equal(activation.row, row);
    assert.equal(activation.detail.ctrlKey, true);
    assert.equal(activation.detail.key, null);
});

test('a click on a control inside the row, or in a cell marked to be ignored, is not an activation', () => {
    assert.equal(rowActivation(event(button), 'click'), null);
    assert.equal(rowActivation(event(ignoredCell), 'click'), null);
});

test('a row that is not clickable is never announced', () => {
    assert.equal(rowActivation(event(plainCell), 'click'), null);
    assert.equal(rowActivation(event(plainRow, { key: 'Enter' }), 'key'), null);
});

test('Enter and Space on the row itself activate it, and Space does not scroll', () => {
    assert.equal(rowActivation(event(row, { key: 'Enter' }), 'key').detail.key, 'Enter');
    const space = event(row, { key: ' ' });
    assert.equal(rowActivation(space, 'key').detail.key, ' ');
    assert.equal(space.prevented, true);
});

test('a key typed inside a control, another key, or a shortcut is not an activation', () => {
    assert.equal(rowActivation(event(button, { key: 'Enter' }), 'key'), null);
    assert.equal(rowActivation(event(cell, { key: ' ' }), 'key'), null);
    assert.equal(rowActivation(event(row, { key: 'Tab' }), 'key'), null);
    assert.equal(rowActivation(event(row, { key: 'Enter', ctrlKey: true }), 'key'), null);
});

test('loading the module twice does not throw when Blazor refuses the second registration', async () => {
    const registered = new Set();
    const blazor = {
        registerCustomEventType(name) {
            if (registered.has(name)) throw new Error(`The event '${name}' is already registered.`);
            registered.add(name);
        },
    };
    const url = '../../src/Klyroo.Table/wwwroot/Klyroo.Table.lib.module.js';
    const first = await import(url + '?first');
    const second = await import(url + '?second');
    first.afterStarted(blazor);
    assert.doesNotThrow(() => second.afterStarted(blazor));
    assert.deepEqual([...registered], ['klyrowactivate']);
});
