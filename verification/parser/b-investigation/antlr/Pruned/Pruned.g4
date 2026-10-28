grammar Pruned;
start : s EOF ;
s : c 'x' ;
c : 'a' ;
WS : [ \t\r\n]+ -> skip ;
