# State — a Set of LR Items (I₀, I₁, …)

> 🎓 This is an **Advanced track** lesson.\
> In the previous [LR item](lr-item.md) chapter we looked at one *dotted production*.\
> But when the parser reads input and pauses at some point — there are usually **several** *possible items*.\
> Bundling those several together is what a **state** is.

> 📍 **Where it lives** · `CanonicalState` · `…/Parsers/Collections/CanonicalState.cs`

## Wait — Let's Lay Out the Example Grammar Again First

Before we dive in properly, let's catch our breath for a moment.\
From here on, our **example grammar** keeps showing up throughout the state discussion. It's the very grammar we've been using together all the way from [FIRST / FOLLOW](first-follow.md). So we don't drift away from it, let's lay it out in front of us again.

<pre class="lrbox">   <span class="nt">Expr</span>   → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>   |  <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>  |  <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>     |  <span class="setm">id</span>
</pre>

- An expression `Expr` is — terms `Term` joined together with `'+'`,
- a term `Term` is — factors `Factor` joined together with `'*'`,
- a factor `Factor` is — a parenthesized expression `'(' Expr ')'`, or a single name `id`.

It's a structure where multiplication `'*'` binds *more tightly* (multiplication first) than addition `'+'`.\
This one little grammar is all we need — **every example from here on comes from it**. Just keep these three lines beside you as you go.

## What a State Is — a Set of Items, `Iₓ`

Suppose the parser has been reading tokens and is now standing at some spot.\
At that spot, the "rule that might currently be in progress" may not be just one but **several**.\
Gathering those *simultaneously possible LR items* together is one **state**.

In textbooks each state gets a number, written as **`I₀`, `I₁`, `I₂` …**. (The `I` is the I of *item set*.)

Words alone are vague, so let's first pin down **why a single item isn't enough** — then crack open one real state and take it apart.

## Why a 'Set' Rather Than 'a Single Item'

Look at one item — `Expr → Term •` (*"we've read up through Term, and Expr is finished"*).

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="nt">Term</span> <span class="lrdot">•</span></pre>

Looking at this one item alone, it seems like *"we've read all of Term, so let's reduce it into Expr"*.\
But — is it really OK to conclude that so firmly? Our grammar also has this rule.

<pre class="lrbox">   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>
</pre>

That is, after that very `Term` a `'*' Factor` could still be attached, turning it into a **bigger `Term`**.\
That possibility is expressed by this item.

<pre class="lrbox">   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span></pre>

Look at the dot — *both are right after `Term`*. The same situation, **"we've read Term up to here"**, seen by two rules each from its own standpoint.\
So to write down this spot honestly — we have to hold **both items at the same time**.\
This thing, *gathering all the items possible at one spot*, is exactly a **state**.

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="nt">Term</span> <span class="lrdot">•</span>                <span style="opacity:.65"># this Term might be where Expr finishes</span>
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span>     <span style="opacity:.65"># or it might be the front part of a bigger Term that gets a '*'</span></pre>

> Exactly *when and how* such a state gets built — that's covered right next, in **[closure](closure-def.md)** and
> **[GOTO](goto.md)**. For now just hold on to *"state = the set of all items possible at one spot"*.

## So, What Do We Do in This State?

The two items say different things.

- **`Expr → Term •`** — the dot has reached the *end*. *"One Term, and that's already an Expr"* — we've seen it all, so we can **reduce it**.
- **`Term → Term • '*' Factor`** — there's a `'*'` left after the dot. *"A `* Factor` might still come after"* — in that case we need to **read more (shift)**.

**Which of the two we do — the next token** decides.

- If the next token is **`*`** → the `Term → Term • '*' Factor` side. **We read the `'*'` further (shift).**
- If the next token is **`+`·`)`·end of input (`$`)** → there's nothing more to attach, so the `Expr → Term •` side. **We reduce Term into Expr.**

> 💡 *"If next is `+`·`)`·`$`, reduce it"* — where have you seen that set? It's exactly **[FOLLOW(Expr)](follow-formula.md)** `= { $, '+', ')' }`.\
> FIRST/FOLLOW gets used *right here* — FOLLOW tells us *"the next tokens for which reducing is allowed"*. (We'll tie this connection up firmly in the *parse table* chapter.)

## 🌱 A Seed — What If Two Actions Overlap? A 'Conflict'

That state just now held two items together — the reduce item `Expr → Term •`, and the read-more (shift) item `Term → Term • '*' Factor`. **Two actions coexisted in one state.**

And yet the parser didn't get confused — because *the next tokens each action responds to didn't overlap with each other*.

| Next token | This state's action |
|:--|:--|
| `'*'` | **shift** — read `'*'` and proceed with `Term → Term • '*' Factor` |
| `$` · `'+'` · `')'`  (`= FOLLOW(Expr)`) | **reduce** — reduce via `Expr → Term •` |

Look at the table — **exactly one action per token**.\
Since `'*'` isn't in the reduce-side tokens `{ $, '+', ')' }`, *whatever token comes, there's exactly one thing to do* — so it split cleanly.

### Then, What If They Did Overlap?

Just imagine it — what if `'*'` were also in `FOLLOW(Expr)`? When the next token is `'*'`, two items *raise their hands at the same time*.

<pre class="lrbox">   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span>    →  "Let's read the '*'!"   (shift)
   <span class="nt">Expr</span> → <span class="nt">Term</span> <span class="lrdot">•</span>               →  "Let's reduce into Expr!"  (reduce — assuming '*' is in FOLLOW(Expr))</pre>

In table form — the `'*'` cell becomes like this.

| Next token | This state's action |
|:--|:--|
| `'*'` | **shift** _and_ **reduce**  ⚠️ |
| `$` · `'+'` · `')'` | reduce |

In the clean table earlier, the `'*'` cell had **just a single shift**. Now **reduce comes in too** — *two actions in one cell*.\
For one and the same token, **both shift and reduce** become possible. The parser can't decide *"reduce, or read more?"*.\
This *"actions splitting at one spot"* is exactly a **conflict** — and specifically a **shift/reduce conflict**.

> Our example grammar fortunately has no such overlap, so no state ever produces a conflict.\
> A grammar with *no conflicts at all* like this has a name — it's called an **LR grammar**, named directly after that parsing method. Ours is conflict-free with even the simplest **SLR(1)** alone, a textbook-typical **SLR(1) grammar**.
> (In order of lookahead precision at which conflicts vanish, it further divides as **SLR(1) ⊂ LALR(1) ⊂ LR(1)**, but that's for the *parse table* chapter. — The often-confused *'context-free grammar (CFG)'* is a much broader category *unrelated* to conflicts, so almost every language grammar is a CFG. The subset of those that can be LR-parsed without conflicts is an **LR grammar**.)\
> (Meanwhile, the most famous example that readily produces a conflict is the *"which `if` does the `else` attach to"* of `if-then-else`.)\
> How to **detect and disentangle** these is dealt with properly much later in the *parse table* chapter. For now we just plant the seed that *"if two actions overlap in one state, that's a conflict"* and move on.

## Code — `CanonicalState`

We said a state is *"a set of items"*. The code is exactly that, word for word.

```csharp
public class CanonicalState : HashSet<LRItem>   // one state = a set of LR items
{
    public int StateNumber { get; }   // that state's number — the x of Iₓ (0 for I0)
}
```

> 💡 It being a set (`HashSet`) is natural — there's no reason for the *same item* to appear twice in one state.\
> Since the identity of an [LR item](lr-item.md) is *"production + dot position"*, identical items get automatically merged into one in the set.

## In Code — Sorting Out the Two Categories

Those two actions we saw earlier — *reducing* and *reading more (shift)* — came down, in the end, to whether the item's **dot has finished**. The code sorts the two out like this.

- **shift item** — one where the dot is *not* yet finished (`A → α • X β`).\
  There's more to read → *read on through* `X`. (code: `ShiftItemList`)
- **complete (reduce) item** — one where the dot has reached the *end* (`A → α •`).\
  We've read it all → *reduce* via this rule. (code: `IsReachedHandle`, `ReachedHandleSet`)

```csharp
public bool IsReachedHandle { get; }                 // is there a complete item in this state
public HashSet<NonTerminalSingle> ReachedHandleSet;  // the completed productions (reduce candidates)
public HashSet<NonTerminalSingle> ShiftItemList;     // the still-in-progress productions
```

Just looking at words it's fuzzy, so let's **plug in the example state directly**. Seeing what gets put in each variable makes it instantly clear.

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="nt">Term</span> <span class="lrdot">•</span>              <span style="opacity:.65">← complete item</span>
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span>   <span style="opacity:.65">← in-progress (shift) item</span></pre>

| Variable | Value in this state | Why |
|:--|:--|:--|
| `IsReachedHandle` | `true` | because there's *at least one* complete item, `Expr → Term •` |
| `ReachedHandleSet` | `{ Expr → Term }` | the **production** of that complete item |
| `ShiftItemList` | `{ Term → Term '*' Factor }` | the **production** of the in-progress item |

Look — the two items of one state split cleanly into their slots: `Expr → Term •` into the *complete* slot (`ReachedHandleSet`), and `Term → Term • '*' Factor` into the *in-progress* slot (`ShiftItemList`).\
(Since both are present, `IsReachedHandle` is `true` — a signal that *"this state has something to reduce too"*.)

And since both `ShiftItemList` and `ReachedHandleSet` are **sets**, when there are many items, several get held in them.\
Picturing a state with *two completes and two in-progress* makes it land at a glance. (Our example grammar doesn't pile up like this — we're *assuming* there were also a division `Term → Term '/' Factor` and a 'a statement is one expression' rule `Stmt → Term`.)

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="nt">Term</span> <span class="lrdot">•</span>              <span style="opacity:.65">← complete</span>
   <span class="nt">Stmt</span> → <span class="nt">Term</span> <span class="lrdot">•</span>              <span style="opacity:.65">← complete   (if Stmt → Term existed)</span>
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span>   <span style="opacity:.65">← in-progress</span>
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'/'</span> <span class="nt">Factor</span>   <span style="opacity:.65">← in-progress (if Term → Term '/' Factor existed)</span></pre>

| Variable | Value (in the assumed state) |
|:--|:--|
| `IsReachedHandle` | `true` |
| `ReachedHandleSet` | `{ Expr → Term, `**`Stmt → Term`**` }` |
| `ShiftItemList` | `{ Term → Term '*' Factor, `**`Term → Term '/' Factor`**` }` |

Two completes, two in-progress — several items got held in each variable.\
In our actual example grammar this state is just one complete · one in-progress, plain and simple, but the point is to show that structurally it's **a set that can grow as large as you like**.

## Symbols Readable in This State — `MarkSymbolSet`

Gathering all the **symbols right after the dot** of the shift items is `MarkSymbolSet`.\
It's the *list of "symbols you can read right now in this state"*. (In the next page, [GOTO](goto.md), we use these symbols to find our way to the next state.)

```csharp
public SymbolSet MarkSymbolSet { get; }   // all the 'symbols after the dot' of the items in the state
```

For the example state above, `MarkSymbolSet = { '*' }`. (Only the `'*'` after the dot in `Term → Term • '*' Factor`.)

## At a Glance — the Full Shape of `CanonicalState`

```csharp
public class CanonicalState : HashSet<LRItem>   // state = a set of LR items
{
    public int StateNumber { get; }                      // state number (the x of Iₓ)

    // ── symbols readable in this state ───────
    public SymbolSet MarkSymbolSet { get; }              // all symbols after the dot

    // ── sorting the two categories ──────────
    public bool IsReachedHandle { get; }                 // is there a complete item
    public HashSet<NonTerminalSingle> ReachedHandleSet;  // complete (reduce) productions
    public CanonicalState ReachedHandleItem { get; }     // a state holding only the complete items
    public HashSet<NonTerminalSingle> ShiftItemList;     // in-progress (shift) productions

    // ── lookup ───────────────────────────────
    public bool   HasItem(LRItem item);
    public LRItem GetItem(LRItem item);
}
```

In one line — **a state `Iₓ` = a set of LR items. Inside it, *read-more* shift items and *reduce* complete items are mixed together, and the next token picks the path.**

## Next Chapter

We've seen *what* a state is — a set of items, `Iₓ`.

But when *building* a state, you don't just gather items haphazardly — you have to fill in, without omission, even the productions of the nonterminal after the dot, before it becomes a *complete* state.\
That "filling in without omission" is exactly **closure**.

👉 **[Closure · Definition](closure-def.md)**

---

👈 Previously: [LR item](lr-item.md)
