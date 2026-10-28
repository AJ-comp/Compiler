class C { constructor(this: C) {} }
class C { get value(this: C): number { return 1; } }
class C { set value(this: C, next: number) {} }
abstract class C { abstract get value(this: C): number; }
declare class C { set value(this: C, next: number); }
abstract class C { abstract set value(next?: number); }
declare class C { set value(next?: number); }