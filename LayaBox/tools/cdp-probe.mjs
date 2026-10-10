#!/usr/bin/env node
/**
 * 重新用 navigate 刷新页面 + 收集日志 + 截图 + 探针。
 */
import { WebSocket } from 'ws';
import fs from 'node:fs/promises';

const TARGET_URL = 'http://localhost:43990/';

async function fetchJson(url) { return (await fetch(url)).json(); }

class Cdp {
    constructor(ws) {
        this.ws = ws; this.id = 0; this.pending = new Map(); this.events = [];
        ws.on('message', (data) => {
            const msg = JSON.parse(data.toString());
            if (msg.id && this.pending.has(msg.id)) {
                const { resolve, reject } = this.pending.get(msg.id);
                this.pending.delete(msg.id);
                if (msg.error) reject(new Error(JSON.stringify(msg.error))); else resolve(msg.result);
            } else if (msg.method) {
                this.events.push(msg);
            }
        });
    }
    send(method, params = {}) {
        const id = ++this.id;
        return new Promise((resolve, reject) => {
            this.pending.set(id, { resolve, reject });
            this.ws.send(JSON.stringify({ id, method, params }));
        });
    }
}

async function main() {
    const targets = await fetchJson('http://localhost:9222/json');
    const page = targets.find((t) => t.url && t.url.startsWith('http://localhost:43990'));
    if (!page) { console.error('game tab not found'); process.exit(1); }

    const ws = new WebSocket(page.webSocketDebuggerUrl);
    await new Promise((r) => ws.once('open', r));
    const cdp = new Cdp(ws);

    await cdp.send('Page.enable');
    await cdp.send('Runtime.enable');
    await cdp.send('Log.enable');

    const logs = [];
    cdp.ws.on('message', (data) => {
        const msg = JSON.parse(data.toString());
        if (msg.method === 'Runtime.consoleAPICalled') {
            const t = msg.params.type;
            const text = msg.params.args.map((a) => a.value !== undefined ? a.value : a.description).join(' ');
            logs.push(`${t} ${text}`);
        } else if (msg.method === 'Runtime.exceptionThrown') {
            logs.push(`EXC ${msg.params.exceptionDetails.text}`);
        } else if (msg.method === 'Log.entryAdded') {
            logs.push(`${msg.params.entry.level} ${msg.params.entry.text}`);
        } else if (msg.method === 'Page.frameNavigated') {
            logs.push(`NAV ${msg.params.url}`);
        }
    });

    await cdp.send('Page.navigate', { url: TARGET_URL });
    await new Promise((r) => setTimeout(r, 8000));

    // 探针: laya 引擎 + fgui + Main 脚本 + UILogin fgui 包加载状态
    const probe = await cdp.send('Runtime.evaluate', {
        expression: `(function(){
            try {
                const r = {};
                r.Laya = typeof Laya;
                r.fgui = typeof fgui;
                r.protobuf = typeof protobuf;
                r.Main = typeof Main;
                r.Long = typeof Long;
                r.stageSize = (window.Laya && window.Laya.stage) ? [window.Laya.stage.width, window.Laya.stage.height, window.Laya.stage.numChildren] : null;
                r.fguiPackages = {};
                if (window.fgui && fgui.UIPackage) {
                    ['UICommon','UILogin','UIMain','UIRoom','UIBag','UICommonAvatar','UILoading'].forEach(n => {
                        r.fguiPackages[n] = !!fgui.UIPackage.getByName(n);
                    });
                }
                // UILogin 的 onAwake 已经构造过吗?查 scene 树里有 m_UserName 等字段的实例
                const scene2D = window.Laya ? Laya.stage.getChildByName('Scene2D') : null;
                r.scene2DChildren = scene2D ? scene2D.numChildren : null;
                return JSON.stringify(r);
            } catch(e) { return 'probe-err: '+e.message; }
        })()`,
        returnByValue: true,
    });
    console.log('--- probe ---');
    console.log(probe.value);

    console.log(`--- console + exceptions + nav (${logs.length} entries) ---`);
    for (const l of logs.slice(-60)) console.log(l);

    // 截图
    const shot = await cdp.send('Page.captureScreenshot', { format: 'png' });
    await fs.writeFile('/tmp/laya-game-2.png', Buffer.from(shot.data, 'base64'));
    console.log('screenshot: /tmp/laya-game-2.png ' + shot.data.length + ' base64 bytes');

    ws.close();
    process.exit(0);
}

main().catch((e) => { console.error('probe failed:', e); process.exit(1); });
