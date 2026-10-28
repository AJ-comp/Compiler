%define lr.type lalr
%start S
%%
S : C 'x' ;
C : 'a' ;
%%
