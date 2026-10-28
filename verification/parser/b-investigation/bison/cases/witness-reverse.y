%define lr.type lalr
%start S
%%
U : U 'u' ;
C : 'a' ;
B : 'a' ;
A : B 'x' ;
S : C 'x' | A U ;
%%
