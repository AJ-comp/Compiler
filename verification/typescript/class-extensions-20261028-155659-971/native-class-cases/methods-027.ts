export {};
class C { private async *run(): AsyncIterable<number> { yield await work(); } }
