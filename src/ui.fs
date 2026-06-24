\ ui.fs — Abstract I/O layer via DEFER words
\
\ All user interaction goes through these three DEFER words so that the game
\ engine (tree.fs, main.fs) has zero dependency on concrete I/O.  Tests
\ redefine these words with scripted answers without touching game logic.

DECIMAL   \ numeric literals below are decimal regardless of the caller's BASE

256 CONSTANT UI-BUFSIZE

\ Internal line buffer used by the default PROMPT-LINE implementation.
\ Tests may redefine PROMPT-LINE without touching this buffer.
CREATE ui-buf UI-BUFSIZE ALLOT

\ Separate buffer for the default ASK-YESNO answer.  Must NOT share ui-buf:
\ a PROMPT-LINE result stays valid until the next PROMPT-LINE call, so the
\ yes/no read between a PROMPT-LINE and its use must not clobber ui-buf.
CREATE ui-yn-buf UI-BUFSIZE ALLOT

\ --- DEFER declarations -----------------------------------------------------

\ ASK-YESNO  ( c-addr u -- flag )
\ Display a yes/no question; return TRUE if the player answered yes.
DEFER ASK-YESNO

\ PROMPT-LINE  ( c-addr u -- c-addr2 u2 )
\ Display a prompt string; read a line of text; return it as ( addr len ).
\ The returned string is valid until the next call to PROMPT-LINE.
DEFER PROMPT-LINE

\ DISPLAY  ( c-addr u -- )
\ Print a string followed by a newline.
DEFER DISPLAY

\ --- default implementations ------------------------------------------------

\ first-nonblank  ( c-addr u -- ch )
\ The first non-space character of the string, or 0 if there is none.
: first-nonblank ( c-addr u -- ch )
  0 ?DO
    DUP I + C@                \ ( c-addr ch )
    DUP BL <> IF              \ non-space → answer found
      NIP UNLOOP EXIT         \ ( ch )
    THEN
    DROP                      \ space → keep scanning   ( c-addr )
  LOOP
  DROP 0                      \ empty / all blanks      ( 0 )
;

\ classify-yn  ( c-addr u -- yes-flag valid-flag )
\ Classify a typed answer by its first non-blank character (case-insensitive):
\   y/Y → ( TRUE  TRUE )   n/N → ( FALSE TRUE )   anything else → ( FALSE FALSE )
: classify-yn ( c-addr u -- yes-flag valid-flag )
  first-nonblank 32 OR                      \ lowercase the letter   ( lch )
  DUP [CHAR] y = IF DROP TRUE  TRUE EXIT THEN
      [CHAR] n = IF      FALSE TRUE EXIT THEN
  FALSE FALSE                               \ unrecognised → invalid
;

\ default-ask-yesno  ( c-addr u -- flag )
\ Print the prompt and read an answer, re-prompting until it is a valid yes/no.
: default-ask-yesno ( c-addr u -- flag )
  BEGIN
    2DUP TYPE ."  (yes/no): "
    ui-yn-buf UI-BUFSIZE ACCEPT
    CR                           \ terminate the input line (ACCEPT eats the Enter)
    ui-yn-buf SWAP classify-yn   \ ( c-addr u yes-flag valid-flag )
    DUP 0=                       \ ( c-addr u yes-flag valid-flag invalid? )
  WHILE
    2DROP                        \ invalid → discard, loop and re-prompt
  REPEAT
  DROP NIP NIP                   \ keep yes-flag, drop valid-flag + prompt
;

\ default-prompt-line  ( c-addr u -- c-addr2 u2 )
: default-prompt-line ( c-addr u -- c-addr2 u2 )
  TYPE ."  "
  ui-buf UI-BUFSIZE ACCEPT
  CR                           \ terminate the input line (ACCEPT eats the Enter)
  ui-buf SWAP
;

\ default-display  ( c-addr u -- )
: default-display ( c-addr u -- )
  TYPE CR
;

' default-ask-yesno  IS ASK-YESNO
' default-prompt-line IS PROMPT-LINE
' default-display     IS DISPLAY
