// This development-only adapter calls Microsoft's pinned TypeScript parser.
// It must be built inside that source module because the parser is internal.
// It does not run the checker, evaluate input, resolve imports, or emit output.
package main

import (
	"bufio"
	"crypto/sha256"
	"encoding/hex"
	"encoding/json"
	"fmt"
	"os"
	"strings"

	"github.com/microsoft/typescript-go/internal/ast"
	"github.com/microsoft/typescript-go/internal/core"
	"github.com/microsoft/typescript-go/internal/parser"
	"github.com/microsoft/typescript-go/internal/tspath"
)

const revision = "2bd066d87f5bafd315be9f40889d0a60b9e58e0b"

type request struct {
	ID string `json:"id"`
	Source string `json:"source"`
	FileName string `json:"fileName"`
	Kind string `json:"kind"`
}

type diagnostic struct {
	Code int32 `json:"code"`
	Start int `json:"start"`
	Length int `json:"length"`
	MessageKey string `json:"messageKey"`
	Arguments []string `json:"arguments,omitempty"`
}

func evaluate(q request) map[string]any {
	kind := strings.ToLower(q.Kind)
	if kind != "" && kind != "ts" && kind != "tsx" && kind != "dts" && kind != "declaration" && kind != "mts" && kind != "cts" {
		panic("Unsupported input kind: " + kind)
	}
	name := q.FileName
	if name == "" {
		switch kind {
		case "tsx": name = "/input.tsx"
		case "dts", "declaration": name = "/input.d.ts"
		case "mts": name = "/input.mts"
		case "cts": name = "/input.cts"
		default: name = "/input.ts"
		}
	}
	file := parser.ParseSourceFile(ast.SourceFileParseOptions{
		FileName: name,
		Path: tspath.Path(name),
	}, q.Source, core.GetScriptKindFromFileName(name))
	diags := make([]diagnostic, 0, len(file.Diagnostics()))
	for _, d := range file.Diagnostics() {
		diags = append(diags, diagnostic{d.Code(), d.Pos(), d.Len(), fmt.Sprint(d.MessageKey()), d.MessageArgs()})
	}
	hash := sha256.Sum256([]byte(q.Source))
	return map[string]any{
		"id": q.ID, "version": "7.0.2", "revision": revision,
		"phase": "parse", "offsetUnit": "UTF-8 bytes", "fileName": name,
		"sourceSha256": hex.EncodeToString(hash[:]), "accept": len(diags) == 0,
		"diagnostics": diags,
	}
}

func main() {
	input := bufio.NewScanner(os.Stdin)
	input.Buffer(make([]byte, 64*1024), 128*1024*1024)
	output := json.NewEncoder(os.Stdout)
	for input.Scan() {
		var q request
		if err := json.Unmarshal(input.Bytes(), &q); err != nil {
			fmt.Fprintln(os.Stderr, err)
			os.Exit(2)
		}
		if err := output.Encode(evaluate(q)); err != nil {
			fmt.Fprintln(os.Stderr, err)
			os.Exit(2)
		}
	}
	if err := input.Err(); err != nil {
		fmt.Fprintln(os.Stderr, err)
		os.Exit(2)
	}
}
