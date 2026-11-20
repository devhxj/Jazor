import Meter from '@jazor/library-meter-fixture/Meter';

export const missingValue = <Meter />;
export const invalidCaption = <Meter value={1} caption={42} />;
export const invalidModel = <Meter value={1} onUpdate:value={(value: string) => value} />;
export const invalidSlots = <Meter value={1} v-slots={{
    reading: ({ amount }) => amount.toUpperCase()
}} />;
export function invalidExpose(meter: InstanceType<typeof Meter>) {
    return meter.Multiply('wrong');
}
