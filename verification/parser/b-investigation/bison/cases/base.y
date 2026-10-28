%define lr.type lalr
%start S
%%
S : A U | 'z' ;
A : B 'x' ;
B : 'a' ;
U : U ;
%%
