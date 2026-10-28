class IndexPublic { [public: string]: number; }
class IndexPrivate { [private: string]: number; }
class IndexReadonly { [readonly: string]: number; }
class IndexLet { [let: string]: number; }
class IndexImplements { [implements: string]: number; }
declare function f(): number;
declare class AmbientLiteral { readonly value = 1; }
declare class AmbientTyped { readonly value: number = 1; }
declare class AmbientNonliteral { readonly value = f(); }
class DeclareLiteral { declare readonly value = 1; }
