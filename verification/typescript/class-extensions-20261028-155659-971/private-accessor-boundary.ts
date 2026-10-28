class C { get #value(): number; read(){ return this.#value; } }
class D { set #value(next: number); write(){ this.#value = 1; } }
declare class E { get #value(): number; }
