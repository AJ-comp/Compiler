namespace Case0 { class C { #m(): void; use() { this.#m(); } } }
namespace Case1 { class C { #m(value: number): void; #m(value: string): void; use() { this.#m(1); } } }
namespace Case2 { class C { #m(value: number): void; #m(value: any) {} use() { this.#m(1); } } }
namespace Case3 { class C { static #m(): void; static use() { this.#m(); } } }
namespace Case4 { class C { static #m(value: number): void; static #m(value: any) {} static use() { this.#m(1); } } }
namespace Case5 { class Outer { #m(): void; create() { return class Inner { use(value: Outer) { value.#m(); } }; } } }
namespace Case6 { class Outer { #m(): void; create() { return class Inner { #m(): void; use() { this.#m(); } }; } } }
namespace Case7 { class C { #\u006d(): void; #m(value: number): void; use() { this.#m(1); } } }
namespace Case8 { class C { #m(): IterableIterator<number>; *#m() { yield 1; } use() { this.#m(); } } }
namespace Case9 { class C { #m(): Promise<void>; async #m() {} use() { this.#m(); } } }
namespace Case10 { class C { #m(): void; use() { this.#missing(); } } }
namespace Case11 { class C { #m(): void; } class D { use() { this.#m(); } } }
namespace Case12 { class C { #m(): void; #m: number; } }
namespace Case13 { class C { static #m(): void; #m() {} } }
namespace Case14 { class C { #m(): void; static #m() {} } }
namespace Case15 { class C { *m(): IterableIterator<number>; } }
namespace Case16 { class C { *#m(): IterableIterator<number>; } }
