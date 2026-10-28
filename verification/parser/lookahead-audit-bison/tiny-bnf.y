%start S
%define lr.type lalr
%define lr.default-reduction accepting
%%
S : G C ;
C : R | 'e' S ;
R : 'a' | 'b' S ;
G : %empty ;
%%
