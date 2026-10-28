%define lr.type canonical-lr
%start S
%%
S : A U | C 'x' ;
A : B 'x' ;
B : 'a' ;
C : 'a' ;
U : U 'u' ;
%%


