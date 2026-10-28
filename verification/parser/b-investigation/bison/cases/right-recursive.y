%define lr.type lalr
%start S
%%
S : A U | C 'x' ;
A : B 'x' ;
B : 'a' ;
C : 'a' ;
U : 'u' U ;
%%

