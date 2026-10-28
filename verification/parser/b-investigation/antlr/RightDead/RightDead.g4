grammar RightDead;
start : s EOF ;
s : a u | c 'x' ;
a : b 'x' ;
b : 'a' ;
c : 'a' ;
u : 'q' u ;
WS : [ \t\r\n]+ -> skip ;
