\ main.fs — Game loop and entry point
\
\ Depends on: src/tree.fs  src/persist.fs

REQUIRE tree.fs
REQUIRE persist.fs

\ ---------------------------------------------------------------------------
\ State
\ ---------------------------------------------------------------------------

s" data/tree.dat" 2CONSTANT SAVE-PATH

VARIABLE game-root   \ holds the live root node pointer

\ ---------------------------------------------------------------------------
\ Init
\ ---------------------------------------------------------------------------

: init-game ( -- )
  SAVE-PATH load-tree
  game-root !
;

\ ---------------------------------------------------------------------------
\ play-round  ( -- )
\ ---------------------------------------------------------------------------

: play-round ( -- )
  game-root                  \ push address of game-root cell
  traverse DROP              \ traverse; discard won-flag (already displayed)
  game-root @ SAVE-PATH save-tree
;

\ ---------------------------------------------------------------------------
\ game-loop  ( -- )
\ ---------------------------------------------------------------------------

: game-loop ( -- )
  BEGIN
    play-round
    s" Would you like to play again?" ASK-YESNO
  WHILE
  REPEAT
;

\ ---------------------------------------------------------------------------
\ Entry point
\ ---------------------------------------------------------------------------

: run-game ( -- )
  init-game
  game-loop
;

run-game
BYE
