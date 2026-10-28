abstract class GetParam { abstract get x(value: number): number; }
abstract class SetZero { abstract set x(); }
abstract class SetRest { abstract set x(...values: number[]); }
abstract class SetTwo { abstract set x(first: number, second: number); }
abstract class SetComma { abstract set x(value: number,); }
declare class AmbientGetParam { get x(value: number): number; }
declare class AmbientSetZero { set x(); }
declare class AmbientSetRest { set x(...values: number[]); }
declare class AmbientSetTwo { set x(first: number, second: number); }
declare class AmbientSetComma { set x(value: number,); }
class SetBodyComma { set x(value: number,) {} }
