---
_layout: landing
_disableToc: true
_disableAffix: true
_disableContribution: true
_disableNextArticle: true
_disableBreadcrumb: true
---

<div class="hero-section">
  <div class="hero-content">
    <div class="hero-badge">Open Source · .NET · LR / LALR</div>
    <h1 class="hero-title"><span class="hero-accent">Janglim</span></h1>
    <p class="hero-subtitle">
      A general-purpose, embeddable <strong>LR/LALR parser-generator</strong> for .NET.<br>
      Define a grammar — in C# or EBNF — and <strong>look inside every parse</strong>.
    </p>
    <div class="hero-actions">
      <a href="ko/index.html" class="btn-primary-hero">Get Started</a>
      <a href="https://polite-island-0b2142200.7.azurestaticapps.net" class="btn-secondary-hero" target="_blank">Live Playground ↗</a>
      <a href="https://github.com/AJ-comp/Compiler" class="btn-ghost-hero" target="_blank">GitHub ↗</a>
    </div>
  </div>
</div>

<div class="features-section">
  <div class="section-label">What you get</div>
  <h2 class="section-title">See <em>inside</em> the parser — not just the result</h2>
  <div class="features-grid">

    <div class="feature-card">
      <div class="feature-icon feature-icon-blue">📊</div>
      <h3>Inspect everything</h3>
      <p>The ACTION/GOTO table, FIRST/FOLLOW sets and the LR automaton are all first-class. It is not a black box.</p>
      <a href="ko/first-follow.html" class="feature-link">See FIRST/FOLLOW →</a>
    </div>

    <div class="feature-card">
      <div class="feature-icon feature-icon-purple">👣</div>
      <h3>Step-through trace</h3>
      <p>Walk every shift / reduce / goto with the parsing stack drawn live, and watch the parse tree build up.</p>
      <a href="https://polite-island-0b2142200.7.azurestaticapps.net" class="feature-link" target="_blank">Open the playground →</a>
    </div>

    <div class="feature-card">
      <div class="feature-icon feature-icon-green">🌿</div>
      <h3>C# <em>or</em> EBNF</h3>
      <p>Build a grammar from C# fields and operators, or read it from plain EBNF text. Same engine either way.</p>
      <a href="ko/getting-started.html" class="feature-link">Get started →</a>
    </div>

    <div class="feature-card">
      <div class="feature-icon feature-icon-orange">⚠️</div>
      <h3>Conflict reports</h3>
      <p>Shift-reduce and reduce-reduce conflicts, pinpointed to the exact state where they happen.</p>
      <a href="ko/first-follow.html" class="feature-link">Learn the theory →</a>
    </div>

  </div>
</div>

<div class="quickstart-section">
  <div class="quickstart-inner">
    <div class="quickstart-text">
      <div class="section-label">Quick install</div>
      <h2>Up and running in seconds</h2>
      <p>Add one NuGet package and you have the whole engine — lexer, grammar, LALR parser, tables and traces.</p>
      <a href="ko/getting-started.html" class="btn-primary-hero" style="margin-top: 1rem;">Full Getting Started guide →</a>
    </div>
    <div class="quickstart-code">
      <div class="code-block">
        <div class="code-header">.NET CLI</div>
        <pre><code>dotnet add package Janglim --prerelease</code></pre>
        <pre><code>var p = new LALRParser(grammar);</code></pre>
        <pre><code>var r = p.Parsing(tokens);  // r.Success == true</code></pre>
      </div>
    </div>
  </div>
</div>

<div class="learn-section">
  <div class="section-label">Learn the internals</div>
  <h2 class="section-title">A manual that teaches, from the ground up</h2>
  <div class="learn-grid">

    <div class="feature-card">
      <div class="feature-icon feature-icon-blue">🚀</div>
      <h3>Getting started</h3>
      <p>Install, then run your first parse in five lines — with every line explained.</p>
      <a href="ko/getting-started.html" class="feature-link">Read →</a>
    </div>

    <div class="feature-card">
      <div class="feature-icon feature-icon-green">🗺️</div>
      <h3>The big picture</h3>
      <p>How text becomes a parse tree, and the hidden “build the table first” step of LR parsing.</p>
      <a href="ko/the-big-picture.html" class="feature-link">Read →</a>
    </div>

    <div class="feature-card">
      <div class="feature-icon feature-icon-purple">🧠</div>
      <h3>FIRST / FOLLOW</h3>
      <p>The cheat-sheet that lets a parser decide where rules start and end — concept plus our code.</p>
      <a href="ko/first-follow.html" class="feature-link">Read →</a>
    </div>

  </div>
  <p style="margin-top: 2rem;">
    <a href="ko/index.html">한국어</a> ·
    <a href="en/index.html">English</a> ·
    <a href="ja/index.html">日本語</a>
  </p>
</div>
