# Makefile — Animal Game (gforth)

GFORTH   = gforth
SRC_MAIN = src/main.fs
TESTS    = tests/test-node.fs tests/test-ui.fs tests/test-tree.fs tests/test-persist.fs

.PHONY: run test test-node test-ui test-tree test-persist clean

run:
	$(GFORTH) $(SRC_MAIN)

test:
	$(GFORTH) $(TESTS)

test-node:
	$(GFORTH) tests/test-node.fs

test-ui:
	$(GFORTH) tests/test-ui.fs

test-tree:
	$(GFORTH) tests/test-tree.fs

test-persist:
	$(GFORTH) tests/test-persist.fs

clean:
	rm -f data/tree.dat data/tree.dat.tmp
