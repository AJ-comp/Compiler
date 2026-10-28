%token ID
%start S
%define lr.type lalr
%define lr.default-reduction accepting
%%
S : C ;
C : P | P 'e' 'x' '?' S ':' S ;
P : R ;
R : ID | ID 'i' S ;
%%
