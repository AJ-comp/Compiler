grammar Overlap;
start : s EOF ;
s : a u | c 'x' ;
a : b 'x' ;
b : 'a' ;
c : 'a' ;
u : u ;
WS : [ \t\r\n]+ -> skip ;
