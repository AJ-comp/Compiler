%token ID
%start S
%define lr.type lalr
%define lr.default-reduction accepting
%%
S : G1 C ;
C : P G2 | P G3 'e' 'x' '?' S ':' S ;
P : R G4 ;
R : ID G5 | ID G6 'i' S ;
G1 : %empty ;
G2 : %empty ;
G3 : %empty ;
G4 : %empty ;
G5 : %empty ;
G6 : %empty ;
%%
