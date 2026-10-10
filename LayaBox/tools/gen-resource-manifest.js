#!/usr/bin/env node
/**
 * 资源版本清单生成器(Laya 资源版本体系,spec §3.4)。
 *
 * 扫描首包资源目录(fairygui UI 包 + config + protobuf),产出:
 * - assets/resources/version.json                { version: "<timestamp>" }
 * - assets/resources/filetable.json              { files: { <path>: { hash, size } } }
 *
 * 远端部署:把 assets/ 内容连同清单发布到资源服务器根目录;
 * 客户端启动链按 <root>/version.json → <root>/<version>/filetable.json 对比差异。
 * 用法:node tools/gen-resource-manifest.js
 */
const fs = require('fs');
const path = require('path');
const crypto = require('crypto');

const root = path.join(__dirname, '..');
const resourceDirs = ['assets/fairygui', 'assets/resources/config', 'assets/resources/protobuf'];
const outputDir = path.join(root, 'assets/resources');
const manifestName = 'filetable.json';
const versionName = 'version.json';

function walk(dir, base, files) {
    if (!fs.existsSync(dir)) {
        return files;
    }

    for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
        const full = path.join(dir, entry.name);
        if (entry.isDirectory()) {
            walk(full, path.posix.join(base, entry.name), files);
        } else if (!entry.name.endsWith('.meta')) {
            files.push({ absolute: full, relative: path.posix.join(base, entry.name) });
        }
    }

    return files;
}

const files = [];
for (const dir of resourceDirs) {
    walk(path.join(root, dir), dir.replace(/^assets\//, ''), files);
}

const fileTable = { files: {} };
let totalSize = 0;
for (const file of files) {
    if (file.relative === `resources/${manifestName}` || file.relative === `resources/${versionName}`) {
        continue;
    }

    const content = fs.readFileSync(file.absolute);
    const hash = crypto.createHash('md5').update(content).digest('hex').slice(0, 16);
    fileTable.files[file.relative] = { hash, size: content.byteLength };
    totalSize += content.byteLength;
}

const version = String(Math.floor(Date.now() / 1000));
fs.mkdirSync(outputDir, { recursive: true });
fs.writeFileSync(path.join(outputDir, versionName), JSON.stringify({ version }, null, 2));
fs.writeFileSync(path.join(outputDir, manifestName), JSON.stringify(fileTable, null, 2));

console.log(`manifest generated: ${Object.keys(fileTable.files).length} files, ${(totalSize / 1024).toFixed(1)} KB, version ${version}`);
console.log(`  ${path.relative(root, path.join(outputDir, versionName))}`);
console.log(`  ${path.relative(root, path.join(outputDir, manifestName))}`);
