declare const obj: { x: (strings: TemplateStringsArray) => unknown } | undefined;
obj?.x`tag`;
obj?.x!`tag`;
obj?.x! = () => 1;
