# Makefile — Animal Game (gforth)
#
# gforth's own exit code does NOT reflect a T{ ... }T assertion failure
# (tester.fs just prints "INCORRECT RESULT" and keeps going) — only an actual
# crash (uncaught THROW) sets it.  run-test checks the exit code (crashes, and
# tests/harness.fs's `tests-done`, which exits 1 when any assertion failed) AND
# greps the output for tester's failure messages, so `make test` fails loudly.
# stdin is /dev/null so gforth can never sit in an interactive prompt.

GFORTH   = gforth
SRC_MAIN = src/main.fs
OUT_DIR  = /tmp/animalgame-test-output
FAIL_RE  = INCORRECT RESULT|INCORRECT CELL RESULT|INCORRECT FP RESULT|WRONG NUMBER OF|test\(s\) FAILED

.PHONY: run test test-node test-ui test-tree test-persist test-harness check-one clean

# run-test <suite.fs>,<name>
define run-test
	@mkdir -p $(OUT_DIR)
	@$(GFORTH) $(1) -e bye < /dev/null > $(OUT_DIR)/$(2).out 2>&1; status=$$?; \
	cat $(OUT_DIR)/$(2).out; \
	if [ $$status -ne 0 ] || grep -Eq "$(FAIL_RE)" $(OUT_DIR)/$(2).out; then \
		echo "FAILED: $(1)"; exit 1; \
	fi
endef

run:
	$(GFORTH) $(SRC_MAIN)

test: test-harness test-node test-ui test-tree test-persist

test-node:
	$(call run-test,tests/test-node.fs,test-node)

test-ui:
	$(call run-test,tests/test-ui.fs,test-ui)

test-tree:
	$(call run-test,tests/test-tree.fs,test-tree)

test-persist:
	$(call run-test,tests/test-persist.fs,test-persist)

# check-one FILE=<suite> — run a single suite through the test runner.
check-one:
	$(call run-test,$(FILE),check-one)

# test-harness — self-check: every fixture under tests/fixtures/ is a
# deliberately failing suite; the runner must report each one as a failure.
FIXTURES = tests/fixtures/fail-assert.fs tests/fixtures/fail-depth.fs tests/fixtures/fail-error.fs
test-harness:
	@for f in $(FIXTURES); do \
		if $(MAKE) --no-print-directory check-one FILE=$$f < /dev/null > /dev/null 2>&1; then \
			echo "HARNESS BROKEN: failing suite $$f was reported as passing"; exit 1; \
		fi; \
	done; echo "test-harness: runner detects failing suites"

clean:
	rm -f data/tree.dat data/tree.dat.tmp
