\ persist.fs — Save and load the decision tree
\
\ Depends on: src/node.fs
\
\ File format: pre-order DFS, one node per line.
\   Q <question text>
\   A <animal name>
\
\ Example:
\   Q Is it a mammal?
\   Q Does it live in the wild?
\   A Wolf
\   A Dog
\   A Parrot
\
\ The tree is reconstructed by reading nodes in pre-order: a QuestionNode
\ consumes the next two sub-trees as its yes and no children.

REQUIRE node.fs

DECIMAL   \ numeric literals below are decimal regardless of the caller's BASE

\ ---------------------------------------------------------------------------
\ Constants
\ ---------------------------------------------------------------------------

256 CONSTANT PERSIST-BUFSIZE

\ THROW code raised when a save file is malformed (unknown line prefix or a
\ question node missing a child).  load-tree CATCHes it and falls back to the
\ default seed tree.  -256 is outside the ANS reserved range (-1..-255).
-256 CONSTANT CORRUPT-TREE

\ ---------------------------------------------------------------------------
\ save-tree  ( root c-addr u -- )
\ ---------------------------------------------------------------------------
\ Writes the tree to a temp file then renames for atomicity.

CREATE persist-tmp-buf PERSIST-BUFSIZE ALLOT
CREATE persist-line-buf PERSIST-BUFSIZE ALLOT

VARIABLE save-fileid

\ node-text$  ( node -- c-addr u )   the node's text as ( addr len )
: node-text$ ( node -- c-addr u )
  DUP NODE-TEXT @ SWAP NODE-TLEN @
;

\ write-str-line  ( prefix-char text-addr text-len -- )
\ Writes "<prefix-char> <text>\n" to the file in save-fileid.
: write-str-line ( prefix-char text-addr text-len -- )
  DUP >R                           \ save text-len                 R: text-len
  persist-line-buf 2 + SWAP MOVE   \ copy text into buf[2..]       ( prefix-char )
  persist-line-buf C!              \ buf[0] = prefix char          ( )
  BL persist-line-buf 1+ C!        \ buf[1] = space
  persist-line-buf  R> 2 +         \ ( c-addr text-len+2 )
  save-fileid @ WRITE-LINE THROW
;

\ save-node  ( node -- )   pre-order DFS write of the subtree to save-fileid.
: save-node ( node -- )
  DUP node-leaf? IF
    [CHAR] A OVER node-text$ write-str-line   \ ( node )
    DROP
  ELSE
    [CHAR] Q OVER node-text$ write-str-line   \ ( node )
    DUP NODE-YES @ RECURSE
    NODE-NO  @ RECURSE
  THEN
;

\ file-save-tree  ( root c-addr u -- )
\ Writes the tree to "<path>.tmp" then renames it onto <path> for atomicity.
: file-save-tree ( root c-addr u -- )
  \ Build temp filename "<path>.tmp" in persist-tmp-buf (counted string)
  2DUP persist-tmp-buf PLACE       \ persist-tmp-buf := path        ( root c-addr u )
  s" .tmp" persist-tmp-buf +PLACE  \ append ".tmp"                  ( root c-addr u )

  \ Open temp file for writing
  persist-tmp-buf COUNT W/O CREATE-FILE THROW   \ ( root c-addr u fileid )
  save-fileid !                    \ ( root c-addr u )

  2>R                              \ save target path              R: c-addr u
  save-node                        \ write entire tree            ( )
  save-fileid @ CLOSE-FILE THROW

  \ Rename temp → target
  persist-tmp-buf COUNT            \ ( tmp-addr tmp-u )
  2R>                              \ ( tmp-addr tmp-u target-addr target-u )
  RENAME-FILE THROW
;

\ ---------------------------------------------------------------------------
\ load-tree  ( c-addr u -- root )
\ ---------------------------------------------------------------------------

VARIABLE load-fileid
CREATE  load-line-buf PERSIST-BUFSIZE ALLOT
VARIABLE load-more     \ non-zero while file has lines

\ heap-copy  ( c-addr u -- c-addr2 u )
\ Copy a string to a fresh heap block, preserving the length.
: heap-copy ( c-addr u -- c-addr2 u )
  TUCK copy-str SWAP
;

\ read-next-node  ( -- node )
: read-next-node ( -- node )
  load-more @ 0= IF 0 EXIT THEN

  load-line-buf PERSIST-BUFSIZE load-fileid @ READ-LINE THROW
  \ READ-LINE: ( c-addr u1 fid -- u2 flag wior ); after THROW: ( u2 flag )
  load-more !                 \ store eof flag, leave byte count   ( u2 )
  DUP 0= IF DROP 0 EXIT THEN   \ empty line → return null

  \ u bytes read; load-line-buf holds "<type> <text>" (no newline).
  \ text occupies buf[2..u);  text-len = u - 2.
  load-line-buf C@ [CHAR] Q = IF
    \ QuestionNode: copy the text out FIRST — the recursive child reads
    \ overwrite load-line-buf before new-question would copy it.
    load-line-buf 2 + SWAP 2 -   \ ( text-addr text-len )
    heap-copy                    \ ( htext-addr text-len )  stable copy
    OVER >R                      \ save heap ptr to free later   R: htext-addr
    RECURSE                      \ yes-child   ( htext len yes )
    RECURSE                      \ no-child    ( htext len yes no )
    2DUP 0= SWAP 0= OR IF        \ either child missing → truncated/corrupt
      CORRUPT-TREE THROW
    THEN
    new-question                 \ ( node )
    R> FREE THROW                \ release the temporary copy
  ELSE
    load-line-buf C@ [CHAR] A = IF
      \ AnimalNode: buffer intact (no recursion); new-animal copies it.
      load-line-buf 2 + SWAP 2 - new-animal
    ELSE
      \ Unrecognised line prefix → corrupt file.
      DROP CORRUPT-TREE THROW
    THEN
  THEN
;

\ default-tree  ( -- root )
\ Returns a single-leaf seed tree used when no save file exists.
: default-tree ( -- root )
  s" Dog" new-animal
;

\ load-from-file  ( -- root )   may THROW CORRUPT-TREE on a malformed file
\ Reads the whole tree; an empty file yields the default seed tree.
: load-from-file ( -- root )
  read-next-node
  DUP 0= IF DROP default-tree THEN
;

\ file-load-tree  ( c-addr u -- root )
\ Loads the tree from <path>; falls back to the default seed tree when the file
\ is missing, empty, or corrupt (the parse is wrapped in CATCH).
: file-load-tree ( c-addr u -- root )
  R/O OPEN-FILE
  IF   \ open failed — return default seed tree
    DROP default-tree EXIT
  THEN
  load-fileid !
  -1 load-more !                    \ assume more lines
  ['] load-from-file CATCH          \ ( root 0 | ior )
  load-fileid @ CLOSE-FILE DROP     \ close regardless; ignore close status
  IF default-tree THEN              \ corruption caught → fall back to default
;

\ ---------------------------------------------------------------------------
\ TreeRepository interface  (DEFER → swappable, like the ui.fs words)
\ ---------------------------------------------------------------------------
\ The game (main.fs) and tests call these; the defaults are the file-backed
\ implementations above.  Tests may bind a fake repository for I/O-free testing.

DEFER save-tree   \ ( root c-addr u -- )
DEFER load-tree   \ ( c-addr u -- root )

' file-save-tree IS save-tree
' file-load-tree IS load-tree
