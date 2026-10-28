grammar MutualDead;
start : s EOF ;
s : a u | c 'x' ;
a : b 'x' ;
b : 'a' ;
c : 'a' ;
u : v ;
v : u ;
WS : [ \t\r\n]+ -> skip ;
