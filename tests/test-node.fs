\ test-node.fs — Unit tests for src/node.fs
\
\ Uses tests/harness.fs (gforth's tester.fs + failure counting): T{ <words> -> <expected> }T

REQUIRE harness.fs
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

\ ---------------------------------------------------------------------------
\ Text length is capped at MAX-TEXT-LEN by the constructors (single choke
\ point), so no node can outgrow the persistence / guess buffers.
\ ---------------------------------------------------------------------------

CREATE t-long 600 ALLOT   t-long 600 CHAR x FILL

T{ t-long MAX-TEXT-LEN new-animal NODE-TLEN @ -> MAX-TEXT-LEN }T
T{ t-long 254 new-animal NODE-TLEN @          -> MAX-TEXT-LEN }T   \ clamped
T{ t-long 254 t-dog t-wolf new-question NODE-TLEN @ -> MAX-TEXT-LEN }T

s" test-node.fs" tests-done
