\ limits.fs — Shared size limits
\
\ MAX-TEXT-LEN caps every animal name and question.  It is enforced at input
\ (ui.fs default-prompt-line re-prompts on longer input) and, as a single
\ choke point, by the node constructors in node.fs (which clamp), so every
\ buffer that later holds node text (persist.fs lines, tree.fs's
\ "Is it a <name>?") can be sized from it.

DECIMAL

200 CONSTANT MAX-TEXT-LEN
