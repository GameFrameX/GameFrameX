#!/usr/bin/env node
import { existsSync } from 'node:fs';
import { join } from 'node:path';
import { platform } from 'node:process';

const PATHS = {
    darwin: [
        '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome',
        '/Applications/Chromium.app/Contents/MacOS/Chromium',
        '/Applications/Microsoft Edge.app/Contents/MacOS/Microsoft Edge',
        '/Applications/Brave Browser.app/Contents/MacOS/Brave Browser',
    ],
    win32: [
        'C:/Program Files/Google/Chrome/Application/chrome.exe',
        'C:/Program Files (x86)/Google/Chrome/Application/chrome.exe',
        'C:/Program Files/Microsoft/Edge/Application/msedge.exe',
    ],
    linux: ['/usr/bin/google-chrome', '/usr/bin/chromium-browser', '/usr/bin/chromium'],
};

function getChromePath() {
    const candidates = PATHS[platform] || [];
    for (const p of candidates) {
        if (existsSync(p)) return p;
    }
    return 'google-chrome';
}

export { getChromePath };
