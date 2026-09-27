\ node.fs — Node structure, allocation, and deallocation
\
\ A node is a heap-allocated block of NODE-SIZE bytes with five cell fields:
\   NODE-TYPE  — 0 = animal (leaf), 1 = question (internal)
\   NODE-TEXT  — heap address of the node's text string
\   NODE-TLEN  — byte length of the string
\   NODE-YES   — yes-child pointer (0 for leaf)
\   NODE-NO    — no-child pointer  (0 for leaf)
\
\ String ownership: the string is copied onto the heap by new-animal and
\ new-question.  free-node must be called to release both the string and
\ the node block.

DECIMAL   \ numeric literals below are decimal regardless of the caller's BASE

0 CONSTANT NODE-ANIMAL       \ type flag value for a leaf node
1 CONSTANT NODE-QUESTION     \ type flag value for an internal node

BEGIN-STRUCTURE NODE-SIZE
  FIELD: NODE-TYPE
  FIELD: NODE-TEXT
  FIELD: NODE-TLEN
  FIELD: NODE-YES
  FIELD: NODE-NO
END-STRUCTURE

\ --- private helper ---------------------------------------------------------

\ copy-str  ( c-addr u -- c-addr2 )
\ Allocates u bytes on the heap, copies the string there, returns heap addr.
: copy-str ( c-addr u -- c-addr2 )
  DUP ALLOCATE THROW         \ allocate u bytes; ( src u dest )
  DUP >R                     \ save dest;        ( src u dest )  R: dest
  SWAP MOVE                  \ ( src dest u ) MOVE — copy string into heap
  R>                         \ ( dest )
;

\ --- public API -------------------------------------------------------------

\ new-animal  ( c-addr u -- node )
\ Allocates a leaf node holding the given animal name.
: new-animal ( c-addr u -- node )
  NODE-SIZE ALLOCATE THROW   \ ( c-addr u node )
  >R                         \ save node addr            R: node
  NODE-ANIMAL R@ NODE-TYPE ! \ ( c-addr u )
  DUP R@ NODE-TLEN !         \ store length, keep u
  copy-str R@ NODE-TEXT !    \ copy string; store heap addr
  0 R@ NODE-YES !
  0 R@ NODE-NO  !
  R>                         \ ( node )
;

\ new-question  ( c-addr u yes no -- node )
\ Allocates an internal question node with yes/no child pointers.
: new-question ( c-addr u yes no -- node )
  NODE-SIZE ALLOCATE THROW   \ ( c-addr u yes no node )
  >R                         \ save node addr            R: node
  NODE-QUESTION R@ NODE-TYPE !
  R@ NODE-NO  !              \ store no-child  (TOS was no)
  R@ NODE-YES !              \ store yes-child
  DUP R@ NODE-TLEN !         \ store length, keep u
  copy-str R@ NODE-TEXT !    \ copy string; store heap addr
  R>                         \ ( node )
;

\ node-leaf?  ( node -- flag )
: node-leaf? ( node -- flag )
  NODE-TYPE @ NODE-ANIMAL =
;

\ free-node  ( node -- )
\ Frees the string buffer and the node block.  Does NOT recurse into children.
: free-node ( node -- )
  DUP NODE-TEXT @ FREE THROW
  FREE THROW
;
