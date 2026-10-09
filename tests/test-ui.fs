\ test-ui.fs — Unit tests for src/ui.fs input classification
\
\ classify-yn decides whether a typed answer is yes, no, or invalid; the
\ default ASK-YESNO uses it to re-prompt until the input is valid.

REQUIRE harness.fs
REQUIRE ../src/ui.fs

DECIMAL

\ classify-yn ( c-addr u -- yes-flag valid-flag )
\   valid-flag TRUE  when the first non-blank char is y/Y or n/N
\   yes-flag   TRUE  for y/Y, FALSE otherwise

T{ s" yes"   classify-yn -> TRUE  TRUE  }T
T{ s" Y"     classify-yn -> TRUE  TRUE  }T   \ case-insensitive
T{ s" no"    classify-yn -> FALSE TRUE  }T
T{ s" N"     classify-yn -> FALSE TRUE  }T
T{ s"   yes" classify-yn -> TRUE  TRUE  }T   \ leading blanks skipped
T{ s" maybe" classify-yn -> FALSE FALSE }T   \ not y/n → invalid
T{ s" "      classify-yn -> FALSE FALSE }T   \ empty → invalid
T{ s"    "   classify-yn -> FALSE FALSE }T   \ all blanks → invalid

\ ---------------------------------------------------------------------------
\ Input length limit: text-len-ok? and default-prompt-line re-prompting
\ ---------------------------------------------------------------------------

T{ MAX-TEXT-LEN     text-len-ok? -> TRUE  }T
T{ MAX-TEXT-LEN 1+  text-len-ok? -> FALSE }T
T{ 1                text-len-ok? -> TRUE  }T

\ Stub the raw line reader: first call "types" an over-long line, second
\ call types "Cat".  default-prompt-line must reject the first and re-prompt.
VARIABLE accept-calls
: stub-accept ( c-addr u1 -- u2 )
  1 accept-calls +!
  accept-calls @ 1 = IF
    MAX-TEXT-LEN 1+ MIN  >R  R@ [CHAR] x FILL  R>   \ too long
  ELSE
    DROP s" Cat" ROT SWAP MOVE  3
  THEN
;
' stub-accept IS ui-accept
0 accept-calls !
T{ s" Animal name:" default-prompt-line s" Cat" COMPARE -> 0 }T
T{ accept-calls @ -> 2 }T
' ACCEPT IS ui-accept

s" test-ui.fs" tests-done
