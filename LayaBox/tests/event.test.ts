import EventSystems from "../src/gameframex/event/EventSystems";
import { EmptyEventArgs } from "../src/gameframex/event/EmptyEventArgs";
import { NetworkMissHeartBeatEventArgs } from "../src/gameframex/event/NetworkEventArgs";
import type GameEventArgs from "../src/gameframex/event/GameEventArgs";
import { test, assertEqual, assertThrows, assert } from "./harness";

test('Event: Fire 入队,update 时统一分发', () => {
    const received: string[] = [];
    const handler = (_sender: unknown, args: GameEventArgs) => received.push(args.Id);
    EventSystems.Subscribe('QueueEvent', handler);
    try {
        EventSystems.Fire(null, EmptyEventArgs.Create('QueueEvent'));
        assertEqual(received.length, 0, 'Fire 后未 update 不应分发');
        EventSystems.instance.update(0, 0);
        assertEqual(received.join(','), 'QueueEvent', 'update 后分发');
    } finally {
        EventSystems.Unsubscribe('QueueEvent', handler);
    }
});

test('Event: FireNow 立即分发,Fire(字符串) 装箱 EmptyEventArgs', () => {
    const received: string[] = [];
    const handler = (_sender: unknown, args: GameEventArgs) => received.push(args.Id);
    EventSystems.Subscribe('StringEvent', handler);
    try {
        EventSystems.FireNow(null, 'StringEvent');
        assertEqual(received.join(','), 'StringEvent', '字符串重载立即分发');
    } finally {
        EventSystems.Unsubscribe('StringEvent', handler);
    }
});

test('Event: 订阅/退订生命周期与重复订阅检查', () => {
    let hits = 0;
    const handler = () => hits++;
    EventSystems.Subscribe('Lifecycle', handler);
    assert(EventSystems.Check('Lifecycle', handler), '订阅后 Check 命中');
    assertEqual(EventSystems.Count('Lifecycle'), 1, '订阅计数');
    assertThrows(() => EventSystems.Subscribe('Lifecycle', handler), '重复订阅应抛错');

    EventSystems.FireNow(null, 'Lifecycle');
    assertEqual(hits, 1, '分发一次');

    EventSystems.Unsubscribe('Lifecycle', handler);
    assert(!EventSystems.Check('Lifecycle', handler), '退订后 Check 未命中');
    assertEqual(EventSystems.Count('Lifecycle'), 0, '退订后计数归零');
    EventSystems.FireNow(null, 'Lifecycle');
    assertEqual(hits, 1, '退订后不再分发');

    EventSystems.CheckSubscribe('Lifecycle', handler);
    EventSystems.CheckSubscribe('Lifecycle', handler);
    assertEqual(EventSystems.Count('Lifecycle'), 1, 'CheckSubscribe 幂等');
    EventSystems.CheckUnsubscribe('Lifecycle', handler);
    assertEqual(EventSystems.Count('Lifecycle'), 0, 'CheckUnsubscribe 后归零');
});

test('Event: 同 id 多 handler 按订阅顺序;处理函数内退订自身安全', () => {
    const order: string[] = [];
    const first = () => {
        order.push('first');
        EventSystems.Unsubscribe('Ordered', second);
    };
    const second = () => order.push('second');
    const third = () => order.push('third');
    EventSystems.Subscribe('Ordered', first);
    EventSystems.Subscribe('Ordered', second);
    EventSystems.Subscribe('Ordered', third);
    try {
        EventSystems.FireNow(null, 'Ordered');
        assertEqual(order.join(','), 'first,third', 'first 内退订 second,second 不再收到');
    } finally {
        EventSystems.Unsubscribe('Ordered', first);
        EventSystems.Unsubscribe('Ordered', third);
    }
});

test('Event: 结构化事件参数携带负载', () => {
    let missCount = -1;
    const handler = (_sender: unknown, args: GameEventArgs) => {
        missCount = (args as NetworkMissHeartBeatEventArgs).MissCount;
    };
    EventSystems.Subscribe(NetworkMissHeartBeatEventArgs.EventId, handler);
    try {
        EventSystems.FireNow(null, NetworkMissHeartBeatEventArgs.Create(null as never, 3));
        assertEqual(missCount, 3, '负载透传');
    } finally {
        EventSystems.Unsubscribe(NetworkMissHeartBeatEventArgs.EventId, handler);
    }
});
