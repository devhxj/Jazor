declare const Bun:
  | {
      file(path: string): { text(): Promise<string> };
      spawn(
        command: string[],
        options: {
          stdin: "pipe";
          stdout: "pipe";
          stderr: "pipe";
        }
      ): {
        stdin: WritableStream<Uint8Array>;
        stdout: ReadableStream<Uint8Array>;
        stderr: ReadableStream<Uint8Array>;
        exited: Promise<number>;
      };
    }
  | undefined;

declare const process: {
  cwd(): string;
};

declare module "node:path" {
  export function dirname(path: string): string;
  export function relative(from: string, to: string): string;
  export function resolve(...paths: string[]): string;
}
