\ test-tree.fs — Unit tests for src/tree.fs
\
\ Replaces DEFER words with scripted answers to test traversal and learning
\ without any real I/O.

REQUIRE test/tester.fs
REQUIRE ../src/tree.fs

DECIMAL

\ ---------------------------------------------------------------------------
\ Scripted I/O helpers
\ ---------------------------------------------------------------------------

\ Script buffers: sequential answer queues.
\ Each queue has independent write (push) and read (consume) cursors so that
\ answers pushed during setup are consumed from the front during the test.
20 CONSTANT MAX-ANSWERS
CREATE yn-answers MAX-ANSWERS ALLOT    \ yes/no flags (1 byte each)
VARIABLE yn-wr   \ write cursor
VARIABLE yn-rd   \ read  cursor

CREATE str-answers MAX-ANSWERS CELLS ALLOT    \ c-addr of answers
CREATE str-lens    MAX-ANSWERS CELLS ALLOT    \ u of answers
VARIABLE str-wr
VARIABLE str-rd

: reset-scripts ( -- )
  0 yn-wr !  0 yn-rd !
  0 str-wr ! 0 str-rd !
;

: push-yn ( flag -- )
  yn-answers yn-wr @ + C!
  yn-wr @ 1+ yn-wr !
;

: push-str ( c-addr u -- )
  str-wr @ CELLS str-lens    + !   \ store len
  str-wr @ CELLS str-answers + !   \ store addr
  str-wr @ 1+ str-wr !
;

: scripted-ask-yesno ( c-addr u -- flag )
  2DROP
  yn-answers yn-rd @ + C@ 0<>     \ normalise byte to a canonical flag
  yn-rd @ 1+ yn-rd !
;

: scripted-prompt-line ( c-addr u -- c-addr2 u2 )
  2DROP
  str-rd @ CELLS str-answers + @
  str-rd @ CELLS str-lens    + @
  str-rd @ 1+ str-rd !
;

: scripted-display ( c-addr u -- )
  2DROP   \ discard; tests don't inspect display output
;

' scripted-ask-yesno  IS ASK-YESNO
' scripted-prompt-line IS PROMPT-LINE
' scripted-display     IS DISPLAY

\ ---------------------------------------------------------------------------
\ Helper: build a small test tree
\
\   Is it a mammal?
\   ├── YES → Wolf
\   └── NO  → Parrot

s" Wolf"   new-animal CONSTANT tr-wolf
s" Parrot" new-animal CONSTANT tr-parrot
s" Is it a mammal?" tr-wolf tr-parrot new-question CONSTANT tr-root

VARIABLE tr-root-cell
tr-root tr-root-cell !

\ ---------------------------------------------------------------------------
\ Test 1: traversal reaches correct leaf — YES branch
\ ---------------------------------------------------------------------------

reset-scripts
TRUE  push-yn    \ answer YES to "Is it a mammal?"
TRUE  push-yn    \ answer YES to "Is it a Wolf?" (correct guess)

T{ tr-root-cell traverse -> TRUE }T   \ game should win

\ ---------------------------------------------------------------------------
\ Test 2: traversal reaches correct leaf — NO branch
\ ---------------------------------------------------------------------------

reset-scripts
FALSE push-yn    \ answer NO  to "Is it a mammal?"
TRUE  push-yn    \ answer YES to "Is it a Parrot?" (correct guess)

T{ tr-root-cell traverse -> TRUE }T

\ ---------------------------------------------------------------------------
\ Test 3: learning — wrong guess triggers tree mutation
\
\ Tree before: root = AnimalNode("Dog")
\ Player thinks of "Wolf"; question = "Does it live in the wild?"; YES=Wolf
\ Tree after:  root = QuestionNode("Does it live in the wild?")
\              ├── YES → Wolf
\              └── NO  → Dog
\ ---------------------------------------------------------------------------

s" Dog" new-animal CONSTANT tr-dog-leaf
VARIABLE leaf-cell
tr-dog-leaf leaf-cell !

reset-scripts
FALSE push-yn          \ "Is it a Dog?" → NO (wrong guess)
s" Wolf" push-str      \ new animal name
s" Does it live in the wild?" push-str    \ distinguishing question
TRUE  push-yn          \ new animal (Wolf) answers YES

T{ leaf-cell traverse -> FALSE }T   \ game lost

\ Check mutation
T{ leaf-cell @ node-leaf?             -> FALSE }T   \ root is now a question
T{ leaf-cell @ NODE-YES @ node-leaf?  -> TRUE  }T   \ yes-child is an animal
T{ leaf-cell @ NODE-NO  @ node-leaf?  -> TRUE  }T   \ no-child  is an animal

\ Check yes-child is Wolf
T{ leaf-cell @ NODE-YES @ NODE-TEXT @ leaf-cell @ NODE-YES @ NODE-TLEN @
   s" Wolf" COMPARE -> 0 }T

\ Check no-child is Dog (original leaf)
T{ leaf-cell @ NODE-NO @ -> tr-dog-leaf }T

CR .( test-tree.fs: all tests passed ) CR
