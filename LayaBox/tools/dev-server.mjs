#!/usr/bin/env node
import { context } from 'esbuild';
import { createServer } from 'node:http';
import { readFile, mkdir } from 'node:fs/promises';
import { existsSync } from 'node:fs';
import { join, extname } from 'node:path';
import { spawn } from 'node:child_process';
import { getChromePath } from './_chromePath.mjs';

const ROOT = process.argv.find((a) => a.startsWith('--root='))?.slice(7) || process.cwd();
const PORT = +(process.argv.find((a) => a.startsWith('--port='))?.slice(7)) || 43990;
const ENGINE_DIR = '/Applications/LayaAirIDE.app/Contents';
const ENGINE_RESOURCES = join(ENGINE_DIR, 'resources');
const OUT_DIR = join(ROOT, 'bin/release');
const BUNDLE_PATH = join(OUT_DIR, 'js/bundle.js');

const MIME = {
    '.html': 'text/html; charset=utf-8',
    '.js': 'text/javascript; charset=utf-8',
    '.css': 'text/css; charset=utf-8',
    '.json': 'application/json; charset=utf-8',
    '.png': 'image/png',
    '.jpg': 'image/jpeg',
    '.atlas': 'image/png',
};

async function main() {
    await mkdir(join(OUT_DIR, 'js'), { recursive: true });
    const ctx = await context({
        entryPoints: [join(ROOT, 'src/Main.ts')],
        bundle: true,
        format: 'iife',
        platform: 'browser',
        target: 'es2017',
        outfile: BUNDLE_PATH,
        logLevel: 'warning',
        tsconfig: join(ROOT, 'tsconfig.json'),
        external: ['fgui', 'Laya', 'wx'],
    });
    await ctx.rebuild();
    console.log(`bundle: ${BUNDLE_PATH}`);

    const server = createServer(async (req, res) => {
        try {
            const p = new URL(req.url, 'http://x').pathname;

            if (p === '/' || p === '/index.html') {
                const tpl = await readFile(join(ROOT, 'bin/web-index.html'), 'utf8');
                res.writeHead(200, { 'Content-Type': MIME['.html'] });
                return res.end(tpl);
            }

            if (p.startsWith('/libs/')) {
                const name = p.slice(6);
                for (const c of [join(ROOT, 'bin/libs', name), join(ENGINE_RESOURCES, 'engine/libs', name)]) {
                    if (existsSync(c)) return serveFrom(c, res);
                }
                res.writeHead(404, { 'Content-Type': 'text/plain' });
                return res.end('lib not found: ' + name);
            }

            if (p === '/js/bundle.js') return serveFrom(BUNDLE_PATH, res);

            if (p.startsWith('/fairygui/')) {
                return serveFrom(join(ROOT, 'assets/fairygui', p.slice(10)), res);
            }

            if (p.startsWith('/assets/fairygui/')) {
                return serveFrom(join(ROOT, 'assets/fairygui', p.slice(15)), res);
            }

            if (p.startsWith('/assets/')) {
                return serveFrom(join(ROOT, 'assets', p.slice(8)), res);
            }

            res.writeHead(404, { 'Content-Type': 'text/plain' });
            res.end('not found');
        } catch (error) {
            res.writeHead(500, { 'Content-Type': 'text/plain' });
            res.end('server error: ' + (error && error.message || error));
        }
    });

    server.listen(PORT, () => console.log(`dev server: http://localhost:${PORT}`));

    if (!process.argv.includes('--no-browser')) {
        const exe = getChromePath();
        spawn(exe, [`--remote-debugging-port=9222`, '--no-first-run', '--user-data-dir=/tmp/laya-cdp-profile', `http://localhost:${PORT}`], { stdio: 'inherit' });
        console.log('chrome launched cdp=9222');
    }
}

function serveFrom(filePath, res) {
    if (!existsSync(filePath)) {
        res.writeHead(404, { 'Content-Type': 'text/plain' });
        return res.end('not found: ' + filePath);
    }
    const mime = MIME[extname(filePath).toLowerCase()] || 'application/octet-stream';
    readFile(filePath).then((buf) => {
        res.writeHead(200, { 'Content-Type': mime });
        res.end(buf);
    }).catch((err) => {
        res.writeHead(500, { 'Content-Type': 'text/plain' });
        res.end('read error: ' + err.message);
    });
}

main().catch((error) => {
    console.error('dev-server failed:', error);
    process.exit(1);
});
