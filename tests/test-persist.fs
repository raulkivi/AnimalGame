\ test-persist.fs — Unit tests for src/persist.fs

REQUIRE test/tester.fs
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
\ Test 9: load-tree / save-tree are a swappable interface (DEFER)
\ ---------------------------------------------------------------------------

s" Stub" new-animal CONSTANT stub-node
: stub-load ( c-addr u -- root ) 2DROP stub-node ;

' stub-load IS load-tree
T{ s" ignored-path" load-tree -> stub-node }T   \ injected repo is used

' file-load-tree IS load-tree   \ restore the real implementation

CR .( test-persist.fs: all tests passed ) CR
