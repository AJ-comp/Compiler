%define lr.type canonical-lr
%start S
%%
S : A U | 'z' ;
A : B 'x' ;
B : 'a' ;
U : U ;
%%

