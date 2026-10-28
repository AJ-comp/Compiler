grammar Base;
start : s EOF ;
s : a u | 'z' ;
a : b 'x' ;
b : 'a' ;
u : u ;
WS : [ \t\r\n]+ -> skip ;
