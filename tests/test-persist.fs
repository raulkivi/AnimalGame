\ test-persist.fs — Unit tests for src/persist.fs

REQUIRE harness.fs
REQUIRE ../src/persist.fs

DECIMAL

s" /tmp/animal-test-tree.dat" 2CONSTANT TEST-PATH

\ ---------------------------------------------------------------------------
\ Build a 3-node test tree:
\
\   Is it a mammal?
\   ├── YES → Wolf
\   └── NO  → Parrot
\ ---------------------------------------------------------------------------

s" Wolf"   new-animal CONSTANT p-wolf
s" Parrot" new-animal CONSTANT p-parrot
s" Is it a mammal?" p-wolf p-parrot new-question CONSTANT p-root

\ ---------------------------------------------------------------------------
\ Test 1: save-tree does not throw
\ ---------------------------------------------------------------------------

p-root TEST-PATH save-tree   \ must complete without exception

\ ---------------------------------------------------------------------------
\ Test 2: load-tree reconstructs root as a QuestionNode
\ ---------------------------------------------------------------------------

TEST-PATH load-tree CONSTANT p-loaded

T{ p-loaded node-leaf? -> FALSE }T

\ ---------------------------------------------------------------------------
\ Test 3: question text survives round-trip
\ ---------------------------------------------------------------------------

T{ p-loaded NODE-TEXT @ p-loaded NODE-TLEN @
   s" Is it a mammal?" COMPARE -> 0 }T

\ ---------------------------------------------------------------------------
\ Test 4: yes-child is Wolf
\ ---------------------------------------------------------------------------

T{ p-loaded NODE-YES @ node-leaf?  -> TRUE }T
T{ p-loaded NODE-YES @ NODE-TEXT @ p-loaded NODE-YES @ NODE-TLEN @
   s" Wolf" COMPARE -> 0 }T

\ ---------------------------------------------------------------------------
\ Test 5: no-child is Parrot
\ ---------------------------------------------------------------------------

T{ p-loaded NODE-NO @ node-leaf?   -> TRUE }T
T{ p-loaded NODE-NO @ NODE-TEXT @ p-loaded NODE-NO @ NODE-TLEN @
   s" Parrot" COMPARE -> 0 }T

\ ---------------------------------------------------------------------------
\ Test 6: load from missing file returns default seed tree
\ ---------------------------------------------------------------------------

s" /tmp/no-such-file-xyz.dat" load-tree CONSTANT p-default

T{ p-default node-leaf? -> TRUE }T   \ seed is a single animal leaf

\ ---------------------------------------------------------------------------
\ Test 7: a corrupt file (unrecognised line prefix) falls back to default
\ ---------------------------------------------------------------------------

s" /tmp/animal-corrupt.dat" 2CONSTANT CORRUPT-FILE

\ Garbage content: a single line that is neither Q nor A.
: write-corrupt ( -- )
  CORRUPT-FILE W/O CREATE-FILE THROW >R
  s" X this is not a valid node line" R@ WRITE-LINE THROW
  R> CLOSE-FILE THROW
;
write-corrupt

T{ CORRUPT-FILE load-tree node-leaf? -> TRUE }T   \ falls back to default leaf

\ ---------------------------------------------------------------------------
\ Test 8: a truncated tree (question node missing its no-child) falls back
\ ---------------------------------------------------------------------------

: write-truncated ( -- )
  CORRUPT-FILE W/O CREATE-FILE THROW >R
  s" Q Is it a mammal?" R@ WRITE-LINE THROW   \ question...
  s" A Wolf"            R@ WRITE-LINE THROW   \ ...with only a yes-child
  R> CLOSE-FILE THROW
;
write-truncated

T{ CORRUPT-FILE load-tree node-leaf? -> TRUE }T   \ falls back to default leaf

\ ---------------------------------------------------------------------------
\ Test 8b: a corrupt file is backed up to <path>.bak, not left to be
\ silently overwritten by the next save-tree
\ ---------------------------------------------------------------------------

s" /tmp/animal-corrupt.dat.bak" 2CONSTANT CORRUPT-BAK
CREATE bak-line 300 ALLOT

: file-exists? ( c-addr u -- flag )
  R/O OPEN-FILE IF DROP FALSE ELSE CLOSE-FILE THROW TRUE THEN
;
\ first-line-of  ( c-addr u -- c-addr2 u2 )   first line of a file
: first-line-of ( c-addr u -- c-addr2 u2 )
  R/O OPEN-FILE THROW >R
  bak-line 298 R@ READ-LINE THROW DROP
  R> CLOSE-FILE THROW
  bak-line SWAP
;

CORRUPT-BAK DELETE-FILE DROP
write-corrupt
T{ CORRUPT-FILE load-tree node-leaf? -> TRUE }T          \ still falls back
T{ CORRUPT-BAK file-exists? -> TRUE }T                   \ ...but backed up
T{ CORRUPT-BAK first-line-of s" X this is not a valid node line" COMPARE -> 0 }T
T{ CORRUPT-FILE file-exists? -> FALSE }T                 \ original moved aside

\ A missing file is NOT corrupt: no backup is made.
s" /tmp/no-such-file-xyz.dat.bak" DELETE-FILE DROP
s" /tmp/no-such-file-xyz.dat" load-tree DROP
T{ s" /tmp/no-such-file-xyz.dat.bak" file-exists? -> FALSE }T

\ ---------------------------------------------------------------------------
\ Test 8c: max-length animal name and question survive a round-trip
\ ---------------------------------------------------------------------------

CREATE long-name 600 ALLOT   long-name 600 CHAR n FILL
CREATE long-q    600 ALLOT   long-q    600 CHAR q FILL

long-name MAX-TEXT-LEN new-animal CONSTANT lp-yes
s" Cat" new-animal CONSTANT lp-no
long-q MAX-TEXT-LEN lp-yes lp-no new-question CONSTANT lp-root

lp-root TEST-PATH save-tree
TEST-PATH load-tree CONSTANT lp-loaded

T{ lp-loaded node-leaf? -> FALSE }T                       \ not the seed tree
T{ lp-loaded NODE-TEXT @ lp-loaded NODE-TLEN @
   long-q MAX-TEXT-LEN COMPARE -> 0 }T
T{ lp-loaded NODE-YES @ DUP NODE-TEXT @ SWAP NODE-TLEN @
   long-name MAX-TEXT-LEN COMPARE -> 0 }T
T{ lp-loaded NODE-NO @ DUP NODE-TEXT @ SWAP NODE-TLEN @
   s" Cat" COMPARE -> 0 }T

\ ---------------------------------------------------------------------------
\ Test 8d: a legacy file with a 254-char name (writable by older versions,
\ whose input buffer was 256) loads instead of collapsing to the seed tree
\ ---------------------------------------------------------------------------

: write-legacy-long ( -- )
  TEST-PATH W/O CREATE-FILE THROW >R
  s" Q Is it big?"   R@ WRITE-LINE THROW
  s" A " R@ WRITE-FILE THROW  long-name 254 R@ WRITE-LINE THROW
  s" A Cat"          R@ WRITE-LINE THROW
  R> CLOSE-FILE THROW
;
write-legacy-long

TEST-PATH load-tree CONSTANT legacy-loaded
T{ legacy-loaded node-leaf? -> FALSE }T                   \ not the seed tree
T{ legacy-loaded NODE-YES @ NODE-TLEN @ -> MAX-TEXT-LEN }T \ clamped on load
T{ legacy-loaded NODE-NO @ DUP NODE-TEXT @ SWAP NODE-TLEN @
   s" Cat" COMPARE -> 0 }T

\ ---------------------------------------------------------------------------
\ Test 9: load-tree / save-tree are a swappable interface (DEFER)
\ ---------------------------------------------------------------------------

s" Stub" new-animal CONSTANT stub-node
: stub-load ( c-addr u -- root ) 2DROP stub-node ;

' stub-load IS load-tree
T{ s" ignored-path" load-tree -> stub-node }T   \ injected repo is used

' file-load-tree IS load-tree   \ restore the real implementation

s" test-persist.fs" tests-done
