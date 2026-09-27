\ tree.fs — Tree traversal and learning
\
\ Depends on: src/node.fs  src/ui.fs
\
\ Design: traverse receives the *address of the cell* that holds the current
\ node pointer.  In-place replacement is then trivial: learn writes the new
\ QuestionNode into that cell with a single store.
\
\   game-root VAR        → [ root-node-ptr ]
\   root NODE-YES field  → [ yes-child-ptr ]
\   root NODE-NO  field  → [ no-child-ptr  ]
\
\ Public words:
\   ask-and-branch  ( node -- yes-flag )
\   traverse        ( cell-addr -- won-flag )

REQUIRE node.fs
REQUIRE ui.fs

DECIMAL   \ numeric literals below are decimal regardless of the caller's BASE

\ ---------------------------------------------------------------------------
\ String helper — build "Is it a <name>?" in a scratch buffer
\ ---------------------------------------------------------------------------

CREATE guess-buf 320 ALLOT

\ build-guess-q  ( node -- c-addr u )
: build-guess-q ( node -- c-addr u )
  0 guess-buf C!             \ init empty counted string
  s" Is it a " guess-buf +PLACE
  DUP NODE-TEXT @ OVER NODE-TLEN @ guess-buf +PLACE
  s" ?" guess-buf +PLACE
  DROP                       \ done with node
  guess-buf COUNT
;

\ ---------------------------------------------------------------------------
\ ask-and-branch  ( node -- yes-flag )
\ ---------------------------------------------------------------------------

: ask-and-branch ( node -- yes-flag )
  DUP NODE-TEXT @ SWAP NODE-TLEN @
  ASK-YESNO
;

\ ---------------------------------------------------------------------------
\ learn  ( cell-addr old-leaf -- )
\ ---------------------------------------------------------------------------
\ cell-addr  : address of the cell that currently holds old-leaf.
\ old-leaf   : the AnimalNode the game guessed (wrong).
\
\ Collects three pieces of input, builds a new QuestionNode, patches the tree.

VARIABLE learn-old-leaf

: learn ( cell-addr old-leaf -- )
  learn-old-leaf !   \ save old leaf                ( cell-addr )
  >R                 \ save cell-addr               R: cell-addr

  \ 1. New animal
  s" I give up!  What animal were you thinking of?" DISPLAY
  s" Animal name: " PROMPT-LINE
  new-animal         \                              ( new-leaf )

  \ 2. Distinguishing question
  s" Give me a yes/no question that tells the new animal from my guess:" DISPLAY
  s" Question: " PROMPT-LINE
                     \                              ( new-leaf q-addr q-len )

  \ 3. Which branch is the new animal on?
  s" For the new animal, is the answer to your question YES?" ASK-YESNO
                     \                              ( new-leaf q-addr q-len yes-flag )
  IF   \ yes=new-leaf  no=old-leaf
    ROT              \ ( q-addr q-len new-leaf )
    learn-old-leaf @ \ ( q-addr q-len new-leaf old-leaf )
  ELSE \ yes=old-leaf  no=new-leaf
    ROT              \ ( q-addr q-len new-leaf )
    learn-old-leaf @ SWAP  \ ( q-addr q-len old-leaf new-leaf )
  THEN
  new-question       \ ( new-qnode )   ( q-addr q-len yes-child no-child -- node )
  R> !               \ patch the cell; tree now updated
;

\ ---------------------------------------------------------------------------
\ play-guess  ( cell-addr node -- won-flag )
\ ---------------------------------------------------------------------------

: play-guess ( cell-addr node -- won-flag )
  DUP build-guess-q ASK-YESNO
  IF
    2DROP
    s" I win!" DISPLAY
    TRUE
  ELSE
    learn   \ learn ( cell-addr old-leaf -- ) patches tree
    FALSE
  THEN
;

\ ---------------------------------------------------------------------------
\ traverse  ( cell-addr -- won-flag )
\ ---------------------------------------------------------------------------
\ cell-addr: the address of the cell holding the node to visit next.

: traverse ( cell-addr -- won-flag )
  DUP @                        \ ( cell-addr node )
  DUP node-leaf? IF
    play-guess
  ELSE
    DUP ask-and-branch IF
      NODE-YES                 \ yes-child field addr
    ELSE
      NODE-NO                  \ no-child  field addr
    THEN
    NIP                        \ drop original cell-addr; keep child-field-addr
    RECURSE
  THEN
;
