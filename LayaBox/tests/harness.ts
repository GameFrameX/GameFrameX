/**
 * 极简测试 harness:无外部依赖(node 可直接执行编译产物)。
 * 全部用例经同一串行链执行(多数套件共享 GameApp/EventSystems 全局态,并发必然交错)。
 * 断言失败抛错由链捕获汇总;有任何失败则 runAll() 抛错(退出码 1)。
 */
const failures: string[] = [];
let chain: Promise<void> = Promise.resolve();

export function test(name: string, fn: () => void | Promise<void>): void {
    chain = chain.then(() => {
        const result = fn();
        if (result instanceof Promise) {
            return result.then(
                () => console.log(`ok - ${name}`),
                (e: unknown) => {
                    failures.push(`${name}: ${e instanceof Error ? e.message : String(e)}`);
                    console.error(`FAIL - ${name}: ${e instanceof Error ? e.message : String(e)}`);
                },
            );
        }

        console.log(`ok - ${name}`);
    });
}

export function assert(condition: boolean, message: string): void {
    if (!condition) {
        throw new Error(message);
    }
}

export function assertEqual<T>(actual: T, expected: T, message: string): void {
    if (actual !== expected) {
        throw new Error(`${message} (expected ${String(expected)}, got ${String(actual)})`);
    }
}

export function assertThrows(fn: () => void, message: string): void {
    let thrown = false;
    try {
        fn();
    } catch {
        thrown = true;
    }

    if (!thrown) {
        throw new Error(`expected throw: ${message}`);
    }
}

export async function runAll(): Promise<void> {
    await chain;
    if (failures.length > 0) {
        console.error(`\n${failures.length} test(s) failed:`);
        for (const failure of failures) {
            console.error(`  - ${failure}`);
        }
        throw new Error(`${failures.length} test(s) failed`);
    }

    console.log('\nall tests passed');
}
