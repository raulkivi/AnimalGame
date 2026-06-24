\ test-ui.fs — Unit tests for src/ui.fs input classification
\
\ classify-yn decides whether a typed answer is yes, no, or invalid; the
\ default ASK-YESNO uses it to re-prompt until the input is valid.

REQUIRE test/tester.fs
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

CR .( test-ui.fs: all tests passed ) CR
