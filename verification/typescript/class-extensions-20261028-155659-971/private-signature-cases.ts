class Bare { #m(): void; use() { this.#m(); } }
class Overload { #m(value: number): void; #m(value: string): void; #m(value: any) {} use() { this.#m(1); } }
class RepeatedBare { #m(value: number): void; #m(value: string): void; use() { this.#m(1); } }
class Outer { #m(): void; create() { return class Inner { use(value: Outer) { value.#m(); } }; } }
class StaticConflict { static #m(): void; #m() {} }
class FieldConflict { #m(): void; #m: number; }
class GeneratorSignature { *m(): IterableIterator<number>; }
