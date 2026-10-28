%define lr.type canonical-lr
%start S
%%
S : C 'x' ;
C : 'a' ;
%%

