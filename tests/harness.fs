\ harness.fs — shared test harness (wraps gforth's tester.fs)
\
\ gforth's tester.fs only PRINTS "INCORRECT RESULT" / "WRONG NUMBER OF
\ RESULTS" and keeps going, so on its own a failing suite still exits 0.
\ This harness hooks tester's ERROR-XT to count failures; each suite ends with
\   s" <suite-name>" tests-done
\ which prints "all tests passed" ONLY when nothing failed, and otherwise
\ prints the failure count and exits gforth with status 1.
\ (The Makefile's run-test also greps the output, as a second line of defence.)

REQUIRE test/tester.fs

DECIMAL

VARIABLE #test-failures   0 #test-failures !

: counting-error ( c-addr u -- )
  1 #test-failures +!
  ERROR1                         \ tester's default: print message + line
;
' counting-error ERROR-XT !

\ tests-done  ( c-addr u -- )   end-of-suite summary; exits 1 on any failure
: tests-done ( c-addr u -- )
  CR TYPE
  #test-failures @ IF
    ." : " #test-failures @ 0 .R ."  test(s) FAILED" CR
    1 (bye)
  THEN
  ." : all tests passed" CR
;
