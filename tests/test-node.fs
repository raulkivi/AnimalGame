\ test-node.fs — Unit tests for src/node.fs
\
\ Uses gforth's built-in tester.fs: T{ <words> -> <expected> }T

REQUIRE test/tester.fs
REQUIRE ../src/node.fs

DECIMAL

\ ---------------------------------------------------------------------------
\ AnimalNode tests
\ ---------------------------------------------------------------------------

s" Dog" new-animal CONSTANT t-dog

T{ t-dog NODE-TYPE @ -> NODE-ANIMAL }T
T{ t-dog NODE-TLEN @ -> 3           }T
T{ t-dog NODE-YES  @ -> 0           }T
T{ t-dog NODE-NO   @ -> 0           }T
T{ t-dog node-leaf? -> TRUE         }T

\ Text content
T{ t-dog NODE-TEXT @ 3 s" Dog" COMPARE -> 0 }T

\ ---------------------------------------------------------------------------
\ QuestionNode tests
\ ---------------------------------------------------------------------------

s" Wolf" new-animal CONSTANT t-wolf
s" Is it a mammal?" t-dog t-wolf new-question CONSTANT t-q1

T{ t-q1 NODE-TYPE @ -> NODE-QUESTION }T
T{ t-q1 node-leaf? -> FALSE          }T
T{ t-q1 NODE-YES  @ -> t-dog         }T
T{ t-q1 NODE-NO   @ -> t-wolf        }T
T{ t-q1 NODE-TEXT @ t-q1 NODE-TLEN @ s" Is it a mammal?" COMPARE -> 0 }T

\ ---------------------------------------------------------------------------
\ free-node does not crash
\ ---------------------------------------------------------------------------

s" Temp" new-animal free-node
\ (no assertion — just must not throw)

CR .( test-node.fs: all tests passed ) CR
