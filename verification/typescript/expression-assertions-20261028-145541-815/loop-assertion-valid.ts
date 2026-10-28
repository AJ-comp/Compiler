let value: any;
const obj = { key: 0 };
const list = ["key"];
type T = any;
for (value as T in obj) {}
for (value satisfies T in obj) {}
for (value as T of list) {}
for (value satisfies T of list) {}
