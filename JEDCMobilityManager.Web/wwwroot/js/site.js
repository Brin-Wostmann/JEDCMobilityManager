[HTMLCollection.prototype, NodeList.prototype].forEach(a => Object.defineProperties(a, {
    toArray: { value: Array.prototype.slice },
    forEach: { value: Array.prototype.forEach },
    map: { value: Array.prototype.map },
    reduce: { value: Array.prototype.reduce },
    filter: { value: Array.prototype.filter },
    group: { value: Array.prototype.group },
    every: { value: Array.prototype.every },
    some: { value: Array.prototype.some },
    find: { value: Array.prototype.find }
}));
