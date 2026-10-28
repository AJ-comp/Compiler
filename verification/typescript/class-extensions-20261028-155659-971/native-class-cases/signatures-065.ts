export {};
class C { run(value: string): string; run(value: number): number; run(value: unknown): unknown { return value; } }
