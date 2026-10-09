\ fail-assert.fs — deliberately failing suite (harness self-check).
\ `make test-harness` asserts that the test runner reports this as a FAILURE.
REQUIRE ../harness.fs
T{ 1 -> 2 }T
s" fail-assert.fs" tests-done
