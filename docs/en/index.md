# Welcome to the Janglim manual

Janglim is an **LR/LALR parser-generator engine for .NET**.\
Sounds hard, right?\
**That's okay.**\
This manual
assumes you don't know a single one of those scary words, and unpacks everything slowly, from the very beginning.

## Who is this manual for?

**It's totally fine if you have no idea what a parser is or how a compiler works.**\
This manual starts from "why do we even
need this" and explains it step by step, in the simplest words possible.\
So by the time you finish reading, two things will stay with you.

1. **The concept of parsing** itself — knowledge you'll use for the rest of your life, no matter which parser you end up using
2. **The tool called Janglim** — and (if you want) even how it's built in code on the inside

## This manual has two tracks

To keep it from being overwhelming, we split it into two courses: **Basic / Advanced**.

**🟢 Basic course — just the concepts, nice and easy.**\
No formulas, no code, just *the concept and the intuition*.\
Each page has:

| Step | What |
|---|---|
| **① Why you need it** | What breaks without it (starting from the problem) |
| **② What it does** | The concept itself + a small example |
| **③ See it in the playground** | Run it yourself in the browser and check |

**🔵 Advanced course — formulas + code.**\
We take the same concept and organize it into *a formula (algorithm) that works for any grammar*,
and then look at **how it's implemented in the Janglim code**.\
At the end of each concept in the basic course, I'll leave a "→ to advanced"
link.

> **If this is your first time, just read straight through the basic course.**\
> The advanced stuff is fine to look at later, whenever you get curious. 🙂

## We go all the way with a single example

Across the whole manual we keep using **just one tiny grammar** — it's a bit like basic arithmetic.

```
Expr   : Expr '+' Term | Term ;
Term   : Term '*' Factor | Factor ;
Factor : '(' Expr ')' | id ;
id     := "[a-zA-Z]+" ;
```

And we keep parsing this input: **`a + a * a`**

Every chapter adds one more layer of understanding to this *same* example.\
(We'll take apart how to read this grammar slowly, together,
the first time it shows up, so there's no need to worry about it in advance.)

## So, shall we get started?

- 👉 **[Quick Start](getting-started.md)** — install it and run your first parse in 5 lines.
- 🌿 **[Live playground](https://polite-island-0b2142200.7.azurestaticapps.net)** — right in the browser, no install needed.

> **Note:** Janglim is still an **early preview (`0.1.0-preview`)**.\
> The public API may change during `0.x`.

---

> ### Acknowledgments
>
> I'd like to express my deep gratitude to those who helped so much in building the skeleton of this project.
>
> - **Seman Oh and Yunsik Son, authors of *Introduction to Compilers*** — I learned the fundamentals of compilers from this book.
> - **Actipro Software** — inspired by the way they specify LL parsers, I completed the grammar-definition structure that writes EBNF as code.
> - **Claude and Codex, my assistants and contributors** — they read through my code with me and organized it into this manual, and they're colleagues who will keep helping me with many things going forward.
>
> Thank you, sincerely. 🙏
