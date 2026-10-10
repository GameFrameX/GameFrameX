import './fsm.test';
import './procedure.test';
import './event.test';
import './packet.test';
import './network.test';
import './ui.test';
import './config.test';
import './startup.test';
import './gameentry.test';
import { runAll } from './harness';

void runAll().catch((e: unknown) => {
    console.error(e instanceof Error ? e.message : String(e));
    throw e;
});
