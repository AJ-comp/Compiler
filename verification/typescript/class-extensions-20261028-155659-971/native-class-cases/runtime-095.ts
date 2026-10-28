export {};
class C { #value = 1; readonly other = this.#value; public get() { return this.#value; } }
