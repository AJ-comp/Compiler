export function run(value: number): number; export function run(value: string): string; export function run(value: number | string): number | string { return value; }
export {};