%define lr.type lalr
%start S
%%
S : U ;
U : U ;
%%
